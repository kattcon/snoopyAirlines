import { createApp } from 'vue';
import './styles/styles.css';
import App from "./App.vue";
import { createRouter, createWebHistory } from "vue-router";
import LoginPage from "./components/LoginPage.vue";
import InternalLandingPage from './components/InternalLandingPage.vue';
import InternalLandingPageRoutes from './views/InternalLandingPageRoutes.vue';
import InternalLandingPageReports from './views/InternalLandingPageReports.vue';
import InternalLandingPageUsers from './views/InternalLandingPageUsers.vue';
import UsersList from './views/UsersList.vue';
import LandingPage from "./components/LandingPage.vue";
import RegisterPage from "./components/RegisterPage.vue";
import AirportsList from "./views/AirportsList.vue";
import RegisterAirport from "./views/RegisterAirport.vue";
import RegisterRoute from "./views/RegisterRoute.vue";
import RegisterUser from "./views/RegisterUser.vue";
import RegisterAircraft from './views/RegisterAircraft.vue';
import AircraftsList from './views/AircraftsList.vue';
import UserInfo from './views/UserInfo.vue';
import EditAirport from './views/EditAirport.vue';
import PassengerInfo from './views/PassengerInfo.vue';
import EditAircraft from './views/EditAircraft.vue';
import PurchaseConfirmation from './views/PurchaseConfirmation.vue';
import OrderConfirmation from './views/OrderConfirmation.vue';
import EditUser from './views/EditUser.vue';
import ClientFlightReport from './components/ClientFlightReport.vue';
import ConfirmCancellation from './components/ConfirmCancellation.vue';
import RoutesList from "./views/RoutesList.vue";

const router = createRouter({
    history: createWebHistory(),
    routes: [
        { path: "/", name: "Home", component: LandingPage },
        { path: "/login", name: "Login", component: LoginPage },
        { path: "/register", name: "Register", component: RegisterPage },
        { path: "/booking", name: "Booking", component: PassengerInfo},
        { path: "/purchase-confirmation/:purchaseOrderId?", name: "PurchaseConfirmation", component: PurchaseConfirmation },
        { path: "/order-confirmation/:bookingGuid?", name: "OrderConfirmation", component: OrderConfirmation },
        { path: "/confirm-cancellation", name: "ConfirmCancellation", component: ConfirmCancellation },
        { path: "/client-flight-report", name: "ClientFlightReport", component: ClientFlightReport },
        { path: "/admin", children: 
            [ { path: 'routes', component: InternalLandingPageRoutes },
              { path: 'users', component: InternalLandingPageUsers},
              { path: 'reports', component: InternalLandingPageReports},
              { path: 'airports', component: AirportsList },
              { path: 'register-airport', component: RegisterAirport },
              { path: 'register-route', component: RegisterRoute },
              { path: 'register-user', component: RegisterUser },
              { path: 'consult-routes', component: RegisterRoute },
              { path: 'list-users', component: UsersList },
              { path: 'register-aircraft', component: RegisterAircraft },
              { path: 'list-aircrafts', component: AircraftsList },
              { path: 'user-info', component: UserInfo },
              { path: 'edit-airport/:id', component: EditAirport },
              { path: 'edit-aircraft/:id', component: EditAircraft },
              { path: 'edit-user/:id', component: EditUser },
              { path: 'list-routes', component: RoutesList },
            ], name: "Admin", component: InternalLandingPage},
    ],
});

createApp(App).use(router).mount("#app");
