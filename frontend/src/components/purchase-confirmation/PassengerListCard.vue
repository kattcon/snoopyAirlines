<template>
  <section class="confirmation-card contentCard">
    <h2 class="card-title">
      <svg viewBox="0 0 24 24" aria-hidden="true">
        <path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2"></path>
        <circle cx="9" cy="7" r="4"></circle>
        <path d="M22 21v-2a4 4 0 0 0-3-3.87"></path>
        <path d="M16 3.13a4 4 0 0 1 0 7.75"></path>
      </svg>
      <span>Pasajeros ({{ passengers.length }})</span>
    </h2>

    <div class="passenger-stack">
      <article v-for="(passenger, index) in passengers" :key="passenger.id || index" class="passenger-card">
        <span class="passenger-badge">Pasajero {{ index + 1 }}</span>
        <strong>{{ passengerName(passenger) }}</strong>
        <span class="passenger-meta">
          Fecha de nacimiento: {{ birthDate(passenger) }} &middot; {{ fieldValue(passenger, "nationality", "Nationality") }}
        </span>

        <div v-if="showLuggage" class="luggage-row">
          <span>
            <svg viewBox="0 0 24 24" aria-hidden="true">
              <rect x="6" y="7" width="12" height="13" rx="2"></rect>
              <path d="M9 7V5a3 3 0 0 1 6 0v2"></path>
              <path d="M9 20v2"></path>
              <path d="M15 20v2"></path>
            </svg>
            Equipaje de mano: {{ luggageLabel(passenger, "carryOnLuggageLabel", "CarryOnLuggageLabel", "equipaje_mano") }}
          </span>
          <span>
            <svg viewBox="0 0 24 24" aria-hidden="true">
              <rect x="6" y="7" width="12" height="13" rx="2"></rect>
              <path d="M9 7V5a3 3 0 0 1 6 0v2"></path>
              <path d="M9 20v2"></path>
              <path d="M15 20v2"></path>
            </svg>
            Maletas facturadas: {{ luggageLabel(passenger, "checkedLuggageLabel", "CheckedLuggageLabel", "maletas_facturadas") }}
          </span>
        </div>
      </article>
    </div>
  </section>
</template>

<script>
export default {
  name: "PassengerListCard",
  props: {
    passengers: {
      type: Array,
      required: true
    },
    showLuggage: {
      type: Boolean,
      default: true
    }
  },
  methods: {
    fieldValue(source, camelCaseKey, pascalCaseKey) {
      return source?.[camelCaseKey] ?? source?.[pascalCaseKey] ?? "";
    },
    passengerName(passenger) {
      const firstName = this.fieldValue(passenger, "firstName", "FirstName");
      const lastName = this.fieldValue(passenger, "lastName", "LastName");
      return `${firstName} ${lastName}`.trim() || this.placeholder("nombre_pasajero");
    },
    birthDate(passenger) {
      const day = this.fieldValue(passenger, "birthDay", "BirthDay");
      const month = this.fieldValue(passenger, "birthMonth", "BirthMonth");
      const year = this.fieldValue(passenger, "birthYear", "BirthYear");
      return [day, month, year].filter(Boolean).join("/") || this.placeholder("fecha_nacimiento");
    },
    luggageLabel(passenger, camelCaseKey, pascalCaseKey, placeholderKey) {
      const value = this.fieldValue(passenger, camelCaseKey, pascalCaseKey);
      return value === "" ? this.placeholder(placeholderKey) : value;
    },
    placeholder(key) {
      return `{{${key}}}`;
    }
  }
};
</script>

<style scoped>
.confirmation-card {
  border: 1px solid #d8dde6;
  border-radius: var(--defaultBorderRadius);
  box-shadow: none;
  padding: 28px;
}

.card-title {
  display: inline-flex;
  align-items: center;
  gap: 10px;
  color: #111827;
  font-size: 1.15rem;
  margin-bottom: 54px;
}

.card-title svg,
.luggage-row svg {
  width: 18px;
  height: 18px;
  fill: none;
  stroke: currentColor;
  stroke-width: 2;
  stroke-linecap: round;
  stroke-linejoin: round;
}

.passenger-stack {
  display: grid;
  gap: 18px;
}

.passenger-card {
  display: grid;
  gap: 6px;
  padding: 18px;
  border-radius: var(--defaultBorderRadius);
  background: #f6f7fa;
}

.passenger-badge {
  width: fit-content;
  padding: 3px 8px;
  border-radius: 4px;
  background: #e8ebf1;
  color: #344154;
  font-size: 0.7rem;
  font-weight: var(--semiboldFontWeight);
}

.passenger-card strong {
  color: #111827;
  font-size: var(--baseFontSize);
}

.passenger-meta {
  padding-bottom: 12px;
  border-bottom: 1px solid #cfd5df;
  color: #526078;
  font-size: var(--smallFontSize);
}

.luggage-row {
  display: flex;
  flex-wrap: wrap;
  gap: 18px;
  color: #526078;
  font-size: var(--smallFontSize);
}

.luggage-row span {
  display: inline-flex;
  align-items: center;
  gap: 8px;
}

@media (max-width: 640px) {
  .confirmation-card {
    padding: 22px;
  }
}
</style>
