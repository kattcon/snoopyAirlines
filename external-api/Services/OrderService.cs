using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Net.Mail;
using System.Text.Json;
using System.Text.RegularExpressions;
using SnoopyAirlines.External.Domain;
using SnoopyAirlines.External.Domain.Intake;
using SnoopyAirlines.External.Domain.View;

namespace SnoopyAirlines.External.Services
{
    public class OrderService
    {
        private static readonly Regex Alpha3Regex = new(
            "^[A-Za-z]{3}$",
            RegexOptions.Compiled);

        private static readonly Regex DigitsRegex = new(
            "^\\d+$",
            RegexOptions.Compiled);

        private static readonly Regex CardExpirationRegex = new(
            "^\\d{4}-(0[1-9]|1[0-2])$",
            RegexOptions.Compiled);

        private static readonly Lazy<IReadOnlySet<string>> IsoAlpha3Countries = new(
            CreateIsoAlpha3Countries);

        private readonly HttpClient _httpClient;
        private readonly UserService _userService;

        public OrderService(
            HttpClient httpClient,
            UserService userService)
        {
            _httpClient = httpClient;
            _userService = userService;
        }

        public async Task<ExternalOrderResponse> CreateOrderAsync(
            ExternalOrderIntake? order,
            CancellationToken cancellationToken = default)
        {
            var validatedOrder = ValidateOrder(order);

            if (await _userService.GetByApiKey(validatedOrder.ApiKey, cancellationToken) is null)
            {
                throw OrderException.InvalidField("apiKey");
            }

            try
            {
                var flight = await GetFlightAsync(
                    validatedOrder.FlightGuid,
                    cancellationToken);

                if (flight is null)
                {
                    throw OrderException.InvalidField("flightGUID");
                }

                var purchaseOrder = await CreatePurchaseOrderAsync(
                    validatedOrder,
                    cancellationToken);
                var booking = await BookAsync(
                    purchaseOrder.Id,
                    validatedOrder,
                    cancellationToken);

                return CreateResponse(validatedOrder, flight, booking);
            }
            catch (BackendRequestException exception)
                when (exception.StatusCode == HttpStatusCode.BadRequest)
            {
                throw ToOrderException(exception);
            }
            catch (BackendRequestException exception)
            {
                throw OrderException.BadRequest(
                    "Backend request failed",
                    exception);
            }
        }

        private async Task<Flight?> GetFlightAsync(
            Guid flightGuid,
            CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.GetAsync(
                $"Flight/{Uri.EscapeDataString(flightGuid.ToString("D"))}",
                cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                throw await BackendRequestException.CreateAsync(
                    response,
                    "get flight",
                    cancellationToken);
            }

            var flight = await response.Content.ReadFromJsonAsync<BackendFlightResponse>(
                cancellationToken: cancellationToken);

            return flight?.ToFlight();
        }

        private async Task<BackendPurchaseOrderResponse> CreatePurchaseOrderAsync(
            ValidatedOrder order,
            CancellationToken cancellationToken = default)
        {
            var request = new BackendPurchaseOrderRequest
            {
                Routes =
                [
                    new BackendPurchaseOrderRouteRequest
                    {
                        SequenceNumber = 1,
                        FlightGuid = order.FlightGuid
                    }
                ],
                SeatClass = order.FirstClass ? "firstClass" : "economy",
                Passengers = order.Passengers
                    .Select(ToBackendPassenger)
                    .ToList()
            };

            using var response = await _httpClient.PostAsJsonAsync(
                "PurchaseOrder",
                request,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw await BackendRequestException.CreateAsync(
                    response,
                    "create purchase order",
                    cancellationToken);
            }

            return await response.Content.ReadFromJsonAsync<BackendPurchaseOrderResponse>(
                    cancellationToken: cancellationToken)
                ?? throw new InvalidOperationException("Backend purchase order response was empty.");
        }

        private async Task<BackendBookingResponse> BookAsync(
            int purchaseOrderId,
            ValidatedOrder order,
            CancellationToken cancellationToken = default)
        {
            var request = new BackendBookingRequest
            {
                PurchaseOrderId = purchaseOrderId,
                Email = order.Buyer.Email!,
                CardBrand = DetectCardBrand(order.Payment.CardNumber!),
                CardLastFour = order.Payment.CardNumber![^4..],
                CardHolderName = order.Payment.CardHolderName!
            };

            using var response = await _httpClient.PostAsJsonAsync(
                "Booking",
                request,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                throw await BackendRequestException.CreateAsync(
                    response,
                    "book purchase order",
                    cancellationToken);
            }

            return await response.Content.ReadFromJsonAsync<BackendBookingResponse>(
                    cancellationToken: cancellationToken)
                ?? throw new InvalidOperationException("Backend booking response was empty.");
        }

        private static ExternalOrderResponse CreateResponse(
            ValidatedOrder order,
            Flight flight,
            BackendBookingResponse booking)
        {
            return new ExternalOrderResponse
            {
                ReservationNumber = booking.ConfirmationCode,
                FirstClass = order.FirstClass,
                Flight = flight,
                Breakup = CreateBreakup(order, flight, booking.TotalAmount),
                Passengers = order.Passengers,
                Buyer = new ExternalOrderBuyerResponse
                {
                    FirstName = order.Buyer.FirstName!,
                    LastName = order.Buyer.LastName!,
                    LastName2 = order.Buyer.LastName2,
                    PhoneNumber = order.Buyer.PhoneNumber!,
                    Email = order.Buyer.Email!
                }
            };
        }

        private static ExternalOrderBreakup CreateBreakup(
            ValidatedOrder order,
            Flight flight,
            decimal totalAmount)
        {
            var passengerCount = order.Passengers.Count;
            var tickets = (order.FirstClass
                    ? flight.FirstClassPrice
                    : flight.TouristPrice)
                * passengerCount;
            var luggage = flight.CarryOnPrice * order.Passengers.Count(passenger => passenger.CarryOn!.Value)
                + flight.CheckedPrice * order.Passengers.Sum(passenger => passenger.Checked!.Value);
            var total = RoundMoney(totalAmount);

            return new ExternalOrderBreakup
            {
                Luggage = RoundMoney(luggage),
                Tickets = RoundMoney(tickets),
                Taxes = RoundMoney(total - tickets - luggage),
                Total = total
            };
        }

        private static decimal RoundMoney(decimal value)
        {
            return decimal.Round(value, 2, MidpointRounding.AwayFromZero);
        }

        private static BackendPassengerRequest ToBackendPassenger(
            ExternalOrderPassenger passenger)
        {
            var birthDate = ParseDate(passenger.BirthDate!);

            return new BackendPassengerRequest
            {
                Gender = passenger.Gender!,
                FirstName = passenger.FirstName!,
                LastName = string.IsNullOrWhiteSpace(passenger.LastName2)
                    ? passenger.LastName!
                    : $"{passenger.LastName} {passenger.LastName2}",
                BirthDay = birthDate.Day.ToString(CultureInfo.InvariantCulture),
                BirthMonth = birthDate.Month.ToString(CultureInfo.InvariantCulture),
                BirthYear = birthDate.Year.ToString(CultureInfo.InvariantCulture),
                Nationality = passenger.PassportCountry!,
                CarryOnLuggage = passenger.CarryOn!.Value ? "1" : "0",
                CheckedLuggage = passenger.Checked!.Value.ToString(CultureInfo.InvariantCulture)
            };
        }

        private static OrderException ToOrderException(
            BackendRequestException exception)
        {
            var backendMessage = exception.BackendMessage ?? string.Empty;

            if (Contains(backendMessage, "enough remaining seats"))
            {
                return OrderException.BadRequest("Not enough seats", exception);
            }

            if ((Contains(backendMessage, "luggage") || Contains(backendMessage, "baggage"))
                && (Contains(backendMessage, "weight") || Contains(backendMessage, "overweight")))
            {
                return OrderException.BadRequest("Luggage overweight", exception);
            }

            return OrderException.InvalidField(
                MapBackendField(backendMessage),
                exception);
        }

        private static string MapBackendField(string backendMessage)
        {
            if (Contains(backendMessage, "email"))
            {
                return "buyer.email";
            }

            if (Contains(backendMessage, "card_last_four"))
            {
                return "payment.cardNumber";
            }

            if (Contains(backendMessage, "card_holder_name"))
            {
                return "payment.cardHolderName";
            }

            return "flightGUID";
        }

        private static bool Contains(string value, string expected)
        {
            return value.Contains(expected, StringComparison.OrdinalIgnoreCase);
        }

        private static ValidatedOrder ValidateOrder(ExternalOrderIntake? order)
        {
            if (order is null)
            {
                throw OrderException.InvalidField("payload");
            }

            var apiKey = RequiredString(order.ApiKey, "apiKey");
            var flightGuid = RequiredGuid(order.FlightGuid, "flightGUID");
            var firstClass = RequiredBoolean(order.FirstClass, "firstClass");
            var passengers = RequiredList(order.Passengers, "passengers");

            for (var index = 0; index < passengers.Count; index++)
            {
                ValidatePassenger(passengers[index], index);
            }

            if (HasDuplicatedPassenger(passengers))
            {
                throw OrderException.BadRequest("Dupicated passenger");
            }

            var buyer = RequiredObject(order.Buyer, "buyer");
            ValidateBuyer(buyer);

            var payment = RequiredObject(order.Payment, "payment");
            ValidatePayment(payment);

            return new ValidatedOrder(
                apiKey,
                flightGuid,
                firstClass,
                passengers,
                buyer,
                payment);
        }

        private static void ValidatePassenger(
            ExternalOrderPassenger passenger,
            int index)
        {
            var prefix = $"passengers[{index}]";

            _ = RequiredBoolean(passenger.CarryOn, $"{prefix}.carryOn");
            _ = RequiredNonNegativeInt(passenger.Checked, $"{prefix}.checked");
            passenger.Passport = RequiredString(
                passenger.Passport,
                $"{prefix}.passport",
                exactLength: 9);
            passenger.PassportExpirationDate = RequiredDateText(
                passenger.PassportExpirationDate,
                $"{prefix}.passportExpirationDate");
            passenger.PassportCountry = RequiredAlpha3(
                passenger.PassportCountry,
                $"{prefix}.passportCountry");
            passenger.FirstName = RequiredString(
                passenger.FirstName,
                $"{prefix}.firstName",
                maxLength: 255);
            passenger.LastName = RequiredString(
                passenger.LastName,
                $"{prefix}.lastName",
                maxLength: 255);
            passenger.LastName2 = NullableString(
                passenger.LastName2,
                $"{prefix}.lastName2",
                maxLength: 255);
            passenger.Gender = RequiredString(passenger.Gender, $"{prefix}.gender");
            passenger.BirthDate = RequiredDateText(passenger.BirthDate, $"{prefix}.birthDate");

            if (passenger.Gender is not "M" and not "F" and not "O")
            {
                throw OrderException.InvalidField($"{prefix}.gender");
            }
        }

        private static void ValidateBuyer(ExternalOrderBuyer buyer)
        {
            buyer.Nationality = RequiredAlpha3(buyer.Nationality, "buyer.nationality");
            buyer.FirstName = RequiredString(buyer.FirstName, "buyer.firstName", maxLength: 255);
            buyer.LastName = RequiredString(buyer.LastName, "buyer.lastName", maxLength: 255);
            buyer.LastName2 = NullableString(buyer.LastName2, "buyer.lastName2", maxLength: 255);
            buyer.PhoneNumber = RequiredString(buyer.PhoneNumber, "buyer.phoneNumber", maxLength: 20);
            buyer.Email = RequiredString(buyer.Email, "buyer.email", maxLength: 255);

            if (!IsValidEmail(buyer.Email))
            {
                throw OrderException.InvalidField("buyer.email");
            }
        }

        private static void ValidatePayment(ExternalOrderPayment payment)
        {
            payment.CardNumber = RequiredString(
                payment.CardNumber,
                "payment.cardNumber",
                exactLength: 16);
            payment.CardExpiration = RequiredString(
                payment.CardExpiration,
                "payment.cardExpiration");
            payment.Cvv = RequiredString(payment.Cvv, "payment.cvv", minLength: 4);
            payment.CardHolderName = RequiredString(
                payment.CardHolderName,
                "payment.cardHolderName",
                maxLength: 256);

            if (!DigitsRegex.IsMatch(payment.CardNumber))
            {
                throw OrderException.InvalidField("payment.cardNumber");
            }

            if (!CardExpirationRegex.IsMatch(payment.CardExpiration))
            {
                throw OrderException.InvalidField("payment.cardExpiration");
            }

            if (!DigitsRegex.IsMatch(payment.Cvv))
            {
                throw OrderException.InvalidField("payment.cvv");
            }
        }

        private static T RequiredObject<T>(T? value, string fieldName)
            where T : class
        {
            return value ?? throw OrderException.MissingField(fieldName);
        }

        private static List<ExternalOrderPassenger> RequiredList(
            List<ExternalOrderPassenger>? value,
            string fieldName)
        {
            if (value is null)
            {
                throw OrderException.MissingField(fieldName);
            }

            if (value.Count == 0)
            {
                throw OrderException.InvalidField(fieldName);
            }

            return value;
        }

        private static bool RequiredBoolean(bool? value, string fieldName)
        {
            return value ?? throw OrderException.MissingField(fieldName);
        }

        private static int RequiredNonNegativeInt(int? value, string fieldName)
        {
            if (!value.HasValue)
            {
                throw OrderException.MissingField(fieldName);
            }

            if (value.Value < 0)
            {
                throw OrderException.InvalidField(fieldName);
            }

            return value.Value;
        }

        private static Guid RequiredGuid(string? value, string fieldName)
        {
            var text = RequiredString(value, fieldName);

            if (!Guid.TryParse(text, out var guid))
            {
                throw OrderException.InvalidField(fieldName);
            }

            return guid;
        }

        private static string RequiredAlpha3(string? value, string fieldName)
        {
            var text = RequiredString(value, fieldName).ToUpperInvariant();

            if (!Alpha3Regex.IsMatch(text)
                || !IsoAlpha3Countries.Value.Contains(text))
            {
                throw OrderException.InvalidField(fieldName);
            }

            return text;
        }

        private static string RequiredDateText(string? value, string fieldName)
        {
            var text = RequiredString(value, fieldName);
            _ = ParseDate(text, fieldName);
            return text;
        }

        private static DateOnly ParseDate(string value, string? fieldName = null)
        {
            if (!DateOnly.TryParseExact(
                    value,
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var date))
            {
                throw OrderException.InvalidField(fieldName ?? "birthDate");
            }

            return date;
        }

        private static string RequiredString(
            string? value,
            string fieldName,
            int? exactLength = null,
            int? maxLength = null,
            int? minLength = null)
        {
            if (value is null)
            {
                throw OrderException.MissingField(fieldName);
            }

            var text = value.Trim();
            if (string.IsNullOrWhiteSpace(text))
            {
                throw OrderException.MissingField(fieldName);
            }

            if (exactLength.HasValue && text.Length != exactLength.Value)
            {
                throw OrderException.InvalidField(fieldName);
            }

            if (maxLength.HasValue && text.Length > maxLength.Value)
            {
                throw OrderException.InvalidField(fieldName);
            }

            if (minLength.HasValue && text.Length < minLength.Value)
            {
                throw OrderException.InvalidField(fieldName);
            }

            return text;
        }

        private static string? NullableString(
            string? value,
            string fieldName,
            int? maxLength = null)
        {
            if (value is null)
            {
                return null;
            }

            var text = value.Trim();
            if (maxLength.HasValue && text.Length > maxLength.Value)
            {
                throw OrderException.InvalidField(fieldName);
            }

            return text;
        }

        private static bool HasDuplicatedPassenger(
            IReadOnlyCollection<ExternalOrderPassenger> passengers)
        {
            return passengers
                .GroupBy(passenger => passenger.Passport, StringComparer.OrdinalIgnoreCase)
                .Any(group => group.Count() > 1);
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var address = new MailAddress(email);
                return string.Equals(address.Address, email, StringComparison.OrdinalIgnoreCase);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private static IReadOnlySet<string> CreateIsoAlpha3Countries()
        {
            return CultureInfo
                .GetCultures(CultureTypes.SpecificCultures)
                .Select(culture => new RegionInfo(culture.Name).ThreeLetterISORegionName)
                .Where(code => Alpha3Regex.IsMatch(code))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        private static string DetectCardBrand(string cardNumber)
        {
            if (cardNumber.StartsWith('4'))
            {
                return "Visa";
            }

            if (cardNumber.StartsWith("34") || cardNumber.StartsWith("37"))
            {
                return "American Express";
            }

            var firstTwoDigits = int.Parse(cardNumber[..2], CultureInfo.InvariantCulture);
            var firstFourDigits = int.Parse(cardNumber[..4], CultureInfo.InvariantCulture);

            if ((firstTwoDigits >= 51 && firstTwoDigits <= 55)
                || (firstFourDigits >= 2221 && firstFourDigits <= 2720))
            {
                return "Mastercard";
            }

            return "Card";
        }

        private sealed record ValidatedOrder(
            string ApiKey,
            Guid FlightGuid,
            bool FirstClass,
            List<ExternalOrderPassenger> Passengers,
            ExternalOrderBuyer Buyer,
            ExternalOrderPayment Payment);

        private sealed class BackendPurchaseOrderResponse
        {
            public int Id { get; set; }
        }

        private sealed class BackendBookingResponse
        {
            public Guid Guid { get; set; }
            public int PurchaseOrderId { get; set; }
            public string ConfirmationCode { get; set; } = string.Empty;
            public decimal TotalAmount { get; set; }
        }

        private sealed class BackendFlightResponse
        {
            public string? FlightGuid { get; set; }
            public int? RouteId { get; set; }
            public DateTime DepartureTime { get; set; }
            public DateTime ArrivalTime { get; set; }
            public string? Duration { get; set; }
            public Airport? DepartureAirport { get; set; }
            public Airport? ArrivalAirport { get; set; }
            public decimal TouristPrice { get; set; }
            public decimal FirstClassPrice { get; set; }
            public decimal CarryOnPrice { get; set; }
            public decimal CheckedPrice { get; set; }

            public Flight ToFlight()
            {
                return new Flight
                {
                    FlightGUID = FlightGuid ?? string.Empty,
                    RouteId = RouteId ?? 0,
                    DepartureTime = DepartureTime,
                    ArrivalTime = ArrivalTime,
                    Duration = Duration ?? string.Empty,
                    DepartureAirport = DepartureAirport
                        ?? new Airport { Code = string.Empty, Name = string.Empty, City = string.Empty },
                    ArrivalAirport = ArrivalAirport
                        ?? new Airport { Code = string.Empty, Name = string.Empty, City = string.Empty },
                    TouristPrice = TouristPrice,
                    FirstClassPrice = FirstClassPrice,
                    CarryOnPrice = CarryOnPrice,
                    CheckedPrice = CheckedPrice
                };
            }
        }

        private sealed class BackendPurchaseOrderRequest
        {
            public List<BackendPurchaseOrderRouteRequest> Routes { get; set; } = [];
            public string SeatClass { get; set; } = string.Empty;
            public List<BackendPassengerRequest> Passengers { get; set; } = [];
        }

        private sealed class BackendPurchaseOrderRouteRequest
        {
            public int SequenceNumber { get; set; }
            public Guid FlightGuid { get; set; }
        }

        private sealed class BackendPassengerRequest
        {
            public string Gender { get; set; } = string.Empty;
            public string FirstName { get; set; } = string.Empty;
            public string LastName { get; set; } = string.Empty;
            public string BirthDay { get; set; } = string.Empty;
            public string BirthMonth { get; set; } = string.Empty;
            public string BirthYear { get; set; } = string.Empty;
            public string Nationality { get; set; } = string.Empty;
            public string CarryOnLuggage { get; set; } = string.Empty;
            public string CheckedLuggage { get; set; } = string.Empty;
        }

        private sealed class BackendBookingRequest
        {
            public int PurchaseOrderId { get; set; }
            public string Email { get; set; } = string.Empty;
            public string CardBrand { get; set; } = string.Empty;
            public string CardLastFour { get; set; } = string.Empty;
            public string CardHolderName { get; set; } = string.Empty;
        }

        private sealed class BackendRequestException : Exception
        {
            private BackendRequestException(
                HttpStatusCode statusCode,
                string operation,
                string? backendMessage)
                : base(CreateMessage(statusCode, operation, backendMessage))
            {
                StatusCode = statusCode;
                Operation = operation;
                BackendMessage = backendMessage;
            }

            public HttpStatusCode StatusCode { get; }
            public string Operation { get; }
            public string? BackendMessage { get; }

            public static async Task<BackendRequestException> CreateAsync(
                HttpResponseMessage response,
                string operation,
                CancellationToken cancellationToken)
            {
                return new BackendRequestException(
                    response.StatusCode,
                    operation,
                    await ReadBackendMessageAsync(response, cancellationToken));
            }

            private static string CreateMessage(
                HttpStatusCode statusCode,
                string operation,
                string? backendMessage)
            {
                return string.IsNullOrWhiteSpace(backendMessage)
                    ? $"Backend failed to {operation} with status {(int)statusCode}."
                    : $"Backend failed to {operation} with status {(int)statusCode}: {backendMessage}";
            }

            private static async Task<string?> ReadBackendMessageAsync(
                HttpResponseMessage response,
                CancellationToken cancellationToken)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                if (string.IsNullOrWhiteSpace(content))
                {
                    return null;
                }

                try
                {
                    using var document = JsonDocument.Parse(content);
                    var root = document.RootElement;

                    if (root.ValueKind == JsonValueKind.Object
                        && TryGetStringProperty(root, "message", out var message))
                    {
                        return message;
                    }

                    if (root.ValueKind == JsonValueKind.Object
                        && TryGetStringProperty(root, "Message", out var capitalizedMessage))
                    {
                        return capitalizedMessage;
                    }
                }
                catch (JsonException)
                {
                    return content;
                }

                return content;
            }

            private static bool TryGetStringProperty(
                JsonElement root,
                string propertyName,
                out string? value)
            {
                value = null;

                if (!root.TryGetProperty(propertyName, out var property)
                    || property.ValueKind != JsonValueKind.String)
                {
                    return false;
                }

                value = property.GetString();
                return true;
            }
        }
    }

    public class OrderException : Exception
    {
        private OrderException(
            string message,
            string? fieldName = null,
            Exception? innerException = null)
            : base(message, innerException)
        {
            FieldName = fieldName;
        }

        public string? FieldName { get; }

        public static OrderException MissingField(string fieldName)
        {
            return new OrderException(
                "Missing field",
                fieldName);
        }

        public static OrderException InvalidField(
            string fieldName,
            Exception? innerException = null)
        {
            return new OrderException(
                "Invalid field",
                fieldName,
                innerException);
        }

        public static OrderException BadRequest(
            string message,
            Exception? innerException = null)
        {
            return new OrderException(
                message,
                null,
                innerException);
        }
    }
}
