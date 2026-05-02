<template>
  <div class="airports-page">
    <!-- Encabezado de la página -->
    <div class="page-header">
      <h1 class="page-title">Snoopy Airlines</h1>
      <p class="page-subtitle">Gestión de Aeropuertos</p>
    </div>

    <!-- Tarjeta principal con la tabla -->
    <div class="airports-card">

      <!-- Barra de herramientas: búsqueda y botón registrar -->
      <div class="toolbar">
        <div class="search-wrapper">
          <span class="search-icon">🔍</span>
          <input
            v-model="searchTerm"
            type="text"
            class="search-input"
            placeholder="Buscar por nombre, código o ciudad..."
          />
        </div>
        <button class="btn-register" @click="$router.push('/register-airport')">
          + Registrar aeropuerto
        </button>
      </div>

      <!-- Tabla de aeropuertos -->
      <table class="airports-table">
        <thead>
          <tr>
            <th>Nombre</th>
            <th>Código IATA</th>
            <th>Ciudad</th>
            <th>País</th>
          </tr>
        </thead>
        <tbody>
          <!-- Fila por cada aeropuerto filtrado -->
          <tr v-for="airport in filteredAirports" :key="airport.id">
            <td>{{ airport.name }}</td>
            <td><span class="iata-badge">{{ airport.code }}</span></td>
            <td>{{ airport.cityName }}</td>
            <td>{{ airport.countryName }}</td>
          </tr>
          <!-- Mensaje cuando no hay resultados -->
          <tr v-if="filteredAirports.length === 0">
            <td colspan="4" class="no-results">No se encontraron aeropuertos</td>
          </tr>
        </tbody>
      </table>

      <!-- Pie de tabla con el conteo -->
      <p class="table-footer">
        Mostrando {{ filteredAirports.length }} de {{ airports.length }} aeropuertos
      </p>
    </div>
  </div>
</template>

<script>
import axios from "axios";

export default {
  name: "AirportsList",
  data() {
    return {
      // Lista completa de aeropuertos traída del backend
      airports: [],
      // Término ingresado en el buscador
      searchTerm: ""
    };
  },
  computed: {
    // Filtra los aeropuertos localmente mientras el usuario escribe
    filteredAirports() {
      const term = this.searchTerm.trim().toLowerCase();
      if (!term) return this.airports;
      return this.airports.filter(
        (a) =>
          a.name.toLowerCase().includes(term) ||
          a.code.toLowerCase().includes(term) ||
          a.cityName.toLowerCase().includes(term)
      );
    }
  },
  mounted() {
    this.loadAirports();
  },
  methods: {
    // Carga todos los aeropuertos desde el backend al iniciar la vista
    loadAirports() {
      const token = localStorage.getItem("token");
      axios
        .get("https://localhost:7080/airport", {
          headers: { Authorization: `Bearer ${token}` }
        })
        .then((response) => {
          this.airports = response.data;
        })
        .catch((error) => {
          console.error("Error cargando aeropuertos:", error);
        });
    }
  }
};
</script>

<style>
/* Fondo gris claro de la página */
.airports-page {
  min-height: 100vh;
  background-color: #f0f2f5;
  padding: 40px;
  box-sizing: border-box;
}

/* Encabezado superior con título y subtítulo */
.page-header {
  margin-bottom: 24px;
}

.page-title {
  font-size: 24px;
  font-weight: bold;
  margin: 0;
  color: #1a1a1a;
}

.page-subtitle {
  font-size: 14px;
  color: #666;
  margin: 4px 0 0 0;
}

/* Tarjeta blanca que contiene toda la tabla */
.airports-card {
  background-color: white;
  border-radius: 12px;
  padding: 24px;
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.08);
}

/* Barra con buscador y botón registrar */
.toolbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

/* Contenedor del input de búsqueda con ícono */
.search-wrapper {
  display: flex;
  align-items: center;
  border: 1px solid #ddd;
  border-radius: 8px;
  padding: 8px 12px;
  width: 320px;
  background-color: #fafafa;
}

.search-icon {
  margin-right: 8px;
  font-size: 14px;
  color: #999;
}

.search-input {
  border: none;
  outline: none;
  background: transparent;
  font-size: 14px;
  width: 100%;
  color: #333;
}

/* Botón para ir al formulario de registro */
.btn-register {
  padding: 10px 18px;
  background-color: white;
  border: 1px solid #ccc;
  border-radius: 8px;
  cursor: pointer;
  font-size: 14px;
  color: #1a1a1a;
}

.btn-register:hover {
  background-color: #f5f5f5;
}

/* Tabla de aeropuertos */
.airports-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 14px;
}

.airports-table th {
  text-align: left;
  padding: 10px 16px;
  color: #555;
  font-weight: 500;
  border-bottom: 1px solid #eee;
}

.airports-table td {
  padding: 14px 16px;
  border-bottom: 1px solid #f0f0f0;
  color: #333;
}

.airports-table tbody tr:last-child td {
  border-bottom: none;
}

/* Badge azul para el código IATA */
.iata-badge {
  background-color: #e8f0fe;
  color: #3b6fd4;
  padding: 4px 10px;
  border-radius: 20px;
  font-weight: 600;
  font-size: 13px;
}

/* Mensaje cuando no hay resultados */
.no-results {
  text-align: center;
  color: #999;
  padding: 30px;
}

/* Pie de la tabla con el conteo */
.table-footer {
  margin-top: 16px;
  font-size: 13px;
  color: #888;
}
</style>
