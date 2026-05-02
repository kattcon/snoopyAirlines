<template>
    <div class="subsection-header">
        <h3 class="subsection-header-title">Administrar Vuelos</h3>
        <h6 class="subsection-header-subtitle">Gestiona aeronaves, aeropuertos, rutas y vuelos</h6>
    </div>
    <div class="container">
        <div class="section-div">
            <h4 class="main-text-section">Crear Aeronave</h4>
            <p class="subtext-aeronave">Registra una nueva aeronave en el sistema</p>
            <router-link to="/createFlight" class="router-button">Crear Aeronave</router-link> <!--CAMBIAR POR LA RUTA DE CREAR Vuelos-->
        </div>
        <div class="section-div">
            <h4 class="main-text-section">Crear Aeropuerto</h4>
            <p class="subtext-aeronave">Agrega un nuevo aeropuerto</p>
            <router-link to="/createRoute" class="router-button">Crear Aeropuerto</router-link> <!--CAMBIAR POR LA RUTA DE CREAR Aeropuertos-->
        </div>
        <div class="section-div">
            <h4 class="main-text-section">Crear Ruta</h4>
            <p class="subtext-aeronave">Crea una nueva ruta en el sistema</p>
            <router-link to="/createAirport" class="router-button">Crear Ruta</router-link> <!--CAMBIAR POR LA RUTA DE CREAR Rutas-->
        </div>
        <div class="section-div">
            <h4 class="main-text-section">Crear Vuelo</h4>
            <p class="subtext-aeronave">Agrega un nuevo vuelo</p>
            <router-link to="/createFlight" class="router-button">Crear Vuelo</router-link> <!--CAMBIAR POR LA RUTA DE CREAR Vuelos-->
        </div>
    </div>

</template>

<script>
    export default {
        
    }
</script>

<style>

.container {
    display: grid;
    grid-template-columns: repeat(2, 1fr);
    gap: 20px;
}

.subsection-header {
    padding-top: 20px;
    padding-left: 15px;
    padding-bottom: 30px;
}

.subsection-header-title {
    font-weight: bold;
}

.subsection-header-subtitle {
    color: #636363;
}

.section-div {
    width: 500px;
    height: 180px;
    border-radius: 10px;
    background-color: #f9f9f9;
    box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
}

.main-text-section {
    padding-left: 50px;
    padding-top: 20px;
}

.subtext-aeronave {
    padding-left: 50px;
    padding-top: 1px;
    margin-bottom: 30px;
    color: #636363;
}

.router-button {
    margin-left: 50px;
    text-decoration: none;
    border: 1px solid black;
    border-radius: 5px;
    padding: 10px;
    font-weight: bold;
    color:#f9f9f9;
    background-color: oklch(0.546 0.245 262.881);
    border: 0;
}

.router-button:hover {
    text-decoration: none;
    border: 1px solid black;
    border-radius: 5px;
    padding: 10px;
    font-weight: bold;
    color:oklch(0.546 0.245 262.881);
    background-color: #f9f9f9;
    border: 0;
}

</style>