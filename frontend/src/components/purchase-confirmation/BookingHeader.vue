<template>
  <header class="booking-header">
    <router-link to="/" class="brand-link" aria-label="Snoopy Airlines">
      <img src="../../assets/logoSA.png" alt="Snoopy Airlines" class="brand-logo" />
      <span class="brand-name">Snoopy Airlines</span>
    </router-link>

    <nav class="main-nav" aria-label="Principal">
      <router-link to="/">Vuelos</router-link>
      <a href="/#ofertas">Ofertas</a>
      <a href="/#destinos">Destinos</a>
      <a href="/#mi-reserva">Mi reserva</a>
    </nav>

    <div class="header-actions">
      <button type="button" class="language-button" aria-label="Idioma">
        <svg viewBox="0 0 24 24" aria-hidden="true">
          <circle cx="12" cy="12" r="10"></circle>
          <path d="M2 12h20"></path>
          <path d="M12 2a15.3 15.3 0 0 1 0 20"></path>
          <path d="M12 2a15.3 15.3 0 0 0 0 20"></path>
        </svg>
        <span>ES</span>
      </button>
      <router-link to="/login" class="secondaryButton login-button">
        <svg viewBox="0 0 24 24" aria-hidden="true">
          <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"></path>
          <circle cx="12" cy="7" r="4"></circle>
        </svg>
        <span>Iniciar sesi&oacute;n</span>
      </router-link>
    </div>
  </header>
</template>

<script>
export default {
  name: "BookingHeader"
};
</script>

<style scoped>
.booking-header {
  width: 100%;
  min-height: 64px;
  padding: 10px 18px;
  border-bottom: 1px solid #d8dde6;
  background: var(--whiteColor);
  display: grid;
  grid-template-columns: auto 1fr auto;
  align-items: center;
  gap: 28px;
}

.brand-link {
  display: inline-flex;
  align-items: center;
  gap: 10px;
  color: #111827;
  text-decoration: none;
  font-weight: var(--boldFontWeight);
  white-space: nowrap;
}

.brand-logo {
  width: 38px;
  height: 38px;
  border-radius: var(--defaultBorderRadius);
}

.brand-name {
  font-size: var(--mediumFontSize);
}

.main-nav {
  display: flex;
  justify-content: center;
  gap: 32px;
}

.main-nav a {
  color: #526078;
  font-size: var(--smallFontSize);
  font-weight: var(--mediumFontWeight);
  text-decoration: none;
}

.main-nav a:hover {
  color: var(--linkColor);
}

.header-actions,
.language-button,
.login-button {
  display: inline-flex;
  align-items: center;
}

.header-actions {
  justify-content: flex-end;
  gap: 14px;
}

.language-button {
  gap: 8px;
  border: none;
  background: transparent;
  color: #111827;
  font-weight: var(--semiboldFontWeight);
  cursor: pointer;
}

.language-button svg,
.login-button svg {
  width: 16px;
  height: 16px;
  fill: none;
  stroke: currentColor;
  stroke-width: 2;
  stroke-linecap: round;
  stroke-linejoin: round;
}

.login-button {
  gap: 8px;
  padding: 8px 14px;
  color: #111827;
  font-size: var(--smallFontSize);
  text-decoration: none;
}

@media (max-width: 760px) {
  .booking-header {
    grid-template-columns: 1fr auto;
  }

  .main-nav {
    display: none;
  }
}

@media (max-width: 520px) {
  .brand-name,
  .language-button span,
  .login-button span {
    display: none;
  }
}
</style>
