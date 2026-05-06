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
            <div class="password-input">
              <input
                v-model="formData.password"
                :type="showPassword ? 'text' : 'password'"
                id="password"
                class="form-control"
                placeholder="Contraseña"
                autocomplete="new-password"
                :disabled="!registrationKey || isSubmitting"
                required
              />
              <button
                type="button"
                class="password-toggle"
                :aria-label="showPassword ? 'Ocultar contraseña' : 'Mostrar contraseña'"
                :disabled="!registrationKey || isSubmitting"
                @click="showPassword = !showPassword"
              >
                <svg v-if="!showPassword" viewBox="0 0 24 24" aria-hidden="true">
                  <path d="M2 12s3.5-6 10-6 10 6 10 6-3.5 6-10 6-10-6-10-6Z" />
                  <circle cx="12" cy="12" r="3" />
                </svg>
                <svg v-else viewBox="0 0 24 24" aria-hidden="true">
                  <path d="m3 3 18 18" />
                  <path d="M10.6 10.6a2 2 0 0 0 2.8 2.8" />
                  <path d="M9.9 5.2A10.6 10.6 0 0 1 12 5c6.5 0 10 7 10 7a17 17 0 0 1-3.2 4.1" />
                  <path d="M6.1 6.8A17 17 0 0 0 2 12s3.5 7 10 7a10.5 10.5 0 0 0 4.2-.9" />
                </svg>
              </button>
            </div>
            <ul class="password-feedback" aria-live="polite">
              <li
                v-for="rule in passwordRules"
                :key="rule.key"
                :class="{ valid: rule.valid }"
              >
                <span class="rule-icon" aria-hidden="true"></span>
                <span>{{ rule.label }}</span>
              </li>
            </ul>
            <span v-if="errors.password" class="error-message">{{ errors.password }}</span>
          </div>

          <div class="form-group">
            <label for="confirmPassword" class="field-title">Confirmar contraseña:</label>
            <div class="password-input">
              <input
                v-model="formData.confirmPassword"
                :type="showConfirmPassword ? 'text' : 'password'"
                id="confirmPassword"
                class="form-control"
                placeholder="Repita la contraseña"
                autocomplete="new-password"
                :disabled="!registrationKey || isSubmitting"
                required
              />
              <button
                type="button"
                class="password-toggle"
                :aria-label="showConfirmPassword ? 'Ocultar contraseña' : 'Mostrar contraseña'"
                :disabled="!registrationKey || isSubmitting"
                @click="showConfirmPassword = !showConfirmPassword"
              >
                <svg v-if="!showConfirmPassword" viewBox="0 0 24 24" aria-hidden="true">
                  <path d="M2 12s3.5-6 10-6 10 6 10 6-3.5 6-10 6-10-6-10-6Z" />
                  <circle cx="12" cy="12" r="3" />
                </svg>
                <svg v-else viewBox="0 0 24 24" aria-hidden="true">
                  <path d="m3 3 18 18" />
                  <path d="M10.6 10.6a2 2 0 0 0 2.8 2.8" />
                  <path d="M9.9 5.2A10.6 10.6 0 0 1 12 5c6.5 0 10 7 10 7a17 17 0 0 1-3.2 4.1" />
                  <path d="M6.1 6.8A17 17 0 0 0 2 12s3.5 7 10 7a10.5 10.5 0 0 0 4.2-.9" />
                </svg>
              </button>
            </div>
            <span v-if="confirmPasswordMismatch" class="error-message">Las contraseñas no coinciden.</span>
            <span v-else-if="errors.confirmPassword" class="error-message">{{ errors.confirmPassword }}</span>
          </div>

          <p v-if="submitError" class="form-alert error">{{ submitError }}</p>

          <button type="submit" class="btn-register" :disabled="!canSubmit">
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
      isSubmitting: false,
      showPassword: false,
      showConfirmPassword: false
    };
  },
  computed: {
    registrationKey() {
      const key = this.$route.query.key;

      return Array.isArray(key) ? key[0] : key;
    },
    passwordRules() {
      const password = this.formData.password;

      return [
        {
          key: "length",
          label: "Al menos 8 caracteres",
          valid: password.length >= 8
        },
        {
          key: "case",
          label: "Mayúscula y minúscula",
          valid: this.hasUpperCase(password) && this.hasLowerCase(password)
        },
        {
          key: "symbol",
          label: "Un símbolo especial (!#$%&@)",
          valid: /[!#$%&@]/.test(password)
        },
        {
          key: "number",
          label: "Al menos un número",
          valid: /[0-9]/.test(password)
        }
      ];
    },
    isPasswordValid() {
      return this.passwordRules.every((rule) => rule.valid);
    },
    confirmPasswordMismatch() {
      return Boolean(
        this.formData.password &&
        this.formData.confirmPassword &&
        this.formData.password !== this.formData.confirmPassword
      );
    },
    passwordsMatch() {
      return Boolean(
        this.formData.password &&
        this.formData.confirmPassword &&
        !this.confirmPasswordMismatch
      );
    },
    canSubmit() {
      return Boolean(
        this.registrationKey &&
        !this.isSubmitting &&
        this.isPasswordValid &&
        this.passwordsMatch
      );
    }
  },
  watch: {
    "formData.password"() {
      this.errors.password = "";
      this.submitError = "";

      if (this.formData.confirmPassword && this.passwordsMatch) {
        this.errors.confirmPassword = "";
      }
    },
    "formData.confirmPassword"() {
      this.errors.confirmPassword = "";
      this.submitError = "";
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
      } else if (!this.isPasswordValid) {
        this.errors.password = "La contraseña no cumple con los requisitos.";
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
        .post(`${process.env.VUE_APP_BACKEND_URL}/user/register`, {
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
        if (this.applyBackendValidationErrors(error.response.data)) return;

        this.submitError = "La llave de registro no es válida o ya fue utilizada.";
      } else if (error.response && error.response.status === 409) {
        this.submitError = "Ya existe un usuario registrado con este correo.";
      } else {
        this.submitError = "Error al completar el registro.";
      }

      console.error(error);
    },
    applyBackendValidationErrors(responseData) {
      if (!responseData || !Array.isArray(responseData.errors)) return false;

      let handled = false;

      responseData.errors.forEach((error) => {
        const fieldName = this.toCamelCase(error.field);

        if (fieldName === "password") {
          this.errors.password = "La contraseña no cumple con los requisitos.";
          handled = true;
        }

        if (fieldName === "pendingKey") {
          this.submitError = "El enlace de registro no incluye una llave válida.";
          handled = true;
        }
      });

      return handled;
    },
    toCamelCase(value) {
      if (!value) return "";

      return value.charAt(0).toLowerCase() + value.slice(1);
    },
    hasUpperCase(value) {
      return Array.from(value).some((character) => (
        character !== character.toLowerCase() &&
        character === character.toUpperCase()
      ));
    },
    hasLowerCase(value) {
      return Array.from(value).some((character) => (
        character !== character.toUpperCase() &&
        character === character.toLowerCase()
      ));
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

.password-input {
  position: relative;
}

.password-input .form-control {
  padding-right: 48px;
}

.password-toggle {
  position: absolute;
  top: 50%;
  right: 7px;
  transform: translateY(-50%);
  display: inline-flex;
  width: 34px;
  height: 34px;
  align-items: center;
  justify-content: center;
  padding: 0;
  border: 0;
  border-radius: 50%;
  background: transparent;
  color: #475569;
  cursor: pointer;
}

.password-toggle:hover:not(:disabled),
.password-toggle:focus-visible {
  outline: none;
  background: #eef2ff;
  color: #1d4ed8;
}

.password-toggle:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.password-toggle svg {
  width: 20px;
  height: 20px;
  fill: none;
  stroke: currentColor;
  stroke-linecap: round;
  stroke-linejoin: round;
  stroke-width: 1.8;
}

.password-feedback {
  display: grid;
  gap: 6px;
  margin: 10px 0 0;
  padding: 0;
  list-style: none;
  color: #64748b;
  font-size: 0.84rem;
  font-weight: 600;
}

.password-feedback li {
  display: flex;
  align-items: center;
  gap: 8px;
}

.password-feedback li.valid {
  color: #15803d;
}

.rule-icon {
  position: relative;
  flex: 0 0 16px;
  width: 16px;
  height: 16px;
  border: 1.5px solid currentColor;
  border-radius: 50%;
}

.password-feedback li.valid .rule-icon {
  border-color: #15803d;
  background: #15803d;
}

.password-feedback li.valid .rule-icon::after {
  position: absolute;
  top: 2px;
  left: 5px;
  width: 4px;
  height: 8px;
  border: solid #ffffff;
  border-width: 0 2px 2px 0;
  content: "";
  transform: rotate(45deg);
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
