using com.b_velop.Slipways.API.Controllers;
using com.b_velop.Slipways.Data.Contracts;
using com.b_velop.Slipways.Data.Dtos;
using com.b_velop.Slipways.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Slipways.API.Tests
{
    public class ExtraControllerTests
    {
        IRepositoryWrapper rep;
        ILogger<ExtraController> logger;
        ICollection<string> Extras;

        [SetUp]
        public void SetUp()
        {
            Extras = new List<string>();

            var repositoryWrapper = new Mock<IRepositoryWrapper>();
            repositoryWrapper
                .Setup(_ => _.Extra.InsertAsync(It.IsAny<Extra>(), It.IsAny<CancellationToken>(), true))
                .Callback((Extra extra, CancellationToken token, bool save) =>
                {
                    Extras.Add(extra.Name);
                });

            rep = repositoryWrapper.Object;

            logger = new Mock<ILogger<ExtraController>>().Object;
        }

        [Test]
        public async Task ExtraController_PostAsync_ReturnsBadRequestWhenDtoIsNull()
        {
            // Arrange
            var sut = new ExtraController(rep, logger);

            // Act
            var actual = await sut.PostAsync(null, CancellationToken.None);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(actual);
        }


        [Test]
        public async Task ExtraController_PostAsync_ReturnsBadRequestWhenDtosNameIsStringEmpty()
        {
            // Arrange
            var sut = new ExtraController(rep, logger);
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
            var sut = new ExtraController(rep, logger);
            var dto = new ExtraDto
            {
                Name = "Sauna"
            };

            // Act
            _ = await sut.PostAsync(dto, CancellationToken.None);

            // Assert
            Assert.That(Extras.Contains("Sauna"));
        }
    }
}
