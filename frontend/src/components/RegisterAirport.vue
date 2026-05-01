<template>
  <div class="container mt-5">
    <h1 class="display-4 text-center">Registrar aeropuerto</h1>
  </div>
</template>

<script>
export default {
  name: "RegisterAirport",
  data() {
    return {
      airport: {
        name: "",
        code: "",
        cityId: "",
        timezone: ""
      },
      countries: [],
      cities: []
    };
  }
};
</script>
