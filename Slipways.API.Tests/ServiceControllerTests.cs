using com.b_velop.Slipways.API.Controllers;
using com.b_velop.Slipways.Data.Contracts;
using com.b_velop.Slipways.Data.Models;
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
    public class ServiceControllerTests
    {
        public IList<Service> Services { get; set; }
        public IList<LogEntry> Logs { get; set; }
        public Guid ServiceId { get; } = Guid.Parse("86B6DD81-0959-4BFE-986D-7881A6C4DCDB");

        [SetUp]
        public void SetUp()
        {
            Services = new List<Service>();
            Logs = new List<LogEntry>();
        }

        private FakeLogger<ServiceController> GetLogger()
            => new FakeLogger<ServiceController>(Logs);

        private IRepositoryWrapper GetRepositoryWrapper(
            bool errorInsertingValue,
            bool throwsException)
        {
            var repositoryWrapper = new Mock<IRepositoryWrapper>();

            if (errorInsertingValue)
            {
                repositoryWrapper.Setup(_ => _.Service.InsertAsync(It.IsAny<Service>(), It.IsAny<CancellationToken>(), It.IsAny<bool>()));
                repositoryWrapper.Setup(_ => _.Service.SelectAllAsync(It.IsAny<CancellationToken>()));
            }
            else if (throwsException)
            {
                repositoryWrapper.Setup(_ => _.Service.InsertAsync(It.IsAny<Service>(), It.IsAny<CancellationToken>(), It.IsAny<bool>()))
                    .ThrowsAsync(new Exception());

                repositoryWrapper.Setup(_ => _.Service.SelectAllAsync(It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new Exception());
            }
            else
            {
                repositoryWrapper.Setup(_ => _.Service.InsertAsync(It.IsAny<Service>(), It.IsAny<CancellationToken>(), It.IsAny<bool>()))
                    .Callback((Service service, CancellationToken token, bool save) =>
                    {
                        service.Id = ServiceId;
                        service.Created = DateTime.Now;
                        Services.Add(service);
                    })
                    .ReturnsAsync(new Service { Id = ServiceId });

                repositoryWrapper.Setup(_ => _.Service.SelectAllAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new List<Service> { new Service { Id = ServiceId, Name = "Bob" } });
            }


            return repositoryWrapper.Object;
        }

        private ServiceController GetSut(
            IRepositoryWrapper repositoryWrapper = null,
            bool errorInsertingValue = false,
            bool throwsException = false)
            => new ServiceController(repositoryWrapper ?? GetRepositoryWrapper(errorInsertingValue, throwsException), GetLogger());

        [Test]
        public async Task ServiceController_GetAsync_ReturnsACollectionWithOneEntity()
        {
            // Arrange
            var sut = GetSut();

            // Act
            var response = await sut.GetAsync(CancellationToken.None);
            var actual = response.ToList();

            // Assert
            Assert.That(actual.Count == 1);
        }

        [Test]
        public async Task ServiceController_GetAsync_LogEventId6666WhenUnexptectedErrorThrown()
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
        public async Task ServiceController_GetAsync_CreateLogWhenUnexptectedErrorThrown()
        {
            // Arrange
            var sut = GetSut(throwsException: true);

            // Act
            _ = await sut.GetAsync(CancellationToken.None);

            // Assert
            Assert.That(Logs.Count == 1);
        }

        [Test]
        public async Task ServiceController_GetAsync_LogExceptionWhenUnexpectedErrorThrown()
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
        public async Task ServiceController_GetAsync_LogLevelIsErrorWhenUnexpectedErrorThrown()
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
    }
}

