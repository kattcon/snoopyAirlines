using SnoopyAirlines.Domain.View;
using SnoopyAirlines.Repositories;

namespace SnoopyAirlines.Services
{
    public class FlightSearchService : IFlightSearchService
    {
        private readonly IFlightRepository _flightRepository;

        public FlightSearchService(IFlightRepository flightRepository)
        {
            _flightRepository = flightRepository;
        }

        public async Task<FlightSearchResult> GetFlightReportByConfirmationAsync(
            string confirmationNumber,
            string lastNames,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(confirmationNumber))
            {
                throw new ArgumentException("El número de reservación es obligatorio.", nameof(confirmationNumber));
            }

            if (string.IsNullOrWhiteSpace(lastNames))
            {
                throw new ArgumentException("Los apellidos son obligatorios.", nameof(lastNames));
            }

            var flightReport = await _flightRepository.GetFlightReportByConfirmationAsync(
                confirmationNumber,
                lastNames,
                cancellationToken);

            if (flightReport == null || flightReport.Legs.Count == 0)
            {
                throw new InvalidOperationException("No se encontró ningún reporte de vuelo para el número de reservación y apellidos proporcionados.");
            }

            return flightReport;
        }
    }
}
