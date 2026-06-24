using snoopy_airlines_backend.Domain.View;
using SnoopyAirlines.Domain.View;
using SnoopyAirlines.Repositories;
using System;

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
                throw new ArgumentException("El número de reservación es obligatorio.", nameof(confirmationNumber));
            }

            if (string.IsNullOrWhiteSpace(lastNames))
            {
                throw new ArgumentException("Los apellidos son obligatorios.", nameof(lastNames));
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

        public async Task<IReadOnlyCollection<PassengerView>> GetPassengersByConfirmationAsync(
            string confirmationNumber,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(confirmationNumber))
            {
                throw new ArgumentException("El número de reservación es obligatorio.", nameof(confirmationNumber));
            }

            var passengers = await _flightSearchRepository.GetPassengersByConfirmationAsync(
                confirmationNumber,
                cancellationToken
            );

            if (passengers == null || passengers.Count == 0)
            {
                throw new InvalidOperationException("No se encontró ningún pasajero con el número de reservación proporcionados.");
            }

            return passengers;
        }
    }
}