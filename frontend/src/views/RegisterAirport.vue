<template>
  <IntakePage
    v-model="airport"
    back-label="Volver a la lista"
    title="Registro de Aeropuerto"
    subtitle="Complete la información para registrar un nuevo aeropuerto"
    :fields="airportFields"
    :errors="errors"
    submit-label="Registrar"
    @back="$router.push('/admin/airports')"
    @cancel="clearForm"
    @submit="registerAirport"
  />
</template>

<script>
import axios from "axios";
import IntakePage from "../components/IntakePage.vue";

export default {
  name: "RegisterAirport",
  components: {
    IntakePage
  },
  data() {
    return {
      airport: {
        name: "",
        code: "",
        countryId: "",
        cityId: ""
      },
      countries: [],
      cities: [],
      errors: { name: "", code: "", countryId: "", cityId: "" }
    };
  },
  computed: {
    airportFields() {
      return [
        {
          name: "name",
          label: "Nombre del Aeropuerto",
          type: "text",
          maxlength: 200,
          placeholder: "ej. Juan Santamaría International Airport"
        },
        {
          name: "code",
          label: "Código IATA",
          type: "text",
          placeholder: "ej. SJO"
        },
        {
          name: "countryId",
          label: "País",
          type: "select",
          placeholder: "Seleccione un país",
          options: this.countries.map((country) => ({
            value: country.id,
            label: country.name
          }))
        },
        {
          name: "cityId",
          label: "Ciudad",
          type: "select",
          placeholder: this.airport.countryId ? "Seleccione una ciudad" : "Primero seleccione un país",
          disabled: !this.airport.countryId,
          options: this.cities.map((city) => ({
            value: city.id,
            label: city.name
          }))
        }
      ];
    }
  },
  mounted() {
    this.loadCountries();
  },
  methods: {
    clearForm() {
      this.airport = { name: "", code: "", countryId: "", cityId: "" };
      this.cities = [];
      this.errors = { name: "", code: "", countryId: "", cityId: "" };
    },
    validate() {
      this.errors = { name: "", code: "", countryId: "", cityId: "" };
      let valid = true;

      if (!this.airport.name || !this.airport.name.trim()) {
        this.errors.name = "Campo obligatorio";
        valid = false;
      }

      if (!this.airport.code || !this.airport.code.trim()) {
        this.errors.code = "Campo obligatorio";
        valid = false;
      } else if (!/^[A-Za-z]{3}$/.test(this.airport.code.trim())) {
        this.errors.code = "Debe tener exactamente 3 letras (ej. SJO)";
        valid = false;
      }

      if (!this.airport.countryId) {
        this.errors.countryId = "Campo obligatorio";
        valid = false;
      }

      if (!this.airport.cityId) {
        this.errors.cityId = "Campo obligatorio";
        valid = false;
      }

      return valid;
    },
    loadCountries() {
      const token = localStorage.getItem("token");
      axios
        .get(`${process.env.VUE_APP_BACKEND_URL}/airport/countries`, {
          headers: { Authorization: `Bearer ${token}` }
        })
        .then((response) => {
          this.countries = response.data;
        })
        .catch((error) => {
          console.error("Error cargando países:", error);
        });
    },
    registerAirport() {
      if (!this.validate()) return;

      const token = localStorage.getItem("token");
      const airportRequest = {
        name: this.airport.name,
        code: this.airport.code,
        cityId: this.airport.cityId
      };

      axios
        .post(`${process.env.VUE_APP_BACKEND_URL}/airport`, airportRequest, {
          headers: { Authorization: `Bearer ${token}` }
        })
        .then(() => {
          alert("Aeropuerto registrado correctamente");
          this.clearForm();
        })
        .catch((error) => {
          if (error.response && error.response.status === 409) {
            alert("Error de registro: código existente");
          } else if (error.response && error.response.status === 400) {
            alert("Datos inválidos: " + JSON.stringify(error.response.data));
          } else {
            alert("Error al registrar el aeropuerto");
          }
          console.error(error);
        });
    }
  },
  watch: {
    "airport.countryId"(newCountryId) {
      this.airport.cityId = "";
      this.cities = [];

      if (newCountryId) {
        const token = localStorage.getItem("token");
        axios
          .get(`${process.env.VUE_APP_BACKEND_URL}/airport/cities?countryId=${newCountryId}`, {
            headers: { Authorization: `Bearer ${token}` }
          })
          .then((response) => {
            this.cities = response.data;
          })
          .catch((error) => {
            console.error("Error cargando ciudades:", error);
          });
      }
    }
  }
};
</script>
