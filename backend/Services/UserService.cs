using SnoopyAirlines.Domain.User;
using SnoopyAirlines.Domain.View;
using SnoopyAirlines.Repositories;

namespace SnoopyAirlines.Services
{
    public class UserService
    {
        private readonly UserRepository _userRepository;

        public UserService(UserRepository userRepository)
        {
            _userRepository = userRepository;
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
    }
}
