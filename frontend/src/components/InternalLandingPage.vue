<template>
    <div class="admin-layout">
        <div class="logo-admin">
            <a href="/"><img src="../assets/logoSA.png" alt="logo" class="logo-image" /></a>
            <h3 class="logo-text">Snoopy Airlines</h3>
        </div>
        <div class="layout-body">   
            <aside class="left-menu">
                <router-link to="/admin/flights" class="left-menu-options">Administrar Vuelos</router-link>
                <router-link to="/admin/users" class="left-menu-options">Administrar Usuarios</router-link>
                <router-link to="/airports" class="left-menu-options">Administrar Aeropuertos</router-link>
                <router-link to="/admin/reports" class="left-menu-options">Reportes</router-link>
            </aside>
            <main class="content">
                <router-view />
            </main>
        </div>
    </div>
    <button class="logout-button">Cerrar sesión</button>
    
</template>

<script>
    export default {
                
    }
    const token = localStorage.getItem('token');
    console.log(token);
</script>

<style>

body {
    background-color: #f1f1f1;
}

.layout-body {
    display: flex;
    min-height: 100vh;
}

.content {
    flex: 1;
    padding: 20px;
    overflow: auto;
}

.logo-admin{
    text-align: left;
    width: 100vw;
    display: flex;
    text-size-adjust: 10px;
    border-bottom: 1px solid #b4b4b4;
    background-color: #f9f9f9;

    .logo-image {
        align-items: left;
        width: 45px;
        height: 45px;
    }

    .logo-text {
        font-size: 1.4rem;
        font-weight: 700;
        color: #2c3e50;
    }
}

.logout-button {
    position: fixed;
    top: 10px;
    right: 10px;
    border: 0;
    background-color: #f9f9f9;
    color: #818181;
    font-weight: bold;
}

.logout-button:hover {
    position: fixed;
    top: 10px;
    right: 10px;
    border: 0;
    background-color: #c4c4c4;
    border-radius: 10px;
    color: #f9f9f9;
}

.left-menu {
    width: 250px;
    height: 100vh;
    border-right: 1px solid #b4b4b4;
    background-color: #f9f9f9;
    padding-top: 20px;

    .left-menu-options {
        text-decoration: none;
        display: flex;
        align-items: center;
        justify-content: left;
        width: 220px;
        height: 20px;
        background-color: #f9f9f9;
        margin-bottom: 10px;
        margin-left: 10px;
        padding: 30px;
        color: #636363;
        font-weight: bold;
        border: 0;
    }

    .left-menu-options:hover {
        display: flex;
        align-items: center;
        justify-content: left;
        width: 220px;
        height: 20px;
        background-color: #e0eaff;
        margin-bottom: 10px;
        margin-left: 10px;
        padding: 30px;
        color: oklch(0.546 0.245 262.881);
        border-radius: 10px;
    }

    .router-link-active {
        display: flex;
        align-items: center;
        justify-content: left;
        width: 220px;
        height: 20px;
        background-color: #e0eaff;
        margin-bottom: 10px;
        margin-left: 10px;
        padding: 30px;
        color: oklch(0.546 0.245 262.881);
        border-radius: 10px;
    }

}

</style>