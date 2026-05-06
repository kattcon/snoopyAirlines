<template>
  <IntakePage
    v-model="aircraft"
    back-label="Volver"
    title="Registro de aeronave"
    subtitle="Complete la información para registrar una nueva aeronave"
    :fields="aircraftFields"
    :errors="errors"
    submit-label="Registrar"
    @back="$router.push('/admin/list-aircrafts')"
    @cancel="clearForm"
    @submit="registerAircraft"
  />
</template>

<script>
import axios from "axios";
import IntakePage from "../components/IntakePage.vue";

export default {
  name: "RegisterAircraft",
  components: {
    IntakePage
  },
  data() {
    return{
      aircraft: {
        model: "",
        touristRows: 0,
        touristColumns: 0,
        firstClassRows: 0,
        firstClassColumns: 0,
        maxWeight: 0
      },
      errors: { model: "", touristRows: "", touristColumns: "", firstClassRows: "", firstClassColumns: "", maxWeight: "" }
    };
  },
  computed:{
    aircraftFields() {
      return [
        {
          name: "model",
          label: "Modelo de la aeronave",
          type: "text",
          maxlength: 50
        },
        {
          name: "touristRows", 
          label: "Número de filas en clase turista", 
          type: "number", 
          min: 1
        },
        {
          name: "touristColumns", 
          label: "Número de asientos por fila en clase turista", 
          type: "number", 
          min: 1
        },
        {
          name: "firstClassRows", 
          label: "Número de filas en primera clase", 
          type: "number", 
          min: 1
        },
        {
          name: "firstClassColumns", 
          label: "Número de asientos por fila en primera clase", 
          type: "number", 
          min: 1
        },
        {
          name: "maxWeight", 
          label: "Peso máximo de la aeronave (kg)", 
          type: "number", 
          min: 1
        }
      ]
    }
  },
  methods: {
    clearForm() {
      this.aircraft = {
        model: "",
        touristRows: 0,
        touristColumns: 0,
        firstClassRows: 0,
        firstClassColumns: 0,
        maxWeight: 0
      };
      this.errors = { model: "", touristRows: "", touristColumns: "", firstClassRows: "", firstClassColumns: "", maxWeight: "" };
    },
    validate(){
      this.errors = { model: "", touristRows: "", touristColumns: "", firstClassRows: "", firstClassColumns: "", maxWeight: "" };
      let valid = true;

      const touristRows = parseInt(this.aircraft.touristRows) || 0;
      const touristColumns = parseInt(this.aircraft.touristColumns) || 0;
      const firstclassRows = parseInt(this.aircraft.firstClassRows) || 0;
      const firstclassColumns = parseInt(this.aircraft.firstClassColumns) || 0;
      
      if (this.aircraft.model.trim() === "") {
        this.errors.model = "El modelo es requerido.";
        valid = false;
      }
      if (touristRows <= 0) {
        this.errors.touristRows = "El número de filas en clase turista debe ser mayor que cero.";
        valid = false;
      }
      if (touristColumns <= 0) {
        this.errors.touristColumns = "El número de asientos por fila en clase turista debe ser mayor que cero.";
        valid = false;
      }
      if (firstclassRows <= 0) {
        this.errors.firstClassRows = "El número de filas en primera clase debe ser mayor que cero.";
        valid = false;
      }
      if (firstclassColumns <= 0) {
        this.errors.firstClassColumns = "El número de asientos por fila en primera clase debe ser mayor que cero.";
        valid = false;
      }
      if (this.aircraft.maxWeight <= 0) {
        this.errors.maxWeight = "El peso máximo debe ser mayor que cero.";
        valid = false;
      }

      const totalSeats = (touristRows * touristColumns) + (firstclassRows * firstclassColumns);

      if (
        touristRows > 0 &&
        touristColumns > 0 &&
        firstclassRows > 0 &&
        firstclassColumns > 0 &&
        totalSeats > 999
      ) {
        const msg = "La aeronave no puede tener más de 999 asientos en total.";
        
        this.errors.touristRows = msg;
        this.errors.touristColumns = msg;
        this.errors.firstClassRows = msg;
        this.errors.firstClassColumns = msg;

        valid = false;
      }
      
      if(!touristRows || !firstclassRows || !touristColumns
      || !firstclassColumns || !this.aircraft.maxWeight) {
        this.errors.touristRows = "Campo obligatorio";
        this.errors.firstClassRows = "Campo obligatorio";
        this.errors.touristColumns = "Campo obligatorio";
        this.errors.firstClassColumns = "Campo obligatorio";
        this.errors.maxWeight = "Campo obligatorio";
        valid = false;
      }
      return valid;

    },
    registerAircraft() {
      if(!this.validate()) return;

      const token = localStorage.getItem("token");
      const airplaneRequest = {
        model: this.aircraft.model.trim(),
        touristRows: parseInt(this.aircraft.touristRows),
        touristColumns: parseInt(this.aircraft.touristColumns),
        firstclassRows: parseInt(this.aircraft.firstClassRows),
        firstclassColumns: parseInt(this.aircraft.firstClassColumns),
        maxWeight: parseFloat(this.aircraft.maxWeight)
      };
      axios.post(`${process.env.VUE_APP_BACKEND_URL}/airplane`, airplaneRequest,{
        headers: { Authorization: `Bearer ${token}` }
      }).then(() => {
        alert("Aeronave registrada exitosamente");
        this.clearForm();
      }).catch((error) => {
        if(error.response && error.response.status === 409) {
          alert("La aeronave ya está registrada");
        } else if(error.response && (error.response.status === 401 || error.response.status === 403)) {
          alert("Acceso no autorizado");
        } else {
          alert("Ocurrió un error al registrar la aeronave. Por favor, inténtelo de nuevo.");
        }
      });
    }
  }
}
</script>