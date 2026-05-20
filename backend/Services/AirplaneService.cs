using SnoopyAirlines.domain;
using SnoopyAirlines.Domain.View;
using SnoopyAirlines.Repositories;

namespace SnoopyAirlines.Services
{
    public class AirplaneService
    {
        private readonly AirplaneRepository _airplaneRepository;

        public AirplaneService(AirplaneRepository airplaneRepository)
        {
            _airplaneRepository = airplaneRepository
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
    }

}