using SnoopyAirlines.Domain.Intake;
using SnoopyAirlines.Domain.FlightCustomerReport;

namespace SnoopyAirlines.Services
{
    public class FlightSearch : IFlightSearch
    {
        private readonly IFlightSearchRepository _flightSearchRepository;

        public FlightSearch(IFlightSearchRepository flightSearchRepository)
        {
            _flightSearchRepository = flightSearchRepository;
        }

        public async Task<IReadOnlyCollection<FlightReportView>> GetFlightReportByConfirmationAsync(string confirmation_code, string last_Names, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(confirmation_code) || string.IsNullOrEmpty(last_Names))
            {
                throw new ValidationException("El número de reservación y los apellidos son obligatorios.", nameof(confirmation_code));
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