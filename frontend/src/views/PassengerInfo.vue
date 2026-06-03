<template>
    <div class="passenger-info-page">
        <div class="logo">
            <a href="/"><img src="../assets/logoSA.png" alt="logo" class="logo-image" /></a>
            <h3 class="logo-text">Snoopy Airlines</h3>
        </div>
        <h2 class="page-title">Completa la informacion de cada pasajero</h2>
        <div v-if="showBanner" class="info-banner">
            <span>Ingresa la informacion tal como aparece en el documento de viaje.</span>
            <button class="banner-close" @click="showBanner = false">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <line x1="18" y1="6" x2="6" y2="18"/>
                    <line x1="6" y1="6" x2="18" y2="18"/>
                </svg>
            </button>
        </div>
        
        <div 
            class="passenger-accordion" 
            v-for="(passenger, index) in passengers"
            :key="index"
            >

            <div class="accordion-header" @click="togglePassenger(index)">
                <div style="display: flex; align-items: center; gap: 8px;">
                    <span>Pasajero {{ index + 1 }}</span>
                    <span class="error-icon" v-if="!PassengerValid(index)">!</span>
                </div>
                <span>{{ openPassenger === index ? '▲' : '▼' }}</span>
            </div>
            <div class="accordion-body" v-if="openPassenger === index">
                <div class="form-row">
                    <div class="form-field">
                        <label>Género</label>
                        <select v-model="passenger.gender" :class="{'input-error': errors[index]?.gender}">
                            <option value="">Seleccionar</option>
                            <option value="masculino">Masculino</option>
                            <option value="femenino">Femenino</option>
                            <option value="otro">Sin especificar</option>
                        </select>
                        <span class="error-msg" v-if="errors[index]?.gender">❗ Obligatorio</span>
                    </div>
                    <div class="form-field">
                        <label>Nombre(s)*</label>
                        <input
                            v-model="passenger.firstName"
                            :class="{'input-error': errors[index]?.firstName}"
                            placeholder="Nombre(s)*"
                        />
                        <span class="error-msg" v-if="errors[index]?.firstName">❗ Obligatorio</span>
                    </div>
                    <div class="form-field">
                        <label>Apellidos(s)*</label>
                        <input
                            v-model="passenger.lastName"
                            :class="{'input-error': errors[index]?.lastName}"
                            placeholder="Apellido(s)*"
                        />
                        <span class="error-msg" v-if="errors[index]?.lastName">❗ Obligatorio</span>
                    </div>
                </div>

                <div class="form-row">
                    <div class="form-field">
                        <label>Fecha de nacimiento</label>
                        <div class="date-selects">
                            <select v-model="passenger.birthDay" :class="{'input-error': errors[index]?.birthDay}">
                                <option value="">Día</option>
                                <option v-for="day in 31" :key="day" :value="day">{{ day }}</option>
                            </select>
                            <select v-model="passenger.birthMonth" :class="{'input-error': errors[index]?.birthMonth}">
                                <option value="">Mes</option>
                                <option v-for="(month,index) in months" :key="index" :value="index + 1">{{ month }}</option>
                            </select>
                            <select v-model="passenger.birthYear" :class="{'input-error': errors[index]?.birthYear}">
                                <option value="">Año</option>
                                <option v-for="year in years" :key="year" :value="year">{{ year }}</option>
                            </select>
                        </div>
                        <span class="error-msg" v-if="errors[index]?.birthdate">❗ Obligatorio</span>
                    </div>
                    <div class="form-field">
                        <label>Nacionalidad del documento de viaje*</label>
                        <select v-model="passenger.nationality" :class="{'input-error': errors[index]?.nationality}">
                            <option value="">Seleccionar</option>
                            <option value="Costa rica">Costa Rica</option>
                            <option value="Estados Unidos">Estados Unidos</option>
                        </select>
                        <span class="error-msg" v-if="errors[index]?.nationality">❗ Obligatorio</span>
                    </div>
                </div>
                
            </div>
            
        </div>
        
        <div class="passenger-accordion">
                    <div class="accordion-header" @click="toggleHolder" >
                        <div style="display: flex; align-items: center; gap: 8px;">
                            <span><strong>Titular de la reserva</strong></span>
                            <span class="error-icon" v-if="!HolderValid()">!</span>

                        </div>
                        
                        <span>{{ openHolder ? '▲' : '▼' }}</span>
                    </div>
                    <div class="accordion-body" v-if="openHolder">
                        <p class="holder-desc">Sera la persona a la cual contactaremos para informar sobre la reserva.</p>
                        <div class="form-row">
                            <div class="form-field">
                                <label>Nombre*</label>
                                <input v-model="holder.firstName" placeholder="Nombre*"/>
                                <span class="error-msg" v-if="errors.holder?.firstName">❗ Obligatorio</span>
                            </div>
                            <div class="form-field">
                                <label>Apellido*</label>
                                <input v-model="holder.lastName" placeholder="Apellido*"/>
                                <span class="error-msg" v-if="errors.holder?.lastName">❗ Obligatorio</span>
                            </div>
                            <div class="form-field">
                                <label>Email*</label>
                                <input v-model="holder.email" placeholder="Email*" type="email"/>
                                <span class="error-msg" v-if="errors.holder?.email">❗ Obligatorio</span>
                            </div>
                        </div>
                    </div>
                </div>
        <button class="btn-submit" @click="submitPassengers">Continuar al pago</button>    
    </div>

</template>

<script>
import axios from "axios";
export default {
    name: "passengerInfo",
    data() {
        const passengerCount = parseInt(this.$route.query.passengersCount);
        
        return {
            showBanner:true,
            openPassenger:0,
            openHolder: false,
            passengers: Array.from({length: passengerCount}, () => ({
                gender: '',
                firstName: '',
                lastName: '',
                birthDay: '',
                birthMonth: '',
                birthYear: '',
                nationality: '',
            })),
            holder: {
                firstName: '',
                lastName: '',
                email: '',
            },
            errors: [],
            months: ['Enero','Febrero','Marzo','Abril','Mayo','Junio','Julio',
            'Agosto','Septiembre','Octubre','Noviembre','Diciembre'],
        };
    },
    computed:{
        years() {
            const current = new Date().getFullYear();
            return Array.from({length:100}, (_,i) => current - i);
        }
    },

    methods: {
        togglePassenger(index) {
            this.openPassenger = this.openPassenger === index ? null : index;
        },
        toggleHolder() {
            this.openHolder = !this.openHolder;
        },
        PassengerValid(index) {
            const passenger = this.passengers[index];
            return passenger.firstName && passenger.lastName && passenger.gender &&
            passenger.birthDay && passenger.birthMonth && passenger.birthYear && passenger.nationality;
        },
        HolderValid() {
            return this.holder.firstName && this.holder.lastName && this.holder.email;
        },
        submitPassengers() {
            this.errors = this.passengers.map(passenger => ({
                firstName: !passenger.firstName,
                lastName: !passenger.lastName,
                gender: !passenger.gender,
                nationality: !passenger.nationality,
                birthdate: !(passenger.birthDay || passenger.birthMonth || passenger.birthYear),
            }));

            this.errors.holder = {
                firstName: !this.holder.firstName,
                lastName: !this.holder.lastName,
                email: !this.holder.email,
            };

            const hasErrorsPassenger = this.errors.some(error => error.firstName ||
            error.lastName || error.gender || error.nationality || error.birthdate);
            const hasErrorsHolder = this.errors.holder.firstName || this.errors.holder.lastName || this.errors.holder.email;
            if(hasErrorsPassenger || hasErrorsHolder){
                return;
            }

            const routes = this.bookingRoutes();
            if (routes.length === 0) {
                alert("No se encontraron los tramos del vuelo seleccionado");
                return;
            }

            const purchaseOrderRequest = {
                routes,
                seatClass: this.$route.query.seatClass,
                passengers: this.passengers.map(p => ({
                    ...p,
                    birthDay: String(p.birthDay),
                    birthMonth: String(p.birthMonth),
                    birthYear: String(p.birthYear),
                }))
            };
            console.log('Request:', JSON.stringify(purchaseOrderRequest));
            axios.post(`${process.env.VUE_APP_BACKEND_URL}/PurchaseOrder`, purchaseOrderRequest)
            .then((response) => {
                const purchaseOrderId = response.data.id ?? response.data.Id;
                sessionStorage.setItem('purchaseOrderId', purchaseOrderId);
                sessionStorage.setItem('bookingHolderEmail', this.holder.email);
                sessionStorage.setItem('bookingHolderName', `${this.holder.firstName} ${this.holder.lastName}`.trim());
                this.$router.push({
                    name: 'PurchaseConfirmation',
                    params: { purchaseOrderId }
                });
            })
            .catch((error) => {
                alert("Error al guardar los datos de los pasajeros");
                console.error(error);
            });
        },
        bookingRoutes() {
            const rawRoutes = Array.isArray(this.$route.query.routes)
                ? this.$route.query.routes[0]
                : this.$route.query.routes;

            if (!rawRoutes) {
                return [];
            }

            try {
                const routes = JSON.parse(rawRoutes);
                if (!Array.isArray(routes)) {
                    return [];
                }

                return routes
                    .map((route, index) => ({
                        sequenceNumber: Number(route.sequenceNumber) || index + 1,
                        routeId: Number(route.routeId),
                        intendedDate: route.intendedDate,
                    }))
                    .filter(route => route.routeId > 0 && route.intendedDate);
            } catch (error) {
                console.error("Error leyendo los tramos del vuelo:", error);
                return [];
            }
        }
    }
}
</script>

<style scoped>

.logo{
    position: fixed;
    top: 0;
    left: 0;
    width: 100%;
    display: flex;
    align-items: center;
    gap: 10px;
    padding: 15px 0 10px 15%;
    border-bottom: 1px solid #b4b4b4;
    background-color: rgb(252, 250, 250);
    z-index: 1000;
    box-sizing: border-box;
}

.passenger-info-page {
  width: 70%;
  display: flex;
  flex-direction: column;
  gap: 16px;
  margin: 0 auto;
  margin-top: 70px;
  padding: 24px;
}

.page-title {
  color: #e5ecf3;
  font-size: 2rem;
}

.info-banner {
  font-size: 18px;
  font-weight: 500;
  color: #324c64;
  display: flex;
  justify-content: space-between;
  align-items: center;
  background: #e1f0f0;
  padding: 12px 16px;
  border-radius: 8px;
  margin-bottom: 20px;
}

.info-banner button {
  background: none;
  border: none;
  cursor: pointer;
  font-size: 16px;
}

.passenger-accordion {
    background-color: white;
    border: 1px solid rgb(192, 187, 187);
    margin-bottom: 30px;
    box-shadow: 0 1px 6px rgba(0,0,0,0.1);
    overflow: hidden;
    border-radius: 12px;
    transition: transform 0.2s, box-shadow 0.2s;
}

.accordion-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 30px 20px;
    cursor: pointer;
    background-color: rgb(252, 250, 250);
    color: #6b7280;
    font-weight: 500;
    transition: background-color 0.2s, color 0.2s;
}
.passenger-accordion:hover {
    transform: translateY(-4px);
    box-shadow: 0 8px 20px rgba(0,0,0,0.15);
}

.accordion-header.active {
    background-color: #0e4f9f;
    color: white;
}

.accordion-body {
  padding: 30px 20px;
  background-color: rgb(252, 250, 250);
}

.form-row {
  display: flex;
  gap: 12px;
  margin-bottom: 16px;
  flex-wrap: wrap;
}

.form-field {
  display: flex;
  flex-direction: column;
  flex: 1;
  min-width: 0; 
}

.form-field label {
  font-size: 12px;
  color: #666;
  margin-bottom: 4px;
}

.form-field input,
.form-field select {
  width: 100%;
  height: 80px; 
  padding: 10px;
  border: 1px solid #ccc;
  border-radius: 8px;
  font-size: 14px;
}

.input-error {
  border-color: red !important;
}

.error-msg {
  color: red;
  font-size: 12px;
  margin-top: 4px;
}

.error-icon {
  display: inline-flex;
    align-items: center;
    justify-content: center;
    width: 20px;
    height: 20px;
    border-radius: 50%;
    background-color: #e53e3e;
    color: white;
    font-size: 13px;
    font-weight: bold;
    margin-left: 8px;
}

.date-selects {
  display: flex;
  gap: 8px;
}

.date-selects select {
  flex: 1;
  padding: 10px;
  border: 1px solid #ccc;
  border-radius: 8px;
}

.form-check, .form-toggle {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 12px;
  font-size: 14px;
}

.holder-desc {
  font-size: 13px;
  color: #555;
  margin-bottom: 16px;
}

.btn-submit {
  padding: 16px 40px;
  background: #0056b3;
  color: white;
  border: none;
  border-radius: 4px;
  font-size: 1.1rem;
  font-weight: 600;
  cursor: pointer;
  transition: transform 0.3s, box-shadow 0.3s;
  align-self: center;
}

.btn-submit:hover {

    transform: translateY(-4px);
    box-shadow: 0 8px 20px rgba(0,0,0,0.15);
}
.banner-close {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 32px;
    height: 32px;
    border: none;
    background: transparent;
    border-radius: 50%;
    cursor: pointer;
    color: #4a5568;
    transition: background-color 0.2s;
}

.banner-close:hover {
    background-color: rgba(0, 0, 0, 0.1);
}

.banner-close svg {
    width: 18px;
    height: 18px;
}

</style>
