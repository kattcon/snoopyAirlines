<template>
    <div>
        <div class="logo"><h1>Snoopy Airlines</h1></div>
        <div class="login-form">
            <h2>Iniciar sesión</h2>
            <h3>Bienvenido a Snoopy Airlines</h3>
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
                <button type="submit" class="btn btn-primary">Iniciar sesión</button>
                <a href="/">
                    <p>¿Olvidaste tu contraseña?</p>
                </a>
            </form>
        </div>
    </div>
</template>

<script>
    export default {
        
    }
</script>

<style lang="scss" scoped>

</style>