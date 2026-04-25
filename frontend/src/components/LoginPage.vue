<template>
    <div>
        <div class="logo"><h3>Snoopy Airlines</h3></div>
        <div class="login-form">
            <h3 class="login-title">Iniciar sesión</h3>
            <p class="welcome-message">Bienvenido a Snoopy Airlines</p>
            <form @submit.prevent="login">
                <div class="form-group">
                    <label for="correo">Correo electrónico:</label>
                    <!--v-model="formData.Email" -->
                    <input
                        type="email"
                        id="email"
                        class="form-control"
                        placeholder="nombre@correo.com"
                        required
                    />
                </div>
                <div class="form-group">
                    <label for="contrasena">Contraseña:</label>
                    <!--v-model="formData.Password" -->
                    <input
                        type="password"
                        id="password"
                        class="form-control"
                        placeholder="Contraseña"
                        required
                    />
                </div>
                <a href="/" class="forget-pw">
                    <p class="forget-pw">¿Olvidaste tu contraseña?</p>
                </a>
                <button type="submit" class="btn-login">Iniciar sesión</button>
            </form>
        </div>
    </div>
</template>

<script>
    export default {
        
    }
</script>

<style>

.logo {
    text-align: left;
    margin-top: 10px;
    text-size-adjust: 10px;
}

.login-form {
    width: 300px;
    margin: 0 auto;
    padding: 20px;
    border: 1px solid #ccc;
    border-radius: 5px;
    background-color: #f9f9f9;

    .login-title {
        text-align: center;
        margin-bottom: 10px;
    }

    .welcome-message {
        text-align: center;
        margin-bottom: 20px;
        color: #818181;
    }

    .btn-login {
        margin-top: 10px;
        margin-bottom: 10px;
        width: 100%;
        padding: 10px;
        background-color: oklch(0.546 0.245 262.881);
        color: white;
        border: none;
        border-radius: 5px;
        cursor: pointer;
    }

    .form-group {   
        margin-bottom: 15px;
        font-weight: bold;
        font-size: small;
    }

    .forget-pw {
        text-decoration: none;
        text-align: right;
        font-size: small;
    }
}

</style>