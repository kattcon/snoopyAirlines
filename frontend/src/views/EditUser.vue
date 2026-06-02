<template>
  <div>
    <p v-if="errorMessage" class="error-acceso">{{ errorMessage }}</p>
    <IntakePage
      v-model="user"
      back-label="Volver a la lista"
      title="Editar Usuario"
      subtitle="Modifique los datos del usuario"
      :fields="userFields"
      :errors="errors"
      submit-label="Guardar"
      @back="$router.push('/admin/list-users')"
      @cancel="$router.push('/admin/list-users')"
      @submit="updateUser"
    />
  </div>
</template>

<script>
import axios from "axios";
import IntakePage from "../components/IntakePage.vue";

export default {
  name: "EditUser",
  components: {
    IntakePage
  },
  data() {
    return {
      user: {
        firstName: "",
        lastNameOne: "",
        lastNameTwo: "",
        identificationNumber: "",
        type: "AD"
      },
      errors: {
        firstName: "",
        lastNameOne: "",
        identificationNumber: "",
        type: ""
      },
      errorMessage: ""
    };
  },
  computed: {
    userId() {
      return this.$route.params.id;
    },
    userFields() {
      return [
        {
          name: "firstName",
          label: "Nombre",
          type: "text",
          maxlength: 100,
          placeholder: "Nombre del usuario"
        },
        {
          name: "lastNameOne",
          label: "Primer apellido",
          type: "text",
          maxlength: 100,
          placeholder: "Primer apellido"
        },
        {
          name: "lastNameTwo",
          label: "Segundo apellido (opcional)",
          type: "text",
          maxlength: 100,
          placeholder: "Segundo apellido"
        },
        {
          name: "identificationNumber",
          label: "Número de identificación",
          type: "text",
          maxlength: 50,
          placeholder: "Número de identificación"
        },
        {
          name: "type",
          label: "Tipo de usuario",
          type: "select",
          options: [
            { value: "AD", label: "Administrador" },
            { value: "OP", label: "Operador" }
          ]
        }
      ];
    }
  },
  mounted() {
    this.loadUser();
  },
  methods: {
    loadUser() {
      const token = localStorage.getItem("token");
      axios
        .get(`${process.env.VUE_APP_BACKEND_URL}/user/${this.userId}`, {
          headers: { Authorization: `Bearer ${token}` }
        })
        .then((response) => {
          const data = response.data;
          this.user.firstName = data.firstName ?? "";
          this.user.lastNameOne = data.lastNameOne ?? "";
          this.user.lastNameTwo = data.lastNameTwo ?? "";
          this.user.identificationNumber = data.identificationNumber ?? "";
          this.user.type = data.type === 0 ? "AD" : "OP";
        })
        .catch((error) => {
          if (error.response) {
            if (error.response.status === 404) {
              this.errorMessage = "No se encontró el usuario.";
              setTimeout(() => this.$router.push("/admin/list-users"), 2000);
            } else if (error.response.status === 401 || error.response.status === 403) {
              this.errorMessage = "Acceso no autorizado.";
            } else {
              this.errorMessage = "Error al cargar el usuario.";
            }
          } else {
            this.errorMessage = "Error de conexión con el servidor.";
          }
          console.error("Error cargando usuario:", error);
        });
    },
    validate() {
      this.errors = { firstName: "", lastNameOne: "", identificationNumber: "", type: "" };
      let valid = true;

      if (!this.user.firstName || !this.user.firstName.trim()) {
        this.errors.firstName = "Campo obligatorio";
        valid = false;
      }
      if (!this.user.lastNameOne || !this.user.lastNameOne.trim()) {
        this.errors.lastNameOne = "Campo obligatorio";
        valid = false;
      }
      if (!this.user.identificationNumber || !this.user.identificationNumber.trim()) {
        this.errors.identificationNumber = "Campo obligatorio";
        valid = false;
      }
      if (!this.user.type || !["AD", "OP"].includes(this.user.type)) {
        this.errors.type = "Tipo inválido";
        valid = false;
      }

      return valid;
    },
    updateUser() {
      if (!this.validate()) return;

      const token = localStorage.getItem("token");
      const payload = {
        firstName: this.user.firstName.trim(),
        lastNameOne: this.user.lastNameOne.trim(),
        lastNameTwo: this.user.lastNameTwo?.trim() || null,
        identificationNumber: this.user.identificationNumber.trim(),
        type: this.user.type
      };

      axios
        .put(`${process.env.VUE_APP_BACKEND_URL}/user/${this.userId}`, payload, {
          headers: { Authorization: `Bearer ${token}` }
        })
        .then(() => {
          this.$router.push("/admin/list-users");
        })
        .catch((error) => {
          if (error.response) {
            if (error.response.status === 404) {
              this.errorMessage = "No se encontró el usuario.";
            } else if (error.response.status === 401 || error.response.status === 403) {
              this.errorMessage = "Acceso no autorizado.";
            } else if (error.response.status === 400) {
              this.errorMessage = "Datos inválidos. Revise los campos e intente de nuevo.";
            } else {
              this.errorMessage = "Error al actualizar el usuario.";
            }
          } else {
            this.errorMessage = "Error de conexión con el servidor.";
          }
          console.error("Error actualizando usuario:", error);
        });
    }
  }
};
</script>

<style scoped>
.error-acceso {
  color: #e53935;
  font-weight: bold;
  text-align: center;
  margin-top: 24px;
  font-size: 15px;
}
</style>
