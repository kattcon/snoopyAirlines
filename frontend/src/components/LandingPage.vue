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
  </div>
</template>

<script>
export default {
  name: 'LandingPage',
  data() {
    return {};
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
</style>