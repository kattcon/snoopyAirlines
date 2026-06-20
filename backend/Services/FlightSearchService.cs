using SnoopyAirlines.Domain.View;
using SnoopyAirlines.Repositories;
using System.ComponentModel.DataAnnotations;

namespace SnoopyAirlines.Services
{
    public class FlightSearchService : IFlightSearchService
    {
        private readonly IFlightSearchRepository _flightSearchRepository;

        public FlightSearchService(IFlightSearchRepository flightSearchRepository)
        {
            _flightSearchRepository = flightSearchRepository;
        }

        public async Task<IReadOnlyCollection<FlightReportView>> GetFlightReportByConfirmationAsync(
            string confirmationNumber,
            string lastNames,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(confirmationNumber))
            {
                throw new ValidationException("El número de reservación es obligatorio.", nameof(confirmationNumber));
            }

            if (string.IsNullOrWhiteSpace(lastNames))
            {
                throw new ValidationException("Los apellidos son obligatorios.", nameof(lastNames));
            }

            var flightReport = await _flightSearchRepository.GetFlightReportByConfirmationAsync(
                confirmationNumber,
                lastNames,
                cancellationToken);

            if (flightReport == null || flightReport.Count == 0)
            {
                throw new InvalidOperationException("No se encontró ningún reporte de vuelo para el número de reservación y apellidos proporcionados.");
            }

            return flightReport;
        }
    }
}