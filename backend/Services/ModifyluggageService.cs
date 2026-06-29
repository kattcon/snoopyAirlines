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

        public ModifyluggageService(IPassengerLuggageRepository passengerLuggageRepository)
        {
            _PassengerLuggageRepository = passengerLuggageRepository;
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
    }
}