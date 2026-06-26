using Moq;
using SnoopyAirlines.Domain.Intake;
using SnoopyAirlines.Domain.User;
using SnoopyAirlines.Domain.View;
using SnoopyAirlines.Repositories;
using SnoopyAirlines.Services;
using SnoopyAirlines.Util.Email;
using Xunit;

namespace backend.Tests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _userRepository = new();
        private readonly Mock<IEmailSender> _emailSender = new();

        [Fact]
        public async Task GetByIdAsync_WhenUserExists_ReturnsUserView()
        {
            // Arrange
            var user = CreateUser();

            _userRepository
                .Setup(r => r.GetByIdAsync(user.Id!.Value, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            var service = CreateService();

            // Act
            var result = await service.GetByIdAsync(user.Id!.Value, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.Id, result.Id);
            Assert.Equal(user.Email, result.Email);
            Assert.Equal(user.FirstName, result.FirstName);
        }

        [Fact]
        public async Task UpdateUserAsync_WhenUserNotFound_ReturnsNull()
        {
            // Arrange
            _userRepository
                .Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            var service = CreateService();
            var intake = CreateUserUpdateIntake();

            // Act
            var result = await service.UpdateUserAsync(99, intake, CancellationToken.None);

            // Assert
            Assert.Null(result);

            _userRepository.Verify(
                r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<UserUpdateIntake>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task ChangePasswordAsync_WhenCurrentPasswordIsWrong_ReturnsFalse()
        {
            // Arrange
            var passwordSalt = BCrypt.Net.BCrypt.GenerateSalt();
            var passwordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword1!", passwordSalt);
            var user = CreateUser(passwordHash: passwordHash, passwordSalt: passwordSalt);

            _userRepository
                .Setup(r => r.GetByIdAsync(user.Id!.Value, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);

            var service = CreateService();

            // Act
            var result = await service.ChangePasswordAsync(
                user.Id!.Value,
                "WrongPassword1!",
                "NewPassword1!",
                CancellationToken.None);

            // Assert
            Assert.False(result);

            _userRepository.Verify(
                r => r.UpdatePasswordAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        private UserService CreateService()
        {
            return new UserService(_userRepository.Object, _emailSender.Object);
        }

        private static User CreateUser(
            int id = 1,
            string email = "user@snoopyairlines.com",
            string firstName = "Jane",
            string lastNameOne = "Doe",
            string passwordHash = "",
            string passwordSalt = "")
        {
            return new User
            {
                Id = id,
                IdentificationNumber = "123456789",
                Email = email,
                FirstName = firstName,
                LastNameOne = lastNameOne,
                LastNameTwo = null,
                Type = UserRole.Operator,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt
            };
        }

        private static UserUpdateIntake CreateUserUpdateIntake(
            string firstName = "Jane",
            string lastNameOne = "Doe",
            string? lastNameTwo = null)
        {
            return new UserUpdateIntake
            {
                FirstName = firstName,
                LastNameOne = lastNameOne,
                LastNameTwo = lastNameTwo
            };
        }
    }
}
