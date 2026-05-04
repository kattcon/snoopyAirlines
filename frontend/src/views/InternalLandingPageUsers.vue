<template>
    <div class="subsection-header">
        <h3 class="subsection-header-title">Administrar Usuarios</h3>
        <h6 class="subsection-header-subtitle">Gestiona y registra nuevos usuarios del sistema</h6>
    </div>
    <div class="container">
        <div class="section-div">
            <h4 class="main-text-section">Usuarios</h4>
            <p class="subtext-aeronave">Mira y registra un nuevo usuario</p>
            <router-link to="/createUser" class="router-button">Ir a Usuarios</router-link> <!--CAMBIAR POR LA RUTA DE CREAR Usuario-->
        </div>
        <div class="section-div">
            <h4 class="main-text-section">Ver Usuarios</h4>
            <p class="subtext-aeronave">Consulta los usuarios registrados en el sistema</p>
            <router-link to="/admin/list-users" class="router-button">Ver Usuarios</router-link>
        </div>
    </div>

</template>

<script>
    export default {
        
    }
</script>

<style>


</style>