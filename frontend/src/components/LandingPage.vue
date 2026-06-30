<template>
  <div class="landing-page">
    <!-- Header -->
    <header class="header">
      <div class="header-container">
        <div class="logo">
          <img src="../assets/logoSA.png" alt="Snoopy Airlines" class="logo-img" />
          <span class="logo-text">Snoopy Airlines</span>
        </div>
        <nav class="nav-menu">
          <a href="#inicio" class="nav-link">Inicio</a>
          <a href="#destinos" class="nav-link">Destinos</a>
          <a href="#reserva" class="nav-link">Mi Reserva</a>
          <a href="#ventajas" class="nav-link">¿Por qué nosotros?</a>
        </nav>
        <div class="header-actions">
          <router-link to="/login" class="btn-login">Iniciar Sesión</router-link>
        </div>
      </div>
    </header>

    <!-- Hero Section -->
    <section id="inicio" class="hero">
      <div class="hero-background">
        <div class="hero-overlay"></div>
      </div>
      <div class="hero-content">
        <h1 class="hero-title">Descubre el mundo con <span class="brand-name">Snoopy Airlines</span></h1>
        <p class="hero-subtitle">Vuelos seguros, cómodos y al mejor precio</p>
        
        <!-- Flight Search -->
        <div class="search-box">
          <div class="search-tabs">
            <button :class="['tab-btn', { active: tripType === 'ida' }]" @click="setTripType('ida')">Ida</button>
          </div>
          <form class="search-form" @submit.prevent="searchFlights">
            <div class="search-row">
              <div class="search-field">
                <label>Origen</label>
                <select v-model="search.origin" required class="search-select">
                  <option value="" disabled>Seleccionar origen</option>
                  <option v-for="city in originCities" :key="city.code" :value="city.code">
                    {{ city.name }} ({{ city.code }})
                  </option>
                </select>
                <div class="checkbox-field">
                  <label class="checkbox-label">
                    <input type="checkbox" v-model="search.includeStopovers" />
                    Incluir vuelos con escalas
                  </label>
                </div>
              </div>
              <div class="search-field">
                <label>Destino</label>
                <select v-model="search.destination" required class="search-select">
                  <option value="" disabled>Seleccionar destino</option>
                  <option v-for="city in destinationCities" :key="city.code" :value="city.code">
                    {{ city.name }} ({{ city.code }})
                  </option>
                </select>
              </div>
              <div class="search-field">
                <label>Fecha de salida</label>
                <input type="date" v-model="search.departureDate" class="search-input" />
              </div>
              <div class="search-field">
                <label>Pasajeros</label>
                <select v-model="search.passengers" class="search-select">
                  <option value="1">1 Pasajero</option>
                  <option value="2">2 Pasajeros</option>
                  <option value="3">3 Pasajeros</option>
                  <option value="4">4 Pasajeros</option>
                  <option value="5">5 Pasajeros</option>
                </select>
              </div>
            </div>
            <button type="submit" class="btn-search" :disabled="loading">
              {{ loading ? 'Buscando...' : 'Buscar Vuelos' }}
            </button>
          </form>

          <!-- Search Results -->
          <div v-if="searchPerformed && searchResults.length > 0" class="search-results">
            <!-- Resultados de vuelos -->
            <div>
              <h3>Vuelos disponibles</h3>
              <div class="flights-grid">
                <div class="flight-card" v-for="flight in searchResults" :key="flight.id">
                  <div class="flight-header">
                    <div class="flight-route">
                      <span class="airport-code">{{ airportCode(flight.departureAirport, search.origin) }}</span>
                      <svg class="flight-arrow" viewBox="0 0 24 24" fill="none" stroke="currentColor">
                        <path d="M5 12h14M12 5l7 7-7 7"></path>
                      </svg>
                      <span class="airport-code">{{ airportCode(flight.arrivalAirport, search.destination) }}</span>
                    </div>
                  </div>
                  <div class="flight-details">
                    <div class="flight-time">
                      <strong>{{ formatTime(flight.departureTime) }}</strong>
                      <span class="flight-duration">{{ formatDurationMinutes(flight.durationMinutes) }}</span>
                      <strong>{{ formatTime(flight.arrivalTime) }}</strong>
                    </div>
                    <div class="flight-date">
                      {{ formatFlightDate(flight.departureTime) }}
                    </div>
                    <div v-if="flight.legs.length > 1" class="flight-stopover-info">
                      <template v-for="(leg, index) in flight.legs" :key="leg.flightGuid || index">
                        <div class="flight-leg-row">
                          <span class="leg-airline">{{ displayAirline(leg) }}</span>
                          <span class="leg-times">
                            {{ airportCode(leg.departureAirport) }} {{ formatTime(leg.departureTime) }}
                            -
                            {{ airportCode(leg.arrivalAirport) }} {{ formatTime(leg.arrivalTime) }}
                          </span>
                        </div>
                        <div v-if="index < flight.legs.length - 1" class="stopover-row">
                          <span class="stopover-label">Escala en {{ airportCode(leg.arrivalAirport) }}:</span>
                          <span>{{ stopoverDuration(flight.legs, index) }}</span>
                        </div>
                      </template>
                    </div>
                  </div>
                  <div class="flight-prices">
                    <div class="seat-options">
                      <div class="seat-option" @click="selectSeatClass(flight, 'economy')">
                        <span class="class-name">Económica</span>
                        <span class="price">${{ flight.priceEconomyClass }}</span>
      
                      </div>
                      <div class="seat-option" @click="selectSeatClass(flight, 'firstClass')">
                        <span class="class-name">Primera clase</span>
                        <span class="price">${{ flight.priceFirstClass }}</span>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>


          </div>
        </div>
      </div>
    </section>

    <!-- Mi reserva -->
    <section id="reserva" class="reservation">
      <div class="reservation-container">
        <div class="reservation-content">
          <div class="reservation-badge">
            <span>✈</span>
            <span>GESTIÓN DE RESERVACIONES</span>
          </div>

          <h2 class="reservation-title">Consulta tu reservación</h2>

          <p class="reservation-description">
            Ingresa tu número de reservación y apellidos para ver los detalles de tu viaje,
            gestionar equipaje o imprimir tu itinerario.
          </p>
        </div>

        <div class="reservation-card">
          <form @submit.prevent="searchReservation" class="reservation-form">
            <div class="form-group">
              <label>Número de reservación</label>
              <input
                v-model="formData.confirmationNumber"
                type="text"
                :class="['reservation-input', { 'input-error': reservationError }]"
                placeholder="AA0A00AA0AA0"
              />
            </div>

            <div class="form-group">
              <label>Apellidos del titular</label>
              <input
                v-model="formData.lastNames"
                type="text"
                placeholder="Ej. García Rodríguez"
                :class="['reservation-input', { 'input-error': reservationError }]"
              />
            </div>

            <button type="submit" class="reservation-button" :disabled="reservationLoading">
              {{ reservationLoading ? 'Buscando...' : 'Buscar reservación' }}
            </button>
            
            <p v-if="reservationError" class="reservation-error">
              {{ reservationError }}
            </p>

            <p class="reservation-help">
              Encuentra tu número de reservación en el correo de confirmación de compra e itinerario.
            </p>
          </form>
        </div>
      </div>
    </section>

    <!-- Advantages -->
    <section id="ventajas" class="advantages">
      <div class="section-container">
        <h2 class="section-title">¿Por qué elegir Snoopy Airlines?</h2>
        <p class="section-subtitle">Tu experiencia de viaje redefine los estándares</p>
        <div class="advantages-grid">
          <div class="advantage-card" v-for="adv in advantages" :key="adv.id">
            <div class="advantage-icon">
              <svg xmlns="http://www.w3.org/2000/svg" width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"></path>
                <polyline points="22 4 12 14.01 9 11.01"></polyline>
              </svg>
            </div>
            <h3>{{ adv.title }}</h3>
            <p>{{ adv.description }}</p>
          </div>
        </div>
      </div>
    </section>

    <!-- Footer -->
    <footer class="footer">
      <div class="footer-container">
        <div class="footer-main">
          <div class="footer-brand">
            <img src="../assets/logoSA.png" alt="Snoopy Airlines" class="footer-logo" />
            <p class="footer-description">
              Conectando destinos con seguridad, comodidad y el mejor servicio desde 2026.
            </p>
            <div class="social-links">
              <a href="#" class="social-link" title="Facebook">Facebook</a>
              <a href="#" class="social-link" title="LinkedIn">LinkedIn</a>
              <a href="#" class="social-link" title="Instagram">Instagram</a>
              <a href="#" class="social-link" title="YouTube">YouTube</a>
            </div>
          </div>
          <div class="footer-links">
            <div class="footer-column">
              <h4>Compañía</h4>
              <a href="#">Sobre nosotros</a>
              <a href="#">Careers</a>
              <a href="#">Prensa</a>
              <a href="#">Blog</a>
            </div>
            <div class="footer-column">
              <h4>Soporte</h4>
              <a href="#">Centro de ayuda</a>
              <a href="#">Contacto</a>
              <a href="#">Preguntas frecuentes</a>
              <a href="#">Estado de vuelo</a>
            </div>
            <div class="footer-column">
              <h4>Legal</h4>
              <a href="#">Términos y condiciones</a>
              <a href="#">Política de privacidad</a>
              <a href="#">Política de cookies</a>
              <a href="#">Condiciones de tarifa</a>
            </div>
          </div>
        </div>
        <div class="footer-bottom">
          <p>&copy; 2026 Snoopy Airlines. Todos los derechos reservados.</p>
        </div>
      </div>
    </footer>
  </div>
</template>

<script>
import axios from 'axios';

const BACKEND_API_BASE = 'http://localhost:5235';
export default {
  name: 'LandingPage',
  data() {
    return {
      formData: {
                    confirmationNumber: '',
                    lastNames: ''
                },
      tripType: 'ida',
      search: {
        origin: '',
        destination: '',
        departureDate: '',
        passengers: '1',
        includeStopovers: false,
      },
      originCities: [],
      destinationCities: [],
      searchResults: [],
      loading: false,
      reservationLoading: false,
      reservationError: '',
      searchPerformed: false,
      advantages: [
        {
          id: 1,
          title: 'Seguridad Garantizada',
          description: 'Los más altos estándares de seguridad aérea con pilotos altamente capacitados'
        },
        {
          id: 2,
          title: 'Comodidad a Bordo',
          description: 'Asientos ergonómicos, entretenimiento y WiFi en todos nuestros vuelos'
        },
        {
          id: 3,
          title: 'Equipaje Incluido',
          description: 'Maleta de bodega y equipaje de mano incluidos en todas las tarifas'
        },
        {
          id: 4,
          title: 'Programa de Millas',
          description: 'Acumula puntos canjeables por vuelos gratis y upgrades'
        },
        {
          id: 5,
          title: 'Flexibilidad',
          description: 'Cambios y cancelaciones sin penalidad hasta 24h antes del vuelo'
        },
        {
          id: 6,
          title: 'Check-in Digital',
          description: 'Check-in online, tarjeta de embarque digital y seguimiento de equipaje'
        }
      ]
    };
  },
  mounted() {
    this.loadAirports();
  },
  methods: {
    /**
     * Carga todos los aeropuertos disponibles desde el Backend
     * y los asigna a las listas de origen y destino.
     */
    async loadAirports() {
      try {
const response = await fetch(`${BACKEND_API_BASE}/airport`);
        if (!response.ok) {
          console.error('Error al cargar aeropuertos:', response.statusText);
          return;
        }

        const airports = await response.json();

        // Convertir formato API { code, name, city } a formato de la app { code, name }
        const formattedCities = airports.map(airport => ({
          code: airport.code,
          name: airport.name
        }));

        this.originCities = formattedCities;
        this.destinationCities = formattedCities;
      } catch (error) {
        console.error('Error cargando aeropuertos:', error);
      }
    },
    setTripType(type) {
      this.tripType = type;
    },

    /**
     * Construye los query params para el Backend.
     * El rango cubre todo el día solicitado (T00:00 → T23:59) para que la
     * búsqueda por día de la semana funcione correctamente.
     */
    buildExternalParams(origin, destination, date, passengers) {
      return new URLSearchParams({
        origin,
        detination: destination,
        earliestDeparture: `${date}T00:00`,
        latestDeparture:   `${date}T23:59`,
        quantityOfPassengers: passengers,
        includeStopovers: this.search.includeStopovers ? 'true' : 'false',
        apiKey: BACKEND_API_BASE,
      });
    },

    /**
     * Normaliza un vuelo del backend al shape que usa el template.
     *
     * Mapeo de campos:
     *   routes[].flightGuid -> id
     *   routeId          → booking routeId
     *   touristPrice      → priceEconomyClass
     *   firstClassPrice   → priceFirstClass
     *   duration "hh:mm" o "hh-mm" → durationMinutes (int)
     *
     * departureTime y arrivalTime ya vienen en formato ISO, no cambian.
     */
    mapFlight(flight) {
      const legs = this.normalizeFlightLegs(flight);
      const firstLeg = legs[0] || {};
      const lastLeg = legs[legs.length - 1] || firstLeg;
      const routes = this.normalizeFlightRoutes(flight);
      const durationMinutes = this.itineraryDurationMinutes(legs, flight);

      return {
        ...flight,
        id: this.flightKey(routes, flight),
        legs,
        routes,
        departureTime: firstLeg.departureTime || this.fieldValue(flight, 'departureTime', 'DepartureTime'),
        arrivalTime: lastLeg.arrivalTime || this.fieldValue(flight, 'arrivalTime', 'ArrivalTime'),
        departureAirport: firstLeg.departureAirport || this.fieldValue(flight, 'departureAirport', 'DepartureAirport'),
        arrivalAirport: lastLeg.arrivalAirport || this.fieldValue(flight, 'arrivalAirport', 'ArrivalAirport'),
        priceEconomyClass: this.sumLegValue(legs, 'touristPrice', this.fieldValue(flight, 'touristPrice', 'TouristPrice')),
        priceFirstClass: this.sumLegValue(legs, 'firstClassPrice', this.fieldValue(flight, 'firstClassPrice', 'FirstClassPrice')),
        durationMinutes,
      };
    },

    fieldValue(source, camelCaseKey, pascalCaseKey) {
      return source?.[camelCaseKey] ?? source?.[pascalCaseKey];
    },

    normalizeFlightLegs(flight) {
      const rawLegs = this.fieldValue(flight, 'flights', 'Flights');
      if (Array.isArray(rawLegs) && rawLegs.length > 0) {
        return rawLegs
          .map((leg, index) => this.normalizeFlightLeg(leg, index))
          .filter(leg => leg.flightGuid || leg.departureTime || leg.arrivalTime);
      }

      return this.normalizeLegacyFlightLegs(flight);
    },

    normalizeLegacyFlightLegs(flight) {
      const routes = this.fieldValue(flight, 'routes', 'Routes') || [];
      const departureAirport = this.normalizeAirport(this.fieldValue(flight, 'departureAirport', 'DepartureAirport'));
      const arrivalAirport = this.normalizeAirport(this.fieldValue(flight, 'arrivalAirport', 'ArrivalAirport'));

      return [
        {
          sequenceNumber: 1,
          routeId: this.nullableNumber(this.fieldValue(flight, 'routeId', 'RouteId')),
          flightGuid: this.fieldValue(routes[0], 'flightGuid', 'FlightGuid'),
          intendedDate: this.dateOnly(this.fieldValue(routes[0], 'intendedDate', 'IntendedDate') || this.fieldValue(flight, 'departureTime', 'DepartureTime')),
          departureTime: this.fieldValue(flight, 'departureTime', 'DepartureTime'),
          arrivalTime: this.fieldValue(flight, 'arrivalTime', 'ArrivalTime'),
          durationMinutes: this.durationToMinutes(this.fieldValue(flight, 'duration', 'Duration')),
          departureAirport,
          arrivalAirport,
          touristPrice: Number(this.fieldValue(flight, 'touristPrice', 'TouristPrice')) || 0,
          firstClassPrice: Number(this.fieldValue(flight, 'firstClassPrice', 'FirstClassPrice')) || 0,
          isExternal: false,
          airline: null,
        }
      ];
    },

    normalizeFlightLeg(leg, index) {
      const departureTime = this.fieldValue(leg, 'departureTime', 'DepartureTime');
      const arrivalTime = this.fieldValue(leg, 'arrivalTime', 'ArrivalTime');
      const durationMinutes = Number(this.fieldValue(leg, 'durationMinutes', 'DurationMinutes'));

      return {
        sequenceNumber: Number(this.fieldValue(leg, 'sequenceNumber', 'SequenceNumber')) || index + 1,
        routeId: this.nullableNumber(this.fieldValue(leg, 'routeId', 'RouteId')),
        flightGuid: this.fieldValue(leg, 'flightGuid', 'FlightGuid'),
        intendedDate: this.dateOnly(this.fieldValue(leg, 'intendedDate', 'IntendedDate') || departureTime),
        departureTime,
        arrivalTime,
        durationMinutes: Number.isFinite(durationMinutes)
          ? durationMinutes
          : this.minutesBetween(departureTime, arrivalTime),
        departureAirport: this.normalizeAirport(this.fieldValue(leg, 'departureAirport', 'DepartureAirport')),
        arrivalAirport: this.normalizeAirport(this.fieldValue(leg, 'arrivalAirport', 'ArrivalAirport')),
        touristPrice: Number(this.fieldValue(leg, 'touristPrice', 'TouristPrice')) || 0,
        firstClassPrice: Number(this.fieldValue(leg, 'firstClassPrice', 'FirstClassPrice')) || 0,
        carryOnPrice: Number(this.fieldValue(leg, 'carryOnPrice', 'CarryOnPrice')) || 0,
        checkedPrice: Number(this.fieldValue(leg, 'checkedPrice', 'CheckedPrice')) || 0,
        isExternal: this.parseBoolean(this.fieldValue(leg, 'isExternal', 'IsExternal')),
        airline: this.fieldValue(leg, 'airline', 'Airline') || null,
      };
    },

    normalizeFlightRoutes(flight) {
      const legs = Array.isArray(flight?.legs) ? flight.legs : this.normalizeFlightLegs(flight);
      if (legs.length > 0) {
        return legs
          .map((leg, index) => ({
            sequenceNumber: Number(leg.sequenceNumber) || index + 1,
            routeId: this.nullableNumber(leg.routeId),
            flightGuid: leg.flightGuid,
            intendedDate: this.dateOnly(leg.intendedDate || leg.departureTime),
          }))
          .filter(route => route.flightGuid);
      }

      const routeId = Number(this.fieldValue(flight, 'routeId', 'RouteId'));
      const departureTime = this.fieldValue(flight, 'departureTime', 'DepartureTime');
      if (!routeId || !departureTime) {
        return [];
      }

      return [
        {
          sequenceNumber: 1,
          routeId,
          intendedDate: this.dateOnly(departureTime),
        }
      ];
    },

    flightKey(routes, flight) {
      const legGuids = routes
        .map(route => route.flightGuid)
        .filter(Boolean);

      if (legGuids.length > 0) {
        return legGuids.join(':');
      }

      const departureTime = this.fieldValue(flight, 'departureTime', 'DepartureTime') ?? '';
      const routeIds = routes.map(route => route.routeId).join(':');
      return `${routeIds}:${departureTime}`;
    },

    dateOnly(value) {
      return value ? String(value).slice(0, 10) : '';
    },

    nullableNumber(value) {
      if (value === null || value === undefined || value === '') {
        return null;
      }

      const number = Number(value);
      return Number.isFinite(number) ? number : null;
    },

    normalizeAirport(airport) {
      if (!airport) {
        return null;
      }

      return {
        code: this.fieldValue(airport, 'code', 'Code'),
        name: this.fieldValue(airport, 'name', 'Name'),
        city: this.fieldValue(airport, 'city', 'City'),
      };
    },

    parseBoolean(value) {
      return value === true || String(value).toLowerCase() === 'true';
    },

    durationToMinutes(duration) {
      const [hours, minutes] = String(duration || '')
        .split(/[:-]/)
        .map(Number);

      const validHours = Number.isFinite(hours) ? hours : 0;
      const validMinutes = Number.isFinite(minutes) ? minutes : 0;
      return validHours * 60 + validMinutes;
    },

    minutesBetween(start, end) {
      const startDate = new Date(start);
      const endDate = new Date(end);

      if (Number.isNaN(startDate.getTime()) || Number.isNaN(endDate.getTime())) {
        return 0;
      }

      return Math.max(0, Math.round((endDate - startDate) / 60000));
    },

    itineraryDurationMinutes(legs, flight) {
      const firstLeg = legs[0];
      const lastLeg = legs[legs.length - 1];

      if (firstLeg?.departureTime && lastLeg?.arrivalTime) {
        return this.minutesBetween(firstLeg.departureTime, lastLeg.arrivalTime);
      }

      return this.durationToMinutes(this.fieldValue(flight, 'duration', 'Duration'));
    },

    sumLegValue(legs, key, fallback) {
      if (!legs.length) {
        return fallback;
      }

      return legs.reduce((total, leg) => total + (Number(leg[key]) || 0), 0);
    },

    airportCode(airport, fallback = '') {
      return airport?.code || airport?.Code || fallback;
    },

    formatTime(value) {
      return value ? String(value).substring(11, 16) : '--:--';
    },

    formatFlightDate(value) {
      const date = new Date(value);
      if (Number.isNaN(date.getTime())) {
        return '';
      }

      return date.toLocaleDateString('es-ES', { weekday: 'short', month: 'short', day: 'numeric' });
    },

    formatDurationMinutes(minutes) {
      const totalMinutes = Number(minutes);
      if (!Number.isFinite(totalMinutes) || totalMinutes <= 0) {
        return '--';
      }

      const hours = Math.floor(totalMinutes / 60);
      const remainingMinutes = totalMinutes % 60;

      if (hours === 0) {
        return `${remainingMinutes}min`;
      }

      return remainingMinutes === 0
        ? `${hours}h`
        : `${hours}h ${remainingMinutes}min`;
    },

    stopoverDuration(legs, index) {
      const currentLeg = legs[index];
      const nextLeg = legs[index + 1];

      return this.formatDurationMinutes(this.minutesBetween(currentLeg?.arrivalTime, nextLeg?.departureTime));
    },

    displayAirline(leg) {
      return leg.airline || 'Snoopy Airlines';
    },

    /**
     * Hace el fetch al Backend y devuelve los vuelos ya normalizados.
     */
    fetchFlights(origin, destination, date, passengers) {
      const params = this.buildExternalParams(origin, destination, date, passengers);
      return fetch(`${BACKEND_API_BASE}/Flight/search?${params}`)
        .then(res => {
          if (!res.ok) throw new Error(`Error buscando vuelos ${origin} → ${destination}`);
          return res.json();
        })
        .then(data => (data.flights ?? data.Flights ?? []).map(flight => this.mapFlight(flight)));
    },

    searchFlights() {
      if (!this.search.origin || !this.search.destination || !this.search.departureDate) {
        alert('Por favor completa todos los campos requeridos');
        return;
      }

      this.loading = true;
      this.searchPerformed = true;

      this.fetchFlights(
        this.search.origin,
        this.search.destination,
        this.search.departureDate,
        this.search.passengers
      )
        .then(data => {
          this.searchResults = data;
          if (data.length === 0) {
            alert('No se encontraron vuelos para esa búsqueda');
          }
        })
        .catch(error => {
          console.error('Error:', error);
          alert('Error al buscar vuelos: ' + error.message);
        })
        .finally(() => {
          this.loading = false;
        });
    },

    selectSeatClass(flight, seatClass) {
      const routes = this.normalizeFlightRoutes(flight);
      if (routes.length === 0) {
        alert('No se pudieron determinar los tramos del vuelo seleccionado');
        return;
      }

      this.$router.push({path: '/booking', query: {
        routes: JSON.stringify(routes),
        seatClass: seatClass,
        passengersCount: this.search.passengers
      }})
    },
    // SEARCH RESERVATION

    searchReservation() {
      this.reservationLoading = true;
      this.reservationError = '';

      axios
          .get(`${process.env.VUE_APP_BACKEND_URL}/Flight/report/confirmation`, {
            params: {
              confirmationNumber: this.formData.confirmationNumber,
              lastNames: this.formData.lastNames
            },
          })
          .then((response) => {
              this.$router.push({
                path: '/client-flight-report',
                state: { flightReport : response.data}
              });
          })
          .catch((error) => {
              const status = error.response?.status;
              const backendMessage = error.response?.data?.message;

              if (status === 400) {
                  this.reservationError = backendMessage || 'Por favor verifica los datos ingresados.';
              } else if (status === 404) {
                  this.reservationError = backendMessage || 'No se encontró ninguna reservación con esos datos.';
              } else {
                  this.reservationError = 'Error al buscar reservación. Por favor intenta de nuevo más tarde.';
              }
              console.error(error);
          })
          .finally(() => {
              this.reservationLoading = false;
           });
    }
  } 
};


</script>

<style scoped>
/* Base Styles */
* {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
}

.landing-page {
  font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
  color: #333;
  line-height: 1.6;
}

/* Header */
.header {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  background: rgba(255, 255, 255, 0.95);
  backdrop-filter: blur(10px);
  z-index: 1000;
  box-shadow: 0 2px 20px rgba(0, 0, 0, 0.1);
}

.header-container {
  max-width: 1400px;
  margin: 0 auto;
  padding: 15px 40px;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.logo {
  display: flex;
  align-items: center;
  gap: 12px;
}

.logo-img {
  width: 45px;
  height: 45px;
  border-radius: 50%;
}

.logo-text {
  font-size: 1.4rem;
  font-weight: 700;
  color: #2c3e50;
}

.nav-menu {
  display: flex;
  gap: 45px;
  margin-right: 40px;
}

.nav-link {
  text-decoration: none;
  color: #555;
  font-weight: 500;
  font-size: 1rem;
  transition: color 0.3s;
  position: relative;
}

.nav-link:hover {
  color: #3498db;
}

.nav-link::after {
  content: '';
  position: absolute;
  bottom: -5px;
  left: 0;
  width: 0;
  height: 2px;
  background: #3498db;
  transition: width 0.3s;
}

.nav-link:hover::after {
  width: 100%;
}

.header-actions {
  display: flex;
  gap: 15px;
}

.btn-login {
  padding: 10px 25px;
  background: #0056b3;
  color: white;
  text-decoration: none;
  border-radius: 4px;
  font-weight: 600;
  transition: transform 0.3s, box-shadow 0.3s;
}

.btn-login:hover {
  transform: translateY(-2px);
  box-shadow: 0 5px 20px rgba(0, 86, 179, 0.4);
}

/* Hero Section */
.hero {
  position: relative;
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 120px 40px 80px;
  overflow: hidden;
}

.hero-background {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: linear-gradient(180deg, #0a1628 0%, #1a3a5c 100%);
}

.hero-overlay {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: url('https://images.unsplash.com/photo-1436491865332-7a61a109cc05?w=1920&h=1080&fit=crop') center/cover;
  opacity: 0.15;
}

.hero-content {
  position: relative;
  z-index: 2;
  max-width: 1000px;
  text-align: center;
}

.hero-title {
  font-size: 3.5rem;
  font-weight: 800;
  color: white;
  margin-bottom: 20px;
  line-height: 1.2;
}

.brand-name {
  color: #4da6ff;
}

.hero-subtitle {
  font-size: 1.3rem;
  color: rgba(255, 255, 255, 0.85);
  margin-bottom: 50px;
}

/* Search Box */
.search-box {
  background: white;
  border-radius: 20px;
  padding: 30px;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
}

.search-tabs {
  display: flex;
  gap: 10px;
  margin-bottom: 25px;
}

.tab-btn {
  padding: 12px 30px;
  border: 2px solid #e0e0e0;
  background: transparent;
  border-radius: 30px;
  font-size: 1rem;
  font-weight: 600;
  color: #666;
  cursor: pointer;
  transition: all 0.3s;
}

.tab-btn.active {
  background: #0056b3;
  color: white;
  border-color: transparent;
}

.search-form {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.search-row {
  display: grid;
  grid-template-columns: repeat(5, 1fr);
  gap: 15px;
}


.leg-title {
  font-weight: 700;
  color: #2c3e50;
}

.btn-add-leg,
.btn-remove-leg {
  padding: 12px 18px;
  background: #0056b3;
  color: white;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  font-weight: 600;
}

.btn-remove-leg {
  background: #d32f2f;
}

.passengers-field {
  max-width: 220px;
}

.search-field {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.search-field label {
  font-size: 0.85rem;
  font-weight: 600;
  color: #555;
}

.input-with-icon {
  position: relative;
  display: flex;
  align-items: center;
}

.search-input,
.search-select {
  width: 100%;
  padding: 14px 16px;
  border: 2px solid #e8e8e8;
  border-radius: 12px;
  font-size: 1rem;
  transition: border-color 0.3s;
  background: white;
}

.search-input:focus,
.search-select:focus {
  outline: none;
  border-color: #0056b3;
}

.checkbox-field {
  margin-top: 8px;
}

.checkbox-label {
  display: flex;
  align-items: center;
  gap: 10px;
  font-weight: 600;
  color: #555;
  font-size: 0.95rem;
}

input[type="checkbox"] {
  width: 16px;
  height: 16px;
  accent-color: #0056b3;
}

.btn-search {
  padding: 16px 40px;
  background: #0056b3;
  color: white;
  border: none;
  border-radius: 4px;
  font-size: 1.1rem;
  font-weight: 600;
  cursor: pointer;
  transition: transform 0.3s, box-shadow 0.3s;
  align-self: center;
}

.btn-search:hover {
  transform: translateY(-2px);
  box-shadow: 0 10px 30px rgba(0, 86, 179, 0.4);
}

.btn-search:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

/* Search Results */
.search-results {
  margin-top: 50px;
  padding-top: 40px;
  border-top: 2px solid #f0f0f0;
}

.search-results h3 {
  font-size: 1.8rem;
  font-weight: 700;
  margin-bottom: 30px;
  color: #003d7a;
}

.flights-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
  gap: 20px;
}

.flight-card {
  background: white;
  border: 1px solid #e0e0e0;
  border-radius: 12px;
  padding: 20px;
  transition: all 0.3s ease;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
}

.flight-card:hover {
  box-shadow: 0 8px 24px rgba(0, 86, 179, 0.15);
  border-color: #0056b3;
  transform: translateY(-4px);
}

.flight-header {
  margin-bottom: 15px;
  border-bottom: 1px solid #f0f0f0;
  padding-bottom: 15px;
}

.flight-route {
  display: flex;
  align-items: center;
  justify-content: space-between;
  font-weight: 600;
  gap: 10px;
}

.airport-code {
  font-size: 1.3rem;
  color: #003d7a;
  min-width: 60px;
  text-align: center;
}

.flight-arrow {
  width: 24px;
  height: 24px;
  color: #0056b3;
  flex-shrink: 0;
}

.flight-details {
  margin-bottom: 15px;
  padding: 15px 0;
  border-bottom: 1px solid #f0f0f0;
}

.flight-time {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 8px;
  font-size: 1.1rem;
  color: #003d7a;
}

.flight-time strong {
  font-weight: 700;
}

.flight-duration {
  font-size: 0.9rem;
  color: #666;
  font-weight: normal;
}

.flight-stopover-info {
  display: grid;
  gap: 6px;
  margin-top: 12px;
  padding: 12px;
  background: #f5f9ff;
  border-radius: 10px;
  border: 1px solid #dce8f9;
}

.flight-leg-row {
  display: grid;
  grid-template-columns: minmax(110px, 0.9fr) minmax(0, 1.4fr);
  gap: 12px;
  align-items: center;
  font-size: 0.9rem;
  color: #2c3e50;
}

.leg-airline {
  color: #003d7a;
  font-weight: 700;
  min-width: 0;
}

.leg-times {
  text-align: right;
  min-width: 0;
}

.stopover-row {
  display: flex;
  justify-content: space-between;
  gap: 10px;
  font-size: 0.9rem;
  color: #2c3e50;
}

.stopover-label {
  font-weight: 600;
  color: #0056b3;
}

.price-option {
  padding: 8px 0;
}

.price-option:last-child {
  margin-bottom: 0;
}

.class-name {
  font-size: 0.9rem;
  color: #666;
  font-weight: 500;
}

.price {
  font-size: 1.1rem;
  font-weight: 700;
  color: #0056b3;
}

.btn-select-flight {
  width: 100%;
  padding: 12px 20px;
  background: linear-gradient(135deg, #0056b3, #003d7a);
  color: white;
  border: none;
  border-radius: 8px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
  font-size: 1rem;
}

.btn-select-flight:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 20px rgba(0, 86, 179, 0.3);
}

.trip-section {
  margin-bottom: 40px;
  padding: 20px;
  background: #f9f9f9;
  border-radius: 12px;
}

.trip-section h3 {
  font-size: 1.5rem;
  font-weight: 700;
  color: #0056b3;
  margin-bottom: 20px;
}

.no-results {
  text-align: center;
  padding: 20px;
  color: #666;
  font-size: 1rem;
}

/* Section Styles */
.section-container {
  max-width: 1400px;
  margin: 0 auto;
  padding: 80px 40px;
}

.section-title {
  font-size: 2.5rem;
  font-weight: 800;
  text-align: center;
  color: #2c3e50;
  margin-bottom: 15px;
}

.section-subtitle {
  font-size: 1.2rem;
  text-align: center;
  color: #666;
  margin-bottom: 50px;
}

/* Destinations */
.destinations {
  background: #f8f9fa;
}

.destinations-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 30px;
}

.destination-card {
  background: white;
  border-radius: 20px;
  overflow: hidden;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.1);
  transition: transform 0.3s, box-shadow 0.3s;
}

.destination-card:hover {
  transform: translateY(-10px);
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.15);
}

.card-image {
  position: relative;
  height: 200px;
  overflow: hidden;
}

.card-image img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  transition: transform 0.3s;
}

.destination-card:hover .card-image img {
  transform: scale(1.1);
}

.card-badge {
  position: absolute;
  top: 15px;
  right: 15px;
  background: #0056b3;
  color: white;
  padding: 8px 16px;
  border-radius: 4px;
  font-size: 0.85rem;
  font-weight: 600;
}

.card-content {
  padding: 25px;
}

.card-content h3 {
  font-size: 1.4rem;
  font-weight: 700;
  color: #2c3e50;
  margin-bottom: 5px;
}

.card-content .country {
  color: #888;
  font-size: 0.95rem;
  margin-bottom: 15px;
}

.card-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.card-footer .price {
  font-size: 1.3rem;
  font-weight: 700;
  color: #0056b3;
}

.btn-view {
  padding: 10px 20px;
  background: transparent;
  border: 2px solid #0056b3;
  color: #0056b3;
  border-radius: 4px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s;
}

.btn-view:hover {
  background: #0056b3;
  color: white;
}

/* Offers */
.offers {
  background: linear-gradient(180deg, #0a1628 0%, #1a3a5c 100%);
}

.offers .section-title,
.offers .section-subtitle {
  color: white;
}

.offers-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 30px;
}

.offer-card {
  background: rgba(255, 255, 255, 0.1);
  backdrop-filter: blur(10px);
  border-radius: 20px;
  padding: 30px;
  border: 1px solid rgba(255, 255, 255, 0.2);
  transition: transform 0.3s;
}

.offer-card:hover {
  transform: translateY(-5px);
}

.offer-discount {
  font-size: 3rem;
  font-weight: 800;
  color: #4da6ff;
  margin-bottom: 15px;
}

.offer-content h3 {
  font-size: 1.4rem;
  color: white;
  margin-bottom: 10px;
}

.offer-content p {
  color: rgba(255, 255, 255, 0.8);
  margin-bottom: 10px;
}

.offer-route {
  display: block;
  color: #4da6ff;
  font-weight: 600;
  margin-bottom: 20px;
}

.btn-offer {
  width: 100%;
  padding: 14px;
  background: #4da6ff;
  color: #0a1628;
  border: none;
  border-radius: 4px;
  font-size: 1rem;
  font-weight: 700;
  cursor: pointer;
  transition: transform 0.3s, background 0.3s;
}

.btn-offer:hover {
  background: #6bb3ff;
  transform: scale(1.02);
}

/* Advantages */
.advantages {
  background: #f8f9fa;
}

.advantages-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 30px;
}

.advantage-card {
  background: white;
  border-radius: 20px;
  padding: 40px 30px;
  text-align: center;
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.08);
  transition: transform 0.3s;
}

.advantage-card:hover {
  transform: translateY(-5px);
}

.advantage-icon {
  width: 80px;
  height: 80px;
  margin: 0 auto 20px;
  background: #e3f2fd;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
}

.advantage-icon svg {
  color: #0056b3;
}

.advantage-card h3 {
  font-size: 1.3rem;
  color: #2c3e50;
  margin-bottom: 15px;
}

.advantage-card p {
  color: #666;
  line-height: 1.7;
}

/* Footer */
.footer {
  background: #0a1628;
  color: white;
}

.footer-container {
  max-width: 1400px;
  margin: 0 auto;
  padding: 60px 40px 30px;
}

.footer-main {
  display: flex;
  gap: 60px;
  margin-bottom: 50px;
}

.footer-brand {
  flex: 1;
  max-width: 350px;
}

.footer-logo {
  width: 80px;
  height: 80px;
  border-radius: 50%;
  margin-bottom: 20px;
}

.footer-description {
  color: rgba(255, 255, 255, 0.7);
  margin-bottom: 25px;
  line-height: 1.7;
}

.social-links {
  display: flex;
  gap: 15px;
}

.social-link {
  width: auto;
  height: auto;
  padding: 8px 16px;
  background: rgba(255, 255, 255, 0.1);
  border-radius: 4px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 0.85rem;
  text-decoration: none;
  color: rgba(255, 255, 255, 0.8);
  transition: background 0.3s, color 0.3s;
}

.social-link:hover {
  background: #0056b3;
}

.footer-links {
  flex: 2;
  display: flex;
  gap: 80px;
}

.footer-column {
  display: flex;
  flex-direction: column;
  gap: 15px;
}

.footer-column h4 {
  font-size: 1.1rem;
  font-weight: 700;
  margin-bottom: 10px;
  color: white;
}

.footer-column a {
  color: rgba(255, 255, 255, 0.7);
  text-decoration: none;
  font-size: 0.95rem;
  transition: color 0.3s;
}

.footer-column a:hover {
  color: #4da6ff;
}

.footer-bottom {
  border-top: 1px solid rgba(255, 255, 255, 0.1);
  padding-top: 25px;
  text-align: center;
}

.footer-bottom p {
  color: rgba(255, 255, 255, 0.5);
  font-size: 0.9rem;
}

.seat-option {
  display: flex;
  justify-content: space-between;
  padding: 10px;
  margin: 5px;
  border: 1px solid #ddd;
  border-radius: 8px;
  cursor: pointer;
  transition: background 0.2s;
}

.seat-option:hover {
  background: #f0f7ff;
  border-color: #4a90e2;
}

/* Reservation Section */
.reservation {
  background: linear-gradient(90deg, #0d3d9f 0%, #2f2ea8 100%);
  padding: 100px 40px;
}

.reservation-container {
  max-width: 1400px;
  margin: 0 auto;
  display: grid;
  grid-template-columns: 1.2fr 0.8fr;
  gap: 80px;
  align-items: center;
}

.reservation-content {
  color: white;
}

.reservation-badge {
  display: inline-flex;
  align-items: center;
  gap: 10px;
  color: #ffc107;
  font-weight: 700;
  font-size: 0.9rem;
  margin-bottom: 24px;
}

.reservation-title {
  font-size: 3rem;
  font-weight: 800;
  margin-bottom: 20px;
  line-height: 1.2;
}

.reservation-description {
  font-size: 1.2rem;
  color: rgba(255, 255, 255, 0.85);
  max-width: 600px;
}

.reservation-card {
  background: rgba(255, 255, 255, 0.12);
  backdrop-filter: blur(12px);
  border-radius: 20px;
  padding: 30px;
  border: 1px solid rgba(255, 255, 255, 0.15);
}

.reservation-form {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.form-group label {
  color: white;
  font-size: 0.9rem;
  font-weight: 600;
}

.reservation-input {
  width: 100%;
  padding: 14px 16px;
  border: 1px solid rgba(255, 255, 255, 0.15);
  border-radius: 12px;
  background: rgba(255, 255, 255, 0.12);
  color: white;
  font-size: 1rem;
}

.reservation-input::placeholder {
  color: rgba(255, 255, 255, 0.5);
}

.reservation-input:focus {
  outline: none;
  border-color: #ffc107;
}

.reservation-input.input-error {
  border-color: #ff6b6b;
  background: rgba(255, 107, 107, 0.1);
}

.reservation-error {
  display: flex;
  align-items: center;
  gap: 8px;
  margin: -8px 0 0;
  padding: 12px 14px;
  border-radius: 10px;
  background: rgba(255, 107, 107, 0.15);
  border: 1px solid rgba(255, 107, 107, 0.4);
  color: #ffd6d6;
  font-size: 0.9rem;
  text-align: left;
}


.reservation-button {
  text-align: center;
  text-decoration: none;
  width: 100%;
  padding: 16px;
  background: #ffc107;
  color: #1f1f1f;
  border: none;
  border-radius: 12px;
  font-weight: 700;
  font-size: 1rem;
  cursor: pointer;
  transition: 0.3s;
}

.reservation-button:hover {
  transform: translateY(-2px);
}

.reservation-help {
  color: rgba(255, 255, 255, 0.6);
  font-size: 0.85rem;
  text-align: center;
}

@media (max-width: 992px) {
  .reservation-container {
    grid-template-columns: 1fr;
    gap: 40px;
  }

  .reservation-title {
    font-size: 2.2rem;
  }

  .reservation-content {
    text-align: center;
  }

  .reservation-description {
    margin: 0 auto;
  }
}

</style>
