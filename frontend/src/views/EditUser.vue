<template>
  <IntakePage
    v-model="user"
    back-label="Volver"
    title="Editar usuario"
    subtitle="Modifique los datos del usuario."
    :fields="userFields"
    :errors="errors"
    submit-label="Guardar cambios"
    @back="$router.push('/admin/list-users')"
    @cancel="$router.push('/admin/list-users')"
    @submit="updateUser"
  />
</template>

<script>
import axios from "axios";
import IntakePage from "../components/IntakePage.vue";

const USER_TYPE_OPTIONS = [
  { value: "ad", label: "Administrador" },
  { value: "op", label: "Operador" }
];

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
        type: ""
      },
      errors: {
        firstName: "",
        lastNameOne: "",
        lastNameTwo: "",
        identificationNumber: "",
        type: ""
      }
    };
  },
  computed: {
    userId() {
      return this.$route.params.id;
    },
    userFields() {
      return [
        { name: "firstName", label: "Nombre", type: "text", placeholder: "Nombre" },
        { name: "lastNameOne", label: "Primer apellido", type: "text", placeholder: "Primer apellido" },
        { name: "lastNameTwo", label: "Segundo apellido (opcional)", type: "text", placeholder: "Segundo apellido" },
        { name: "identificationNumber", label: "Número de identificación", type: "text", placeholder: "Número de identificación" },
        { name: "type", label: "Tipo de usuario", type: "select", placeholder: "Seleccione un tipo", options: USER_TYPE_OPTIONS }
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
          this.user = {
            firstName: data.firstName,
            lastNameOne: data.lastNameOne,
            lastNameTwo: data.lastNameTwo ?? "",
            identificationNumber: data.identificationNumber,
            type: data.type === 0 ? "ad" : "op"
          };
        })
        .catch((error) => {
          if (error.response && error.response.status === 404) {
            alert("No se encontró el usuario.");
            this.$router.push("/admin/list-users");
          } else if (error.response && (error.response.status === 401 || error.response.status === 403)) {
            alert("Acceso no autorizado.");
            this.$router.push("/admin/list-users");
          } else {
            alert("Ocurrió un error al cargar el usuario.");
          }
        });
    },
    validate() {
      this.errors = { firstName: "", lastNameOne: "", lastNameTwo: "", identificationNumber: "", type: "" };
      let valid = true;

      if (!this.user.firstName.trim()) {
        this.errors.firstName = "El nombre es requerido.";
        valid = false;
      }
      if (!this.user.lastNameOne.trim()) {
        this.errors.lastNameOne = "El primer apellido es requerido.";
        valid = false;
      }
      if (!this.user.identificationNumber.trim()) {
        this.errors.identificationNumber = "El número de identificación es requerido.";
        valid = false;
      }
      if (!this.user.type) {
        this.errors.type = "El tipo de usuario es requerido.";
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
        lastNameTwo: this.user.lastNameTwo.trim() || null,
        identificationNumber: this.user.identificationNumber.trim(),
        type: this.user.type
      };

      axios
        .put(`${process.env.VUE_APP_BACKEND_URL}/user/${this.userId}`, payload, {
          headers: { Authorization: `Bearer ${token}` }
        })
        .then(() => {
          alert("Usuario actualizado exitosamente.");
          this.$router.push("/admin/list-users");
        })
        .catch((error) => {
          if (error.response && error.response.status === 404) {
            alert("No se encontró el usuario.");
          } else if (error.response && (error.response.status === 401 || error.response.status === 403)) {
            alert("Acceso no autorizado.");
          } else if (error.response && error.response.status === 400) {
            alert("Datos inválidos. Por favor, revise el formulario.");
          } else {
            alert("Ocurrió un error al actualizar el usuario. Por favor, inténtelo de nuevo.");
          }
        });
    }
  }
};
</script>
