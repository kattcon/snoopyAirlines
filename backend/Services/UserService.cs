using SnoopyAirlines.Domain.User;
using SnoopyAirlines.Domain.View;
using SnoopyAirlines.Repositories;
using System.Security.Cryptography;
using System.Text;

namespace SnoopyAirlines.Services
{
    public class UserService
    {
        private readonly UserRepository _userRepository;
        private readonly IEmailSender _emailSender;

        public UserService(
            UserRepository userRepository,
            IEmailSender emailSender)
        {
            _userRepository = userRepository;
            _emailSender = emailSender;
        }

        public Task<IReadOnlyCollection<UserView>> GetUsersAsync(CancellationToken cancellationToken)
        {
            return _userRepository.GetAllAsync(cancellationToken);
        }

        public Task<PendingUser> SavePendingUserAsync(
            PendingUser pendingUser,
            CancellationToken cancellationToken)
        {
            return _userRepository.SaveAsync(pendingUser, cancellationToken);
        }

        public Task<UserView> SaveUserAsync(
            User user,
            CancellationToken cancellationToken)
        {
            return _userRepository.SaveAsync(user, cancellationToken);
        }

        public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken)
        {
            return _userRepository.ExistsByEmailAsync(email, cancellationToken);
        }

        public Task<bool> ExistsPendingByEmailAsync(string email, CancellationToken cancellationToken)
        {
            return _userRepository.ExistsPendingByEmailAsync(email, cancellationToken);
        }

        public (string Value, string Hash) GenerateRegistrationKey()
        {
            var registrationKey = GenerateRegistrationKeyValue();

            return (registrationKey, HashRegistrationKey(registrationKey));
        }

        public Task SendRegistrationEmailAsync(
            string email,
            string registrationKey,
            CancellationToken cancellationToken)
        {
            var registrationUrl = $"snoopyairlines.com/register?key={Uri.EscapeDataString(registrationKey)}";
            var body = $"Por favor ingrese al siguiente enlace para finalizar su registro: {registrationUrl}";

            return _emailSender.SendAsync(
                email,
                "Bienvenido a Snoopy Airlines",
                body,
                cancellationToken);
        }

        public async Task<UserView?> RegisterPendingUserAsync(
            string pendingKey,
            string password,
            CancellationToken cancellationToken)
        {
            var pendingKeyHash = HashRegistrationKey(pendingKey);
            var pendingUser = await _userRepository.GetPendingByRegistrationKeyHashAsync(
                pendingKeyHash,
                cancellationToken);

            if (pendingUser is null)
            {
                return null;
            }

            if (await _userRepository.ExistsByEmailAsync(pendingUser.Email, cancellationToken))
            {
                throw new InvalidOperationException("A registered user already exists with this email.");
            }

            var passwordSalt = BCrypt.Net.BCrypt.GenerateSalt();
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password, passwordSalt);

            var user = new User
            {
                IdentificationNumber = pendingUser.IdentificationNumber,
                Email = pendingUser.Email,
                FirstName = pendingUser.FirstName,
                LastNameOne = pendingUser.LastNameOne,
                LastNameTwo = pendingUser.LastNameTwo,
                Type = pendingUser.Type,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt
            };

            return await _userRepository.SaveUserAndDeletePendingAsync(
                user,
                cancellationToken);
        }

        public async Task<User?> LoginAsync(string email, string password, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByCredentialsAsync(email, password, cancellationToken);

            if (user is null)
            {
                return null;
            }

            bool passwordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

            if (!passwordValid)
            {
                return null;
            }

            return user;
        }

        private static string GenerateRegistrationKeyValue()
        {
            var bytes = RandomNumberGenerator.GetBytes(32);

            return Convert
                .ToBase64String(bytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        private static string HashRegistrationKey(string registrationKey)
        {
            var keyBytes = Encoding.UTF8.GetBytes(registrationKey);
            var hashBytes = SHA256.HashData(keyBytes);

            return Convert.ToHexString(hashBytes).ToLowerInvariant();
        }
    }
}
