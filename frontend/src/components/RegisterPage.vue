<template>
  <div class="register-container">
    <header class="register-header">
      <router-link to="/" class="brand-link">
        <img src="../assets/logoSA.png" alt="logo" class="logo-image" />
        <span class="logo-text">Snoopy Airlines</span>
      </router-link>
    </header>

    <main class="register-content">
      <section class="register-form" aria-labelledby="register-title">
        <img src="../assets/logoSA.png" alt="logo" class="logo-image-register" />
        <h3 id="register-title" class="register-title">Completar registro</h3>
        <p class="welcome-message">
          Bienvenido a SnoopyAirlines. Ingrese una contraseña para completar su registro.
        </p>

        <p v-if="!registrationKey" class="form-alert error">
          El enlace de registro no incluye una llave válida.
        </p>

        <form @submit.prevent="completeRegistration">
          <div class="form-group">
            <label for="password" class="field-title">Contraseña:</label>
            <input
              v-model="formData.password"
              type="password"
              id="password"
              class="form-control"
              placeholder="Contraseña"
              autocomplete="new-password"
              :disabled="!registrationKey || isSubmitting"
              required
            />
            <span v-if="errors.password" class="error-message">{{ errors.password }}</span>
          </div>

          <div class="form-group">
            <label for="confirmPassword" class="field-title">Confirmar contraseña:</label>
            <input
              v-model="formData.confirmPassword"
              type="password"
              id="confirmPassword"
              class="form-control"
              placeholder="Repita la contraseña"
              autocomplete="new-password"
              :disabled="!registrationKey || isSubmitting"
              required
            />
            <span v-if="errors.confirmPassword" class="error-message">{{ errors.confirmPassword }}</span>
          </div>

          <p v-if="submitError" class="form-alert error">{{ submitError }}</p>

          <button type="submit" class="btn-register" :disabled="!registrationKey || isSubmitting">
            {{ isSubmitting ? "Registrando..." : "Completar registro" }}
          </button>
        </form>
      </section>

      <router-link class="volver-a-inicio" to="/">
        <p>Volver al inicio</p>
      </router-link>
    </main>
  </div>
</template>

<script>
import axios from "axios";

export default {
  name: "RegisterPage",
  data() {
    return {
      formData: {
        password: "",
        confirmPassword: ""
      },
      errors: {
        password: "",
        confirmPassword: ""
      },
      submitError: "",
      isSubmitting: false
    };
  },
  computed: {
    registrationKey() {
      const key = this.$route.query.key;

      return Array.isArray(key) ? key[0] : key;
    }
  },
  methods: {
    clearErrors() {
      this.errors = {
        password: "",
        confirmPassword: ""
      };
      this.submitError = "";
    },
    validateForm() {
      this.clearErrors();
      let valid = true;

      if (!this.formData.password) {
        this.errors.password = "Ingrese una contraseña.";
        valid = false;
      }

      if (!this.formData.confirmPassword) {
        this.errors.confirmPassword = "Confirme su contraseña.";
        valid = false;
      }

      if (
        this.formData.password &&
        this.formData.confirmPassword &&
        this.formData.password !== this.formData.confirmPassword
      ) {
        this.errors.confirmPassword = "Las contraseñas no coinciden.";
        valid = false;
      }

      return valid;
    },
    completeRegistration() {
      if (!this.registrationKey) {
        this.submitError = "El enlace de registro no incluye una llave válida.";
        return;
      }

      if (!this.validateForm()) return;

      this.isSubmitting = true;

      axios
        .post(`${process.env.VUE_APP_BACKEND_URL}/user/registration`, {
          password: this.formData.password,
          pendingKey: this.registrationKey
        })
        .then(() => {
          alert("Registro completado correctamente. Ya puede iniciar sesión.");
          this.$router.push("/login");
        })
        .catch((error) => {
          this.handleRegistrationError(error);
        })
        .finally(() => {
          this.isSubmitting = false;
        });
    },
    handleRegistrationError(error) {
      if (error.response && error.response.status === 400) {
        this.submitError = "La llave de registro no es válida o ya fue utilizada.";
      } else if (error.response && error.response.status === 409) {
        this.submitError = "Ya existe un usuario registrado con este correo.";
      } else {
        this.submitError = "Error al completar el registro.";
      }

      console.error(error);
    }
  }
};
</script>

<style scoped>
.register-container {
  min-height: 100vh;
  background: linear-gradient(to bottom right, #1a3a6b, #b0bec5);
}

.register-header {
  width: 100%;
  border-bottom: 1px solid #b4b4b4;
  background-color: #f9f9f9;
}

.brand-link {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  padding: 0 12px;
  min-height: 46px;
  text-decoration: none;
}

.logo-image {
  width: 45px;
  height: 45px;
}

.logo-text {
  font-size: 1.4rem;
  font-weight: 700;
  color: #2c3e50;
}

.register-content {
  display: flex;
  min-height: calc(100vh - 47px);
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 30px 16px;
}

.register-form {
  width: min(450px, 100%);
  min-height: 560px;
  padding: 25px;
  border-radius: 20px;
  background-color: #ffffff;
  box-shadow: 0 16px 45px rgba(20, 38, 70, 0.22);
}

.logo-image-register {
  width: 120px;
  height: 120px;
  display: block;
  margin: 0 auto;
}

.register-title {
  text-align: center;
  margin-bottom: 10px;
  color: #1f2937;
}

.welcome-message {
  text-align: center;
  margin-bottom: 24px;
  color: #818181;
  line-height: 1.45;
}

.form-group {
  margin-bottom: 22px;
  font-weight: bold;
  font-size: medium;
}

.field-title {
  display: block;
  margin-bottom: 10px;
}

.form-control {
  width: 100%;
  padding: 10px;
  border: 1px solid #ccc;
  border-radius: 5px;
  font-size: 1rem;
}

.form-control:focus {
  outline: none;
  border-color: #345ef0;
  box-shadow: 0 0 0 3px rgba(52, 94, 240, 0.13);
}

.form-control:disabled {
  background: #f1f5f9;
  cursor: not-allowed;
}

.error-message {
  display: block;
  margin-top: 6px;
  color: #d32f2f;
  font-size: 0.82rem;
  font-weight: 600;
}

.form-alert {
  margin-bottom: 16px;
  padding: 10px 12px;
  border-radius: 8px;
  font-size: 0.9rem;
  line-height: 1.35;
}

.form-alert.error {
  border: 1px solid #ffcdd2;
  background: #fff5f5;
  color: #b71c1c;
}

.btn-register {
  width: 100%;
  padding: 10px;
  margin-top: 8px;
  background-color: oklch(0.546 0.245 262.881);
  color: white;
  border: none;
  border-radius: 15px;
  cursor: pointer;
  font-weight: 700;
}

.btn-register:hover:not(:disabled) {
  filter: brightness(0.95);
}

.btn-register:disabled {
  opacity: 0.65;
  cursor: not-allowed;
}

.volver-a-inicio {
  margin-top: 24px;
  text-align: center;
  text-decoration: none;
  color: #ffffff;
}

.volver-a-inicio:hover {
  color: #ebebeb;
}

@media (max-width: 520px) {
  .register-content {
    justify-content: flex-start;
  }

  .register-form {
    min-height: auto;
    padding: 22px;
  }
}
</style>
