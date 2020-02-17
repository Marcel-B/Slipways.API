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
using System.Threading;
using System.Threading.Tasks;

namespace Slipways.API.Tests
{
    [ExcludeFromCodeCoverage]
    public class ManufacturersControllerTests
    {
        public IList<LogEntry> Logs { get; set; }
        public IList<Manufacturer> Manufacturers { get; set; }

        private FakeLogger<ManufacturersController> GetLogger()
            => new FakeLogger<ManufacturersController>(Logs);

        private ManufacturersController GetSut(
            bool errorInsertingValue = false,
            bool throwsException = false)
            => new ManufacturersController(GetRepositoryWrapper(errorInsertingValue, throwsException), GetLogger());

        private IRepositoryWrapper GetRepositoryWrapper(
            bool errorInsertingValue,
            bool throwsException)
        {
            var mock = new Mock<IRepositoryWrapper>();

            if (throwsException)
            {
                mock.Setup(_ => _.Manufacturer.SelectAllAsync(It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new Exception());

                mock.Setup(_ => _.Manufacturer.InsertAsync(It.IsAny<Manufacturer>(), It.IsAny<CancellationToken>(), It.IsAny<bool>()))
                    .ThrowsAsync(new Exception());
            }
            else if (errorInsertingValue)
            {
                mock.Setup(_ => _.Manufacturer.InsertAsync(It.IsAny<Manufacturer>(), It.IsAny<CancellationToken>(), It.IsAny<bool>()));
            }
            else
            {
                mock.Setup(_ => _.Manufacturer.SelectAllAsync(It.IsAny<CancellationToken>()))
                    .Returns(Task.FromResult((IEnumerable<Manufacturer>)new[]
                    {
                        GetManufacturer("Mercury"),
                        GetManufacturer("Honda"),
                        GetManufacturer("Yamaha")
                    }));

                mock.Setup(_ => _.Manufacturer.InsertAsync(It.IsAny<Manufacturer>(), It.IsAny<CancellationToken>(), It.IsAny<bool>()))
                    .Callback((Manufacturer manufacturer, CancellationToken token, bool save) =>
                    {
                        manufacturer.Created = DateTime.Now;
                        manufacturer.Id = Guid.NewGuid();
                        Manufacturers.Add(manufacturer);
                    })
                    .Returns(Task.FromResult(new Manufacturer { Id = Guid.NewGuid() }));
            }
            return mock.Object;
        }

        private Manufacturer GetManufacturer(
            string name)
            => new Manufacturer
            {
                Name = name,
                Created = DateTime.Now,
                Id = Guid.NewGuid(),
            };

        private ManufacturerDto GetDto(
            string name = "Mercury")
            => new ManufacturerDto
            {
                Name = name,
                Id = Guid.Empty
            };

        [SetUp]
        public void SetUp()
        {
            Logs = new List<LogEntry>();
            Manufacturers = new List<Manufacturer>();
        }

        [Test]
        public async Task ManufacturerController_GetAsync_Returns3Manufacturers()
        {
            // Arrange
            var sut = GetSut();
            const int expected = 3;

            // Act
            var response = await sut.GetAsync(CancellationToken.None) as OkObjectResult;
            var manufactuerDtos = response.Value as IEnumerable<ManufacturerDto>;
            var actual = manufactuerDtos.ToList().Count;

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public async Task ManufacturerController_GetAsync_LogsErrorWhenUnexpectedErrorOccures()
        {
            // Arrange
            var sut = GetSut(throwsException: true);

            // Act
            _ = await sut.GetAsync(CancellationToken.None);

            // Assert
            Assert.That(Logs.Count == 1);
        }

        [Test]
        public async Task ManufacturerController_GetAsync_LogEventId6666WhenUnexpectedErrorOccures()
        {
            // Arrange
            var sut = GetSut(throwsException: true);
            const int expected = 6666;

            // Act
            _ = await sut.GetAsync(CancellationToken.None);
            var actual = Logs.First().EventId;

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public async Task ManufacturerController_GetAsync_LogLevelErrorWhenUnexpectedErrorOccures()
        {
            // Arrange
            var sut = GetSut(throwsException: true);
            var expected = LogLevel.Error;

            // Act
            _ = await sut.GetAsync(CancellationToken.None);
            var actual = Logs.First().LogLevel;

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public async Task ManufacturerController_GetAsync_LogExceptionWhenUnexpectedErrorOccures()
        {
            // Arrange
            var sut = GetSut(throwsException: true);

            // Act
            _ = await sut.GetAsync(CancellationToken.None);
            var actual = Logs.First().Exception;

            // Assert

            Assert.IsInstanceOf<Exception>(actual);
        }

        [Test]
        public async Task ManufacturerController_GetAsync_StatusCode500WhenUnexpectedErrorOccures()
        {
            // Arrange
            var sut = GetSut(throwsException: true);
            const int expected = 500;

            // Act
            var response = await sut.GetAsync(CancellationToken.None) as StatusCodeResult;
            var actual = response.StatusCode;

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public async Task ManufacturerController_PostAsync_ReturnsBadRequestWhenDtoIsNull()
        {
            // Arrange
            var sut = GetSut();
            var expected = 400;

            // Act
            var response = await sut.PostAsync(null);
            var badRequestObjectResult = response as BadRequestObjectResult;
            var actual = badRequestObjectResult.StatusCode;

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public async Task ManufacturerController_PostAsync_LogsErrorWhenDtoIsNull()
        {
            // Arrange
            var sut = GetSut();

            // Act
            _ = await sut.PostAsync(null);

            // Assert
            Assert.That(Logs.Count == 1);
        }

        [Test]
        public async Task ManufactuerController_PostAsync_LogLevelWarningWhenDtoIsNull()
        {
            // Arrange
            var sut = GetSut();
            var expected = LogLevel.Warning;

            // Act
            _ = await sut.PostAsync(null);
            var actual = Logs.First().LogLevel;

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public async Task ManufactuerController_PostAsync_ReturnsStatusCode500WhenUnexpectedErrorOccurres()
        {
            // Arrange
            var sut = GetSut(throwsException: true);
            const int expected = 500;

            // Act
            var response = await sut.PostAsync(GetDto());
            var statusCodeResult = response as StatusCodeResult;
            var actual = statusCodeResult.StatusCode;

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public async Task ManufactuerController_PostAsync_LogsErrorWhenUnexpectedErrorOccurres()
        {
            // Arrange
            var sut = GetSut(throwsException: true);

            // Act
            _ = await sut.PostAsync(GetDto());

            // Assert
            Assert.That(Logs.Count == 1);
        }


        [Test]
        public async Task ManufactuerController_PostAsync_LogExceptionWhenUnexpectedErrorOccurres()
        {
            // Arrange
            var sut = GetSut(throwsException: true);

            // Act
            _ = await sut.PostAsync(GetDto());
            var actual = Logs.First().Exception;

            // Assert
            Assert.IsInstanceOf<Exception>(actual);
        }

        [Test]
        public async Task ManufactuerController_PostAsync_LogLevelErrorWhenUnexpectedErrorOccurres()
        {
            // Arrange
            var sut = GetSut(throwsException: true);
            var expected = LogLevel.Error;

            // Act
            _ = await sut.PostAsync(GetDto());
            var actual = Logs.First().LogLevel;

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public async Task ManufactuerController_PostAsync_LogsEventIdIs6666WhenUnexpectedErrorOccurres()
        {
            // Arrange
            var sut = GetSut(throwsException: true);
            const int expected = 6666;

            // Act
            _ = await sut.PostAsync(GetDto());
            var actual = Logs.First().EventId;

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public async Task ManufactuerController_PostAsync_ReturnsDtoWithNonEmptyId()
        {
            // Arrange
            var sut = GetSut();

            // Act
            var response = await sut.PostAsync(GetDto()) as JsonResult;
            var value = response.Value as ManufacturerDto;

            // Assert
            Assert.That(value.Id != Guid.Empty);
        }

        [Test]
        public async Task ManufactuerController_PostAsync_LogsErrorWhenInsertValueFails()
        {
            // Arrange
            var sut = GetSut(errorInsertingValue: true);

            // Act
            _ = await sut.PostAsync(GetDto());

            // Assert
            Assert.That(Logs.Count == 1);
        }

        [Test]
        public async Task ManufactuerController_PostAsync_LogsErrorWithEventId5005WhenInsertValueFails()
        {
            // Arrange
            var sut = GetSut(errorInsertingValue: true);
            const int expected = 5005;

            // Act
            _ = await sut.PostAsync(GetDto());
            var actual = Logs.First().EventId;

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public async Task ManufactuerController_PostAsync_ReturnsStatusCode500WhenInsertValueFails()
        {
            // Arrange
            var sut = GetSut(errorInsertingValue: true);
            const int expected = 500;

            // Act
            var response = await sut.PostAsync(GetDto());
            var statusCodeResult = response as StatusCodeResult;
            var actual = statusCodeResult.StatusCode;

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public async Task ManufactuerController_PostAsync_LogLevelErrorWhenInsertValueFails()
        {
            // Arrange
            var sut = GetSut(errorInsertingValue: true);
            var expected = LogLevel.Error;

            // Act
            _ = await sut.PostAsync(GetDto());
            var actual = Logs.First().LogLevel;

            // Assert
            Assert.AreEqual(expected, actual);
        }
    }
}
