<template>
  <section class="confirmation-card contentCard">
    <h2 class="card-title">
      <svg viewBox="0 0 24 24" aria-hidden="true">
        <path d="M10.5 13.5 2 22l2-7-2-2 7-2 8.5-8.5a2.1 2.1 0 0 1 3 3L12 14"></path>
      </svg>
      <span>Detalles del vuelo</span>
    </h2>

    <div class="flight-meta">
      <span>{{ details.flightLabel }}</span>
      <span class="flight-date">
        <svg viewBox="0 0 24 24" aria-hidden="true">
          <rect x="3" y="4" width="18" height="18" rx="2"></rect>
          <path d="M16 2v4"></path>
          <path d="M8 2v4"></path>
          <path d="M3 10h18"></path>
        </svg>
        {{ details.dateLabel }}
      </span>
    </div>

    <div class="flight-timeline">
      <div class="timeline-point origin">
        <strong>{{ details.departureTime }}</strong>
        <span>{{ details.departureCode }}</span>
        <small>{{ details.departureCity }}</small>
      </div>

      <div class="timeline-line">
        <span>{{ details.durationLabel }}</span>
        <small>Directo</small>
      </div>

      <div class="timeline-point destination">
        <strong>{{ details.arrivalTime }}</strong>
        <span>{{ details.arrivalCode }}</span>
        <small>{{ details.arrivalCity }}</small>
      </div>
    </div>

    <div :class="['seat-class-row', { 'has-extra-fields': extraFields.length > 0 }]">
      <div class="detail-field">
        <span>Clase</span>
        <strong>{{ details.seatClassLabel }}</strong>
      </div>
      <div v-for="field in extraFields" :key="field.label" class="detail-field">
        <span>{{ field.label }}</span>
        <strong>{{ field.value }}</strong>
      </div>
    </div>
  </section>
</template>

<script>
export default {
  name: "FlightDetailsCard",
  props: {
    details: {
      type: Object,
      required: true
    }
  },
  computed: {
    extraFields() {
      return this.details.extraFields || [];
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

.card-title,
.flight-date {
  display: inline-flex;
  align-items: center;
}

.card-title {
  gap: 10px;
  color: #111827;
  font-size: 1.15rem;
  margin-bottom: 48px;
}

.card-title svg,
.flight-date svg {
  width: 18px;
  height: 18px;
  fill: none;
  stroke: currentColor;
  stroke-width: 2;
  stroke-linecap: round;
  stroke-linejoin: round;
}

.flight-meta {
  display: flex;
  justify-content: space-between;
  gap: 18px;
  color: #526078;
  font-size: var(--smallFontSize);
  margin-bottom: 24px;
}

.flight-date {
  gap: 8px;
  white-space: nowrap;
}

.flight-timeline {
  display: grid;
  grid-template-columns: minmax(90px, 1fr) minmax(120px, 3fr) minmax(90px, 1fr);
  align-items: center;
  gap: 8px;
  margin-bottom: 42px;
}

.timeline-point {
  display: grid;
  gap: 3px;
}

.timeline-point strong {
  color: #111827;
  font-size: 1.5rem;
  font-weight: var(--mediumFontWeight);
  line-height: 1;
}

.timeline-point span {
  color: #111827;
  font-size: var(--smallFontSize);
  font-weight: var(--semiboldFontWeight);
}

.timeline-point small,
.timeline-line small,
.timeline-line span {
  color: #526078;
  font-size: var(--extraSmallFontSize);
}

.destination {
  text-align: right;
}

.timeline-line {
  position: relative;
  display: grid;
  justify-items: center;
  gap: 2px;
  min-height: 34px;
}

.timeline-line::before {
  content: "";
  position: absolute;
  top: 13px;
  left: 0;
  right: 0;
  height: 1px;
  background: #b7becb;
}

.timeline-line span,
.timeline-line small {
  position: relative;
  z-index: 1;
  background: var(--whiteColor);
  padding: 0 8px;
}

.seat-class-row {
  display: grid;
  grid-template-columns: 1fr;
  gap: 12px;
  min-height: 64px;
  padding: 0 16px;
  border-radius: var(--defaultBorderRadius);
  background: #f0f2f6;
  color: #526078;
  font-size: var(--smallFontSize);
}

.seat-class-row.has-extra-fields {
  grid-template-columns: repeat(2, minmax(0, 1fr));
  background: transparent;
  padding: 0;
}

.detail-field {
  display: grid;
  align-content: center;
  gap: 4px;
  min-height: 64px;
  padding: 0 16px;
  border-radius: var(--defaultBorderRadius);
  background: #f0f2f6;
}

.detail-field strong {
  color: #111827;
}

@media (max-width: 640px) {
  .confirmation-card {
    padding: 22px;
  }

  .flight-meta {
    flex-direction: column;
  }

  .flight-timeline {
    grid-template-columns: 1fr;
    gap: 18px;
  }

  .timeline-line {
    justify-items: start;
  }

  .timeline-line::before {
    display: none;
  }

  .timeline-line span,
  .timeline-line small {
    padding: 0;
  }

  .destination {
    text-align: left;
  }
}
</style>
