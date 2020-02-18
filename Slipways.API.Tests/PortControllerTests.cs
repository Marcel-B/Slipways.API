using com.b_velop.Slipways.API.Controllers;
using com.b_velop.Slipways.Data.Contracts;
using com.b_velop.Slipways.Data.Dtos;
using com.b_velop.Slipways.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Slipways.API.Tests
{
    [ExcludeFromCodeCoverage]
    public class PortControllerTests
    {
        public IList<LogEntry> Logs { get; set; }
        public IList<Port> Ports { get; set; }
        public IList<Slipway> Slipways { get; set; }

        [SetUp]
        public void SetUp()
        {
            Logs = new List<LogEntry>();
            Ports = new List<Port>();
            Slipways = new List<Slipway>();
        }

        private PortDto GetDto(
            string name = "Test")
            => new PortDto
            {
                City = "Köln",
                Id = Guid.NewGuid(),
                Name = name,
                Slipways = new[]
                {
                    new SlipwayDto{Id = Guid.NewGuid(), Name = "Test_1"},
                    new SlipwayDto{Id = Guid.NewGuid(), Name = "Test_2"},
                }
            };

        private FakeLogger<PortController> GetLogger()
            => new FakeLogger<PortController>(Logs);

        private IRepositoryWrapper GetRepositoryWrapper(
            bool errorInsertingValue,
            bool throwsException)
        {
            var mock = new Mock<IRepositoryWrapper>();

            if (throwsException)
            {
                mock.Setup(_ => _.Port.InsertAsync(It.IsAny<Port>(), It.IsAny<CancellationToken>(), It.IsAny<bool>()))
                    .ThrowsAsync(new Exception());
            }
            else if (errorInsertingValue)
            {
                mock.Setup(_ => _.Port.InsertAsync(It.IsAny<Port>(), It.IsAny<CancellationToken>(), It.IsAny<bool>()));
            }
            else
            {
                mock.Setup(_ => _.Port.InsertAsync(It.IsAny<Port>(), It.IsAny<CancellationToken>(), It.IsAny<bool>()))
                    .Callback((Port port, CancellationToken token, bool saveChanges) =>
                    {
                        port.Id = Guid.NewGuid();
                        port.Created = DateTime.Now;
                        Ports.Add(port);
                    })
                    .Returns(Task.FromResult(new Port { Id = Guid.NewGuid(), Name = "Test" }));

            }
            mock.Setup(_ => _.SaveChanges());
            mock.Setup(_ => _.Slipway.AddPortToSlipwayAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(Task.FromResult(new Slipway { Name = "Kirk", Id = Guid.NewGuid() }));
            return mock.Object;
        }
        private PortController GetSut(
            bool errorInsertingValue = false,
            bool throwsException = false)
            => new PortController(GetRepositoryWrapper(errorInsertingValue, throwsException), GetLogger());

        [Test]
        public async Task PortController_PostAsync_ReturnsBadRequetsWhenDtoIsNull()
        {
            // Arrange
            var sut = GetSut();

            // Act
            var actual = await sut.PostAsync(null, CancellationToken.None);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(actual);
        }

        [Test]
        public async Task PortController_PostAsync_CreateLogWhenDtoIsNull()
        {
            // Arrange
            var sut = GetSut();

            // Act
            _ = await sut.PostAsync(null, CancellationToken.None);

            // Assert
            Assert.That(Logs.Count == 1);
        }

        [Test]
        public async Task PortController_PostAsync_LogWaringWhenDtoIsNull()
        {
            // Arrange
            var sut = GetSut();
            var expected = LogLevel.Warning;

            // Act
            _ = await sut.PostAsync(null, CancellationToken.None);
            var actual = Logs.First().LogLevel;

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public async Task PortController_PostAsync_CreateLogWithEventId5000WhenDtoIsNull()
        {
            // Arrange
            var sut = GetSut();
            var expected = 5000;

            // Act
            _ = await sut.PostAsync(null, CancellationToken.None);
            var actual = Logs.First().EventId;

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public async Task PortController_PostAsync_CreateLogWithEventId5000WhenDtosNameIsEmpty()
        {
            // Arrange
            var sut = GetSut();
            var expected = 5000;

            // Act
            _ = await sut.PostAsync(GetDto(""), CancellationToken.None);
            var actual = Logs.First().EventId;

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public async Task PortController_PostAsync_ReturnsStatusCode500WhenUnexpectedErrorOccurres()
        {
            // Arrange
            var sut = GetSut(throwsException: true);
            const int expected = 500;

            // Act
            var response = await sut.PostAsync(GetDto(), CancellationToken.None);
            var statusCodeResult = response as StatusCodeResult;
            var actual = statusCodeResult.StatusCode;

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public async Task PortController_PostAsync_LogsErrorWhenUnexpectedErrorOccurres()
        {
            // Arrange
            var sut = GetSut(throwsException: true);

            // Act
            _ = await sut.PostAsync(GetDto(), CancellationToken.None);

            // Assert
            Assert.That(Logs.Count == 1);
        }

        [Test]
        public async Task PortController_PostAsync_LogExceptionWhenUnexpectedErrorOccurres()
        {
            // Arrange
            var sut = GetSut(throwsException: true);

            // Act
            _ = await sut.PostAsync(GetDto(), CancellationToken.None);
            var actual = Logs.First().Exception;

            // Assert
            Assert.IsInstanceOf<Exception>(actual);
        }

        [Test]
        public async Task PortController_PostAsync_LogLevelErrorWhenUnexpectedErrorOccurres()
        {
            // Arrange
            var sut = GetSut(throwsException: true);
            var expected = LogLevel.Error;

            // Act
            _ = await sut.PostAsync(GetDto(), CancellationToken.None);
            var actual = Logs.First().LogLevel;

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public async Task PortController_PostAsync_LogsEventIdIs6666WhenUnexpectedErrorOccurres()
        {
            // Arrange
            var sut = GetSut(throwsException: true);
            const int expected = 6666;

            // Act
            _ = await sut.PostAsync(GetDto(), CancellationToken.None);
            var actual = Logs.First().EventId;

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public async Task PortController_PostAsync_ReturnsDtoWithNonEmptyId()
        {
            // Arrange
            var sut = GetSut();

            // Act
            var response = await sut.PostAsync(GetDto(), CancellationToken.None) as JsonResult;
            var value = response.Value as PortDto;

            // Assert
            Assert.That(value.Id != Guid.Empty);
        }

        [Test]
        public async Task PortController_PostAsync_LogsErrorWhenInsertValueFails()
        {
            // Arrange
            var sut = GetSut(errorInsertingValue: true);

            // Act
            _ = await sut.PostAsync(GetDto(), CancellationToken.None);

            // Assert
            Assert.That(Logs.Count == 1);
        }

        [Test]
        public async Task PortController_PostAsync_LogsErrorWithEventId5005WhenInsertValueFails()
        {
            // Arrange
            var sut = GetSut(errorInsertingValue: true);
            const int expected = 5005;

            // Act
            _ = await sut.PostAsync(GetDto(), CancellationToken.None);
            var actual = Logs.First().EventId;

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public async Task PortController_PostAsync_ReturnsStatusCode500WhenInsertValueFails()
        {
            // Arrange
            var sut = GetSut(errorInsertingValue: true);
            const int expected = 500;

            // Act
            var response = await sut.PostAsync(GetDto(), CancellationToken.None);
            var statusCodeResult = response as StatusCodeResult;
            var actual = statusCodeResult.StatusCode;

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public async Task PortController_PostAsync_LogLevelErrorWhenInsertValueFails()
        {
            // Arrange
            var sut = GetSut(errorInsertingValue: true);
            var expected = LogLevel.Error;

            // Act
            _ = await sut.PostAsync(GetDto(), CancellationToken.None);
            var actual = Logs.First().LogLevel;

            // Assert
            Assert.AreEqual(expected, actual);
        }
    }
}

