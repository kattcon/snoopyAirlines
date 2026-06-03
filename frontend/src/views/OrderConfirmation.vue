<template>
  <div class="order-confirmation-page">
    <BookingHeader />

    <main class="confirmation-shell">
      <section class="success-hero">
        <div class="success-icon" aria-hidden="true">
          <svg viewBox="0 0 24 24">
            <circle cx="12" cy="12" r="10"></circle>
            <path d="m8 12 3 3 5-6"></path>
          </svg>
        </div>
        <h1>¡Reserva confirmada!</h1>
        <p>Tu vuelo ha sido reservado exitosamente</p>

        <span class="reservation-label">Número de reserva</span>
        <strong class="reservation-code">{{ confirmationCode }}</strong>
      </section>

      <section v-if="loading" class="status-card contentCard">
        Cargando reserva...
      </section>

      <section v-else-if="error" class="status-card contentCard">
        {{ error }}
      </section>

      <template v-else>
        <section class="email-card contentCard">
          <span class="mail-icon" aria-hidden="true">
            <svg viewBox="0 0 24 24">
              <rect x="3" y="5" width="18" height="14" rx="2"></rect>
              <path d="m3 7 9 6 9-6"></path>
            </svg>
          </span>
          <div>
            <h2>Confirmación enviada por email</h2>
            <p>Hemos enviado los detalles de tu reserva y las tarjetas de embarque a <strong>{{ bookingEmail }}</strong></p>
          </div>
        </section>

        <FlightDetailsCard
          v-for="details in flightDetailsList"
          :key="details.sequenceNumber"
          :details="details"
        />
        <PassengerListCard :passengers="passengers" :show-luggage="false" />

        <section class="important-card contentCard">
          <h2>Información importante</h2>
          <ul>
            <li>Presenta tu número de reserva <strong>{{ confirmationCode }}</strong> en el mostrador de check-in del aeropuerto</li>
            <li>El check-in online abre 24 horas antes de la salida del vuelo</li>
            <li>Llega al aeropuerto con al menos 3 horas de anticipación para vuelos internacionales</li>
            <li>Asegúrate de que tu pasaporte tenga al menos 6 meses de validez desde la fecha de viaje</li>
          </ul>
        </section>

        <div class="action-row">
          <button type="button" class="secondaryButton download-button">
            <svg viewBox="0 0 24 24" aria-hidden="true">
              <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"></path>
              <path d="M7 10l5 5 5-5"></path>
              <path d="M12 15V3"></path>
            </svg>
            <span>Descargar confirmación</span>
          </button>
          <router-link to="/" class="primaryButton home-button">
            <span>Volver al inicio</span>
            <svg viewBox="0 0 24 24" aria-hidden="true">
              <path d="M5 12h14"></path>
              <path d="m12 5 7 7-7 7"></path>
            </svg>
          </router-link>
        </div>
      </template>
    </main>
  </div>
</template>

<script>
import BookingHeader from "../components/purchase-confirmation/BookingHeader.vue";
import FlightDetailsCard from "../components/purchase-confirmation/FlightDetailsCard.vue";
import PassengerListCard from "../components/purchase-confirmation/PassengerListCard.vue";
import { getBooking, getPurchaseOrder, getRoute } from "../services/purchaseConfirmationService";

export default {
  name: "OrderConfirmation",
  components: {
    BookingHeader,
    FlightDetailsCard,
    PassengerListCard
  },
  data() {
    return {
      loading: false,
      error: "",
      booking: null,
      purchaseOrder: null,
      routeDetails: []
    };
  },
  computed: {
    bookingGuid() {
      return this.$route.params.bookingGuid || this.$route.query.bookingGuid || this.$route.query.id;
    },
    purchaseOrderId() {
      return this.fieldValue(this.booking, "purchaseOrderId", "PurchaseOrderId");
    },
    confirmationCode() {
      return this.fieldValue(this.booking, "confirmationCode", "ConfirmationCode") || this.placeholder("numero_reserva");
    },
    bookingEmail() {
      return this.fieldValue(this.booking, "email", "Email") || this.placeholder("email");
    },
    purchaseRoutes() {
      return this.fieldValue(this.purchaseOrder, "routes", "Routes") || [];
    },
    passengers() {
      return this.fieldValue(this.purchaseOrder, "passengers", "Passengers") || [];
    },
    seatClass() {
      return this.fieldValue(this.purchaseOrder, "seatClass", "SeatClass");
    },
    totalPaid() {
      return this.formatCurrency(this.fieldValue(this.booking, "totalAmount", "TotalAmount"));
    },
    flightDetailsList() {
      return this.routeDetails.map((routeDetail, index) =>
        this.toFlightDetails(routeDetail, index === 0));
    }
  },
  mounted() {
    this.loadOrderConfirmation();
  },
  methods: {
    async loadOrderConfirmation() {
      if (!this.bookingGuid) {
        this.error = "No se encontro el identificador de la reserva en la URL.";
        return;
      }

      this.loading = true;
      this.error = "";

      try {
        const booking = await getBooking(this.bookingGuid);
        this.booking = booking;

        const purchaseOrder = await getPurchaseOrder(this.fieldValue(booking, "purchaseOrderId", "PurchaseOrderId"));
        this.purchaseOrder = purchaseOrder;
        this.routeDetails = await this.loadRouteDetails(this.purchaseRoutes);
      } catch (loadError) {
        console.error("Error cargando la confirmacion de reserva:", loadError);
        this.error = "No se pudo cargar la confirmacion de reserva.";
      } finally {
        this.loading = false;
      }
    },
    fieldValue(source, camelCaseKey, pascalCaseKey) {
      return source?.[camelCaseKey] ?? source?.[pascalCaseKey];
    },
    async loadRouteDetails(routes) {
      if (!Array.isArray(routes) || routes.length === 0) {
        throw new Error("Purchase order does not have routes.");
      }

      return Promise.all(
        routes
          .slice()
          .sort((left, right) => this.sequenceNumber(left) - this.sequenceNumber(right))
          .map(async (routeLeg, index) => ({
            routeLeg,
            sequenceNumber: this.sequenceNumber(routeLeg) || index + 1,
            route: await getRoute(this.routeId(routeLeg))
          }))
      );
    },
    toFlightDetails(routeDetail, includeTotalPaid) {
      const route = routeDetail.route;
      const routeLeg = routeDetail.routeLeg;
      const departureAirport = this.fieldValue(route, "departureAirport", "DepartureAirport") || {};
      const arrivalAirport = this.fieldValue(route, "arrivalAirport", "ArrivalAirport") || {};

      return {
        sequenceNumber: routeDetail.sequenceNumber,
        flightLabel: `Tramo ${routeDetail.sequenceNumber}`,
        dateLabel: this.formatDate(this.intendedDate(routeLeg)),
        departureTime: this.formatTime(this.fieldValue(route, "departureTime", "DepartureTime"), "hora_salida"),
        arrivalTime: this.formatTime(this.fieldValue(route, "arrivalTime", "ArrivalTime"), "hora_llegada"),
        durationLabel: this.formatDuration(this.fieldValue(route, "durationMinutes", "DurationMinutes")),
        departureCode: this.fieldValue(departureAirport, "code", "Code") || this.placeholder("origen"),
        departureCity: this.fieldValue(departureAirport, "city", "City") || this.placeholder("ciudad_salida"),
        arrivalCode: this.fieldValue(arrivalAirport, "code", "Code") || this.placeholder("destino"),
        arrivalCity: this.fieldValue(arrivalAirport, "city", "City") || this.placeholder("ciudad_llegada"),
        seatClassLabel: this.seatClassLabel(this.seatClass),
        extraFields: includeTotalPaid
          ? [
              {
                label: "Total pagado",
                value: this.totalPaid
              }
            ]
          : []
      };
    },
    sequenceNumber(routeLeg) {
      return Number(this.fieldValue(routeLeg, "sequenceNumber", "SequenceNumber"));
    },
    routeId(routeLeg) {
      return Number(this.fieldValue(routeLeg, "routeId", "RouteId"));
    },
    intendedDate(routeLeg) {
      return this.fieldValue(routeLeg, "intendedDate", "IntendedDate");
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
      if (!Number.isFinite(amount)) return this.placeholder("total_pagado");

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
.order-confirmation-page {
  min-height: 100vh;
  background: #f7f7f9;
  color: #111827;
}

.confirmation-shell {
  width: min(840px, calc(100% - 32px));
  margin: 0 auto;
  padding: 56px 0 56px;
}

.success-hero {
  display: grid;
  justify-items: center;
  text-align: center;
  margin-bottom: 42px;
}

.success-icon {
  width: 78px;
  height: 78px;
  display: grid;
  place-items: center;
  border-radius: 50%;
  background: #d8f9e4;
  color: #05a451;
  margin-bottom: 28px;
}

.success-icon svg {
  width: 42px;
  height: 42px;
  fill: none;
  stroke: currentColor;
  stroke-width: 2;
  stroke-linecap: round;
  stroke-linejoin: round;
}

.success-hero h1 {
  font-size: 1.55rem;
  margin-bottom: 14px;
}

.success-hero p,
.reservation-label {
  color: #526078;
  font-size: var(--smallFontSize);
}

.reservation-label {
  margin-top: 28px;
  margin-bottom: 10px;
}

.reservation-code {
  min-width: 150px;
  padding: 14px 24px;
  border-radius: var(--defaultBorderRadius);
  background: #03021a;
  color: var(--whiteColor);
  font-size: 2rem;
  letter-spacing: 0;
}

.status-card,
.email-card,
.important-card {
  border: 1px solid #d8dde6;
  border-radius: var(--defaultBorderRadius);
  box-shadow: none;
}

.email-card {
  display: flex;
  align-items: center;
  gap: 18px;
  margin-bottom: 28px;
}

.mail-icon {
  width: 38px;
  height: 38px;
  display: grid;
  place-items: center;
  flex: 0 0 auto;
  border-radius: 50%;
  background: #e8f0ff;
  color: #3d7bff;
}

.mail-icon svg,
.action-row svg {
  width: 18px;
  height: 18px;
  fill: none;
  stroke: currentColor;
  stroke-width: 2;
  stroke-linecap: round;
  stroke-linejoin: round;
}

.email-card h2,
.important-card h2 {
  font-size: var(--baseFontSize);
  margin-bottom: 8px;
}

.email-card p,
.important-card li {
  color: #526078;
  font-size: var(--smallFontSize);
}

.email-card strong,
.important-card strong {
  color: #111827;
}

.important-card {
  margin-top: 28px;
}

.important-card ul {
  display: grid;
  gap: 14px;
  padding-left: 18px;
  margin-top: 34px;
}

.action-row {
  display: grid;
  grid-template-columns: 1fr 1.8fr;
  gap: 18px;
  margin-top: 28px;
}

.download-button,
.home-button {
  min-height: 42px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
  border-radius: var(--smallBorderRadius);
  text-decoration: none;
}

.home-button {
  background: #03021a;
}

@media (max-width: 640px) {
  .confirmation-shell {
    width: min(100% - 24px, 840px);
    padding-top: 36px;
  }

  .email-card {
    align-items: flex-start;
  }

  .action-row {
    grid-template-columns: 1fr;
  }
}
</style>
