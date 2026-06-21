<template>
    <div class="aircrafts-page">

        <div class="page-top-bar">
            <button class="btn-volver-aircrafts" @click="$router.push('/admin/routes')">Volver</button>
        </div>

        <div class="page-header">
            <h1 class="page-title">Gestión de Aeronaves</h1>
        </div>

        <div class="aircrafts-card">
            <div class="toolbar">
                <div class="search-wrapper">
                    <span class="search-icon">🔍</span>
                    <input
                        v-model="searchTerm"
                        type="text"
                        class="search-input"
                        placeholder="Buscar por modelo"
                    />
                </div>
                <button class="btn-registrer" @click="$router.push('/admin/register-aircraft')">
                    + Registrar aeronave
                </button>
            </div>
                <AppList
                    :columns="aircraftColumns"
                    :items="filteredAircrafts"
                    empty-message="No hay aeronaves disponibles"
                    >
                    <template #cell-actions="{ item }">
                        <div style="display: flex; gap: 8px;">
                            <button class="primaryButton" @click.stop="$router.push(`/admin/edit-aircraft/${item.id}`)">Editar</button>
                            <button class="dangerButton" @click.stop="confirmDelete(item)">Eliminar</button>
                        </div>
                    </template>
                </AppList>
                <p v-if="errorMsg" class="error-acceso">{{ errorMsg }}</p>
                <div class="table-footer-bar">
                    <p class="table-footer">Mostrando {{ filteredAircrafts.length }} de {{ aircrafts.length }} aeronaves</p>
                </div>
        </div>
    </div>
</template>

<script>
import axios from "axios";
import AppList from "../components/AppList.vue";
export default{
    name:"AircraftsList",
    components:{
        AppList
    },
    data(){
        return{
            aircrafts:[],
            searchTerm:"",
            errorMsg:"",
            aircraftColumns:[
                {key: "model", label:"Modelo"},
                {key: "touristRows", label:"Filas clase turista"},
                {key: "touristColumns", label:"Asientos por fila clase turista"},
                {key: "firstclassRows", label:"Filas clase ejecutiva"},
                {key: "firstclassColumns", label:"Asientos por fila clase ejecutiva"},
                {key: "maxWeight", label:"Peso máximo permitido"},
                { key: "actions", label: "Acciones" }
            ]
        }
    },
    computed: {
        filteredAircrafts() {
            const term = this.searchTerm.trim().toLowerCase();
            if (!term) {
                return this.aircrafts;
            }
            return this.aircrafts.filter(aircraft => 
                aircraft.model.toLowerCase().includes(term)
            );
        }
    },
    mounted() {
        this.loadAircrafts();
    },
    methods: {
        loadAircrafts() {
            const token = localStorage.getItem("token");
            axios.get(`${process.env.VUE_APP_BACKEND_URL}/airplane`, {
                headers: {Authorization: `Bearer ${token}`}
            }).then((response) => {
                this.aircrafts = response.data;
            }).catch((error) => {
                if(error.response && (error.response.status === 401 || error.response.status === 403)) {
                this.errorMsg = "Acceso no autorizado";
                }
                console.error("Error cargando aeronaves:", error);
            });
        },
        confirmDelete(aircraft) {
            if (!confirm(`¿Estás seguro de que deseas eliminar la aeronave "${aircraft.model}"?`)) return;
            const token = localStorage.getItem("token");
            axios.delete(`${process.env.VUE_APP_BACKEND_URL}/airplane/${aircraft.id}`, {
                headers: { Authorization: `Bearer ${token}` }
            }).then(() => {
                this.aircrafts = this.aircrafts.filter(a => a.id !== aircraft.id);
            }).catch((error) => {
                if (error.response && (error.response.status === 401 || error.response.status === 403)) {
                    this.errorMsg = "Acceso no autorizado";
                } else {
                    this.errorMsg = "Error al eliminar la aeronave.";
                }
                console.error("Error eliminando aeronave:", error);
            });
        }
    }
};
</script>


<style scoped>
.aircrafts-page {
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

.btn-volver-aircrafts {
  background-color: #1a2b4a;
  color: white;
  border: none;
  border-radius: 8px;
  padding: 10px 18px;
  cursor: pointer;
  font-size: 14px;
  font-weight: bold;
}

.btn-volver-aircrafts:hover {
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

.aircrafts-card {
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