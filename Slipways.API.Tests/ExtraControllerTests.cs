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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Slipways.API.Tests
{
    public class ExtraControllerTests
    {
        public class FakeLogger : ILogger<ExtraController>, IDisposable
        {
            private List<LogEntry> Logs;

            public FakeLogger(
                List<LogEntry> logs)
            {
                this.Logs = logs;
            }

            public IDisposable BeginScope<TState>(TState state)
            {
                return this;
            }

            public void Dispose()
            {
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                Logs.Add(new LogEntry { Exception = exception, EventId = eventId.Id, LogLevel = logLevel, Message = state.ToString() });
            }
        }
        public class LogEntry
        {
            public int EventId { get; set; }
            public LogLevel LogLevel { get; set; }
            public string Message { get; set; }
            public Exception Exception { get; set; }
        }

        public List<LogEntry> Logs { get; set; }

        ICollection<string> Extras;

        private const string ExtraName = "Sauna";

        private IRepositoryWrapper GetRepositoryWrapper(
            bool fails = false,
            bool throwsException = false)
        {
            var repositoryWrapper = new Mock<IRepositoryWrapper>();

            if (throwsException)
                repositoryWrapper
                    .Setup(_ => _.Extra.InsertAsync(It.IsAny<Extra>(), It.IsAny<CancellationToken>(), true))
                    .ThrowsAsync(new Exception());
            else
                repositoryWrapper
                    .Setup(_ => _.Extra.InsertAsync(It.IsAny<Extra>(), It.IsAny<CancellationToken>(), true))
                    .Callback((Extra extra, CancellationToken token, bool save) =>
                    {
                        extra.Id = Guid.NewGuid();
                        extra.Created = DateTime.Now;
                        Extras.Add(extra.Name);
                    })
                    .Returns(Task.FromResult(fails ? null : new Extra { Id = Guid.NewGuid(), Name = ExtraName, Created = DateTime.Now }));

            return repositoryWrapper.Object;
        }

        [SetUp]
        public void SetUp()
        {
            Extras = new List<string>();
            Logs = new List<LogEntry>();
        }

        [Test]
        public async Task ExtraController_PostAsync_ReturnsBadRequestWhenDtoIsNull()
        {
            // Arrange
            var sut = new ExtraController(GetRepositoryWrapper(), new FakeLogger(Logs));

            // Act
            var actual = await sut.PostAsync(null, CancellationToken.None);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(actual);
        }


        [Test]
        public async Task ExtraController_PostAsync_ReturnsBadRequestWhenDtosNameIsStringEmpty()
        {
            // Arrange
            var sut = new ExtraController(GetRepositoryWrapper(), new FakeLogger(Logs));
            var dto = new ExtraDto();

            // Act
            var actual = await sut.PostAsync(dto, CancellationToken.None);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(actual);
        }

        [Test]
        public async Task ExtraController_PostAsync_DtoAddToRepository()
        {
            // Arrange
            var sut = new ExtraController(GetRepositoryWrapper(), new FakeLogger(Logs));
            var dto = new ExtraDto
            {
                Name = ExtraName
            };

            // Act
            _ = await sut.PostAsync(dto, CancellationToken.None);

            // Assert
            Assert.That(Extras.Contains(ExtraName));
        }

        [Test]
        public async Task ExtraController_PostAsync_ReturnJsonWithName()
        {
            // Arrange
            var sut = new ExtraController(GetRepositoryWrapper(), new FakeLogger(Logs));
            var expected = ExtraName;

            var dto = new ExtraDto
            {
                Name = ExtraName
            };

            // Act
            var value = await sut.PostAsync(dto, CancellationToken.None);
            var jsonResult = value as JsonResult;
            var resultDto = jsonResult.Value as ExtraDto;
            var actual = resultDto.Name;

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public async Task ExtraController_PostAsync_ReturnJsonWithId()
        {
            // Arrange
            var sut = new ExtraController(GetRepositoryWrapper(), new FakeLogger(Logs));

            var dto = new ExtraDto
            {
                Name = ExtraName
            };

            // Act
            var value = await sut.PostAsync(dto, CancellationToken.None);
            var jsonResult = value as JsonResult;
            var resultDto = jsonResult.Value as ExtraDto;
            var actual = resultDto.Id;

            // Assert
            Assert.That(actual != Guid.Empty);
        }

        [Test]
        public async Task ExtraController_PostAsync_LogsErrorWhenResultIsNull()
        {
            // Arrange
            var sut = new ExtraController(GetRepositoryWrapper(true), new FakeLogger(Logs));

            var dto = new ExtraDto
            {
                Name = ExtraName
            };

            // Act
            _ = await sut.PostAsync(dto, CancellationToken.None);
            var actual = Logs.ToList();

            // Assert
            Assert.That(actual.Count > 0);
        }


        [Test]
        public async Task ExtraController_PostAsync_LogsErrorWithEventID6600WhenResultIsNull()
        {
            // Arrange
            var sut = new ExtraController(GetRepositoryWrapper(true), new FakeLogger(Logs));
            var expected = 6600;

            var dto = new ExtraDto
            {
                Name = ExtraName
            };

            // Act
            _ = await sut.PostAsync(dto, CancellationToken.None);
            var actual = Logs.First().EventId;

            // Assert
            Assert.AreEqual(expected, actual);
        }


        [Test]
        public async Task ExtraController_PostAsync_ReturnsServerErrorWhenResultIsNull()
        {
            // Arrange
            var sut = new ExtraController(GetRepositoryWrapper(true), new FakeLogger(Logs));
            var expected = 500;

            var dto = new ExtraDto
            {
                Name = ExtraName
            };

            // Act
            var response = await sut.PostAsync(dto, CancellationToken.None) as StatusCodeResult;
            var actual = response.StatusCode;

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public async Task ExtraController_PostAsync_LogsErrorWithEventID6666WhenUnexpectedErrorOccurs()
        {
            // Arrange
            var sut = new ExtraController(GetRepositoryWrapper(throwsException: true), new FakeLogger(Logs));
            var expected = 6666;

            var dto = new ExtraDto
            {
                Name = ExtraName
            };

            // Act
            _ = await sut.PostAsync(dto, CancellationToken.None);
            var actual = Logs.First().EventId;

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public async Task ExtraController_PostAsync_ReturnsServerErrorWhenUnexpectedExceptionIsThrown()
        {
            // Arrange
            var sut = new ExtraController(GetRepositoryWrapper(throwsException: true), new FakeLogger(Logs));
            var expected = 500;

            var dto = new ExtraDto
            {
                Name = ExtraName
            };

            // Act
            var response = await sut.PostAsync(dto, CancellationToken.None) as StatusCodeResult;
            var actual = response.StatusCode;

            // Assert
            Assert.AreEqual(expected, actual);
        }
    }
}

