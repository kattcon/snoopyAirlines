<template>
  <div class="reservation-page">
    <!-- Back -->
    <div class="back-link">
        <a class="back-link" href="/">
            <p>← Volver al inicio</p>
        </a>
    </div>

    <!-- Header -->
    <section class="hero">
      <div class="hero-content">
        <div>
          <p class="hero-subtitle">
            SNOOPY AIRLINES — TU RESERVACIÓN
          </p>

          <h1 class="hero-title">
            {{ MOCK_ORIGIN }} → {{ MOCK_DESTINATION }}
          </h1>

          <p class="hero-location">
            {{ MOCK_DESTINATION_LOCATION }}
          </p>

          <div class="hero-tags">
            <span>{{ MOCK_RESERVATION_NUMBER }}</span>
            <span>{{ MOCK_DEPARTURE_DATE }}</span>
            <span>{{ MOCK_PASSENGERS }} pasajeros</span>
            <span>{{ MOCK_TRIP_TYPE }}</span>
          </div>
        </div>

        <div class="countdown-card">
          <div class="countdown-number">
            {{ MOCK_DAYS_LEFT }}
          </div>

          <div class="countdown-text">
            días para viajar
          </div>
        </div>
      </div>
    </section>

    <!-- Main Content -->
    <section class="content">
      <!-- Left Column -->
      <div class="left-column">
        <!-- Outbound Flight -->
        <div class="flight-card">
          <div class="flight-header">
            <div>
              <div class="flight-type">
                VUELO DE IDA
              </div>

              <div class="route">
                {{ MOCK_ORIGIN }} → {{ MOCK_DESTINATION }}
              </div>
            </div>

            <div class="flight-number">
              {{ MOCK_OUTBOUND_FLIGHT }}
            </div>
          </div>

          <div class="flight-body">
            <div class="airport">
              <h2>{{ MOCK_OUTBOUND_DEPARTURE_TIME }}</h2>
              <span>SJO</span>
              <small>San José</small>
            </div>

            <div class="flight-center">
              <div>{{ MOCK_FLIGHT_DURATION }}</div>
              <small>Directo</small>
            </div>

            <div class="airport airport-right">
              <h2>{{ MOCK_OUTBOUND_ARRIVAL_TIME }}</h2>
              <span>MIA</span>
              <small>Miami</small>
            </div>
          </div>

          <div class="flight-footer">
            <div>
              <label>Fecha</label>
              <span>{{ MOCK_DEPARTURE_DATE }}</span>
            </div>

            <div>
              <label>Aeronave</label>
              <span>{{ MOCK_OUTBOUND_AIRCRAFT }}</span>
            </div>

            <div>
              <label>Terminal</label>
              <span>{{ MOCK_OUTBOUND_TERMINAL }}</span>
            </div>
          </div>
        </div>

        <!-- Return Flight -->
        <div class="flight-card">
          <div class="flight-header">
            <div>
              <div class="flight-type">
                VUELO DE REGRESO
              </div>

              <div class="route">
                {{ MOCK_DESTINATION }} → {{ MOCK_ORIGIN }}
              </div>
            </div>

            <div class="flight-number">
              {{ MOCK_RETURN_FLIGHT }}
            </div>
          </div>

          <div class="flight-body">
            <div class="airport">
              <h2>{{ MOCK_RETURN_DEPARTURE_TIME }}</h2>
              <span>MIA</span>
              <small>Miami</small>
            </div>

            <div class="flight-center">
              <div>{{ MOCK_FLIGHT_DURATION }}</div>
              <small>Directo</small>
            </div>

            <div class="airport airport-right">
              <h2>{{ MOCK_RETURN_ARRIVAL_TIME }}</h2>
              <span>SJO</span>
              <small>San José</small>
            </div>
          </div>

          <div class="flight-footer">
            <div>
              <label>Fecha</label>
              <span>{{ MOCK_RETURN_DATE }}</span>
            </div>

            <div>
              <label>Aeronave</label>
              <span>{{ MOCK_RETURN_AIRCRAFT }}</span>
            </div>

            <div>
              <label>Terminal</label>
              <span>{{ MOCK_RETURN_TERMINAL }}</span>
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
              CG
            </div>

            <div>
              <strong>
                {{ MOCK_PASSENGER_NAME }}
              </strong>

              <p>
                {{ MOCK_PASSENGERS }}
                pasajeros ·
                {{ MOCK_TRIP_TYPE }}
              </p>
            </div>
          </div>
        </div>
      </div>
    </section>
  </div>
</template>

<script setup>
// ======================================
// MOCK DATA - REPLACE WITH API RESPONSE
// ======================================

const MOCK_ORIGIN = 'San José';
const MOCK_DESTINATION = 'Miami';
const MOCK_DESTINATION_LOCATION = 'Florida, EE.UU.';

const MOCK_RESERVATION_NUMBER = 'SA-2026-001234';

const MOCK_DEPARTURE_DATE = 'Sábado, 5 de julio de 2026';
const MOCK_RETURN_DATE = 'Sábado, 12 de julio de 2026';

const MOCK_PASSENGERS = 2;
const MOCK_TRIP_TYPE = 'Ida y vuelta';

const MOCK_DAYS_LEFT = 19;

const MOCK_OUTBOUND_FLIGHT = 'SN 204';
const MOCK_RETURN_FLIGHT = 'SN 108';

const MOCK_OUTBOUND_DEPARTURE_TIME = '07:45';
const MOCK_OUTBOUND_ARRIVAL_TIME = '12:00';

const MOCK_RETURN_DEPARTURE_TIME = '15:30';
const MOCK_RETURN_ARRIVAL_TIME = '19:45';

const MOCK_FLIGHT_DURATION = '4h 15min';

const MOCK_OUTBOUND_AIRCRAFT = 'Boeing 737-800';
const MOCK_RETURN_AIRCRAFT = 'Airbus A320neo';

const MOCK_OUTBOUND_TERMINAL = 'Terminal 2';
const MOCK_RETURN_TERMINAL = 'Concourse D';

const MOCK_PASSENGER_NAME = 'Carlos García Rodríguez';
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

.hero-location {
  opacity: 0.9;
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