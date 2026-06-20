using SnoopyAirlines.Domain.Intake;
using SnoopyAirlines.Repositories;
using SnoopyAirlines.Domain;

namespace SnoopyAirlines.Services
{
    public class FlightSearchService : IFlightSearchService
    {
        private readonly IFlightSearchRepository _flightSearchRepository;

        public FlightSearchService(IFlightSearchRepository flightSearchRepository)
        {
            _flightSearchRepository = flightSearchRepository;
        }

        public async Task<IReadOnlyCollection<FlightCustomerReport>> GetFlightReportByConfirmationAsync(string confirmation_code, string last_Names, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(confirmation_code) || string.IsNullOrEmpty(last_Names))
            {
                throw new InvalidOperationException("El número de reservación y los apellidos son obligatorios.");
            }
            
            var flightReport = await _flightSearchRepository.GetFlightReportByConfirmationAsync(confirmation_code, last_Names, cancellationToken);

            if (flightReport == null || flightReport.Count == 0)
            {
                throw new InvalidOperationException("No se encontró ningún reporte de vuelo para el número de reservación y apellidos proporcionados.");
            }

            return flightReport;
        }
    }
}