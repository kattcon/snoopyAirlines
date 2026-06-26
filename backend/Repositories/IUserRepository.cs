using SnoopyAirlines.Domain.Intake;
using SnoopyAirlines.Domain.User;
using SnoopyAirlines.Domain.View;

namespace SnoopyAirlines.Repositories
{
    public interface IUserRepository
    {
        Task<IReadOnlyCollection<UserView>> GetAllAsync(CancellationToken cancellationToken);
        Task<PendingUser> SaveAsync(PendingUser pendingUser, CancellationToken cancellationToken);
        Task<UserView> SaveAsync(User user, CancellationToken cancellationToken);
        Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken);
        Task<bool> ExistsPendingByEmailAsync(string email, CancellationToken cancellationToken);
        Task<PendingUser?> GetPendingByRegistrationKeyHashAsync(string registrationKeyHash, CancellationToken cancellationToken);
        Task<UserView> SaveUserAndDeletePendingAsync(User user, CancellationToken cancellationToken);
        Task<User?> GetByCredentialsAsync(string email, string password, CancellationToken cancellationToken);
        Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken);
        Task<UserView> UpdateAsync(int id, UserUpdateIntake intake, CancellationToken cancellationToken);
        Task UpdatePasswordAsync(int id, string passwordHash, string passwordSalt, CancellationToken cancellationToken);
        Task<UserView?> AdminUpdateAsync(int id, AdminUserUpdateIntake intake, CancellationToken cancellationToken);
        Task DeleteUserAsync(int actorUserId, int targetUserId, CancellationToken cancellationToken);
    }
}