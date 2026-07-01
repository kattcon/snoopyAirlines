using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SnoopyAirlines.Controllers;
using SnoopyAirlines.Domain.User;
using SnoopyAirlines.Repositories;
using SnoopyAirlines.Services;
using SnoopyAirlines.Util.Email;
using System.Security.Claims;
using Xunit;

namespace backend.Tests.Controllers
{
    public class AdminUserDeleteControllerTests
    {
        private readonly Mock<IUserRepository> _userRepository = new();
        private readonly Mock<IEmailSender> _emailSender = new();

        [Fact]
        public async Task TestDeleteWhenActorClaimMissingReturnsUnauthorized()
        {
            // Arrange
            var controller = CreateControllerWithActorClaim(actorUserId: null);

            // Act
            var result = await controller.Delete(10, CancellationToken.None);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
            _userRepository.Verify(
                r => r.DeleteUserAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task TestDeleteWhenDeleteSucceedsReturnsNoContent()
        {
            // Arrange
            var controller = CreateControllerWithActorClaim(actorUserId: 7);

            _userRepository
                .Setup(r => r.DeleteUserAsync(7, 11, It.IsAny<CancellationToken>()))
                .ReturnsAsync(DeleteUserResult.Success);

            // Act
            var result = await controller.Delete(11, CancellationToken.None);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _userRepository.Verify(
                r => r.DeleteUserAsync(7, 11, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task TestDeleteWhenSelfDeleteAttemptReturnsConflict()
        {
            // Arrange
            var controller = CreateControllerWithActorClaim(actorUserId: 9);

            _userRepository
                .Setup(r => r.DeleteUserAsync(9, 9, It.IsAny<CancellationToken>()))
                .ReturnsAsync(DeleteUserResult.SelfDelete);

            // Act
            var result = await controller.Delete(9, CancellationToken.None);

            // Assert
            Assert.IsType<ConflictObjectResult>(result);
        }

        [Fact]
        public async Task TestDeleteWhenInitialAdminDeleteAttemptReturnsConflict()
        {
            // Arrange
            var controller = CreateControllerWithActorClaim(actorUserId: 7);

            _userRepository
                .Setup(r => r.DeleteUserAsync(7, 1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(DeleteUserResult.ProtectedInitialAdmin);

            // Act
            var result = await controller.Delete(1, CancellationToken.None);

            // Assert
            Assert.IsType<ConflictObjectResult>(result);
        }

        private AdminUserController CreateControllerWithActorClaim(int? actorUserId)
        {
            var service = new UserService(_userRepository.Object, _emailSender.Object);
            var controller = new AdminUserController(service);

            var claims = new List<Claim>();
            if (actorUserId.HasValue)
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, actorUserId.Value.ToString()));
            }

            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = principal
                }
            };

            return controller;
        }
    }
}
