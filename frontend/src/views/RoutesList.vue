<template>
    <div class="aircrafts-page">

        <div class="page-top-bar">
            <button class="btn-volver-aircrafts" @click="$router.push('/admin/routes')">Volver</button>
        </div>

        <div class="page-header">
            <h1 class="page-title">Gestión de Rutas</h1>
        </div>

        <div class="aircrafts-card">
            <div class="toolbar">
                <div class="search-wrapper">
                    <span class="search-icon">🔍</span>
                    <input
                        v-model="searchTerm"
                        type="text"
                        class="search-input"
                        placeholder="Buscar por aeropuerto"
                    />
                </div>
                <button class="btn-registrer" @click="$router.push('/admin/register-route')">
                    + Registrar ruta
                </button>
            </div>
                <AppList
                    :columns="routeColumns"
                    :items="filteredRoutes"
                    empty-message="No hay rutas disponibles"
                    >
                    <template #cell-actions="{ item }">
                        <div style="display: flex; gap: 8px;">
                            <button class="dangerButton" @click.stop="confirmDelete(item)">Eliminar</button>
                        </div>
                    </template>
                </AppList>
                <p v-if="errorMsg" class="error-acceso">{{ errorMsg }}</p>
                <div class="table-footer-bar">
                    <p class="table-footer">Mostrando {{ filteredRoutes.length }} de {{ activeRoutes.length }} rutas</p>
                </div>
        </div>
    </div>

    <div
        v-if="showDeleteModal"
        class="modal-overlay"
        @click.self="showDeleteModal = false"
    >
        <div class="delete-modal">
            <h2>Eliminar ruta</h2>

            <p>
                ¿Está seguro de que desea eliminar la ruta?
            </p>

            <p class="route-name" v-if="routeToDelete">
                <strong>
                    {{ routeToDelete.departureAirport }}
                    →
                    {{ routeToDelete.arrivalAirport }}
                </strong>
            </p>

            <div class="modal-buttons">
                <button
                    class="cancelButton"
                    @click="showDeleteModal = false"
                >
                    Cancelar
                </button>

                <button
                    class="dangerButton"
                    @click="deleteRoute"
                >
                    Eliminar
                </button>
            </div>
        </div>
    </div>

    <div
        v-if="showErrorModal"
        class="modal-overlay"
        @click.self="showErrorModal = false"
    >
        <div class="delete-modal">
            <h2>{{ errorTitle }}</h2>

            <p>{{ errorMessage }}</p>

            <div class="modal-buttons">
                <button
                    class="cancelButton"
                    @click="showErrorModal = false"
                >
                    Aceptar
                </button>
            </div>
        </div>
    </div>

</template>
<script>
import axios from "axios";
import AppList from "../components/AppList.vue";

export default{
    name:"RoutesList",
    components:{
        AppList
    },
    data(){
        return{
            routes:[],
            searchTerm:"",
            errorMsg:"",

            showDeleteModal: false,
            routeToDelete: null,

            showErrorModal: false,
            errorTitle: "",
            errorMessage: "",

            errorMessages: {
                400: {
                    title: "Solicitud inválida",
                    message: "La solicitud enviada no es válida."
                },
                401: {
                    title: "Acceso denegado",
                    message: "Debe iniciar sesión."
                },
                403: {
                    title: "Acceso denegado",
                    message: "No tiene permisos para realizar esta acción."
                },
                404: {
                    title: "No encontrado",
                    message: "El recurso solicitado no existe."
                },
                500: {
                    title: "Error interno",
                    message: "Ocurrió un error en el servidor."
                }
            },

            routeColumns:[
                {key: "airplaneModel", label:"Avión asignado"},
                {key: "departureAirport", label:"Aeropuerto de salida"},
                {key: "arrivalAirport", label:"Aeropuerto de llegada"},
                {key: "departureTime", label:"Hora de salida"},
                {key: "arrivalTime", label:"Hora de llegada"},
                { key: "actions", label: "Acciones" }
            ]
        }
    },
    computed: {
        filteredRoutes() {
            const term = this.searchTerm.trim().toLowerCase();
            const mapped = this.routes.filter(route => !route.isDeleted).map(route => ({
                ...route,
                departureAirport: `${route.departureAirportCode} - ${route.departureAirportName}`,
                arrivalAirport: `${route.arrivalAirportCode} - ${route.arrivalAirportName}`,
                departureTime: this.formatTime(route.departureTime),
                arrivalTime: this.formatTime(route.arrivalTime)
            }));

            if (!term) {
                return mapped;
            }
            return mapped.filter(route =>
                route.departureAirport.toLowerCase().includes(term) ||
                route.arrivalAirport.toLowerCase().includes(term) ||
                route.airplaneModel.toLowerCase().includes(term)
            );
        },
        activeRoutes() {
            return this.routes.filter(route => !route.isDeleted);
        }
    },
    mounted() {
        this.loadRoutes();
    },
    methods: {
        formatTime(value) {
            if (!value) return "";
            const [hours, minutes] = value.split(":");
            return `${hours}:${minutes}`;
        },
        loadRoutes() {
            const token = localStorage.getItem("token");
            axios.get(`${process.env.VUE_APP_BACKEND_URL}/route/list`, {
                headers: {Authorization: `Bearer ${token}`}
            }).then((response) => {
                this.routes = response.data.filter(route => !route.isDeleted);
            }).catch((error) => {
                this.handleApiError(error);
                console.error(error);

            });
        },
        confirmDelete(route) {
            this.routeToDelete = route;
            this.showDeleteModal = true;
        },
        deleteRoute() {
            if (!this.routeToDelete || !this.routeToDelete.id) {
                this.showDeleteModal = false;
                this.showError(
                    "Error",
                    "No se pudo identificar la ruta que se desea eliminar."
                );
                return;
            }

            const token = localStorage.getItem("token");

            axios.delete(
                `${process.env.VUE_APP_BACKEND_URL}/route/${this.routeToDelete.id}`,
                {
                    headers: {
                        Authorization: `Bearer ${token}`
                    }
                }
            ).then(() => {

                this.routes = this.routes.filter(
                    r => r.id !== this.routeToDelete.id
                );

                this.showDeleteModal = false;
                this.routeToDelete = null;

            }).catch((error) => {
                this.handleApiError(error);
                console.error(error);
            });
        },
        showError(title, message) {
            this.errorTitle = title;
            this.errorMessage = message;
            this.showErrorModal = true;
        },
        handleApiError(error) {

            if (!error.response) {
                this.showError(
                    "Error de conexión",
                    "No fue posible conectarse con el servidor."
                );
                return;
            }

            const errorInfo = this.errorMessages[error.response.status] ?? {
                title: "Error",
                message: "Ocurrió un error inesperado."
            };

            this.showError(
                errorInfo.title,
                error.response.data || errorInfo.message
            );
        },
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

.modal-overlay {
    position: fixed;
    inset: 0;
    background: rgba(0,0,0,.45);
    display: flex;
    justify-content: center;
    align-items: center;
    z-index: 1000;
}

.delete-modal {
    width: 420px;
    max-width: 90%;
    background: white;
    border-radius: 10px;
    padding: 24px;
    box-shadow: 0 10px 25px rgba(0,0,0,.2);
}

.modal h2 {
    margin-top: 0;
    margin-bottom: 16px;
}

.route-name {
    margin: 20px 0;
    color: #444;
}

.modal-buttons {
    display: flex;
    justify-content: flex-end;
    gap: 12px;
}

.cancelButton {
    background: #e5e5e5;
    color: #333;
    border: none;
    padding: 10px 18px;
    border-radius: 6px;
    cursor: pointer;
}

.cancelButton:hover {
    background: #d6d6d6;
}

</style>