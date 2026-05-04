<template>
  <IntakePage
    v-model="user"
    back-label="Volver a usuarios"
    title="Registro de Usuario"
    subtitle="Complete la informacion para invitar un nuevo usuario"
    :fields="userFields"
    :errors="errors"
    submit-label="Registrar"
    @back="$router.push('/admin/list-users')"
    @cancel="clearForm"
    @submit="registerUser"
  />
</template>

<script>
import axios from "axios";
import IntakePage from "../components/IntakePage.vue";

export default {
  name: "RegisterUser",
  components: {
    IntakePage
  },
  data() {
    return {
      user: this.emptyUser(),
      errors: this.emptyErrors()
    };
  },
  computed: {
    userFields() {
      return [
        {
          name: "identificationNumber",
          label: "Numero de identificacion",
          type: "text",
          maxlength: 50,
          placeholder: "ej. 1-1111-1111"
        },
        {
          name: "email",
          label: "Correo electronico",
          type: "email",
          maxlength: 200,
          placeholder: "ej. usuario@snoopyairlines.com"
        },
        {
          name: "firstName",
          label: "Nombre",
          type: "text",
          maxlength: 100,
          placeholder: "ej. Charlie"
        },
        {
          name: "lastNameOne",
          label: "Primer apellido",
          type: "text",
          maxlength: 100,
          placeholder: "ej. Brown"
        },
        {
          name: "lastNameTwo",
          label: "Segundo apellido",
          type: "text",
          maxlength: 100,
          placeholder: "Opcional"
        },
        {
          name: "type",
          label: "Rol",
          type: "select",
          placeholder: "Seleccione un rol",
          options: [
            { value: "ad", label: "Administrador" },
            { value: "op", label: "Operador" }
          ]
        }
      ];
    }
  },
  methods: {
    emptyUser() {
      return {
        identificationNumber: "",
        email: "",
        firstName: "",
        lastNameOne: "",
        lastNameTwo: "",
        type: ""
      };
    },
    emptyErrors() {
      return {
        identificationNumber: "",
        email: "",
        firstName: "",
        lastNameOne: "",
        lastNameTwo: "",
        type: ""
      };
    },
    clearForm() {
      this.user = this.emptyUser();
      this.errors = this.emptyErrors();
    },
    trimmed(fieldName) {
      return (this.user[fieldName] || "").trim();
    },
    validate() {
      this.errors = this.emptyErrors();
      let valid = true;

      valid = this.validateRequired("identificationNumber") && valid;
      valid = this.validateEmail() && valid;
      valid = this.validateRequired("firstName") && valid;
      valid = this.validateRequired("lastNameOne") && valid;
      valid = this.validateRequired("type") && valid;

      return valid;
    },
    validateRequired(fieldName) {
      if (!this.trimmed(fieldName)) {
        this.errors[fieldName] = "Campo obligatorio";
        return false;
      }

      return true;
    },
    validateEmail() {
      if (!this.validateRequired("email")) return false;

      if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(this.trimmed("email"))) {
        this.errors.email = "Ingrese un correo valido";
        return false;
      }

      return true;
    },
    buildUserRequest() {
      const lastNameTwo = this.trimmed("lastNameTwo");

      return {
        identificationNumber: this.trimmed("identificationNumber"),
        email: this.trimmed("email"),
        firstName: this.trimmed("firstName"),
        lastNameOne: this.trimmed("lastNameOne"),
        lastNameTwo: lastNameTwo || null,
        type: this.user.type
      };
    },
    registerUser() {
      if (!this.validate()) return;

      const token = localStorage.getItem("token");

      axios
        .post(`${process.env.VUE_APP_BACKEND_URL}/user`, this.buildUserRequest(), {
          headers: { Authorization: `Bearer ${token}` }
        })
        .then(() => {
          alert("Usuario guardado correctamente. Se enviara el correo de registro si corresponde.");
          this.clearForm();
        })
        .catch((error) => {
          this.handleRegistrationError(error);
        });
    },
    handleRegistrationError(error) {
      if (error.response && error.response.status === 400) {
        this.applyBackendErrors(error.response.data);
        alert("Datos invalidos: revise los campos del formulario");
      } else if (error.response && (error.response.status === 401 || error.response.status === 403)) {
        alert("Acceso no autorizado");
      } else {
        alert("Error al registrar el usuario");
      }

      console.error(error);
    },
    applyBackendErrors(responseData) {
      const fieldMap = {
        IdentificationNumber: "identificationNumber",
        Email: "email",
        FirstName: "firstName",
        LastNameOne: "lastNameOne",
        LastNameTwo: "lastNameTwo",
        Type: "type"
      };

      if (!responseData || !Array.isArray(responseData.errors)) return;

      responseData.errors.forEach((error) => {
        const fieldName = fieldMap[error.field];

        if (fieldName && Object.prototype.hasOwnProperty.call(this.errors, fieldName)) {
          this.errors[fieldName] = error.message || "Valor invalido";
        }
      });
    }
  }
};
</script>
