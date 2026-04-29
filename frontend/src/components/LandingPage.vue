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
</style>