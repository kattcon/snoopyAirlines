<template>
  <div class="page-background">
    <div class="register-form">
      <h2 class="form-title">Registro de Aeropuerto</h2>
      <p class="form-subtitle">Complete la información para registrar un nuevo aeropuerto</p>

      <!-- Campo para el nombre completo del aeropuerto -->
      <div class="form-group">
        <label>Nombre del Aeropuerto</label>
        <input
          v-model="airport.name"
          type="text"
          class="form-control"
          placeholder="ej. Juan Santamaría International Airport"
        />
      </div>

      <!-- Campo para el código IATA de 3 letras -->
      <div class="form-group">
        <label>Código IATA</label>
        <input
          v-model="airport.code"
          type="text"
          class="form-control"
          placeholder="ej. SJO"
        />
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
        <select v-model="airport.cityId" class="form-control" :disabled="!selectedCountry">
          <option value="">Primero seleccione un país</option>
          <option v-for="city in cities" :key="city.id" :value="city.id">
            {{ city.name }}
          </option>
        </select>
      </div>

      <!-- Botones de acción del formulario -->
      <div class="button-group">
        <button class="btn-Cancelar">Cancelar</button>
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
      cities: []
    };
  },
  // mounted() se ejecuta automáticamente cuando el componente carga en la página
  mounted() {
    this.loadCountries();
  },
  methods: {
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
