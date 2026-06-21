<template>
  <div class="airports-page">
    <div class="page-top-bar">
      <button class="btn-volver-airports" @click="$router.push('/admin/routes')">Volver</button>
    </div>

    <div class="page-header">
      <h1 class="page-title">Gestión de Aeropuertos</h1>
    </div>

    <div class="airports-card">
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
        <button class="btn-register" @click="$router.push('/admin/register-airport')">
          + Registrar aeropuerto
        </button>
      </div>

      <AppList
        :columns="airportColumns"
        :items="filteredAirports"
        empty-message="No hay aeropuertos disponibles."
      >
        <template #cell-code="{ value }">
          <span class="iata-badge">{{ value }}</span>
        </template>
        <template #cell-actions="{ item: airport }">
          <div style="display: flex; gap: 8px;">
            <button class="primaryButton" @click="$router.push(`/admin/edit-airport/${airport.id}`)">Editar</button>
            <button class="dangerButton" @click="confirmDelete(airport)">Eliminar</button>
          </div>
        </template>
      </AppList>

      <p v-if="errorMsg" class="error-acceso">{{ errorMsg }}</p>

      <div class="table-footer-bar">
        <p class="table-footer">Mostrando {{ filteredAirports.length }} de {{ airports.length }} aeropuertos</p>
        <button class="btn-crear">Crear</button>
      </div>
    </div>
  </div>
</template>

<script>
import axios from "axios";
import AppList from "../components/AppList.vue";

export default {
  name: "AirportsList",
  components: {
    AppList
  },
  data() {
    return {
      airports: [],
      searchTerm: "",
      errorMsg: "",
      airportColumns: [
        { key: "name", label: "Nombre" },
        { key: "code", label: "Código IATA" },
        { key: "cityName", label: "Ciudad" },
        { key: "countryName", label: "País" },
        { key: "actions", label: "Acciones" }
      ]
    };
  },
  computed: {
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
    loadAirports() {
      const token = localStorage.getItem("token");
      axios
        .get(`${process.env.VUE_APP_BACKEND_URL}/airport`, {
          headers: { Authorization: `Bearer ${token}` }
        })
        .then((response) => {
          this.airports = response.data;
        })
        .catch((error) => {
          if (error.response && (error.response.status === 401 || error.response.status === 403)) {
            this.errorMsg = "Acceso no autorizado";
          }
          console.error("Error cargando aeropuertos:", error);
        });
    },
    confirmDelete(airport) {
      if (!confirm(`¿Estás seguro de que deseas eliminar el aeropuerto "${airport.name}" (${airport.code})?`)) return;
      const token = localStorage.getItem("token");
      axios
        .delete(`${process.env.VUE_APP_BACKEND_URL}/airport/${airport.id}`, {
          headers: { Authorization: `Bearer ${token}` }
        })
        .then(() => {
          this.airports = this.airports.filter((a) => a.id !== airport.id);
        })
        .catch((error) => {
          if (error.response && (error.response.status === 401 || error.response.status === 403)) {
            this.errorMsg = "Acceso no autorizado";
          } else {
            this.errorMsg = "Error al eliminar el aeropuerto.";
          }
          console.error("Error eliminando aeropuerto:", error);
        });
    }
  }
};
</script>

<style scoped>
.airports-page {
  position: relative;
  min-height: 100%;
  background: linear-gradient(to bottom right, #1a3a6b, #b0bec5);
  padding: 40px;
  box-sizing: border-box;
}

.page-top-bar {
  position: absolute;
  top: 24px;
  right: 30px;
}

.btn-volver-airports {
  background-color: #1a2b4a;
  color: white;
  border: none;
  border-radius: 8px;
  padding: 10px 18px;
  cursor: pointer;
  font-size: 14px;
  font-weight: bold;
}

.btn-volver-airports:hover {
  background-color: #2c3e6b;
}

.page-header {
  margin-bottom: 24px;
}

.page-title {
  font-size: 22px;
  font-weight: bold;
  margin: 0;
  color: white;
}

.airports-card {
  background-color: white;
  border-radius: 12px;
  padding: 24px;
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.08);
}

.toolbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

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

.iata-badge {
  background-color: #e8f0fe;
  color: #3b6fd4;
  padding: 4px 10px;
  border-radius: 20px;
  font-weight: 600;
  font-size: 13px;
}

.table-footer-bar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: 16px;
}

.table-footer {
  font-size: 13px;
  color: #888;
  margin: 0;
}

.btn-crear {
  background-color: #1a2b4a;
  color: white;
  border: none;
  border-radius: 6px;
  padding: 8px 16px;
  cursor: pointer;
  font-size: 13px;
}

.btn-crear:hover {
  background-color: #2c3e6b;
}

.error-acceso {
  color: #e53935;
  font-weight: bold;
  margin-top: 12px;
  text-align: center;
}
</style>
