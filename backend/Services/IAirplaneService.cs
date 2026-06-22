using SnoopyAirlines.domain;
using SnoopyAirlines.Domain.Intake;

namespace SnoopyAirlines.Services
{
    public interface IAirplaneService
    {
        Task<Airplane> CreateAirplaneAsync(Airplane airplane, CancellationToken cancellationToken);
        Task<IReadOnlyCollection<Airplane>> GetAirplanesAsync(CancellationToken cancellationToken);
        Task<bool> ExistsByModelAsync(string model, CancellationToken cancellationToken);
        Task<Airplane?> GetAirplaneByModelAync(string model, CancellationToken cancellationToken);
        Task<Airplane?> GetAirplaneByIdAsync(int airplaneId, CancellationToken cancellationToken);
        Task UpdateAirplaneCapacitiesAsync(int airplaneId, AirplaneUpdateIntake intake, CancellationToken cancellationToken);
        Task DeleteAirplaneAsync(int airplaneId, CancellationToken cancellationToken);
    }
}
