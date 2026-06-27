<template>
    <Teleport to="body">
        <div class="paymentOverlay">
            <div class="paymentCard">
                <div class="paymentModalHeader">
                    <img src="@/assets/SnoopyMoney.png" alt="Snoopy" class="paymentHeaderImage" />
                    <button class="paymentCloseButton" type="button" aria-label="Cerrar" @click="$emit('cancel')">
                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round">
                            <line x1="18" y1="6" x2="6" y2="18"/>
                            <line x1="6" y1="6" x2="18" y2="18"/>
                        </svg>
                    </button>
                </div>

                <h3 class="baggageModalTitle">Datos de pago</h3>

                <PaymentMethodCard
                    v-model="localPaymentDetails"
                    :errors="errors"
                />

                <button
                    type="button"
                    class="primaryButton baggagePayButton"
                    @click="submit"
                    :disabled="isProcessing"
                >
                    Confirmar pago ${{ totalToPay }} {{ currency }}
                </button>
            </div>
        </div>
    </Teleport>
</template>

<script>
import PaymentMethodCard from './purchase-confirmation/PaymentMethodCard.vue'

export default {
    name: 'PaymentModal',
    components: { PaymentMethodCard },
    props: {
        totalToPay:   { type: Number,  required: true },
        currency:     { type: String,  required: true },
        isProcessing: { type: Boolean, default: false }
    },
    emits: ['confirm', 'cancel'],
    data() {
        return {
            localPaymentDetails: {
                email: '',
                cardNumber: '',
                cardholderName: '',
                expirationDate: '',
                cvv: ''
            },
            errors: {}
        };
    },
    methods: {
        validate() {
            const errors = {};
            const { email, cardNumber, cardholderName, expirationDate, cvv } = this.localPaymentDetails;

            if (!email || !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email))
                errors.email = 'Ingresa un correo válido';

            const cleanCard = cardNumber.replace(/\s/g, '');
            if (!cleanCard || !/^\d{16}$/.test(cleanCard))
                errors.cardNumber = 'Ingresa un número de tarjeta válido de 16 dígitos';

            if (!cardholderName || cardholderName.trim().length < 3)
                errors.cardholderName = 'Ingresa el nombre del titular';

            if (!expirationDate || !/^(0[1-9]|1[0-2])\/\d{2}$/.test(expirationDate)) {
                errors.expirationDate = 'Ingresa una fecha válida (MM/AA)';
            } else {
                const [month, year] = expirationDate.split('/');
                const expiry = new Date(2000 + parseInt(year), parseInt(month) - 1);
                if (expiry < new Date())
                    errors.expirationDate = 'La tarjeta está vencida';
            }

            if (!cvv || !/^\d{3,4}$/.test(cvv))
                errors.cvv = 'Ingresa un CVV válido';

            this.errors = errors;
            return Object.keys(errors).length === 0;
        },
        submit() {
            if (!this.validate()) return;
            this.$emit('confirm', this.localPaymentDetails);
        }
    }
};
</script>

<style scoped>
.paymentOverlay {
    position: fixed;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background: rgba(0, 0, 0, 0.5);
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 2000;
}

.paymentCard {
    background: white;
    border-radius: 12px;
    padding: 2rem;
    width: 600px;
    max-width: 95vw;
    max-height: 90vh;
    overflow-y: auto;
    display: flex;
    flex-direction: column;
    gap: 1rem;
    position: relative;
}
.paymentModalHeader {
    display: flex;
    justify-content: center;
    align-items: center;
    position: relative;
    margin-bottom: 1rem;
}

.paymentHeaderImage {
    width: 100px;
    height: 100px;
    object-fit: contain;
}

.baggageCloseButton {
    position: absolute;
    right: 0;
    top: 0;
}
.paymentCloseButton {
    position: absolute;
    right: 0;
    top: 0;
    background: none;
    border: none;
    cursor: pointer;
    padding: 4px;
    display: flex;
    align-items: center;
    justify-content: center;
}

.paymentCloseButton svg {
    width: 24px;
    height: 24px;
    stroke: #111827;
}
</style>