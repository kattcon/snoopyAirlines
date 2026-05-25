<template>
  <div>
    <p v-if="errorMessage" class="error-acceso">{{ errorMessage }}</p>
    <IntakePage
      v-model="airport"
      back-label="Volver a la lista"
      title="Editar Aeropuerto"
      subtitle="Modifique el nombre del aeropuerto"
      :fields="airportFields"
      :errors="errors"
      submit-label="Guardar"
      @back="$router.push('/admin/airports')"
      @cancel="$router.push('/admin/airports')"
      @submit="updateAirportName"
    />
  </div>
</template>

<script>
import axios from "axios";
import IntakePage from "../components/IntakePage.vue";

export default {
  name: "EditAirport",
  components: {
    IntakePage
  },
  data() {
    return {
      airport: {
        name: ""
      },
      errors: { name: "" },
      errorMessage: ""
    };
  },
  computed: {
    airportId() {
      return this.$route.params.id;
    },
    airportFields() {
      return [
        {
          name: "name",
          label: "Nombre del Aeropuerto",
          type: "text",
          maxlength: 200,
          placeholder: "ej. Juan Santamaría International Airport"
        }
      ];
    }
  },
  mounted() {
    this.loadAirport();
  },
  methods: {
    loadAirport() {
      const token = localStorage.getItem("token");
      axios
        .get(`${process.env.VUE_APP_BACKEND_URL}/airport/${this.airportId}`, {
          headers: { Authorization: `Bearer ${token}` }
        })
        .then((response) => {
          this.airport.name = response.data.name;
        })
        .catch((error) => {
          if (error.response && error.response.status === 404) {
            this.errorMessage = "Aeropuerto no encontrado";
          } else {
            this.errorMessage = "Error al cargar el aeropuerto";
          }
          console.error("Error cargando aeropuerto:", error);
        });
    },
    validate() {
      this.errors = { name: "" };

      if (!this.airport.name || !this.airport.name.trim()) {
        this.errors.name = "Campo obligatorio";
        return false;
      }

      return true;
    },
    updateAirportName() {
      if (!this.validate()) return;

      const token = localStorage.getItem("token");
      axios
        .put(
          `${process.env.VUE_APP_BACKEND_URL}/airport/${this.airportId}`,
          { name: this.airport.name.trim() },
          { headers: { Authorization: `Bearer ${token}` } }
        )
        .then(() => {
          this.$router.push("/admin/airports");
        })
        .catch((error) => {
          if (error.response) {
            if (error.response.status === 400) {
              this.errors.name = error.response.data.message ?? "Nombre inválido";
            } else if (error.response.status === 404) {
              this.errorMessage = "Aeropuerto no encontrado";
            } else if (error.response.status === 401 || error.response.status === 403) {
              this.errorMessage = "Acceso no autorizado";
            }
          }
          console.error("Error actualizando aeropuerto:", error);
        });
    }
  }
};
</script>
