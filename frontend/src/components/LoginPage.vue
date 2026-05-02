<template>
    <div class="login-container">
        <div class="logo">
            <a href="/"><img src="../assets/logoSA.png" alt="logo" class="logo-image" /></a>
            <h3 class="logo-text">Snoopy Airlines</h3>
        </div>
        <div class="login-form">
            <img src="../assets/logoSA.png" alt="logo" class="logo-image-login" />
            <h3 class="login-title">Iniciar sesión</h3>
            <p class="welcome-message">Bienvenido a Snoopy Airlines</p>
            <form @submit.prevent="login">
                <div class="form-group-email">
                    <label for="correo" class="email-title">Correo electrónico:</label>
                    <input
                        v-model="formData.Email"
                        type="email"
                        id="email"
                        class="form-control"
                        placeholder="nombre@correo.com"
                        required
                    />
                </div>
                <div class="form-group-password">
                    <label for="contrasena" class="password-title">Contraseña:</label>
                    <input
                        v-model="formData.Password"
                        type="password"
                        id="password"
                        class="form-control"
                        placeholder="Contraseña"
                        required
                    />
                </div>
                <button type="submit" class="btn-login">Iniciar sesión</button>
            </form>
        </div>
        <a class="volver-a-inicio" href="/">
            <p>Volver al inicio</p>
        </a>
    </div>
</template>

<script>
    import axios from 'axios';
    export default {
        data() {
            return {
                formData: {
                    Email: '',
                    Password: ''
                }
            };
        },
        methods: {
            login() {
                axios
                    .post('https://localhost:7080/user/login', {
                        email: this.formData.Email,
                        password: this.formData.Password
                    })
                    .then(function (response) {
                        localStorage.setItem('token', response.data.token);
                        alert("Inicio de sesión exitoso");
                        console.log(response);
                        window.location.href = '/admin';
                    })
                    .catch(function (error) {
                        if (error.response && error.response.status === 401)
                        {
                            alert("Correo electrónico o contraseña incorrectos");
                        }
                        else
                        {
                            alert("Error al iniciar sesión.");
                        }
                        console.error(error);
                    });
            },
        },
    };
</script>

<style>

body {
    background-color: #f9f9f9;
}


.logo{
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

.volver-a-inicio {
    text-align: center;
    text-decoration: none;
    color: #818181;
    
}

.volver-a-inicio:hover {
    text-align: center;
    text-decoration: none;
    color: #818181;
    
}

.login-form {
    width: 450px;
    height: 530px;
    margin: 0 auto;
    padding: 25px;
    border-radius: 20px;
    background-color: #ffffff;
    box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
    margin-bottom: 40px;
    margin-top: 30px;

    .logo-image-login {
        width: 120px;
        height: 120px;
        display: block;
        margin: 0 auto;
    }
    
    .login-title {
        margin-top: 0 auto;
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
        border-radius: 15px;
        cursor: pointer;
    }

    .form-group-email {   
        margin-bottom: 20px;
        font-weight: bold;
        font-size: medium;

        .email-title {
            margin-bottom: 10px;
        }
    }

    .form-group-password {   
        margin-bottom: 30px;
        font-weight: bold;
        font-size: medium;

        .password-title {
            margin-bottom: 10px;
        }
    }

    .form-control {
        width: 100%;
        padding: 10px;
        border: 1px solid #ccc;
        border-radius: 5px;
    }
}


</style>