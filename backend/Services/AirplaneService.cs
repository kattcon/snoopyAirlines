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
            _airplaneRepository = airplaneRepository;
        }


        public Task<Airplane> CreateAirplaneAsync(Airplane airplane, CancellationToken cancellationToken)
        {

            return _airplaneRepository.CreateAirplaneAsync(airplane, cancellationToken);

        }
    }

}