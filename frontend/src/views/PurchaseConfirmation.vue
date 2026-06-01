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
          <PaymentMethodCard v-model="paymentDetails" />
        </div>

        <PurchaseSummaryCard :items="summaryItems" :total="summaryTotal" />
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
import { getPurchaseOrder, getRoute } from "../services/purchaseConfirmationService";

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
      paymentDetails: {
        cardNumber: "",
        cardholderName: "",
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
      return this.placeholder("total_compra");
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

      return new Intl.NumberFormat("es-ES", {
        style: "currency",
        currency: "EUR",
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
