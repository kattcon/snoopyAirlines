<template>
  <IntakePage
    v-model="aircraft"
    back-label="Volver"
    title="Registro de aeronave"
    subtitle="Complete la información para registrar una nueva aeronave"
    :fields="aircraftFields"
    :errors="errors"
    submit-label="Registrar"
    @back="$router.push('/admin/register-aircraft')"
    @cancel="clearForm"
    @submit="registerAircraft"
  />
</template>

<script>
//import axios from "axios";
import IntakePage from "../components/IntakePage.vue";

export default {
  name: "RegisterAircraft",
  components: {
    IntakePage
  },
  data() {
    return{
      aircraft: {
        model: ""
      }
    }
  },
  computed:{
    aircraftFields() {
      return [
        {
          name: "model",
          label: "Modelo de la aeronave",
          type: "text",
          maxlength: 100, //  revisar en la base de datos el lenght
          placeholder: "ej. Boeing 737-800"
        }
      ]
    }
  }
}



</script>