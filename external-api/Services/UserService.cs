using System.Security.Cryptography;
using SnoopyAirlines.External.Domain;
using SnoopyAirlines.External.Repositories;

namespace SnoopyAirlines.External.Services
{
    public class UserService
    {
        public const string AdminRole = "AD";
        public const string UserRole = "US";

        private const int ApiKeyLength = 256;
        private const string ApiKeyAlphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        private readonly UserRepository _userRepository;

        public UserService(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public Task<ApiUser?> GetByApiKey(
            string apiKey,
            CancellationToken cancellationToken = default)
        {
            return _userRepository.GetByApiKey(apiKey, cancellationToken);
        }

        public async Task<ApiUser> Create(
            string name,
            string role,
            CancellationToken cancellationToken = default)
        {
            var user = new ApiUser
            {
                ApiKey = await GenerateUniqueApiKey(cancellationToken),
                Name = name,
                Role = role
            };

            return await _userRepository.Save(user, cancellationToken);
        }

        public Task<ApiUser> Save(
            ApiUser user,
            CancellationToken cancellationToken = default)
        {
            return _userRepository.Save(user, cancellationToken);
        }

        private async Task<string> GenerateUniqueApiKey(CancellationToken cancellationToken)
        {
            for (var attempt = 0; attempt < 5; attempt++)
            {
                var apiKey = GenerateApiKey();
                var existingUser = await _userRepository.GetByApiKey(apiKey, cancellationToken);

                if (existingUser is null)
                {
                    return apiKey;
                }
            }

            throw new InvalidOperationException("Unable to generate a unique API key.");
        }

        private static string GenerateApiKey()
        {
            return RandomNumberGenerator.GetString(ApiKeyAlphabet, ApiKeyLength);
        }
    }
}
