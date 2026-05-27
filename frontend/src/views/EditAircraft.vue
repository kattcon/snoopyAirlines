<template>
  <IntakePage
    v-model="aircraft"
    back-label="Volver"
    title="Editar aeronave"
    subtitle="Modifique las capacidades de la aeronave. Los valores solo pueden aumentarse."
    :fields="aircraftFields"
    :errors="errors"
    submit-label="Guardar cambios"
    @back="$router.push('/admin/list-aircrafts')"
    @cancel="$router.push('/admin/list-aircrafts')"
    @submit="updateAircraft"
  />
</template>

<script>
import axios from "axios";
import IntakePage from "../components/IntakePage.vue";

const MAXIMUM_ALLOWED_SEATS = 999;

export default {
  name: "EditAircraft",
  components: {
    IntakePage
  },
  data() {
    return {
      aircraft: {
        touristRows: 0,
        touristColumns: 0,
        firstClassRows: 0,
        firstClassColumns: 0,
        maxWeight: 0
      },
      errors: {
        touristRows: "",
        touristColumns: "",
        firstClassRows: "",
        firstClassColumns: "",
        maxWeight: ""
      }
    };
  },
  computed: {
    airplaneId() {
      return this.$route.params.id;
    },
    aircraftFields() {
      return [
        { name: "touristRows", label: "Número de filas en clase turista", type: "number", min: 1 },
        { name: "touristColumns", label: "Número de asientos por fila en clase turista", type: "number", min: 1 },
        { name: "firstClassRows", label: "Número de filas en primera clase", type: "number", min: 1 },
        { name: "firstClassColumns", label: "Número de asientos por fila en primera clase", type: "number", min: 1 },
        { name: "maxWeight", label: "Peso máximo de la aeronave (kg)", type: "number", min: 1 }
      ];
    }
  },
  mounted() {
    this.loadAircraft();
  },
  methods: {
    loadAircraft() {
      axios.get(`${process.env.VUE_APP_BACKEND_URL}/airplane/id/${this.airplaneId}`)
        .then((response) => {
          const data = response.data;
          this.aircraft = {
            touristRows: data.touristRows,
            touristColumns: data.touristColumns,
            firstClassRows: data.firstclassRows,
            firstClassColumns: data.firstclassColumns,
            maxWeight: data.maxWeight
          };
        })
        .catch((error) => {
          if (error.response && error.response.status === 404) {
            alert("No se encontró la aeronave.");
            this.$router.push("/admin/list-aircrafts");
          } else {
            alert("Ocurrió un error al cargar la aeronave.");
          }
        });
    },
    validate() {
      this.errors = { touristRows: "", touristColumns: "", firstClassRows: "", firstClassColumns: "", maxWeight: "" };
      let valid = true;

      const touristRows = parseInt(this.aircraft.touristRows) || 0;
      const touristColumns = parseInt(this.aircraft.touristColumns) || 0;
      const firstClassRows = parseInt(this.aircraft.firstClassRows) || 0;
      const firstClassColumns = parseInt(this.aircraft.firstClassColumns) || 0;

      if (touristRows <= 0) {
        this.errors.touristRows = "El número de filas en clase turista debe ser mayor que cero.";
        valid = false;
      }
      if (touristColumns <= 0) {
        this.errors.touristColumns = "El número de asientos por fila en clase turista debe ser mayor que cero.";
        valid = false;
      }
      if (firstClassRows <= 0) {
        this.errors.firstClassRows = "El número de filas en primera clase debe ser mayor que cero.";
        valid = false;
      }
      if (firstClassColumns <= 0) {
        this.errors.firstClassColumns = "El número de asientos por fila en primera clase debe ser mayor que cero.";
        valid = false;
      }
      if (this.aircraft.maxWeight <= 0) {
        this.errors.maxWeight = "El peso máximo debe ser mayor que cero.";
        valid = false;
      }

      const totalSeats = (touristRows * touristColumns) + (firstClassRows * firstClassColumns);
      if (touristRows > 0 && touristColumns > 0 && firstClassRows > 0 && firstClassColumns > 0 && totalSeats > MAXIMUM_ALLOWED_SEATS) {
        const msg = `La aeronave no puede tener más de ${MAXIMUM_ALLOWED_SEATS} asientos en total.`;
        this.errors.touristRows = msg;
        this.errors.touristColumns = msg;
        this.errors.firstClassRows = msg;
        this.errors.firstClassColumns = msg;
        valid = false;
      }

      return valid;
    },
    updateAircraft() {
      if (!this.validate()) return;

      const token = localStorage.getItem("token");
      const airplaneRequest = {
        touristRows: parseInt(this.aircraft.touristRows),
        touristColumns: parseInt(this.aircraft.touristColumns),
        firstClassRows: parseInt(this.aircraft.firstClassRows),
        firstClassColumns: parseInt(this.aircraft.firstClassColumns),
        maxWeight: parseFloat(this.aircraft.maxWeight)
      };

      axios.put(`${process.env.VUE_APP_BACKEND_URL}/airplane/${this.airplaneId}`, airplaneRequest, {
        headers: { Authorization: `Bearer ${token}` }
      }).then(() => {
        alert("Aeronave actualizada exitosamente.");
        this.$router.push("/admin/list-aircrafts");
      }).catch((error) => {
        if (error.response && error.response.status === 400) {
          alert(error.response.data);
        } else if (error.response && error.response.status === 404) {
          alert("No se encontró la aeronave.");
        } else if (error.response && (error.response.status === 401 || error.response.status === 403)) {
          alert("Acceso no autorizado.");
        } else {
          alert("Ocurrió un error al actualizar la aeronave. Por favor, inténtelo de nuevo.");
        }
      });
    }
  }
};
</script>
