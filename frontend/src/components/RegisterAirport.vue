<template>
  <div class="page-background">
    <div class="page-top-bar">
      <button class="btn-volver" @click="$router.push('/airports')">Volver a lista de aeropuertos</button>
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
        <select v-model="selectedCountry" class="form-control">
          <option value="">Seleccione un país</option>
          <option v-for="country in countries" :key="country.id" :value="country.id">
            {{ country.name }}
          </option>
        </select>
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
        <button class="btn-Cancelar" @click="clearForm">Cancelar</button>
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
      // Datos del aeropuerto que se van a enviar al backend
      airport: {
        name: "",
        code: "",
        cityId: ""
      },
      // País seleccionado. Se usa para cargar las ciudades pero no se manda al backend
      selectedCountry: "",
      // Lista de países cargada desde el backend
      countries: [],
      // Lista de ciudades del país seleccionado
      cities: [],
      // Errores de validación por campo
      errors: { name: "", code: "", cityId: "" }
    };
  },
  // mounted() se ejecuta automáticamente cuando el componente carga en la página
  mounted() {
    this.loadCountries();
  },
  methods: {
    clearForm() {
      this.airport = { name: "", code: "", cityId: "" };
      this.selectedCountry = "";
      this.cities = [];
      this.errors = { name: "", code: "", cityId: "" };
    },
    validate() {
      this.errors = { name: "", code: "", cityId: "" };
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
      if (!this.airport.cityId) {
        this.errors.cityId = "Campo obligatorio";
        valid = false;
      }
      return valid;
    },
    // Llama al backend para traer todos los países y llenar el dropdown
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
    // Envía los datos del formulario al backend para registrar el aeropuerto
    registerAirport() {
      if (!this.validate()) return;
      const token = localStorage.getItem("token");
      axios
        .post("https://localhost:7080/airport", this.airport, {
          headers: { Authorization: `Bearer ${token}` }
        })
        .then(() => {
          alert("Aeropuerto registrado correctamente");
          this.airport = { name: "", code: "", cityId: "" };
          this.selectedCountry = "";
          this.cities = [];
        })
        .catch((error) => {
          if (error.response && error.response.status === 409) {
            alert("Ya existe un aeropuerto con ese código IATA");
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
    // Cuando el usuario cambia el país, limpia la ciudad y carga las nuevas ciudades
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

<style>
/* Fondo de toda la página con degradado azul */
.page-background {
  position: relative;
  min-height: 100vh;
  background: linear-gradient(to bottom right, #1a3a6b, #b0bec5);
  display: flex;
  justify-content: center;
  align-items: center;
}

/* Tarjeta blanca del formulario centrada en la página */
.register-form {
  background-color: white;
  border-radius: 15px;
  padding: 40px;
  width: 480px;
  box-shadow: 0 0 15px rgba(0, 0, 0, 0.2);
}

/* Barra superior con el botón volver */
.page-top-bar {
  position: absolute;
  top: 20px;
  right: 30px;
}

.btn-volver {
  background-color: #1a2b4a;
  color: white;
  border: none;
  border-radius: 6px;
  padding: 8px 16px;
  cursor: pointer;
  font-size: 13px;
  font-weight: bold;
}

.btn-volver:hover {
  background-color: #2c3e6b;
}

/* Título principal del formulario */
.form-title {
  text-align: center;
  margin-bottom: 5px;
  font-size: 22px;
}

/* Subtítulo debajo del título */
.form-subtitle {
  text-align: center;
  color: #888;
  font-size: 13px;
  margin-bottom: 25px;
}

/* Contenedor de cada campo del formulario */
.form-group {
  margin-bottom: 18px;
  font-size: 14px;
}

/* Etiqueta de cada campo en azul y negrita */
.form-group label {
  display: block;
  margin-bottom: 6px;
  font-weight: bold;
  color: #2c5fa8;
}

/* Estilo base para inputs y selects */
.form-control {
  width: 100%;
  padding: 10px;
  border: 1px solid #ccc;
  border-radius: 8px;
  font-size: 14px;
  box-sizing: border-box;
  background-color: #f5f5f5;
}

/* Campo con error resaltado en rojo */
.field-error {
  border-color: #e53935 !important;
}

/* Mensaje de error debajo del campo */
.error-msg {
  color: #e53935;
  font-size: 12px;
  margin-top: 4px;
  display: block;
}

/* Cuando el campo está deshabilitado se ve opaco */
.form-control:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

/* Contenedor de los botones al final del formulario */
.button-group {
  display: flex;
  gap: 15px;
  margin-top: 25px;
}
/* Botón Cancelar con fondo blanco y borde */
.btn-Cancelar {
  flex: 1;
  padding: 10px;
  background-color: white;
  border: 1px solid #ccc;
  border-radius: 8px;
  cursor: pointer;
  font-size: 14px;
}

/* Botón Registrar con fondo oscuro */
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
</style>
