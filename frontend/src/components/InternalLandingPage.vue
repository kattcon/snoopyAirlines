<template>
    <div class="logo-admin">
            <a href="/"><img src="../assets/logoSA.png" alt="logo" class="logo-image" /></a>
            <h3>Snoopy Airlines</h3>
        </div>
    <div class="left-menu">
        <button class="left-menu-options">Administrar Vuelos</button>
        <button class="left-menu-options">Administrar Usuarios</button>
        <button class="left-menu-options">Reportes</button>
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

.logo-admin{
    text-align: left;
    width: 100vw;
    display: flex;
    text-size-adjust: 10px;
    border-bottom: 1px solid #b4b4b4;
    background-color: #f9f9f9;

    .logo-image {
        align-items: left;
        width: 50px;
        height: 50px;
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

}

</style>