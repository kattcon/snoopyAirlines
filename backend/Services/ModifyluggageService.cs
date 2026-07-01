using Microsoft.AspNetCore.Mvc;
using snoopy_airlines_backend.Domain.Intake;
using snoopy_airlines_backend.Domain.View;
using snoopy_airlines_backend.Repositories;
using SnoopyAirlines.Domain.View;
using SnoopyAirlines.Repositories;
using System;

namespace snoopy_airlines_backend.Services
{
    public class ModifyLuggageService: IModifyLuggageService
    {
        private readonly IPassengerLuggageRepository _PassengerLuggageRepository;
        private readonly IFlightLuggageRepository _flightLuggageRepository;
        private readonly IModifyLuggageRepository _modifyLuggageRepository;

        public ModifyLuggageService(IPassengerLuggageRepository passengerLuggageRepository, IFlightLuggageRepository flightLuggageRepository,
            IModifyLuggageRepository modifyLuggageRepository)
        {
            _PassengerLuggageRepository = passengerLuggageRepository;
            _flightLuggageRepository = flightLuggageRepository;
            _modifyLuggageRepository = modifyLuggageRepository;
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

        public async Task<IReadOnlyCollection<FlightLuggageView>> GetFlightLuggageInfoByConfirmationAsync(
            string confirmationNumber,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(confirmationNumber))
            {
                throw new ArgumentException("El número de reservación es obligatorio.", nameof(confirmationNumber));
            }

            return await _flightLuggageRepository.GetFlightLuggageInfoByConfirmationAsync(
                confirmationNumber,
                cancellationToken
            );
        }

        public async Task<ModifyLuggageInfo> GetLuggageInfoAsync(
            string confirmationNumber,
            CancellationToken cancellationToken)

            {
            var passengersTask = GetPassengersByConfirmationAsync(confirmationNumber, cancellationToken);
            var flightLuggageTask = GetFlightLuggageInfoByConfirmationAsync(confirmationNumber, cancellationToken);

            await Task.WhenAll(passengersTask, flightLuggageTask);

            return new ModifyLuggageInfo
            {
                Passengers = passengersTask.Result,
                LuggageInfo = flightLuggageTask.Result
            };
        }

        public async Task UpdateLuggageAsync(
            ModifyLuggageRequest modifyLuggageRequest,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(modifyLuggageRequest.ConfirmationNumber))
                throw new ArgumentException("El numero de confirmacion es obligatorio.");

            if (modifyLuggageRequest.Passengers == null || modifyLuggageRequest.Passengers.Count == 0)
                throw new ArgumentException("Debe incluir al menor un pasajero.");

            try
            {
                await _modifyLuggageRepository.UpdateLuggageAsync(modifyLuggageRequest, cancellationToken);

            } catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 50001)
            {
                throw new InvalidOperationException("No se encontro ninguna reservacion con ese código");
            } catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 50002)
            {
                throw new InvalidOperationException("Uno o mas pasajeros no pertenecen a esta reservación");
            } catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 50003)
            {
                throw new ArgumentException(ex.Message);
            }
        }
    }
}