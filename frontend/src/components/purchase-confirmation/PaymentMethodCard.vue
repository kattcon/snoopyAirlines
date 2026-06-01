<template>
  <section class="confirmation-card contentCard">
    <h2 class="card-title">
      <svg viewBox="0 0 24 24" aria-hidden="true">
        <rect x="2" y="5" width="20" height="14" rx="2"></rect>
        <path d="M2 10h20"></path>
      </svg>
      <span>M&eacute;todo de pago</span>
    </h2>

    <form class="payment-form">
      <label class="field">
        <span>N&uacute;mero de tarjeta</span>
        <input
          class="formInput"
          inputmode="numeric"
          autocomplete="cc-number"
          placeholder="1234 5678 9012 3456"
          :value="modelValue.cardNumber"
          @input="updateField('cardNumber', $event.target.value)"
        />
      </label>

      <label class="field">
        <span>Nombre del titular</span>
        <input
          class="formInput"
          autocomplete="cc-name"
          placeholder="Como aparece en la tarjeta"
          :value="modelValue.cardholderName"
          @input="updateField('cardholderName', $event.target.value)"
        />
      </label>

      <div class="field-grid">
        <label class="field">
          <span>Fecha de expiraci&oacute;n</span>
          <input
            class="formInput"
            autocomplete="cc-exp"
            placeholder="MM/AA"
            :value="modelValue.expirationDate"
            @input="updateField('expirationDate', $event.target.value)"
          />
        </label>

        <label class="field">
          <span>CVV</span>
          <input
            class="formInput"
            inputmode="numeric"
            autocomplete="cc-csc"
            placeholder="123"
            :value="modelValue.cvv"
            @input="updateField('cvv', $event.target.value)"
          />
        </label>
      </div>
    </form>

    <p class="security-note">
      <svg viewBox="0 0 24 24" aria-hidden="true">
        <rect x="5" y="11" width="14" height="10" rx="2"></rect>
        <path d="M8 11V7a4 4 0 0 1 8 0v4"></path>
      </svg>
      Tu informaci&oacute;n de pago est&aacute; protegida con encriptaci&oacute;n de grado bancario. No almacenamos los datos de tu tarjeta.
    </p>
  </section>
</template>

<script>
export default {
  name: "PaymentMethodCard",
  props: {
    modelValue: {
      type: Object,
      required: true
    }
  },
  emits: ["update:modelValue"],
  methods: {
    updateField(fieldName, value) {
      this.$emit("update:modelValue", {
        ...this.modelValue,
        [fieldName]: value
      });
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
.security-note svg {
  width: 18px;
  height: 18px;
  fill: none;
  stroke: currentColor;
  stroke-width: 2;
  stroke-linecap: round;
  stroke-linejoin: round;
}

.payment-form {
  display: grid;
  gap: 16px;
}

.field {
  display: grid;
  gap: 7px;
}

.field span {
  color: #111827;
  font-size: var(--smallFontSize);
  font-weight: var(--semiboldFontWeight);
}

.formInput {
  height: 42px;
  border: none;
  background: #f0f2f6;
  font-size: var(--smallFontSize);
}

.field-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16px;
}

.security-note {
  display: flex;
  gap: 10px;
  margin-top: 22px;
  padding: 18px;
  border-radius: var(--defaultBorderRadius);
  background: #f7f8fb;
  color: #526078;
  font-size: var(--smallFontSize);
  line-height: 1.5;
}

.security-note svg {
  flex: 0 0 auto;
  margin-top: 1px;
}

@media (max-width: 560px) {
  .confirmation-card {
    padding: 22px;
  }

  .field-grid {
    grid-template-columns: 1fr;
  }
}
</style>
