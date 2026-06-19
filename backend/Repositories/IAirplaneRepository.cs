using SnoopyAirlines.domain;
using SnoopyAirlines.Domain.Intake;

namespace SnoopyAirlines.Repositories
{
    public interface IAirplaneRepository
    {
        Task<Airplane> CreateAirplaneAsync(Airplane airplane, CancellationToken cancellationToken);
        Task<IReadOnlyCollection<Airplane>> GetAirplanesAsync(CancellationToken cancellationToken);
        Task<bool> ExistsByModelAsync(string model, CancellationToken cancellationToken);
        Task<Airplane?> GetAirplaneByModelAsync(string model, CancellationToken cancellationToken);
        Task<Airplane?> GetAirplaneByIdAsync(int airplaneId, CancellationToken cancellationToken);
        Task UpdateAirplaneCapacitiesAsync(int airplaneId, AirplaneUpdateIntake intake, CancellationToken cancellationToken);
        Task<bool> AirplaneHasPurchasesAsync(int airplaneId, CancellationToken cancellationToken);
        Task SoftDeleteAirplaneAsync(int airplaneId, CancellationToken cancellationToken);
        Task HardDeleteAirplaneAsync(int airplaneId, CancellationToken cancellationToken);
    }
}
