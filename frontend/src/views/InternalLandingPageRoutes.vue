<template>
    <div class="subsection-header">
        <h3 class="subsection-header-title">Administrar Rutas</h3>
        <h6 class="subsection-header-subtitle">Gestiona aeronaves, aeropuertos y rutas</h6>
    </div>
    <div class="container">
        <div class="section-div">
            <h4 class="main-text-section">Aeronaves</h4>
            <p class="subtext-aeronave">Mira y registra una nueva aeronave en el sistema</p>
            <router-link to="/admin/list-aircrafts" class="router-button">Ir a Aeronaves</router-link> <!--CAMBIAR POR LA RUTA DE CREAR Rutas-->
        </div>
        <div class="section-div">
            <h4 class="main-text-section">Aeropuertos</h4>
            <p class="subtext-aeronave">Mira y registra un nuevo aeropuerto</p>
            <router-link to="/admin/airports" class="router-button">Ir a Aeropuertos</router-link>
        </div>
        <div class="section-div">
            <h4 class="main-text-section">Rutas</h4>
            <p class="subtext-aeronave">Mira y registra una nueva ruta</p>
            <router-link to="/admin/list-routes" class="router-button">Ir a Rutas</router-link> <!--CAMBIAR POR LA RUTA DE CREAR Rutas-->
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
    color: #ffffff;
}

.subsection-header-subtitle {
    color: #ffffff;
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
