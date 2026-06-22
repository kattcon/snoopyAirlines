using SnoopyAirlines.domain;
using SnoopyAirlines.Domain.Intake;
using SnoopyAirlines.Domain.View;
using SnoopyAirlines.Repositories;

namespace SnoopyAirlines.Services
{
    public class AirplaneService : IAirplaneService
    {
        private readonly IAirplaneRepository _airplaneRepository;

        public AirplaneService(IAirplaneRepository airplaneRepository)
        {
            _airplaneRepository = airplaneRepository;
        }


        public Task<Airplane> CreateAirplaneAsync(Airplane airplane, CancellationToken cancellationToken)
        {
            return _airplaneRepository.CreateAirplaneAsync(airplane, cancellationToken);
        }

        public Task<IReadOnlyCollection<Airplane>> GetAirplanesAsync(CancellationToken cancellationToken)
        {
            return _airplaneRepository.GetAirplanesAsync(cancellationToken);
        }

        public Task<bool> ExistsByModelAsync(string model, CancellationToken cancellationToken)
        {
            return _airplaneRepository.ExistsByModelAsync(model, cancellationToken);
        }

        public Task<Airplane?> GetAirplaneByModelAync(string model, CancellationToken cancellationToken)
        {
            return _airplaneRepository.GetAirplaneByModelAsync(model, cancellationToken);
        }

        public Task<Airplane?> GetAirplaneByIdAsync(int airplaneId, CancellationToken cancellationToken)
        {
            return _airplaneRepository.GetAirplaneByIdAsync(airplaneId, cancellationToken);
        }

        public Task UpdateAirplaneCapacitiesAsync(int airplaneId, AirplaneUpdateIntake intake, CancellationToken cancellationToken)
        {
            return _airplaneRepository.UpdateAirplaneCapacitiesAsync(airplaneId, intake, cancellationToken);
        }

        public async Task DeleteAirplaneAsync(int airplaneId, CancellationToken cancellationToken)
        {
            var airplane = await _airplaneRepository.GetAirplaneByIdAsync(airplaneId, cancellationToken)
                ?? throw new KeyNotFoundException($"No se encontró la aeronave con id {airplaneId}.");

            var hasPurchases = await _airplaneRepository.AirplaneHasPurchasesAsync(airplaneId, cancellationToken);

            if (hasPurchases)
                await _airplaneRepository.SoftDeleteAirplaneAsync(airplaneId, cancellationToken);
            else
                await _airplaneRepository.HardDeleteAirplaneAsync(airplaneId, cancellationToken);
        }
    }

}