<template>
  <div class="register-airport-view">
    <div class="page-top-bar">
      <button class="btn-volver" @click="$router.push('/admin/airports')">Volver a la lista</button>
    </div>
    <div class="register-form">
      <h2 class="form-title">Registro de Aeropuerto</h2>
      <p class="form-subtitle">Complete la información para registrar un nuevo aeropuerto</p>

      <!-- Campo para el nombre completo del aeropuerto -->
      <div class="form-group">
        <label>Nombre del Aeropuerto</label>
        <input
          v-model="airport.name"
          type="text"
          maxlength="200"
          :class="['form-control', { 'field-error': errors.name }]"
          placeholder="ej. Juan Santamaría International Airport"
        />
        <span v-if="errors.name" class="error-msg">{{ errors.name }}</span>
      </div>

      <!-- Campo para el código IATA de 3 letras -->
      <div class="form-group">
        <label>Código IATA</label>
        <input
          v-model="airport.code"
          type="text"
          :class="['form-control', { 'field-error': errors.code }]"
          placeholder="ej. SJO"
        />
        <span v-if="errors.code" class="error-msg">{{ errors.code }}</span>
      </div>

      <!-- Dropdown de países. Al seleccionar uno, se cargan sus ciudades -->
      <div class="form-group">
        <label>País</label>
        <select v-model="selectedCountry" :class="['form-control', { 'field-error': errors.country }]">
          <option value="">Seleccione un país</option>
          <option v-for="country in countries" :key="country.id" :value="country.id">
            {{ country.name }}
          </option>
        </select>
        <span v-if="errors.country" class="error-msg">{{ errors.country }}</span>
      </div>

      <!-- Dropdown de ciudades. Está deshabilitado hasta que se elija un país -->
      <div class="form-group">
        <label>Ciudad</label>
        <select v-model="airport.cityId" :class="['form-control', { 'field-error': errors.cityId }]" :disabled="!selectedCountry">
          <option value="">Primero seleccione un país</option>
          <option v-for="city in cities" :key="city.id" :value="city.id">
            {{ city.name }}
          </option>
        </select>
        <span v-if="errors.cityId" class="error-msg">{{ errors.cityId }}</span>
      </div>

      <!-- Botones de acción del formulario -->
      <div class="button-group">
        <button class="btn-cancelar" @click="clearForm">Cancelar</button>
        <button class="btn-registrar" @click="registerAirport">Registrar</button>
      </div>

    </div>
  </div>
</template>

<script>
import axios from "axios";

export default {
  name: "RegisterAirport",
  data() {
    return {
      airport: {
        name: "",
        code: "",
        cityId: ""
      },
      selectedCountry: "",
      countries: [],
      cities: [],
      errors: { name: "", code: "", country: "", cityId: "" }
    };
  },
  mounted() {
    this.loadCountries();
  },
  methods: {
    clearForm() {
      this.airport = { name: "", code: "", cityId: "" };
      this.selectedCountry = "";
      this.cities = [];
      this.errors = { name: "", code: "", country: "", cityId: "" };
    },
    validate() {
      this.errors = { name: "", code: "", country: "", cityId: "" };
      let valid = true;
      if (!this.airport.name || !this.airport.name.trim()) {
        this.errors.name = "Campo obligatorio";
        valid = false;
      }
      if (!this.airport.code || !this.airport.code.trim()) {
        this.errors.code = "Campo obligatorio";
        valid = false;
      } else if (!/^[A-Za-z]{3}$/.test(this.airport.code.trim())) {
        this.errors.code = "Debe tener exactamente 3 letras (ej. SJO)";
        valid = false;
      }
      if (!this.selectedCountry) {
        this.errors.country = "Campo obligatorio";
        valid = false;
      }
      if (!this.airport.cityId) {
        this.errors.cityId = "Campo obligatorio";
        valid = false;
      }
      return valid;
    },
    loadCountries() {
      const token = localStorage.getItem("token");
      axios
        .get("https://localhost:7080/airport/countries", {
          headers: { Authorization: `Bearer ${token}` }
        })
        .then((response) => {
          this.countries = response.data;
        })
        .catch((error) => {
          console.error("Error cargando países:", error);
        });
    },
    registerAirport() {
      if (!this.validate()) return;
      const token = localStorage.getItem("token");
      axios
        .post("https://localhost:7080/airport", this.airport, {
          headers: { Authorization: `Bearer ${token}` }
        })
        .then(() => {
          alert("Aeropuerto registrado correctamente");
          this.clearForm();
        })
        .catch((error) => {
          if (error.response && error.response.status === 409) {
            alert("Error de registro: código existente");
          } else if (error.response && error.response.status === 400) {
            alert("Datos inválidos: " + JSON.stringify(error.response.data));
          } else {
            alert("Error al registrar el aeropuerto");
          }
          console.error(error);
        });
    }
  },
  watch: {
    selectedCountry(newCountryId) {
      this.airport.cityId = "";
      this.cities = [];
      if (newCountryId) {
        const token = localStorage.getItem("token");
        axios
          .get(`https://localhost:7080/airport/cities?countryId=${newCountryId}`, {
            headers: { Authorization: `Bearer ${token}` }
          })
          .then((response) => {
            this.cities = response.data;
          })
          .catch((error) => {
            console.error("Error cargando ciudades:", error);
          });
      }
    }
  }
};
</script>

<style scoped>
.register-airport-view {
  position: relative;
  min-height: 100%;
  background: linear-gradient(to bottom right, #1a3a6b, #b0bec5);
  padding: 24px;
  display: flex;
  justify-content: center;
  align-items: center;
  box-sizing: border-box;
}

.page-top-bar {
  position: absolute;
  top: 24px;
  right: 30px;
}

.register-form {
  background-color: white;
  border-radius: 15px;
  padding: 40px;
  width: 480px;
  box-shadow: 0 0 15px rgba(0, 0, 0, 0.1);
}

.form-title {
  text-align: center;
  margin-bottom: 5px;
  font-size: 22px;
}

.form-subtitle {
  text-align: center;
  color: #888;
  font-size: 13px;
  margin-bottom: 25px;
}

.form-group {
  margin-bottom: 18px;
  font-size: 14px;
}

.form-group label {
  display: block;
  margin-bottom: 6px;
  font-weight: bold;
  color: #2c5fa8;
}

.form-control {
  width: 100%;
  padding: 10px;
  border: 1px solid #ccc;
  border-radius: 8px;
  font-size: 14px;
  box-sizing: border-box;
  background-color: #f5f5f5;
}

.field-error {
  border-color: #e53935 !important;
}

.error-msg {
  color: #e53935;
  font-size: 12px;
  margin-top: 4px;
  display: block;
}

.form-control:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.button-group {
  display: flex;
  gap: 15px;
  margin-top: 25px;
}

.btn-cancelar {
  flex: 1;
  padding: 10px;
  background-color: white;
  border: 1px solid #ccc;
  border-radius: 8px;
  cursor: pointer;
  font-size: 14px;
}

.btn-registrar {
  flex: 1;
  padding: 10px;
  background-color: #1a2b4a;
  color: white;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  font-size: 14px;
}

.btn-volver {
  background-color: #1a2b4a;
  color: white;
  border: none;
  border-radius: 8px;
  padding: 10px 18px;
  cursor: pointer;
  font-size: 14px;
  font-weight: bold;
}

.btn-volver:hover {
  background-color: #2c3e6b;
}
</style>
