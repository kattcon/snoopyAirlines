<template>
  <aside class="summary-card contentCard">
    <h2>Resumen de compra</h2>

    <div class="summary-lines">
      <div v-for="item in items" :key="item.label" class="summary-line">
        <span>{{ item.label }}</span>
        <strong>{{ item.value }}</strong>
      </div>
    </div>

    <div class="summary-total">
      <span>Total</span>
      <strong>{{ total }}</strong>
    </div>

    <button type="button" class="primaryButton finish-button" disabled>
      <svg viewBox="0 0 24 24" aria-hidden="true">
        <circle cx="12" cy="12" r="10"></circle>
        <path d="m9 12 2 2 4-4"></path>
      </svg>
      <span>Finalizar compra</span>
    </button>
  </aside>
</template>

<script>
export default {
  name: "PurchaseSummaryCard",
  props: {
    items: {
      type: Array,
      required: true
    },
    total: {
      type: String,
      required: true
    }
  }
};
</script>

<style scoped>
.summary-card {
  position: sticky;
  top: 96px;
  align-self: start;
  border: 1px solid #d8dde6;
  border-radius: var(--defaultBorderRadius);
  box-shadow: none;
  padding: 28px;
}

.summary-card h2 {
  color: #111827;
  font-size: var(--baseFontSize);
  font-weight: var(--mediumFontWeight);
  margin-bottom: 52px;
}

.summary-lines {
  display: grid;
  gap: 22px;
  padding-bottom: 26px;
  border-bottom: 1px solid #d8dde6;
}

.summary-line,
.summary-total {
  display: flex;
  justify-content: space-between;
  gap: 20px;
  color: #526078;
  font-size: var(--smallFontSize);
}

.summary-line strong,
.summary-total strong {
  color: #111827;
  font-weight: var(--semiboldFontWeight);
  text-align: right;
}

.summary-total {
  align-items: center;
  margin: 30px 0 48px;
}

.summary-total span,
.summary-total strong {
  color: #111827;
  font-size: var(--baseFontSize);
}

.summary-total strong {
  font-size: 1.6rem;
  font-weight: var(--mediumFontWeight);
}

.finish-button {
  width: 100%;
  min-height: 42px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  border-radius: var(--smallBorderRadius);
  background: #8c8f99;
}

.finish-button svg {
  width: 16px;
  height: 16px;
  fill: none;
  stroke: currentColor;
  stroke-width: 2;
  stroke-linecap: round;
  stroke-linejoin: round;
}

@media (max-width: 980px) {
  .summary-card {
    position: static;
  }
}
</style>
