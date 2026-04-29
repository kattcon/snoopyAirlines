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
            <button :class="['tab-btn', { active: tripType === 'ida' }]" @click="tripType = 'ida'">Ida</button>
            <button :class="['tab-btn', { active: tripType === 'ida-vuelta' }]" @click="tripType = 'ida-vuelta'">Ida y Vuelta</button>
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
            <button type="submit" class="btn-search">Buscar Vuelos</button>
          </form>
        </div>
      </div>
    </section>

    <!-- Popular Destinations -->
    <section id="destinos" class="destinations">
      <div class="section-container">
        <h2 class="section-title">Destinos Populares</h2>
        <p class="section-subtitle">Explora los destinos más solicitados por nuestros viajeros</p>
        <div class="destinations-grid">
          <div class="destination-card" v-for="dest in destinations" :key="dest.id">
            <div class="card-image">
              <img :src="dest.image" :alt="dest.name" />
              <div class="card-badge" v-if="dest.badge">{{ dest.badge }}</div>
            </div>
            <div class="card-content">
              <h3>{{ dest.name }}</h3>
              <p class="country">{{ dest.country }}</p>
              <div class="card-footer">
                <span class="price">Desde ${{ dest.price }}</span>
                <button class="btn-view">Ver vuelos</button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- Special Offers -->
    <section id="ofertas" class="offers">
      <div class="section-container">
        <h2 class="section-title">Ofertas Especiales</h2>
        <p class="section-subtitle">Aprovecha nuestros descuentos exclusivos</p>
        <div class="offers-grid">
          <div class="offer-card" v-for="offer in offers" :key="offer.id">
            <div class="offer-discount">{{ offer.discount }}%</div>
            <div class="offer-content">
              <h3>{{ offer.title }}</h3>
              <p>{{ offer.description }}</p>
              <span class="offer-route">{{ offer.route }}</span>
              <button class="btn-offer">Reservar Ahora</button>
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
export default {
  name: 'LandingPage',
  data() {
    return {
      tripType: 'ida-vuelta',
      search: {
        origin: '',
        destination: '',
        departureDate: '',
        returnDate: '',
        passengers: '1'
      },
      originCities: [
        { code: 'SJO', name: 'San José' },
        { code: 'LIM', name: 'Lima' },
        { code: 'BOG', name: 'Bogotá' },
        { code: 'MEX', name: 'Ciudad de México' },
        { code: 'PTY', name: 'Panamá' },
        { code: 'MIA', name: 'Miami' },
        { code: 'JFK', name: 'Nueva York' },
        { code: 'LAX', name: 'Los Ángeles' },
        { code: 'MAD', name: 'Madrid' },
        { code: 'BCN', name: 'Barcelona' }
      ],
      destinationCities: [
        { code: 'SJO', name: 'San José' },
        { code: 'LIM', name: 'Lima' },
        { code: 'BOG', name: 'Bogotá' },
        { code: 'MEX', name: 'Ciudad de México' },
        { code: 'PTY', name: 'Panamá' },
        { code: 'MIA', name: 'Miami' },
        { code: 'JFK', name: 'Nueva York' },
        { code: 'LAX', name: 'Los Ángeles' },
        { code: 'MAD', name: 'Madrid' },
        { code: 'BCN', name: 'Barcelona' },
        { code: 'LHR', name: 'Londres' },
        { code: 'CDG', name: 'París' },
        { code: 'NRT', name: 'Tokio' },
        { code: 'DXB', name: 'Dubái' }
      ],
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
  methods: {
    searchFlights() {
      console.log('Buscando vuelos:', this.search);
      alert('Búsqueda de vuelos: ' + JSON.stringify(this.search, null, 2));
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
  gap: 35px;
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

</style>