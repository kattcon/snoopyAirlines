import { createApp } from 'vue';
import './styles/styles.css';
import App from "./App.vue";
import { createRouter, createWebHistory } from "vue-router";
import LoginPage from "./components/LoginPage.vue";
import InternalLandingPage from './components/InternalLandingPage.vue';
import InternalLandingPageFlights from './views/InternalLandingPageFlights.vue';
import InternalLandingPageReports from './views/InternalLandingPageReports.vue';
import InternalLandingPageUsers from './views/InternalLandingPageUsers.vue';
import UsersList from './views/UsersList.vue';
import LandingPage from "./components/LandingPage.vue";
import RegisterPage from "./components/RegisterPage.vue";
import AirportsList from "./views/AirportsList.vue";
import RegisterAirport from "./views/RegisterAirport.vue";
import RegisterFlight from "./views/RegisterFlight.vue";
import RegisterUser from "./views/RegisterUser.vue";
import RegisterAircraft from './views/RegisterAircraft.vue';
import AircraftsList from './views/AircraftsList.vue';
import UserInfo from './views/UserInfo.vue';
import EditAirport from './views/EditAirport.vue';

const router = createRouter({
    history: createWebHistory(),
    routes: [
        { path: "/", name: "Home", component: LandingPage },
        { path: "/login", name: "Login", component: LoginPage },
        { path: "/register", name: "Register", component: RegisterPage },
        { path: "/admin", children: 
            [ { path: 'flights', component: InternalLandingPageFlights },
              { path: 'users', component: InternalLandingPageUsers},
              { path: 'reports', component: InternalLandingPageReports},
              { path: 'airports', component: AirportsList },
              { path: 'register-airport', component: RegisterAirport },
              { path: 'register-flight', component: RegisterFlight },
              { path: 'register-user', component: RegisterUser },
              { path: 'consult-flights', component: RegisterFlight },
              { path: 'list-users', component: UsersList },
              { path: 'register-aircraft', component: RegisterAircraft },
              { path: 'list-aircrafts', component: AircraftsList },
              { path: 'user-info', component: UserInfo },
              { path: 'edit-airport/:id', component: EditAirport },
            ], name: "Admin", component: InternalLandingPage},
    ],
});

createApp(App).use(router).mount("#app");
