<template>
  <div class="reservation-page">
    <!-- Back -->
    <div class="back-link">
        <a class="back-link" href="/">
            <p>← Volver al inicio</p>
        </a>
    </div>

    <!-- Empty state: no llegaron datos (ej. recarga de página) -->
    <div v-if="!hasReport" class="empty-state">
      <p>No hay información de reservación para mostrar.</p>
      <p>Por favor busca tu reservación nuevamente desde la página de inicio.</p>
      <router-link to="/" class="reservation-button empty-state-link">Volver al inicio</router-link>
    </div>

    <template v-else>
      <!-- Header -->
      <section class="hero">
        <div class="hero-content">
          <div>
            <p class="hero-subtitle">
              SNOOPY AIRLINES — TU RESERVACIÓN
            </p>

            <h1 class="hero-title">
              {{ firstLeg.departureCity }} → {{ lastLeg.arrivalCity }}
            </h1>

            <div class="hero-tags">
              <span>{{ reservationNumber }}</span>
              <span>{{ formatDate(firstLeg.departureDate) }}</span>
              <span>{{ passengerCount }} pasajero{{ passengerCount === 1 ? '' : 's' }}</span>
              <span>{{ tripTypeLabel }}</span>
            </div>
          </div>

          <div v-if="daysUntilDeparture !== null" class="countdown-card">
            <div class="countdown-number">
              {{ Math.abs(daysUntilDeparture) }}
            </div>

            <div class="countdown-text">
              {{ countdownLabel }}
            </div>
          </div>
        </div>
      </section>

      <!-- Main Content -->
      <section class="content">
        <!-- Left Column -->
        <div class="left-column">
          <!-- Una tarjeta por cada tramo de la reservación -->
          <div class="flight-card" v-for="leg in flightLegs" :key="leg.sequenceNumber">
            <div class="flight-header">
              <div>
                <div class="flight-type">
                  {{ legLabel(leg) }}
                </div>

                <div class="route">
                  {{ leg.departureCity }} → {{ leg.arrivalCity }}
                </div>
              </div>

              <div class="flight-number">
                Tramo {{ leg.sequenceNumber }}
              </div>
            </div>

            <div class="flight-body">
              <div class="airport">
                <h2>{{ formatTime(leg.departureTime) }}</h2>
                <span>{{ leg.departureAirportCode }}</span>
                <small>{{ leg.departureCity }}</small>
              </div>

              <div class="flight-center">
                <div>{{ formatDuration(leg.durationMinutes) }}</div>
                <small>Directo</small>
              </div>

              <div class="airport airport-right">
                <h2>{{ formatTime(leg.arrivalTime) }}</h2>
                <span>{{ leg.arrivalAirportCode }}</span>
                <small>{{ leg.arrivalCity }}</small>
              </div>
            </div>

            <div class="flight-footer">
              <div>
                <label>Fecha</label>
                <span>{{ formatDate(leg.departureDate) }}</span>
              </div>

              <div>
                <label>Aeronave</label>
                <span>{{ leg.airplaneModel }}</span>
              </div>
            </div>
          </div>
        </div>

        <!-- Right Column -->
        <div class="right-column">
          <div class="action-card">
            <h3>Equipaje</h3>

            <p>
              Agrega equipaje adicional a tu reservación.
            </p>

            <button class="primary-button">
              Comprar equipaje
            </button>
          </div>

          <div class="action-card">
            <h3>Cancelar reservación</h3>

            <button class="danger-button">
              Cancelar reservación
            </button>
          </div>

          <button class="pdf-button">
            Imprimir itinerario (PDF)
          </button>

          <div class="passenger-card">
            <div class="passenger-title">
              TITULAR DE LA RESERVACIÓN
            </div>

            <div class="passenger-info">
              <div class="avatar">
                {{ cardHolderInitials }}
              </div>

              <div>
                <strong>
                  {{ cardHolderName }}
                </strong>

                <p>
                  {{ passengerCount }} pasajero{{ passengerCount === 1 ? '' : 's' }} ·
                  {{ tripTypeLabel }}
                </p>
              </div>
            </div>
          </div>
        </div>
      </section>
    </template>
  </div>
</template>

<script>
export default {
  name: 'ClientFlightReport',
  data() {
    return {
      // Los tramos llegan vía router state desde LandingPage.vue (búsqueda de reservación).
      // Cada elemento corresponde a una fila devuelta por dbo.GetFlightReportByConfirmation.
      flightLegs: [],
    };
  },
  created() {
    const stateReport = window.history.state?.flightReport;

    if (Array.isArray(stateReport) && stateReport.length > 0) {
      this.flightLegs = [...stateReport].sort((a, b) => a.sequenceNumber - b.sequenceNumber);
    }
  },
  computed: {
    hasReport() {
      return this.flightLegs.length > 0;
    },
    firstLeg() {
      return this.flightLegs[0] ?? {};
    },
    lastLeg() {
      return this.flightLegs[this.flightLegs.length - 1] ?? {};
    },
    reservationNumber() {
      return this.firstLeg.reservationNumber ?? '';
    },
    cardHolderName() {
      return this.firstLeg.cardHolderName ?? 'Titular no disponible';
    },
    cardHolderInitials() {
      return this.cardHolderName
        .split(' ')
        .filter(Boolean)
        .slice(0, 2)
        .map(word => word[0]?.toUpperCase() ?? '')
        .join('');
    },
    passengerCount() {
      return this.firstLeg.passengerCount ?? 0;
    },
    tripTypeLabel() {
      return this.flightLegs.length > 1 ? 'Con escala(s)' : 'Directo';
    },
    daysUntilDeparture() {
      const departureDate = this.firstLeg.departureDate;
      if (!departureDate) return null;

      // Comparar solo fechas (sin horas) para evitar resultados parciales por zona horaria
      const today = new Date();
      const todayDateOnly = new Date(today.getFullYear(), today.getMonth(), today.getDate());
      const departure = new Date(`${departureDate}T00:00:00`);

      const msPerDay = 1000 * 60 * 60 * 24;
      return Math.round((departure - todayDateOnly) / msPerDay);
    },
    countdownLabel() {
      if (this.daysUntilDeparture === 0) return 'tu vuelo es hoy';
      if (this.daysUntilDeparture > 0) return 'días para viajar';
      return 'días desde tu vuelo';
    },
  },
  methods: {
    legLabel(leg) {
      if (this.flightLegs.length === 1) {
        return 'VUELO';
      }
      if (leg.sequenceNumber === this.firstLeg.sequenceNumber) {
        return 'PRIMER TRAMO';
      }
      if (leg.sequenceNumber === this.lastLeg.sequenceNumber) {
        return 'ÚLTIMO TRAMO';
      }
      return `TRAMO ${leg.sequenceNumber}`;
    },
    formatTime(value) {
      // El backend envía TimeOnly serializado como "HH:mm:ss"
      return value ? String(value).slice(0, 5) : '';
    },
    formatDate(value) {
      if (!value) return '';
      // El backend envía DateOnly serializado como "YYYY-MM-DD"
      return new Date(`${value}T00:00:00`).toLocaleDateString('es-ES', {
        weekday: 'long',
        day: 'numeric',
        month: 'long',
        year: 'numeric',
      });
    },
    formatDuration(minutes) {
      if (!minutes && minutes !== 0) return '';
      const hours = Math.floor(minutes / 60);
      const remainingMinutes = minutes % 60;
      return `${hours}h ${remainingMinutes}min`;
    },
  },
};
</script>

<style scoped>
.reservation-page {
  min-height: 100vh;
  background-color: #f4f6f9;
}

.back-link {
  text-decoration: none;
  padding: 20px 40px;
  font-weight: 600;
  color: #005fa3;
  cursor: pointer;
}

.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 12px;
  padding: 80px 40px;
  text-align: center;
  color: #555;
}

.empty-state-link {
  display: inline-block;
  width: auto;
  margin-top: 12px;
  padding: 14px 28px;
  background: #ffc107;
  color: #1f1f1f;
  border-radius: 12px;
  font-weight: 700;
  text-decoration: none;
}

.hero {
  background: linear-gradient(
    135deg,
    #003b7a,
    #0a5eb0
  );
  color: white;
  padding: 40px;
}

.hero-content {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 24px;
}

.hero-subtitle {
  font-size: 0.9rem;
  opacity: 0.8;
  letter-spacing: 1px;
}

.hero-title {
  margin: 10px 0;
  font-size: 2.8rem;
  font-weight: 700;
}

.hero-tags {
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
  margin-top: 20px;
}

.hero-tags span {
  padding: 10px 16px;
  border-radius: 999px;
  background: rgba(255, 255, 255, 0.15);
  border: 1px solid rgba(255, 255, 255, 0.2);
}

.countdown-card {
  min-width: 150px;
  padding: 20px;
  text-align: center;
  border-radius: 16px;
  background: rgba(255, 255, 255, 0.12);
}

.countdown-number {
  font-size: 3rem;
  font-weight: bold;
  color: #ffd54a;
}

.countdown-text {
  font-size: 0.9rem;
  opacity: 0.85;
}

.content {
  display: grid;
  grid-template-columns: 2fr 1fr;
  gap: 24px;
  padding: 32px 40px;
}

.flight-card {
  background: white;
  border-radius: 16px;
  overflow: hidden;
  margin-bottom: 24px;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.08);
}

.flight-header {
  display: flex;
  justify-content: space-between;
  align-items: center;

  background: linear-gradient(
    90deg,
    #004f8f,
    #2451d3
  );

  color: white;
  padding: 20px;
}

.flight-type {
  font-size: 0.8rem;
  opacity: 0.8;
}

.route {
  font-weight: 600;
  margin-top: 4px;
}

.flight-number {
  font-size: 1.3rem;
  font-weight: bold;
}

.flight-body {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 30px;
}

.airport {
  display: flex;
  flex-direction: column;
}

.airport h2 {
  margin: 0;
  font-size: 2rem;
}

.airport span {
  margin-top: 8px;
  font-weight: 600;
}

.airport small {
  color: #666;
}

.airport-right {
  text-align: right;
}

.flight-center {
  text-align: center;
}

.flight-center small {
  display: block;
  margin-top: 8px;
  color: #1f8f47;
  font-weight: 600;
}

.flight-footer {
  display: flex;
  justify-content: space-between;
  gap: 16px;

  padding: 20px 30px;

  border-top: 1px solid #e5e7eb;
}

.flight-footer div {
  display: flex;
  flex-direction: column;
}

.flight-footer label {
  font-size: 0.85rem;
  color: #777;
  margin-bottom: 4px;
}

.action-card {
  background: white;
  border-radius: 16px;
  padding: 24px;
  margin-bottom: 20px;

  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.08);
}

.action-card h3 {
  margin-top: 0;
}

.action-card p {
  color: #666;
  margin-bottom: 16px;
}

.primary-button,
.danger-button,
.pdf-button {
  width: 100%;
  border: none;
  border-radius: 12px;
  padding: 14px;
  cursor: pointer;
  font-weight: 600;
}

.primary-button {
  background: #005b8f;
  color: white;
}

.danger-button {
  background: white;
  color: #d62828;
  border: 1px solid #d62828;
}

.pdf-button {
  margin-bottom: 20px;
  background: #ffc107;
}

.passenger-card {
  background: white;
  border-radius: 16px;
  padding: 24px;

  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.08);
}

.passenger-title {
  font-size: 0.85rem;
  color: #005fa3;
  margin-bottom: 16px;
}

.passenger-info {
  display: flex;
  align-items: center;
  gap: 12px;
}

.avatar {
  width: 48px;
  height: 48px;

  border-radius: 50%;

  display: flex;
  justify-content: center;
  align-items: center;

  background: #005b8f;
  color: white;
  font-weight: bold;
}

@media (max-width: 900px) {
  .hero-content {
    flex-direction: column;
  }

  .content {
    grid-template-columns: 1fr;
  }

  .flight-body {
    flex-direction: column;
    gap: 24px;
  }

  .flight-footer {
    flex-direction: column;
  }

  .airport-right {
    text-align: left;
  }

  .hero-title {
    font-size: 2rem;
  }
}
</style>