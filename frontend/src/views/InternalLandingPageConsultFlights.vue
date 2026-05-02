<template>
    <div class="subsection-header">
        <h3 class="subsection-header-title">Consultar Vuelos</h3>
        <h6 class="subsection-header-subtitle">Gestiona aeronaves, aeropuertos, rutas y vuelos</h6>
    </div>
    <div class="container">
        <div class="section-div">
            <h4 class="main-text-section">Ver Aeronaves</h4>
            <p class="subtext-aeronave">Registra una nueva aeronave en el sistema</p>
            <router-link to="/createFlight" class="router-button">Ver Aeronaves</router-link> <!-- TODO: ruta pendiente -->
        </div>
        <div class="section-div">
            <h4 class="main-text-section">Ver Aeropuertos</h4>
            <p class="subtext-aeronave">Agrega un nuevo aeropuerto</p>
            <router-link to="/admin/airports" class="router-button">Ver Aeropuertos</router-link>
        </div>
        <div class="section-div">
            <h4 class="main-text-section">Ver Rutas</h4>
            <p class="subtext-aeronave">Crea una nueva ruta en el sistema</p>
            <router-link to="/createAirport" class="router-button">Ver Rutas</router-link> <!-- TODO: ruta pendiente -->
        </div>
        <div class="section-div">
            <h4 class="main-text-section">Ver Vuelos</h4>
            <p class="subtext-aeronave">Agrega un nuevo vuelo</p>
            <router-link to="/createFlight" class="router-button">Ver Vuelos</router-link> <!-- TODO: ruta pendiente -->
        </div>
    </div>
</template>

<script>
    export default {
        name: "InternalLandingPageConsultFlights"
    }
</script>
