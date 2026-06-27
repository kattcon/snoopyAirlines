<template>
  <div style="text-align:center; padding:80px 40px;">
    <template v-if="loading">
      <p>Procesando cancelación...</p>
    </template>
    <template v-else-if="success">
      <h1>✅ Reservación cancelada</h1>
      <p>Tu reservación ha sido cancelada exitosamente.</p>
      <a href="/">Volver al inicio</a>
    </template>
    <template v-else>
      <h1>❌ Enlace inválido o expirado</h1>
      <p>El enlace de cancelación no es válido o ya expiró.</p>
      <a href="/">Volver al inicio</a>
    </template>
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
    console.log('Token recibido:', token);

    if (!token) {
      this.loading = false;
      this.success = false;
      return;
    }

    try {
      const response = await axios.post(`${process.env.VUE_APP_BACKEND_URL}/Booking/cancel-booking`, { token });
      console.log('Respuesta del backend:', response.data);
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