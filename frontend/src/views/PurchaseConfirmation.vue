<template>
  <div class="purchase-confirmation-page">
    <BookingHeader />

    <button type="button" class="back-button" @click="$router.back()">
      <svg viewBox="0 0 24 24" aria-hidden="true">
        <path d="M19 12H5"></path>
        <path d="m12 19-7-7 7-7"></path>
      </svg>
      <span>Volver</span>
    </button>

    <main class="confirmation-shell">
      <section class="page-intro">
        <h1>Confirma tu reserva</h1>
        <p>Revisa los detalles de tu viaje antes de finalizar la compra</p>
      </section>

      <section v-if="loading" class="status-card contentCard">
        Cargando reserva...
      </section>

      <section v-else-if="error" class="status-card contentCard">
        {{ error }}
      </section>

      <section v-else class="confirmation-layout">
        <div class="confirmation-main">
          <FlightDetailsCard :details="flightDetails" />
          <PassengerListCard :passengers="passengers" />
          <PaymentMethodCard v-model="paymentDetails" :errors="paymentErrors" />
        </div>

        <PurchaseSummaryCard
          :items="summaryItems"
          :total="summaryTotal"
          :disabled="confirmDisabled"
          :loading="bookingSubmitting"
          :success-message="bookingSuccessMessage"
          :error-message="bookingError"
          @confirm="confirmPurchase"
        />
      </section>
    </main>
  </div>
</template>

<script>
import BookingHeader from "../components/purchase-confirmation/BookingHeader.vue";
import FlightDetailsCard from "../components/purchase-confirmation/FlightDetailsCard.vue";
import PassengerListCard from "../components/purchase-confirmation/PassengerListCard.vue";
import PaymentMethodCard from "../components/purchase-confirmation/PaymentMethodCard.vue";
import PurchaseSummaryCard from "../components/purchase-confirmation/PurchaseSummaryCard.vue";
import { confirmBooking, getPurchaseOrder, getRoute } from "../services/purchaseConfirmationService";

export default {
  name: "PurchaseConfirmation",
  components: {
    BookingHeader,
    FlightDetailsCard,
    PassengerListCard,
    PaymentMethodCard,
    PurchaseSummaryCard
  },
  data() {
    return {
      loading: false,
      error: "",
      purchaseOrder: null,
      route: null,
      booking: null,
      bookingSubmitting: false,
      bookingError: "",
      paymentErrors: {},
      paymentDetails: {
        email: sessionStorage.getItem("bookingHolderEmail") || "",
        cardNumber: "",
        cardholderName: sessionStorage.getItem("bookingHolderName") || "",
        expirationDate: "",
        cvv: ""
      }
    };
  },
  computed: {
    purchaseOrderId() {
      return (
        this.$route.params.purchaseOrderId ||
        this.$route.query.purchaseOrderId ||
        this.$route.query.id
      );
    },
    routeId() {
      return this.fieldValue(this.purchaseOrder, "routeId", "RouteId");
    },
    intendedDate() {
      return this.fieldValue(this.purchaseOrder, "intendedDate", "IntendedDate");
    },
    passengers() {
      return this.fieldValue(this.purchaseOrder, "passengers", "Passengers") || [];
    },
    seatClass() {
      return this.fieldValue(this.purchaseOrder, "seatClass", "SeatClass");
    },
    flightDetails() {
      const departureAirport = this.airport("departureAirport", "DepartureAirport");
      const arrivalAirport = this.airport("arrivalAirport", "ArrivalAirport");

      return {
        flightLabel: `Snoopy Airlines - ${this.placeholder("numero_vuelo")}`,
        dateLabel: this.formatDate(this.intendedDate),
        departureTime: this.formatTime(this.fieldValue(this.route, "departureTime", "DepartureTime"), "hora_salida"),
        arrivalTime: this.formatTime(this.fieldValue(this.route, "arrivalTime", "ArrivalTime"), "hora_llegada"),
        durationLabel: this.formatDuration(this.fieldValue(this.route, "durationMinutes", "DurationMinutes")),
        departureCode: this.fieldValue(departureAirport, "code", "Code") || this.placeholder("origen"),
        departureCity: this.fieldValue(departureAirport, "city", "City") || this.placeholder("ciudad_salida"),
        arrivalCode: this.fieldValue(arrivalAirport, "code", "Code") || this.placeholder("destino"),
        arrivalCity: this.fieldValue(arrivalAirport, "city", "City") || this.placeholder("ciudad_llegada"),
        seatClassLabel: this.seatClassLabel(this.seatClass)
      };
    },
    summaryItems() {
      const passengerCount = this.passengers.length;
      const passengerLabel = passengerCount === 1 ? "pasajero" : "pasajeros";

      return [
        {
          label: `Vuelos (${passengerCount} ${passengerLabel})`,
          value: this.formatCurrency(this.flightSubtotal)
        },
        {
          label: "Equipaje facturado",
          value: this.placeholder("equipaje_facturado_total")
        },
        {
          label: "Tasas e impuestos",
          value: this.placeholder("tasas_impuestos")
        }
      ];
    },
    unitFlightPrice() {
      if (!this.route) return null;

      if (this.seatClass === "economy") {
        return Number(this.fieldValue(this.route, "priceEconomyClass", "PriceEconomyClass"));
      }

      if (this.seatClass === "firstClass") {
        return Number(this.fieldValue(this.route, "priceFirstClass", "PriceFirstClass"));
      }

      return null;
    },
    flightSubtotal() {
      if (!Number.isFinite(this.unitFlightPrice)) return null;
      return this.unitFlightPrice * this.passengers.length;
    },
    summaryTotal() {
      const bookedTotal = Number(this.fieldValue(this.booking, "totalAmount", "TotalAmount"));
      if (Number.isFinite(bookedTotal)) {
        return this.formatCurrency(bookedTotal);
      }

      return this.placeholder("total_compra");
    },
    confirmDisabled() {
      return this.loading || Boolean(this.error) || !this.purchaseOrder || Boolean(this.booking);
    },
    bookingSuccessMessage() {
      if (!this.booking) return "";

      const confirmationCode = this.fieldValue(this.booking, "confirmationCode", "ConfirmationCode");
      return confirmationCode
        ? `Compra confirmada. Codigo de reserva: ${confirmationCode}.`
        : "Compra confirmada.";
    }
  },
  mounted() {
    this.loadPurchaseConfirmation();
  },
  methods: {
    async loadPurchaseConfirmation() {
      if (!this.purchaseOrderId) {
        this.error = "No se encontro el identificador de la orden de compra en la URL.";
        return;
      }

      this.loading = true;
      this.error = "";

      try {
        const purchaseOrder = await getPurchaseOrder(this.purchaseOrderId);
        this.purchaseOrder = purchaseOrder;

        const routeId = this.fieldValue(purchaseOrder, "routeId", "RouteId");
        if (routeId) {
          try {
            this.route = await getRoute(routeId);
          } catch (routeError) {
            console.error("Error cargando la ruta de la orden:", routeError);
            this.route = null;
          }
        }
      } catch (purchaseOrderError) {
        console.error("Error cargando la orden de compra:", purchaseOrderError);
        this.error = "No se pudo cargar la orden de compra.";
      } finally {
        this.loading = false;
      }
    },
    async confirmPurchase() {
      this.bookingError = "";

      if (!this.validatePayment()) {
        return;
      }

      this.bookingSubmitting = true;

      try {
        const booking = await confirmBooking({
          purchaseOrderId: Number(this.purchaseOrderId),
          email: this.paymentDetails.email.trim(),
          cardBrand: this.cardBrand(this.paymentDetails.cardNumber),
          cardLastFour: this.cardLastFour(this.paymentDetails.cardNumber),
          cardHolderName: this.paymentDetails.cardholderName.trim()
        });

        this.booking = booking;
        sessionStorage.removeItem("bookingHolderEmail");
        sessionStorage.removeItem("bookingHolderName");
        this.$router.push({
          name: "OrderConfirmation",
          params: { bookingGuid: this.fieldValue(booking, "guid", "Guid") }
        });
      } catch (bookingError) {
        console.error("Error confirmando la compra:", bookingError);
        this.bookingError = this.backendErrorMessage(bookingError);
      } finally {
        this.bookingSubmitting = false;
      }
    },
    validatePayment() {
      const errors = {};
      const cardDigits = this.cardDigits(this.paymentDetails.cardNumber);
      const email = this.paymentDetails.email.trim();
      const cardholderName = this.paymentDetails.cardholderName.trim();
      const expirationDate = this.paymentDetails.expirationDate.trim();
      const cvv = this.paymentDetails.cvv.trim();

      if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
        errors.email = "Ingrese un correo valido";
      }

      if (cardDigits.length < 12 || cardDigits.length > 19) {
        errors.cardNumber = "Ingrese un numero de tarjeta valido";
      }

      if (!cardholderName) {
        errors.cardholderName = "Ingrese el nombre del titular";
      }

      if (!/^(0[1-9]|1[0-2])\/\d{2}$/.test(expirationDate)) {
        errors.expirationDate = "Use el formato MM/AA";
      }

      if (!/^\d{3,4}$/.test(cvv)) {
        errors.cvv = "Ingrese un CVV valido";
      }

      this.paymentErrors = errors;
      return Object.keys(errors).length === 0;
    },
    cardDigits(cardNumber) {
      return String(cardNumber || "").replace(/\D/g, "");
    },
    cardLastFour(cardNumber) {
      return this.cardDigits(cardNumber).slice(-4);
    },
    cardBrand(cardNumber) {
      const digits = this.cardDigits(cardNumber);

      if (digits.startsWith("4")) return "Visa";
      if (/^5[1-5]/.test(digits) || /^2(2[2-9]|[3-6]\d|7[01]|720)/.test(digits)) return "Mastercard";
      if (/^3[47]/.test(digits)) return "American Express";
      if (digits.startsWith("6")) return "Discover";

      return "Unknown";
    },
    backendErrorMessage(error) {
      return (
        error?.response?.data?.message ||
        error?.response?.data?.Message ||
        "No se pudo confirmar la compra."
      );
    },
    fieldValue(source, camelCaseKey, pascalCaseKey) {
      return source?.[camelCaseKey] ?? source?.[pascalCaseKey];
    },
    airport(camelCaseKey, pascalCaseKey) {
      return this.fieldValue(this.route, camelCaseKey, pascalCaseKey) || {};
    },
    placeholder(key) {
      return `{{${key}}}`;
    },
    formatDate(value) {
      if (!value) return this.placeholder("fecha_vuelo");

      const date = new Date(`${String(value).slice(0, 10)}T00:00:00`);
      if (Number.isNaN(date.getTime())) return this.placeholder("fecha_vuelo");

      return new Intl.DateTimeFormat("es-ES", {
        day: "numeric",
        month: "long",
        year: "numeric"
      }).format(date);
    },
    formatTime(value, placeholderKey) {
      if (!value) return this.placeholder(placeholderKey);
      return String(value).slice(0, 5);
    },
    formatDuration(durationMinutes) {
      const minutes = Number(durationMinutes);
      if (!Number.isFinite(minutes) || minutes <= 0) return this.placeholder("duracion");

      const hours = Math.floor(minutes / 60);
      const remainingMinutes = minutes % 60;

      if (hours === 0) return `${remainingMinutes}m`;
      if (remainingMinutes === 0) return `${hours}h`;
      return `${hours}h ${remainingMinutes}m`;
    },
    formatCurrency(value) {
      const amount = Number(value);
      if (!Number.isFinite(amount)) return this.placeholder("monto_vuelos");

      return new Intl.NumberFormat("en-US", {
        style: "currency",
        currency: "USD",
        minimumFractionDigits: Number.isInteger(amount) ? 0 : 2,
        maximumFractionDigits: 2
      }).format(amount);
    },
    seatClassLabel(value) {
      if (value === "economy") return "Economica";
      if (value === "firstClass") return "Primera clase";
      return value || this.placeholder("clase");
    }
  }
};
</script>

<style scoped>
.purchase-confirmation-page {
  min-height: 100vh;
  background: #f7f7f9;
  color: #111827;
}

.back-button {
  width: 100%;
  min-height: 58px;
  padding: 0 18px;
  border: none;
  border-bottom: 1px solid #d8dde6;
  background: var(--whiteColor);
  color: #526078;
  display: inline-flex;
  align-items: center;
  gap: 8px;
  font-family: var(--primaryFontFamily);
  font-size: var(--baseFontSize);
  cursor: pointer;
}

.back-button:hover {
  color: var(--linkColor);
}

.back-button svg {
  width: 18px;
  height: 18px;
  fill: none;
  stroke: currentColor;
  stroke-width: 2;
  stroke-linecap: round;
  stroke-linejoin: round;
}

.confirmation-shell {
  width: min(1230px, calc(100% - 32px));
  margin: 0 auto;
  padding: 42px 0 88px;
}

.page-intro {
  margin-bottom: 32px;
}

.page-intro h1 {
  color: #111827;
  font-size: 1.55rem;
  font-weight: var(--boldFontWeight);
  margin-bottom: 12px;
}

.page-intro p {
  color: #526078;
  font-size: var(--smallFontSize);
}

.confirmation-layout {
  display: grid;
  grid-template-columns: minmax(0, 1fr) minmax(300px, 380px);
  gap: 32px;
  align-items: start;
}

.confirmation-main {
  display: grid;
  gap: 24px;
}

.status-card {
  border: 1px solid #d8dde6;
  border-radius: var(--defaultBorderRadius);
  box-shadow: none;
  color: #526078;
}

@media (max-width: 980px) {
  .confirmation-layout {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 640px) {
  .confirmation-shell {
    width: min(100% - 24px, 1230px);
    padding-top: 28px;
  }

  .back-button {
    min-height: 50px;
  }
}
</style>
