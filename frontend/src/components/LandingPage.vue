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
          <a href="#ofertas" class="nav-link">Ofertas</a>
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
            <button :class="['tab-btn', { active: tripType === 'ida-vuelta' }]" @click="setTripType('ida-vuelta')">Ida y Vuelta</button>
            <button :class="['tab-btn', { active: tripType === 'multiciudad' }]" @click="setTripType('multiciudad')">Multiciudad</button>
          </div>
          <form class="search-form" @submit.prevent="searchFlights">
            <div v-if="tripType !== 'multiciudad'" class="search-row">
              <div class="search-field">
                <label>Origen</label>
                <select v-model="search.origin" required class="search-select">
                  <option value="" disabled>Seleccionar origen</option>
                  <option v-for="city in originCities" :key="city.code" :value="city.code">
                    {{ city.name }} ({{ city.code }})
                  </option>
                </select>
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
              <div class="search-field" v-if="tripType === 'ida-vuelta'">
                <label>Fecha de retorno</label>
                <input type="date" v-model="search.returnDate" class="search-input" />
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
            <div v-else class="multicity-section">
              <div class="multicity-legs">
                <div class="multicity-leg" v-for="(leg, index) in search.legs" :key="index">
                  <div class="leg-title">Tramo {{ index + 1 }}</div>
                  <div class="search-field">
                    <label>Origen</label>
                    <select v-model="leg.origin" required class="search-select">
                      <option value="" disabled>Seleccionar origen</option>
                      <option v-for="city in originCities" :key="city.code" :value="city.code">
                        {{ city.name }} ({{ city.code }})
                      </option>
                    </select>
                  </div>
                  <div class="search-field">
                    <label>Destino</label>
                    <select v-model="leg.destination" required class="search-select">
                      <option value="" disabled>Seleccionar destino</option>
                      <option v-for="city in destinationCities" :key="city.code" :value="city.code">
                        {{ city.name }} ({{ city.code }})
                      </option>
                    </select>
                  </div>
                  <div class="search-field">
                    <label>Fecha</label>
                    <input type="date" v-model="leg.departureDate" required class="search-input" />
                  </div>
                  <button v-if="index > 1" type="button" class="btn-remove-leg" @click="removeLeg(index)">Eliminar tramo</button>
                </div>
              </div>
              <button type="button" class="btn-add-leg" @click="addLeg">Agregar otro tramo</button>
              <div class="search-field passengers-field">
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
          <div v-if="searchPerformed && (searchResults.length > 0 || outboundFlights.length > 0)" class="search-results">
            <!-- Resultados para Solo Ida -->
            <div v-if="tripType === 'ida'">
              <h3>Vuelos disponibles</h3>
              <div class="flights-grid">
                <div class="flight-card" v-for="flight in searchResults" :key="flight.id" @click="toggleFlight(flight.id)" style="cursor:pointer">
                  <div class="flight-header">
                    <div class="flight-route">
                      <span class="airport-code">{{ search.origin }}</span>
                      <svg class="flight-arrow" viewBox="0 0 24 24" fill="none" stroke="currentColor">
                        <path d="M5 12h14M12 5l7 7-7 7"></path>
                      </svg>
                      <span class="airport-code">{{ search.destination }}</span>
                    </div>
                  </div>
                  <div class="flight-details">
                    <div class="flight-time">
                      <strong>{{ flight.departureTime.substring(11, 16) }}</strong>
                      <span class="flight-duration">{{ flight.durationMinutes }}min</span>
                      <strong>{{ flight.arrivalTime.substring(11, 16) }}</strong>
                    </div>
                    <div class="flight-date">
                      {{ new Date(flight.departureTime).toLocaleDateString('es-ES', { weekday: 'short', month: 'short', day: 'numeric' }) }}
                    </div>
                  </div>
                  <div class="flight-prices">
                    <div v-if="selectedFlightId === flight.id" class="seat-options">
                      <div class="seat-option" @click.stop="selectSeatClass(flight, 'economy')">
                        <span class="class-name">Económica</span>
                        <span class="price">${{ flight.priceEconomyClass }}</span>
      
                      </div>
                      <div class="seat-option" @click.stop="selectSeatClass(flight, 'firstClass')">
                        <span class="class-name">Primera clase</span>
                        <span class="price">${{ flight.priceFirstClass }}</span>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <!-- Resultados para Ida y Vuelta -->
            <div v-if="tripType === 'ida-vuelta'">
              <!-- Vuelos de IDA -->
              <div class="trip-section">
                <h3>Vuelos de IDA - {{ search.origin }} → {{ search.destination }}</h3>
                <div v-if="outboundFlights.length > 0" class="flights-grid">
                  <div class="flight-card" v-for="flight in outboundFlights" :key="'out-' + flight.id" @click="toggleOutbound(flight.id)" style="cursor:pointer">
                    <div class="flight-header">
                      <div class="flight-route">
                        <span class="airport-code">{{ search.origin }}</span>
                        <svg class="flight-arrow" viewBox="0 0 24 24" fill="none" stroke="currentColor">
                          <path d="M5 12h14M12 5l7 7-7 7"></path>
                        </svg>
                        <span class="airport-code">{{ search.destination }}</span>
                      </div>
                    </div>
                    <div class="flight-details">
                      <div class="flight-time">
                        <strong>{{ flight.departureTime.substring(11, 16) }}</strong>
                        <span class="flight-duration">{{ flight.durationMinutes }}min</span>
                        <strong>{{ flight.arrivalTime.substring(11, 16) }}</strong>
                      </div>
                      <div class="flight-date">
                        {{ new Date(flight.departureTime).toLocaleDateString('es-ES', { weekday: 'short', month: 'short', day: 'numeric' }) }}
                      </div>
                    </div>
                    <div class="flight-prices">
                      <div v-if="selectedOutboundId === flight.id" class="seat-options">
                      <div class="seat-option" @click.stop="selectSeatClass(flight, 'economy')">
                        <span class="class-name">Económica</span>
                        <span class="price">${{ flight.priceEconomyClass }}</span>
      
                      </div>
                      <div class="seat-option" @click.stop="selectSeatClass(flight, 'firstClass')">
                        <span class="class-name">Primera clase</span>
                        <span class="price">${{ flight.priceFirstClass }}</span>
                      </div>
                    </div>
                    </div>
                  </div>
                </div>
                <p v-else class="no-results">No se encontraron vuelos de ida para esa fecha</p>
              </div>

              <!-- Vuelos de VUELTA -->
              <div class="trip-section">
                <h3>Vuelos de VUELTA - {{ search.destination }} → {{ search.origin }}</h3>
                <div v-if="returnFlights.length > 0" class="flights-grid">
                  <div class="flight-card" v-for="flight in returnFlights" :key="'ret-' + flight.id" @click="toggleReturn(flight.id)" style="cursor:pointer">
                    <div class="flight-header">
                      <div class="flight-route">
                        <span class="airport-code">{{ search.destination }}</span>
                        <svg class="flight-arrow" viewBox="0 0 24 24" fill="none" stroke="currentColor">
                          <path d="M5 12h14M12 5l7 7-7 7"></path>
                        </svg>
                        <span class="airport-code">{{ search.origin }}</span>
                      </div>
                    </div>
                    <div class="flight-details">
                      <div class="flight-time">
                        <strong>{{ flight.departureTime.substring(11, 16) }}</strong>
                        <span class="flight-duration">{{ flight.durationMinutes }}min</span>
                        <strong>{{ flight.arrivalTime.substring(11, 16) }}</strong>
                      </div>
                      <div class="flight-date">
                        {{ new Date(flight.departureTime).toLocaleDateString('es-ES', { weekday: 'short', month: 'short', day: 'numeric' }) }}
                      </div>
                    </div>
                    <div class="flight-prices">
                      <div v-if="selectedReturnId === flight.id" class="seat-options">
                      <div class="seat-option" @click.stop="selectSeatClass(flight, 'economy')">
                        <span class="class-name">Económica</span>
                        <span class="price">${{ flight.priceEconomyClass }}</span>
      
                      </div>
                      <div class="seat-option" @click.stop="selectSeatClass(flight, 'firstClass')">
                        <span class="class-name">Primera clase</span>
                        <span class="price">${{ flight.priceFirstClass }}</span>
                      </div>
                    </div>
                    </div>
                    <button class="btn-select-flight">Seleccionar</button>
                  </div>
                </div>
                <p v-else class="no-results">No se encontraron vuelos de vuelta para esa fecha</p>
              </div>
            </div>

            <!-- Resultados para Multiciudad -->
            <div v-if="tripType === 'multiciudad'">
              <div
                class="trip-section"
                v-for="(legFlights, index) in multiCityResults"
                :key="'leg-' + index"
              >
                <h3>
                  Tramo {{ index + 1 }} —
                  {{ search.legs[index].origin }} → {{ search.legs[index].destination }}
                </h3>

                <div v-if="legFlights.length > 0" class="flights-grid">
                  <div
                    class="flight-card"
                    v-for="flight in legFlights"
                    :key="'mc-' + index + '-' + flight.id"
                  >
                    <div class="flight-header">
                      <div class="flight-route">
                        <span class="airport-code">{{ search.legs[index].origin }}</span>
                        <svg class="flight-arrow" viewBox="0 0 24 24" fill="none" stroke="currentColor">
                          <path d="M5 12h14M12 5l7 7-7 7"></path>
                        </svg>
                        <span class="airport-code">{{ search.legs[index].destination }}</span>
                      </div>
                    </div>
                    <div class="flight-details">
                      <div class="flight-time">
                        <strong>{{ flight.departureTime.substring(11, 16) }}</strong>
                        <span class="flight-duration">{{ flight.durationMinutes }}min</span>
                        <strong>{{ flight.arrivalTime.substring(11, 16) }}</strong>
                      </div>
                      <div class="flight-date">
                        {{ new Date(flight.departureTime).toLocaleDateString('es-ES', {
                          weekday: 'short', month: 'short', day: 'numeric'
                        }) }}
                      </div>
                    </div>
                    <div class="flight-prices">
                      <div v-if="selectedOutboundId === flight.id" class="seat-options">
                      <div class="seat-option" @click.stop="selectSeatClass(flight, 'economy')">
                        <span class="class-name">Económica</span>
                        <span class="price">${{ flight.priceEconomyClass }}</span>
      
                      </div>
                      <div class="seat-option" @click.stop="selectSeatClass(flight, 'firstClass')">
                        <span class="class-name">Primera clase</span>
                        <span class="price">${{ flight.priceFirstClass }}</span>
                      </div>
                    </div>
                    </div>
                  </div>
                </div>

                <p v-else class="no-results">
                  No se encontraron vuelos para este tramo en esa fecha
                </p>
              </div>
            </div>
          </div>
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


const BACKEND_API_BASE = 'http://localhost:5235';
const EXTERNAL_API_KEY  = 'u2fdwOUDKCv8X7GhhX9m6ZU2Rak5Z4b2Qi7wV35BqYEnA0wh2rDndiJSuTO3bFVERudTzF9GQwuC0AvXRoWTp3uVQev6ID18yxtby2kiMR3ak0RGvPmFKz1NHQXFTcVNbTIuj60bxVhNeiZQrGem83mVfFRXocAeNfFoO7IM2qJwi27VrV8SfqvtCh62xIlgpquCqRr53KVL02Rvk1s4w9IFAL2Xod1MCjtzyvdnffgXcxMDco4Vw1u1BiZFHSv5';

export default {
  name: 'LandingPage',
  data() {
    return {
      selectedFlightId: null,
      selectedOutboundId: null,      // para ida de ida-vuelta
      selectedReturnId: null,        // para vuelta de ida-vuelta
      selectedMultiCityIds: [],
      tripType: 'ida-vuelta',
      search: {
        origin: '',
        destination: '',
        departureDate: '',
        returnDate: '',
        passengers: '1',
        legs: [
          { origin: '', destination: '', departureDate: '' },
          { origin: '', destination: '', departureDate: '' }
        ]
      },
      originCities: [],
      destinationCities: [],
      searchResults: [],
      outboundFlights: [],
      returnFlights: [],
      multiCityResults: [],
      loading: false,
      searchPerformed: false,
      destinations: [
        {
          id: 1,
          name: 'Nueva York',
          country: 'Estados Unidos',
          price: 299,
          image: 'https://images.unsplash.com/photo-1496442226666-8d4d0e62e6e9?w=400&h=300&fit=crop',
          badge: 'Más popular'
        },
        {
          id: 2,
          name: 'Londres',
          country: 'Reino Unido',
          price: 449,
          image: 'https://images.unsplash.com/photo-1513635269975-59663e0ac1ad?w=400&h=300&fit=crop',
          badge: null
        },
        {
          id: 3,
          name: 'París',
          country: 'Francia',
          price: 379,
          image: 'https://images.unsplash.com/photo-1502602898657-3e91760cbb34?w=400&h=300&fit=crop',
          badge: 'Best seller'
        },
        {
          id: 4,
          name: 'Tokio',
          country: 'Japón',
          price: 599,
          image: 'https://images.unsplash.com/photo-1540959733332-eab4deabeeaf?w=400&h=300&fit=crop',
          badge: null
        },
        {
          id: 5,
          name: 'Miami',
          country: 'Estados Unidos',
          price: 249,
          image: 'https://images.unsplash.com/photo-1535498730771-e735b998cd64?w=400&h=300&fit=crop',
          badge: 'Oferta'
        },
        {
          id: 6,
          name: 'Dubai',
          country: 'Emiratos Árabes',
          price: 529,
          image: 'https://images.unsplash.com/photo-1512453979798-5ea266f8880c?w=400&h=300&fit=crop',
          badge: null
        }
      ],
      offers: [
        {
          id: 1,
          title: 'Descuento de Temporada',
          description: 'Hasta 40% de descuento en vuelos a Europa',
          route: 'San José → Madrid',
          discount: 40
        },
        {
          id: 2,
          title: 'Viaja en Grupo',
          description: '3era persona gratis viajando en grupo',
          route: 'Varios destinos',
          discount: 50
        },
        {
          id: 3,
          title: 'Early Bird',
          description: '30% de descuento reservando con 60 días de anticipación',
          route: 'América Central',
          discount: 30
        }
      ],
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
     * Carga todos los aeropuertos disponibles desde el External API
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
      if (type === 'multiciudad' && this.search.legs.length < 2) {
        this.search.legs = [
          { origin: '', destination: '', departureDate: '' },
          { origin: '', destination: '', departureDate: '' }
        ];
      }
      if (type !== 'multiciudad') {
        this.search.legs = [
          { origin: '', destination: '', departureDate: '' },
          { origin: '', destination: '', departureDate: '' }
        ];
      }
    },
    addLeg() {
      if (this.search.legs.length < 5) {
        this.search.legs.push({ origin: '', destination: '', departureDate: '' });
      }
    },
    removeLeg(index) {
      if (this.search.legs.length > 2) {
        this.search.legs.splice(index, 1);
      }
    },

    /**
     * Construye los query params para el External API.
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
        apiKey: EXTERNAL_API_KEY,
      });
    },

    /**
     * Normaliza un vuelo del External API al shape que usa el template.
     *
     * Mapeo de campos:
     *   flightGUID        → id
     *   touristPrice      → priceEconomyClass
     *   firstClassPrice   → priceFirstClass
     *   duration "hh:mm" o "hh-mm" → durationMinutes (int)
     *
     * departureTime y arrivalTime ya vienen en formato ISO, no cambian.
     */
    mapFlight(flight) {
      const [hours, minutes] = (flight.duration ?? '')
        .split(/[:-]/)
        .map(Number);
      const validHours = Number.isFinite(hours) ? hours : 0;
      const validMinutes = Number.isFinite(minutes) ? minutes : 0;

      return {
        ...flight,
        id:                flight.flightGUID,
        priceEconomyClass: flight.touristPrice,
        priceFirstClass:   flight.firstClassPrice,
        durationMinutes:   validHours * 60 + validMinutes,
      };
    },

    /**
     * Hace el fetch al External API y devuelve los vuelos ya normalizados.
     */
    fetchFlights(origin, destination, date, passengers) {
      const params = this.buildExternalParams(origin, destination, date, passengers);
      return fetch(`${BACKEND_API_BASE}/Flight/search?${params}`)
        .then(res => {
          if (!res.ok) throw new Error(`Error buscando vuelos ${origin} → ${destination}`);
          return res.json();
        })
        .then(data => data.flights.map(this.mapFlight));
    },

    searchFlights() {
      // ── Multiciudad
      if (this.tripType === 'multiciudad') {
        for (let i = 0; i < this.search.legs.length; i++) {
          const leg = this.search.legs[i];
          if (!leg.origin || !leg.destination || !leg.departureDate) {
            alert(`Por favor completa todos los campos del tramo ${i + 1}`);
            return;
          }
        }

        this.loading = true;
        this.searchPerformed = true;
        this.multiCityResults = [];

        const legPromises = this.search.legs.map(leg =>
          this.fetchFlights(leg.origin, leg.destination, leg.departureDate, this.search.passengers)
        );

        Promise.all(legPromises)
          .then(results => {
            this.multiCityResults = results;
            if (results.every(r => r.length === 0)) {
              alert('No se encontraron vuelos para ninguno de los tramos');
            }
          })
          .catch(error => {
            console.error('Error:', error);
            alert('Error al buscar vuelos: ' + error.message);
          })
          .finally(() => {
            this.loading = false;
          });

        return;
      }

      // ── Ida / Ida y Vuelta
      if (!this.search.origin || !this.search.destination || !this.search.departureDate) {
        alert('Por favor completa todos los campos requeridos');
        return;
      }

      if (this.tripType === 'ida-vuelta' && !this.search.returnDate) {
        alert('Por favor selecciona la fecha de retorno');
        return;
      }

      this.loading = true;
      this.searchPerformed = true;

      const outboundPromise = this.fetchFlights(
        this.search.origin,
        this.search.destination,
        this.search.departureDate,
        this.search.passengers
      );

      // Solo ida
      if (this.tripType === 'ida') {
        outboundPromise
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
        return;
      }

      // Ida y vuelta — buscar ambos tramos en paralelo
      const returnPromise = this.fetchFlights(
        this.search.destination,
        this.search.origin,
        this.search.returnDate,
        this.search.passengers
      );

      Promise.all([outboundPromise, returnPromise])
        .then(([outboundData, returnData]) => {
          this.outboundFlights = outboundData;
          this.returnFlights   = returnData;
          this.searchResults   = [];

          if (outboundData.length === 0 && returnData.length === 0) {
            alert('No se encontraron vuelos para esas fechas');
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

    toggleFlight(flightId) {
      this.selectedFlightId = this.selectedFlightId === flightId ? null : flightId;
    },

    toggleOutbound(flightId) {
      this.selectedOutboundId = this.selectedOutboundId === flightId ? null : flightId;
    },

    toggleReturn(flightId) {
      this.selectedReturnId = this.selectedReturnId === flightId ? null : flightId;
    },

    toggleMultiCity(flightId) {
      if (this.selectedMultiCityIds.includes(flightId)) {
        this.selectedMultiCityIds = this.selectedMultiCityIds.filter(id => id !== flightId);
      } else {
        this.selectedMultiCityIds.push(flightId);
      }
    },

    selectSeatClass(flight, seatClass) {

      this.$router.push({path: '/booking', query: {
        flightId: flight.id,
        seatClass: seatClass,
        passengersCount: this.search.passengers
      }})
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

.multicity-section {
  display: flex;
  flex-direction: column;
  gap: 18px;
}

.multicity-legs {
  display: grid;
  gap: 20px;
}

.multicity-leg {
  padding: 20px;
  border: 1px solid #e0e0e0;
  border-radius: 16px;
  background: #fafafa;
  display: grid;
  gap: 16px;
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

.flight-date {
  font-size: 0.9rem;
  color: #666;
  text-align: center;
}

.flight-prices {
  margin-bottom: 15px;
  padding: 15px 0;
  border-bottom: 1px solid #f0f0f0;
}

.price-option {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 10px;
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
</style>