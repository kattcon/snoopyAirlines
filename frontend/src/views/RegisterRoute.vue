<template>
  <IntakePage
    v-model="route"
    back-label="Volver a rutas"
    title="Registro de Ruta"
    subtitle="Complete la información para registrar una nueva ruta"
    :fields="routeFields"
    :errors="errors"
    submit-label="Registrar"
    @back="$router.push('/admin/routes')"
    @cancel="clearForm"
    @submit="registerRoute"
  />
</template>

<script>
import axios from "axios";
import IntakePage from "../components/IntakePage.vue";

export default {
  name: "RegisterRoute",
  components: {
    IntakePage
  },
  data() {
    return {
      route: this.emptyRoute(),
      aircrafts: [],
      airports: [],
      errors: this.emptyErrors()
    };
  },
  computed: {
    aircraftOptions() {
      return this.aircrafts
        .map((aircraft, index) => {
          const value = this.aircraftId(aircraft, index);
          return {
            value,
            label: this.aircraftLabel(aircraft, value)
          };
        });
    },
    airportOptions() {
      return this.airports.map((airport) => ({
        value: airport.id,
        label: `${airport.code} - ${airport.name}`
      }));
    },
    weekdayOptions() {
      return [
        { value: "monday", label: "Lun" },
        { value: "tuesday", label: "Mar" },
        { value: "wednesday", label: "Mie" },
        { value: "thursday", label: "Jue" },
        { value: "friday", label: "Vie" },
        { value: "saturday", label: "Sab" },
        { value: "sunday", label: "Dom" }
      ];
    },
    routeFields() {
      return [
        {
          name: "airplaneId",
          label: "Aeronave",
          type: "select",
          placeholder: "Seleccione la aeronave",
          options: this.aircraftOptions
        },
        {
          name: "departureAirportId",
          label: "Aeropuerto de Salida",
          type: "select",
          placeholder: "Seleccione el aeropuerto de salida",
          options: this.airportOptions
        },
        {
          name: "arrivalAirportId",
          label: "Aeropuerto de Llegada",
          type: "select",
          placeholder: "Seleccione el aeropuerto de llegada",
          options: this.airportOptions
        },
        {
          name: "departureTime",
          label: "Hora de Salida",
          type: "time"
        },
        {
          name: "arrivalTime",
          label: "Hora de Llegada",
          type: "time"
        },
        {
          name: "frequency",
          label: "Frecuencia",
          type: "weekday-selector",
          options: this.weekdayOptions
        },
        {
          name: "durationMinutes",
          label: "Duración en Minutos",
          type: "number",
          min: 1,
          step: 1,
          placeholder: "ej. 120"
        },
        {
          name: "priceFirstClass",
          label: "Precio Primera Clase",
          type: "number",
          min: 0,
          step: 0.01,
          placeholder: "ej. 350.00"
        },
        {
          name: "priceEconomyClass",
          label: "Precio Clase Económica",
          type: "number",
          min: 0,
          step: 0.01,
          placeholder: "ej. 150.00"
        },
        {
          name: "priceCarryOnBaggage",
          label: "Precio Equipaje de Mano",
          type: "number",
          min: 0,
          step: 0.01,
          placeholder: "ej. 25.00"
        },
        {
          name: "priceCheckedBaggage",
          label: "Precio Equipaje Facturado",
          type: "number",
          min: 0,
          step: 0.01,
          placeholder: "ej. 45.00"
        },
        {
          name: "weightLimitCarryOnBaggage",
          label: "Peso Máximo Equipaje de Mano",
          type: "number",
          min: 0,
          step: 1,
          placeholder: "ej. 10"
        },
        {
          name: "weightLimitCheckedBaggage",
          label: "Peso Máximo Equipaje Facturado",
          type: "number",
          min: 0,
          step: 1,
          placeholder: "ej. 23"
        },
        {
          name: "checkedBaggagePriceMultiplier",
          label: "Multiplicador de Equipaje Facturado",
          type: "number",
          min: 0,
          step: 0.0001,
          placeholder: "ej. 1.0000"
        }
      ];
    }
  },
  mounted() {
    this.loadAircrafts();
    this.loadAirports();
  },
  methods: {
    emptyFrequency() {
      return {
        monday: false,
        tuesday: false,
        wednesday: false,
        thursday: false,
        friday: false,
        saturday: false,
        sunday: false
      };
    },
    emptyRoute() {
      return {
        airplaneId: "",
        departureAirportId: "",
        arrivalAirportId: "",
        departureTime: "",
        arrivalTime: "",
        frequency: this.emptyFrequency(),
        durationMinutes: "",
        priceFirstClass: "",
        priceEconomyClass: "",
        priceCarryOnBaggage: "",
        priceCheckedBaggage: "",
        weightLimitCarryOnBaggage: "",
        weightLimitCheckedBaggage: "",
        checkedBaggagePriceMultiplier: ""
      };
    },
    emptyErrors() {
      return {
        airplaneId: "",
        departureAirportId: "",
        arrivalAirportId: "",
        departureTime: "",
        arrivalTime: "",
        frequency: "",
        durationMinutes: "",
        priceFirstClass: "",
        priceEconomyClass: "",
        priceCarryOnBaggage: "",
        priceCheckedBaggage: "",
        weightLimitCarryOnBaggage: "",
        weightLimitCheckedBaggage: "",
        checkedBaggagePriceMultiplier: ""
      };
    },
    clearForm() {
      this.route = this.emptyRoute();
      this.errors = this.emptyErrors();
    },
    validate() {
      this.errors = this.emptyErrors();
      let valid = true;

      valid = this.validatePositiveInteger("airplaneId") && valid;
      valid = this.validateRequired("departureAirportId") && valid;
      valid = this.validateRequired("arrivalAirportId") && valid;
      valid = this.validateTime("departureTime") && valid;
      valid = this.validateTime("arrivalTime") && valid;
      valid = this.validateFrequency() && valid;
      valid = this.validatePositiveInteger("durationMinutes") && valid;
      valid = this.validateNonNegativeNumber("priceFirstClass") && valid;
      valid = this.validateNonNegativeNumber("priceEconomyClass") && valid;
      valid = this.validateNonNegativeNumber("priceCarryOnBaggage") && valid;
      valid = this.validateNonNegativeNumber("priceCheckedBaggage") && valid;
      valid = this.validateNonNegativeInteger("weightLimitCarryOnBaggage") && valid;
      valid = this.validateNonNegativeInteger("weightLimitCheckedBaggage") && valid;
      valid = this.validateNonNegativeNumber("checkedBaggagePriceMultiplier") && valid;

      if (
        this.route.departureAirportId &&
        this.route.arrivalAirportId &&
        this.route.departureAirportId === this.route.arrivalAirportId
      ) {
        this.errors.arrivalAirportId = "Debe ser diferente al aeropuerto de salida";
        valid = false;
      }

      if (
        this.route.departureTime &&
        this.route.arrivalTime &&
        this.timeToMinutes(this.route.arrivalTime) <= this.timeToMinutes(this.route.departureTime)
      ) {
        this.errors.arrivalTime = "Debe ser posterior a la salida";
        valid = false;
      }

      return valid;
    },
    validateRequired(fieldName) {
      if (this.route[fieldName] === "" || this.route[fieldName] === null || this.route[fieldName] === undefined) {
        this.errors[fieldName] = "Campo obligatorio";
        return false;
      }

      return true;
    },
    validateTime(fieldName) {
      if (!this.validateRequired(fieldName)) return false;

      if (!/^([01]\d|2[0-3]):[0-5]\d$/.test(this.route[fieldName])) {
        this.errors[fieldName] = "Debe tener formato HH:mm";
        return false;
      }

      return true;
    },
    validateFrequency() {
      const frequency = this.route.frequency || {};
      const hasSelectedDay = this.weekdayOptions.some((day) => Boolean(frequency[day.value]));

      if (!hasSelectedDay) {
        this.errors.frequency = "Seleccione al menos un dia";
        return false;
      }

      return true;
    },
    timeToMinutes(time) {
      const [hours, minutes] = time.split(":").map(Number);
      return hours * 60 + minutes;
    },
    validatePositiveInteger(fieldName) {
      if (!this.validateRequired(fieldName)) return false;

      const value = Number(this.route[fieldName]);
      if (!Number.isInteger(value) || value <= 0) {
        this.errors[fieldName] = "Debe ser un número entero mayor a 0";
        return false;
      }

      return true;
    },
    validateNonNegativeInteger(fieldName) {
      if (!this.validateRequired(fieldName)) return false;

      const value = Number(this.route[fieldName]);
      if (!Number.isInteger(value) || value < 0) {
        this.errors[fieldName] = "Debe ser un número entero mayor o igual a 0";
        return false;
      }

      return true;
    },
    validateNonNegativeNumber(fieldName) {
      if (!this.validateRequired(fieldName)) return false;

      const value = Number(this.route[fieldName]);
      if (!Number.isFinite(value) || value < 0) {
        this.errors[fieldName] = "Debe ser un número mayor o igual a 0";
        return false;
      }

      return true;
    },
    fieldValue(source, camelCaseKey, pascalCaseKey) {
      return source[camelCaseKey] ?? source[pascalCaseKey];
    },
    aircraftId(aircraft, index) {
      const id = this.fieldValue(aircraft, "id", "Id");
      return id === null || id === undefined || id === "" ? index + 1 : id;
    },
    aircraftLabel(aircraft, id) {
      const model = this.fieldValue(aircraft, "model", "Model");
      const touristRows = this.fieldValue(aircraft, "touristRows", "TouristRows");
      const touristColumns = this.fieldValue(aircraft, "touristColumns", "TouristColumns");
      const firstClassRows = this.fieldValue(aircraft, "firstclassRows", "FirstclassRows");
      const firstClassColumns = this.fieldValue(aircraft, "firstclassColumns", "FirstclassColumns");
      const maxWeightValue = this.fieldValue(aircraft, "maxWeight", "MaxWeight");
      const touristSeats = Number(touristRows) * Number(touristColumns);
      const firstClassSeats = Number(firstClassRows) * Number(firstClassColumns);
      const totalSeats = touristSeats + firstClassSeats;
      const maxWeight = Number(maxWeightValue);
      const seatsLabel = Number.isFinite(totalSeats) ? `${totalSeats} asientos` : "capacidad no disponible";
      const weightLabel = Number.isFinite(maxWeight) ? `${maxWeight} kg max` : "peso no disponible";

      return `${model || "Aeronave"} #${id} - ${seatsLabel} - ${weightLabel}`;
    },
    loadAircrafts() {
      const token = localStorage.getItem("token");
      axios
        .get(`${process.env.VUE_APP_BACKEND_URL}/airplane`, {
          headers: { Authorization: `Bearer ${token}` }
        })
        .then((response) => {
          this.aircrafts = response.data;
        })
        .catch((error) => {
          console.error("Error cargando aeronaves:", error);
        });
    },
    loadAirports() {
      const token = localStorage.getItem("token");
      axios
        .get(`${process.env.VUE_APP_BACKEND_URL}/airport`, {
          headers: { Authorization: `Bearer ${token}` }
        })
        .then((response) => {
          this.airports = response.data;
        })
        .catch((error) => {
          console.error("Error cargando aeropuertos:", error);
        });
    },
    registerRoute() {
      if (!this.validate()) return;

      const token = localStorage.getItem("token");
      const routeRequest = {
        airplaneId: Number(this.route.airplaneId),
        departureAirportId: Number(this.route.departureAirportId),
        arrivalAirportId: Number(this.route.arrivalAirportId),
        departureTime: this.route.departureTime,
        arrivalTime: this.route.arrivalTime,
        frequency: { ...this.route.frequency },
        durationMinutes: Number(this.route.durationMinutes),
        priceFirstClass: Number(this.route.priceFirstClass),
        priceEconomyClass: Number(this.route.priceEconomyClass),
        priceCarryOnBaggage: Number(this.route.priceCarryOnBaggage),
        priceCheckedBaggage: Number(this.route.priceCheckedBaggage),
        weightLimitCarryOnBaggage: Number(this.route.weightLimitCarryOnBaggage),
        weightLimitCheckedBaggage: Number(this.route.weightLimitCheckedBaggage),
        checkedBaggagePriceMultiplier: Number(this.route.checkedBaggagePriceMultiplier)
      };

      axios
        .post(`${process.env.VUE_APP_BACKEND_URL}/route`, routeRequest, {
          headers: { Authorization: `Bearer ${token}` }
        })
        .then(() => {
          alert("Ruta registrada correctamente");
          this.clearForm();
        })
        .catch((error) => {
          if (error.response && error.response.status === 400) {
            alert("Datos inválidos: " + JSON.stringify(error.response.data));
          } else {
            alert("Error al registrar la ruta");
          }
          console.error(error);
        });
    }
  }
};
</script>
