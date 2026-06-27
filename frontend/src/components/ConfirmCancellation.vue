<template>
  <div class="confirmation-page">
    <div class="logo">
      <a href="/">
        <img src="../assets/logoSA.png" alt="logo" class="logo-image" />
      </a>
      <h3 class="logo-text">Snoopy Airlines</h3>
    </div>

    <div class="confirmation-card">
      <img src="../assets/logoSA.png" alt="logo" class="confirmation-logo" />

      <template v-if="loading">
        <div class="status-icon">⏳</div>
        <h2>Procesando cancelación...</h2>
        <p>Estamos procesando tu solicitud. Esto tomará unos segundos.</p>
      </template>

      <template v-else-if="success">
        <div class="status-icon success">✅</div>
        <h2>Reservación cancelada</h2>
        <p>
          Tu reservación ha sido cancelada exitosamente.
        </p>

        <a href="/" class="primary-button">
          Volver al inicio
        </a>
      </template>

      <template v-else>
        <div class="status-icon error">❌</div>
        <h2>Enlace inválido o expirado</h2>
        <p>
          El enlace de cancelación no es válido o ya expiró.
        </p>

        <a href="/" class="primary-button">
          Volver al inicio
        </a>
      </template>
    </div>
  </div>
</template>

<script>
import axios from 'axios';

export default {
  name: 'ConfirmCancellation',
  data() {
    return {
      loading: true,
      success: false,
    };
  },
  async created() {
    const token = this.$route.query.token;

    if (!token) {
      this.loading = false;
      this.success = false;
      return;
    }

    try {
      const response = await axios.post(`${process.env.VUE_APP_BACKEND_URL}/Booking/cancel-booking`, { token });
      this.success = response.data.success === true;
    } catch (e) {
      console.error('Error:', e.response?.status, e.response?.data);
      this.success = false;
    } finally {
      this.loading = false;
    }
  },
};
</script>

<style scoped>
html,
body {
  height: 100%;
  margin: 0;
}

.confirmation-page {
  min-height: 100vh;
  background: linear-gradient(to bottom right, #1a3a6b, #b0bec5);
}

.logo {
  display: flex;
  align-items: center;
  background: #f9f9f9;
  border-bottom: 1px solid #d9d9d9;
  padding: 10px 20px;
}

.logo-image {
  width: 45px;
  height: 45px;
}

.logo-text {
  margin-left: 10px;
  color: #2c3e50;
  font-size: 1.4rem;
  font-weight: 700;
}

.confirmation-card {
  width: 430px;
  max-width: 90%;
  margin: 50px auto;
  background: white;
  border-radius: 20px;
  padding: 35px;
  text-align: center;
  box-shadow: 0 8px 25px rgba(0, 0, 0, 0.15);
}

.confirmation-logo {
  width: 90px;
  margin-bottom: 20px;
}

.status-icon {
  font-size: 3rem;
  margin-bottom: 15px;
}

h2 {
  color: #1a3a6b;
  margin-bottom: 15px;
}

p {
  color: #555;
  margin-bottom: 30px;
  line-height: 1.5;
}

.primary-button {
  display: inline-block;
  background: #1a3a6b;
  color: white;
  text-decoration: none;
  padding: 12px 28px;
  border-radius: 8px;
  font-weight: 600;
  transition: background 0.2s;
}

.primary-button:hover {
  background: #274b86;
}

.success {
  color: #28a745;
}

.error {
  color: #dc3545;
}
</style>