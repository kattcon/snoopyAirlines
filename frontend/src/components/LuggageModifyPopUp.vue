<template>
    <Teleport to="body">
        <Transition name="luggage-fade">
            <div
                class="luggageModalOverlay"
                @click.self="closeModal"    
                >
                <div class="baggageModalCard" role="dialog" aria-modal="true" aria-labelledby="baggageModalTitle">
                    <button class="baggageCloseButton" type="button" aria-label="Cerrar" @click="closeModal">
                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round">
                            <line x1="18" y1="6" x2="6" y2="18"/>
                            <line x1="6" y1="6" x2="18" y2="18"/>
                        </svg>
                    </button>

                    <div v-if="isLoading" class="baggageLoadingState">
                      <p>Cargando informacion de equipaje...</p>
                    </div>

                    <div v-else-if="loadError" class="baggageErrorState">
                      <p class="baggageCapacityError">{{ loadError }}</p>
                      <button type="button" class="primaryButton" @click="fetchReservationData">
                        Reintentar
                      </button>
                    </div>

                    <template v-else-if="!paymentSuccess">

                        <div class="baggageModalHeader">
                            <div class="baggageHeaderIcon">
                            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                                <rect x="3" y="7" width="18" height="13" rx="2"/>
                                <path d="M8 7V5a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"/>
                                <line x1="3" y1="12" x2="21" y2="12" />
                            </svg>
                            </div>
                            <div>
                            <h2 id="baggageModalTitle" class="baggageModalTitle">Maletas documentadas</h2>
                            <p class="baggageModalSubtitle">
                                Agrega maletas adicionales por ${{ pricePerBag }} {{ currency }} por maleta
                            </p>
                            </div>
                        </div>

                        <div class="baggageDivider" ></div>

                        <p v-if="errorMessage" class="baggageCapacityError">
                            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                            <circle cx="12" cy="12" r="10"/>
                            <line x1="12" y1="8" x2="12" y2="12"/>
                            <line x1="12" y1="16" x2="12" y2="16"/>
                            </svg>
                            {{ errorMessage }}
                        </p>


                        <div class="baggagePassengerList">
                            <div v-for="passenger in localPassengers" :key="passenger.id" class="baggageRow">
                                <div class="baggagePassengerInfo">
                                    <div class="baggageAvatar">
                                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                                        <circle cx="12" cy="8" r="4" />
                                        <path d="M4 20c0-4 3.5-7 8-7s8 3 8 7" />
                                    </svg>
                                    </div>
                                    <p class="baggagePassengerName">{{ passenger.firstName }} {{ passenger.lastName }}</p>
                                </div>

                                <div class="baggageCounter">

                                    <button
                                        type="button"
                                        class="baggageCounterButton"
                                        :disabled="passenger.currentBags <=0"
                                        aria-label="Quitar maleta"
                                        @click="decreaseBags(passenger)"
                                        >
                                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                                            <line x1="5" y1="12" x2="19" y2="12"/>
                                        </svg>                        
                                    </button>

                                    <span class="baggageCounterValue">{{ passenger.currentBags }}</span>

                                    <button 
                                        type="button"
                                        class="baggageCounterButton"
                                        :disabled="!canIncrement(passenger)"
                                        aria-label="Agregar maleta"
                                        @click="increaseBags(passenger)"
                                        >
                                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                                            <line x1="5" y1="12" x2="19" y2="12"/>
                                            <line x1="12" y1="5" x2="12" y2="19" />
                                        </svg>                        
                                    </button>
                                </div>
                            </div>
                        </div>

                        <div class="baggageTotalBox">
                            <span class="baggageTotalLabel">Precio total</span>
                            <span class="baggageTotalValue">
                                ${{ totalToPay }} <span class="baggageTotalCurrency">{{ currency }}</span>
                            </span>
                        </div>

                        <button
                            type="button"
                            class="primaryButton baggagePayButton"
                            :disabled="isProcessing || totalExtraBags === 0"
                            @click="handlePay"
                            >
                            {{ isProcessing ? 'Procesando...' : 'Pagar' }}
                        </button>
                    </template>

                    <template v-else>
                        <div class="baggageSuccessState">
                            <div class="baggageSuccesIcon">
                                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                                    <circle cx="12" cy="12" r="10"/>
                                    <path d="m9 12 2 2 4-4"/>
                                </svg>
                            </div>
                            <h2 class="baggageModalTitle">Pago realizado</h2>
                            <p class="baggageModalSubtitle">{{ successMessage }}</p>
                            <button type="button" class="primaryButton baggagePayButton" @click="closeModal">
                                Listo
                            </button>
                        </div>
                    </template>                    
                </div>
            </div>
        </Transition>
    </Teleport>
</template>

<script>
import axios from 'axios'
import '../styles/styles.css'
export default {
  name: 'LuggageModifyPopUp',
  props: {
    reservationNumber: {
      type: String,
      required: true
    }
  },
  data() {
    return {
      isLoading: false,
      loadError:'',
      isProcessing: false,
      paymentSuccess: false,
      errorMessage: '',
      lastPaidAmount: 0,
      localPassengers:[],
      pricePerBag: 0,
      currency: 'USD',
      availableCapacity:0
    };
  },
  computed: {
    totalExtraBags() {
      return this.localPassengers.reduce((sum, p) => sum + this.extrabagsFor(p), 0);
    },
    totalToPay() {
      return this.totalExtraBags * this.pricePerBag;
    },
    successMessage() {
        if(this.lastPaidAmount === 0) {
            return 'Tu equipaje fue actualizado sin costo adicional.';
        }
        return `Se cobraron $${this.lastPaidAmount} ${this.currency}. Tu equipaje documentado fue actualizado`;
    }
  },
  mounted() {
    this.fetchReservationData();
  },
  methods: {
    async fetchReservationData() {
        this.isLoading = true;
        this.loadError = '';

        this.pricePerBag = 45;
        this.currency = 'USD';
        this.availableCapacity = 4;

        try{
            const response = await axios.get(`${process.env.VUE_APP_BACKEND_URL}/flight-search/searchPassengers`, {
            params: {
                confirmationNumber : this.reservationNumber
            }
            })
            this.localPassengers = response.data.map(p => ({
            firstName:    p.firstName,
            lastName:     p.lastName,
            originalBags: parseInt(p.checkedLuggage),
            currentBags:  parseInt(p.checkedLuggage)
            }))

        }catch (error){
            if(error.response) {
                if (error.response.status ===400) {
                    this.errorMessage = 'Código de confirmación inválido'
                }
                if (error.response.status === 404) {
                    this.errorMessage = 'No se encontró ninguna reservación con ese código'
                }
                if (error.response.status === 500) {
                    this.errorMessage = 'Error en el servidor. Inténtelo de nuevo más tarde'
                }
            }
        }finally {
            this.isLoading = false
        }
    },
    extrabagsFor(passenger) {
      return Math.max(0, passenger.currentBags - passenger.originalBags);
    },
    canIncrement(passenger) {
      const currentExtra = this.extrabagsFor(passenger);
      const tentativeExtra = Math.max(0, (passenger.currentBags + 1) - passenger.originalBags);
      const deltaExtra = tentativeExtra - currentExtra;
      if (deltaExtra <= 0) return true;
      return (this.totalExtraBags + deltaExtra) <= this.availableCapacity;
    },
    increaseBags(passenger) {
      if (!this.canIncrement(passenger)) {
        this.errorMessage = 'No hay capacidad disponible en la aeronave para agregar mas maletas en este momento';
        return;
      }
      this.errorMessage = '';
      passenger.currentBags++;
    },
    decreaseBags(passenger) {
      if (passenger.currentBags <= 0) return;
      this.errorMessage = '';
      passenger.currentBags--;
    },
    async handlePay() {
      if (this.isProcessing) return;
      this.isProcessing = true;
      this.errorMessage = '';

      try {
        this.lastPaidAmount = this.totalToPay;
                this.localPassengers.forEach(p => { p.originalBags = p.currentBags; });
                this.paymentSuccess = true;

                this.$emit('payment-success', {
                    passengers:  this.localPassengers.map(p => ({ id: p.id, totalBags: p.currentBags })),
                    amountPaid:  this.lastPaidAmount,
                    currency:    this.currency
                });
      } catch(error) {
        this.errorMessage = 'Error al procesar el pago. Por favor intenta de nuevo';
        console.error('[LuggageModifyPopUp] handlePay:', error);
      } finally {

        this.isProcessing = false;
      }
    },
    closeModal() {
        if (this.isProcessing) return;
        this.$emit('close');
    }
  }
};

</script>

<style scoped>
.baggageModalOverlay {
  position: fixed;
  inset: 0;
  background-color: var(--modalOverlayColor);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: var(--mediumSpacing);
  z-index: 1000;
}

.baggageModalCard {
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  width: min(100%, var(--maximumFormWidth));
  background-color: var(--whiteColor);
  border-radius: var(--largeCardBorderRadius);
  box-shadow: var(--largeCardShadow);
  padding: var(--extraLargeSpacing);
  font-family: var(--primaryFontFamily);
  max-height: calc(100vh - 80px);
  overflow-y: auto;
  box-sizing: border-box;
}

.baggageCloseButton {
  position: absolute;
  top: var(--mediumSpacing);
  right: var(--mediumSpacing);
  width: 28px;
  height: 28px;
  display: flex;
  align-items: center;
  justify-content: center;
  border: none;
  background: transparent;
  color: var(--mutedTextColor);
  cursor: pointer;
  border-radius: var(--smallBorderRadius);
}

.baggageCloseButton:hover {
  background-color: var(--lightBackgroundColor);
}

.baggageCloseButton svg {
  width: 18px;
  height: 18px;
}

.baggageModalHeader {
  display: flex;
  align-items: flex-start;
  gap: var(--mediumSpacing);
}

.baggageHeaderIcon {
  flex-shrink: 0;
  width: 44px;
  height: 44px;
  border-radius: 50%;
  background-color: var(--accentColor);
  color: var(--whiteColor);
  display: flex;
  align-items: center;
  justify-content: center;
}

.baggageHeaderIcon svg {
  width: 22px;
  height: 22px;
}

.baggageModalTitle {
  margin: 0 0 4px 0;
  font-size: var(--mediumFontSize);
  font-weight: var(--boldFontWeight);
  color: var(--secondaryTextColor);
}

.baggageModalSubtitle {
  margin: 0;
  font-size: var(--smallFontSize);
  color: var(--mutedTextColor);
}

.baggageDivider {
  height: 1px;
  background-color: var(--tertiaryBorderColor);
  margin: var(--largeSpacing) 0;
  opacity: 0.6;
}

.baggageCapacityError {
  display: flex;
  align-items: center;
  gap: var(--smallSpacing);
  background-color: var(--errorBackgroundColor);
  color: var(--errorColor);
  border-radius: var(--defaultBorderRadius);
  padding: var(--smallSpacing) var(--mediumSpacing);
  font-size: var(--smallFontSize);
  margin: 0 0 var(--mediumSpacing) 0;
}

.baggageCapacityError svg {
  width: 18px;
  height: 18px;
  flex-shrink: 0;
}

.baggagePassengerList {
  display: flex;
  flex-direction: column;
  gap: var(--smallSpacing);
}

.baggageRow {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--mediumSpacing);
  background-color: var(--rowBackgroundColor);
  border: 1px solid var(--rowBorderColor);
  border-radius: var(--defaultBorderRadius);
  padding: var(--smallSpacing) var(--mediumSpacing);
}

.baggagePassengerInfo {
  display: flex;
  align-items: center;
  gap: var(--smallSpacing);
  min-width: 0;
}

.baggageAvatar {
  flex-shrink: 0;
  width: 36px;
  height: 36px;
  border-radius: 50%;
  background-color: var(--avatarBackgroundColor);
  color: var(--whiteColor);
  display: flex;
  align-items: center;
  justify-content: center;
}

.baggageAvatar svg {
  width: 18px;
  height: 18px;
}

.baggagePassengerName {
  margin: 0;
  font-size: var(--baseFontSize);
  font-weight: var(--semiboldFontWeight);
  color: var(--secondaryTextColor);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.baggagePassengerRole {
  margin: 2px 0 0 0;
  font-size: var(--extraSmallFontSize);
  color: var(--placeholderTextColor);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.baggageCounter {
  flex-shrink: 0;
  display: flex;
  align-items: center;
  gap: var(--smallSpacing);
}

.baggageCounterButton {
  width: var(--counterButtonSize);
  height: var(--counterButtonSize);
  border-radius: var(--smallBorderRadius);
  background-color: var(--counterButtonBackgroundColor);
  border: 1px solid var(--counterButtonBorderColor);
  color: var(--secondaryTextColor);
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
}

.baggageCounterButton:hover:not(:disabled) {
  background-color: var(--whiteColor);
}

.baggageCounterButton:disabled {
  opacity: 0.45;
  cursor: not-allowed;
}

.baggageCounterButton svg {
  width: 16px;
  height: 16px;
}

.baggageCounterValue {
  min-width: 20px;
  text-align: center;
  font-weight: var(--boldFontWeight);
  color: var(--secondaryTextColor);
}

.baggageTotalBox {
  display: flex;
  align-items: center;
  justify-content: space-between;
  background-color: var(--totalBoxBackgroundColor);
  border: 1px solid var(--totalBoxBorderColor);
  border-radius: var(--defaultBorderRadius);
  padding: var(--mediumSpacing);
  margin-top: var(--largeSpacing);
}

.baggageTotalLabel {
  color: var(--totalBoxTextColor);
  font-weight: var(--semiboldFontWeight);
  font-size: var(--baseFontSize);
}

.baggageTotalValue {
  color: var(--totalBoxTextColor);
  font-weight: var(--boldFontWeight);
  font-size: var(--largeFontSize);
}

.baggageTotalCurrency {
  font-size: var(--smallFontSize);
  font-weight: var(--semiboldFontWeight);
}

.baggagePayButton {
  width: 100%;
  margin-top: var(--mediumSpacing);
  padding: 12px var(--mediumSpacing);
  font-size: var(--baseFontSize);
}

.baggageSuccessState {
  text-align: center;
  padding: var(--mediumSpacing) 0;
}

.baggageSuccessIcon {
  width: 56px;
  height: 56px;
  margin: 0 auto var(--mediumSpacing) auto;
  border-radius: 50%;
  background-color: var(--successBackgroundColor);
  color: var(--successColor);
  display: flex;
  align-items: center;
  justify-content: center;
}

.baggageSuccessIcon svg {
  width: 28px;
  height: 28px;
}

.baggage-fade-enter-active,
.baggage-fade-leave-active {
  transition: opacity 0.18s ease;
}

.baggage-fade-enter-from,
.baggage-fade-leave-to {
  opacity: 0;
}

@media (max-width: 480px) {
  .baggageModalCard {
    padding: var(--largeSpacing);
  }
}
</style>