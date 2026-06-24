using snoopy_airlines_backend.Domain.View;
using snoopy_airlines_backend.Repositories;
using SnoopyAirlines.Domain.View;
using SnoopyAirlines.Repositories;
using System;

namespace snoopy_airlines_backend.Services
{
    public class ModifyluggageService: IModifyluggageService
    {
        private readonly IPassengerLuggageRepository _PassengerLuggageRepository;
        private readonly IFlightLuggageRepository _flightLuggageRepository;

        public ModifyluggageService(IPassengerLuggageRepository passengerLuggageRepository, IFlightLuggageRepository flightLuggageRepository)
        {
            _PassengerLuggageRepository = passengerLuggageRepository;
            _flightLuggageRepository = flightLuggageRepository;
        }

        public async Task<IReadOnlyCollection<PassengerView>> GetPassengersByConfirmationAsync(
            string confirmationNumber,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(confirmationNumber))
            {
                throw new ArgumentException("El número de reservación es obligatorio.", nameof(confirmationNumber));
            }

            var passengers = await _PassengerLuggageRepository.GetPassengersByConfirmationAsync(
                confirmationNumber,
                cancellationToken
            );

            if (passengers == null || passengers.Count == 0)
            {
                throw new InvalidOperationException("No se encontró ningún pasajero con el número de reservación proporcionados.");
            }
            return passengers;
        }

        public async Task<FlightLuggageView> GetFlightLuggageInfoByConfirmationAsync(
            string confirmationNumber,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(confirmationNumber))
            {
                throw new ArgumentException("El número de reservación es obligatorio.", nameof(confirmationNumber));
            }

            var flightInfo = await _flightLuggageRepository.GetFlightLuggageInfoByConfirmationAsync(
                confirmationNumber,
                cancellationToken
            );

            return flightInfo == null
                ? throw new InvalidOperationException("No se encontró ningún pasajero con el número de reservación proporcionados.")
                : flightInfo;
        }

        public async Task<ModifyLuggageInfo> GetLuggageInfoAsync(
            string confirmationNumber,
            CancellationToken cancellationToken)

            {
            var passengers = await GetPassengersByConfirmationAsync(confirmationNumber, cancellationToken);
            var flightLuggage = await GetFlightLuggageInfoByConfirmationAsync(confirmationNumber, cancellationToken);

            return new ModifyLuggageInfo
            {
                Passengers = passengers,
                LuggageInfo = flightLuggage
            };
        }
    }
}