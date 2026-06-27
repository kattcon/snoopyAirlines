<template>
  <div class="reservation-page">
    <!-- Back -->
    <div class="back-link">
        <a class="back-link" href="/">
            <p>← Volver al inicio</p>
        </a>
    </div>

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

          <div v-else-if="bookingStatus === 'Cancelada'" class="countdown-card cancelled-card">
            <div class="countdown-number" style="font-size:2rem;">✕</div>
            <div class="countdown-text">Reservación cancelada</div>
          </div>

        </div>
      </section>

      <!-- Main Content -->
      <section class="content">
        <!-- Left Column -->
        <div class="left-column">
          <!-- Una tarjeta por cada tramo de la reservacion -->
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
            <p v-if="bookingStatus === 'Cancelada'"
                style="color:#d62828;font-weight:600;margin:0;">
              Esta reservación ya fue cancelada.
            </p>

            <button 
              v-else
              class="danger-button"
              @click="showCancelConfirm = true">
              Cancelar reservación
            </button>
          </div>

          <button class="pdf-button" @click="printItinerary">
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

    <!-- Modal de confirmación -->
  <div v-if="showCancelConfirm" class="modal-overlay">
    <div class="modal-box">
      <div class="modal-icon">⚠️</div>
      <h2>¿Cancelar reservación?</h2>
      <p>
        Esta acción cancelará todos los vuelos de la reservación
        <strong>{{ reservationNumber }}</strong>.
      </p>
      <button
        class="danger-button-solid"
        @click="requestCancellation"
        :disabled="cancelLoading">
        {{ cancelLoading ? 'Enviando...' : 'Sí, cancelar reservación' }}
      </button>
      <button
        class="back-button-modal"
        @click="showCancelConfirm = false">
        Volver
      </button>
    </div>
  </div>

  <!-- Modal de éxito -->
  <div v-if="showCancelSuccess" class="modal-overlay">
    <div class="modal-box">
      <div class="modal-icon">✅</div>
      <h2>Correo enviado</h2>
      <p>
        Hemos enviado un correo al titular de la reservación con las
        instrucciones para confirmar la cancelación.
      </p>
      <button
        class="close-button-modal"
        @click="showCancelSuccess = false">
        Cerrar
      </button>
    </div>
  </div>
</template>

<script>
import axios from 'axios';

export default {
  name: 'ClientFlightReport',
  data() {
    return {
      // Datos que llegan por state desde LandingPage.vue.
      flightLegs: [],
      passengers: [],
      bookingStatus: '',
      showCancelConfirm: false,
      showCancelSuccess: false,
      cancelLoading: false,
    };
  },
  created() {
    const state = window.history.state?.flightReport;
    this.bookingStatus = state.status ?? '';

    if (state?.legs?.length > 0) {
      this.flightLegs = [...state.legs].sort((a, b) => a.sequenceNumber - b.sequenceNumber);
      this.passengers = state.passengers ?? [];
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

      const today = new Date();
      const todayDateOnly = new Date(today.getFullYear(), today.getMonth(), today.getDate());
      const departure = new Date(`${departureDate}T00:00:00`);

      const msPerDay = 1000 * 60 * 60 * 24;
      return Math.round((departure - todayDateOnly) / msPerDay);
    },
    countdownLabel() {
      if (this.bookingStatus === 'Cancelada') return 'Reservación cancelada';
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
      // El backend envia TimeOnly serializado como "HH:mm:ss"
      return value ? String(value).slice(0, 5) : '';
    },
    formatDate(value) {
      if (!value) return '';
      // El backend envia DateOnly serializado como "YYYY-MM-DD"
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

    async requestCancellation() {
      this.cancelLoading = true;
      try {
        await axios.post(`${process.env.VUE_APP_BACKEND_URL}/Booking/${this.reservationNumber}/cancellation-request`);
        this.showCancelConfirm = false;
        this.showCancelSuccess = true;
      } catch (e) {
        console.error('Error al solicitar cancelación', e);
        alert('Ocurrió un error. Por favor intenta de nuevo.');
      } finally {
        this.cancelLoading = false;
      }
    },

    /**
     * Abre una ventana nueva con el HTML del correo de itinerario ya relleno
     * con los datos reales (vuelos + pasajeros + equipaje) y usa window.print().
     */
    printItinerary() {
      const html = this.buildItineraryHtml();
      const printWindow = window.open('', '_blank');
      if (!printWindow) {
        console.error('No se pudo abrir la ventana de impresión. Revisa los bloqueadores de pop-ups.');
        return;
      }

      printWindow.document.write(html);
      printWindow.document.close();

      printWindow.onload = () => {
        printWindow.focus();
        printWindow.print();
        printWindow.close();
      };
    },

    /**
     * Construye el HTML del itinerario (basado en itineraryEmail.html)
     * reemplazando los placeholders con los datos reales de la reservacion.
     * Incluye una sección de vuelos por tramo, pasajeros y equipaje.
     */
    buildItineraryHtml() {

      return `
<!DOCTYPE html>
<html lang="es">
<head>
  <meta charset="UTF-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1.0" />
  <title>Itinerario de Viaje - Snoopy Airlines</title>
  <style>
    @media print {
      body { -webkit-print-color-adjust: exact; print-color-adjust: exact; }
    }
    body { margin:0; padding:0; background-color:#f0f4f8; font-family:'Segoe UI',Arial,sans-serif; }
  </style>
</head>
<body>
  <table width="100%" cellpadding="0" cellspacing="0" style="background-color:#f0f4f8;padding:32px 0;">
    <tr>
      <td align="center">
        <table width="600" cellpadding="0" cellspacing="0"
          style="max-width:600px;width:100%;background-color:#ffffff;border-radius:12px;overflow:hidden;box-shadow:0 4px 24px rgba(0,0,0,0.08);">

          <!-- Header -->
          <tr>
            <td style="background-color:#1a2b4a;padding:28px 40px;text-align:center;">
              <span style="color:#ffffff;font-size:22px;font-weight:700;letter-spacing:0.3px;">Snoopy Airlines</span>
              <br /><br />
              <span style="color:#ffffff;font-size:16px;font-weight:600;">Itinerario de Viaje</span>
            </td>
          </tr>

          <!-- Body -->
          <tr>
            <td style="padding:40px 48px 32px 48px;">
              <h1 style="margin:0 0 8px 0;font-size:22px;font-weight:700;color:#111827;">Tu Itinerario de Viaje</h1>
              <p style="margin:0 0 28px 0;font-size:14px;line-height:1.6;color:#4b5563;">
                A continuación encontrarás toda la información de tu vuelo, pasajeros y equipaje.
              </p>

              <!-- Número de reserva -->
              <table cellpadding="0" cellspacing="0" style="width:100%;margin-bottom:28px;">
                <tr>
                  <td style="background-color:#eff6ff;border:1px solid #bfdbfe;border-radius:8px;padding:16px;text-align:center;">
                    <p style="margin:0 0 4px 0;font-size:12px;color:#6b7280;">Número de Reserva</p>
                    <p style="margin:0;font-size:26px;font-weight:700;color:#1a2b4a;">${this.reservationNumber}</p>
                  </td>
                </tr>
              </table>

              <!-- ✈ Tramos de vuelo -->
              <table cellpadding="0" cellspacing="0" style="width:100%;margin-bottom:8px;">
                <tr><td style="padding-bottom:12px;">
                  <span style="font-size:16px;font-weight:700;color:#111827;">✈ Información del Vuelo</span>
                </td></tr>
              </table>

              ${this.flightLegs.map(leg => `
              <table cellpadding="0" cellspacing="0" style="width:100%;border:1px solid #e5e7eb;border-radius:8px;margin-bottom:16px;">
                <tr>
                  <td colspan="3" style="padding:10px 20px;background-color:#1a2b4a;border-radius:8px 8px 0 0;">
                    <span style="color:#ffffff;font-size:13px;font-weight:600;">${this.legLabel(leg)} — Tramo ${leg.sequenceNumber}</span>
                  </td>
                </tr>
                <tr>
                  <td style="padding:16px 20px;border-bottom:1px solid #e5e7eb;" width="50%">
                    <p style="margin:0 0 4px 0;font-size:11px;color:#9ca3af;">📍 Origen</p>
                    <p style="margin:0;font-size:15px;font-weight:600;color:#111827;">${leg.departureCity} (${leg.departureAirportCode})</p>
                  </td>
                  <td style="padding:16px 20px;border-bottom:1px solid #e5e7eb;border-left:1px solid #e5e7eb;" width="50%">
                    <p style="margin:0 0 4px 0;font-size:11px;color:#9ca3af;">📍 Destino</p>
                    <p style="margin:0;font-size:15px;font-weight:600;color:#111827;">${leg.arrivalCity} (${leg.arrivalAirportCode})</p>
                  </td>
                </tr>
                <tr>
                  <td style="padding:16px 20px;border-bottom:1px solid #e5e7eb;" width="33%">
                    <p style="margin:0 0 4px 0;font-size:11px;color:#9ca3af;">📅 Fecha</p>
                    <p style="margin:0;font-size:14px;font-weight:600;color:#111827;">${this.formatDate(leg.departureDate)}</p>
                  </td>
                  <td style="padding:16px 20px;border-bottom:1px solid #e5e7eb;border-left:1px solid #e5e7eb;" width="33%">
                    <p style="margin:0 0 4px 0;font-size:11px;color:#9ca3af;">🕐 Salida</p>
                    <p style="margin:0;font-size:14px;font-weight:600;color:#111827;">${this.formatTime(leg.departureTime)}</p>
                  </td>
                  <td style="padding:16px 20px;border-bottom:1px solid #e5e7eb;border-left:1px solid #e5e7eb;" width="33%">
                    <p style="margin:0 0 4px 0;font-size:11px;color:#9ca3af;">🕐 Llegada</p>
                    <p style="margin:0;font-size:14px;font-weight:600;color:#111827;">${this.formatTime(leg.arrivalTime)}</p>
                  </td>
                </tr>
                <tr>
                  <td colspan="3" style="padding:14px 20px;">
                    <span style="font-size:13px;color:#4b5563;"><strong>Aeronave:</strong> ${leg.airplaneModel}</span>
                    <span style="font-size:13px;color:#4b5563;margin-left:16px;">|</span>
                    <span style="font-size:13px;color:#4b5563;margin-left:16px;"><strong>Duración:</strong> ${this.formatDuration(leg.durationMinutes)}</span>
                  </td>
                </tr>
              </table>
              `).join('')}

              <!-- 👤 Pasajeros -->
              <table cellpadding="0" cellspacing="0" style="width:100%;margin-bottom:8px;margin-top:8px;">
                <tr><td style="padding-bottom:12px;">
                  <span style="font-size:16px;font-weight:700;color:#111827;">👤 Información del Pasajero</span>
                </td></tr>
              </table>

              ${this.passengers.map(p => `
              <table cellpadding="0" cellspacing="0" style="width:100%;border:1px solid #e5e7eb;border-radius:8px;margin-bottom:12px;">
                <tr>
                  <td style="padding:16px 20px;border-bottom:1px solid #e5e7eb;" width="50%">
                    <p style="margin:0 0 4px 0;font-size:11px;color:#9ca3af;">Nombre completo</p>
                    <p style="margin:0;font-size:14px;font-weight:600;color:#111827;">${p.firstName} ${p.lastName}</p>
                  </td>
                  <td style="padding:16px 20px;border-bottom:1px solid #e5e7eb;border-left:1px solid #e5e7eb;" width="50%">
                    <p style="margin:0 0 4px 0;font-size:11px;color:#9ca3af;">Género</p>
                    <p style="margin:0;font-size:14px;font-weight:600;color:#111827;">${p.gender}</p>
                  </td>
                </tr>
                <tr>
                  <td style="padding:16px 20px;" width="50%">
                    <p style="margin:0 0 4px 0;font-size:11px;color:#9ca3af;">Fecha de nacimiento</p>
                    <p style="margin:0;font-size:14px;font-weight:600;color:#111827;">${p.birthDay} ${p.birthMonth} ${p.birthYear}</p>
                  </td>
                  <td style="padding:16px 20px;border-left:1px solid #e5e7eb;" width="50%">
                    <p style="margin:0 0 4px 0;font-size:11px;color:#9ca3af;">Nacionalidad</p>
                    <p style="margin:0;font-size:14px;font-weight:600;color:#111827;">${p.nationality}</p>
                  </td>
                </tr>
              </table>
              `).join('')}

              <!-- 🧳 Equipaje -->
              <table cellpadding="0" cellspacing="0" style="width:100%;margin-bottom:8px;margin-top:24px;">
                <tr><td style="padding-bottom:12px;">
                  <span style="font-size:16px;font-weight:700;color:#111827;">🧳 Equipaje</span>
                </td></tr>
              </table>

              ${this.passengers.map(p => `
              <table cellpadding="0" cellspacing="0" style="width:100%;border:1px solid #e5e7eb;border-radius:8px;margin-bottom:12px;">
                <tr>
                  <td colspan="2" style="padding:12px 20px;background-color:#f8fafc;border-bottom:1px solid #e5e7eb;">
                    <p style="margin:0;font-size:13px;font-weight:600;color:#1a2b4a;">👤 ${p.firstName} ${p.lastName}</p>
                  </td>
                </tr>
                <tr>
                  <td style="padding:16px 20px;" width="50%">
                    <p style="margin:0 0 4px 0;font-size:11px;color:#9ca3af;">🎒 Equipaje de mano</p>
                    <p style="margin:0;font-size:14px;font-weight:600;color:#111827;">${p.carryOnLuggage} pieza${p.carryOnLuggage === 1 ? '' : 's'}</p>
                  </td>
                  <td style="padding:16px 20px;border-left:1px solid #e5e7eb;" width="50%">
                    <p style="margin:0 0 4px 0;font-size:11px;color:#9ca3af;">🧳 Equipaje de bodega</p>
                    <p style="margin:0;font-size:14px;font-weight:600;color:#111827;">${p.checkedLuggage} pieza${p.checkedLuggage === 1 ? '' : 's'}</p>
                  </td>
                </tr>
              </table>
              `).join('')}

            </td>
          </tr>

          <!-- Footer -->
          <tr>
            <td style="background-color:#f8fafc;padding:20px 48px;text-align:center;border-top:1px solid #e5e7eb;">
              <p style="margin:0 0 4px 0;font-size:12px;color:#9ca3af;">
                Este documento fue generado automáticamente. Número de reserva: ${this.reservationNumber}
              </p>
              <p style="margin:0;font-size:12px;color:#9ca3af;">© 2026 Snoopy Airlines. Todos los derechos reservados.</p>
            </td>
          </tr>

        </table>
      </td>
    </tr>
  </table>
</body>
</html>`;
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

.cancelled-card {
  background: rgba(214, 40, 40, 0.15);
  border: 1px solid rgba(214, 40, 40, 0.3);
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

.pdf-button:disabled {
  opacity: 0.6;
  cursor: not-allowed;
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

.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.45);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.modal-box {
  background: white;
  border-radius: 20px;
  padding: 40px 36px;
  max-width: 420px;
  width: 90%;
  text-align: center;
  box-shadow: 0 8px 40px rgba(0, 0, 0, 0.18);
}

.modal-box h2 {
  margin: 16px 0 8px;
  font-size: 1.3rem;
  color: #111;
}

.modal-box p {
  color: #555;
  font-size: 0.95rem;
  line-height: 1.5;
  margin-bottom: 24px;
}

.modal-icon {
  font-size: 2.5rem;
}

.danger-button-solid {
  width: 100%;
  padding: 14px;
  border: none;
  border-radius: 12px;
  background: #d62828;
  color: white;
  font-weight: 700;
  font-size: 1rem;
  cursor: pointer;
  margin-bottom: 12px;
}

.danger-button-solid:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.back-button-modal {
  width: 100%;
  padding: 14px;
  border: none;
  border-radius: 12px;
  background: #f0f0f0;
  color: #333;
  font-weight: 600;
  cursor: pointer;
}

.close-button-modal {
  width: 100%;
  padding: 14px;
  border: none;
  border-radius: 12px;
  background: #111827;
  color: white;
  font-weight: 700;
  cursor: pointer;
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