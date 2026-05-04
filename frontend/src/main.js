import { createApp } from 'vue';
import App from "./App.vue";
import { createRouter, createWebHistory } from "vue-router";
import LoginPage from "./components/LoginPage.vue";
import InternalLandingPage from './components/InternalLandingPage.vue';
import InternalLandingPageFlights from './views/InternalLandingPageFlights.vue';
import InternalLandingPageReports from './views/InternalLandingPageReports.vue';
import InternalLandingPageUsers from './views/InternalLandingPageUsers.vue';
import InternalLandingPageConsultFlights from './views/InternalLandingPageConsultFlights.vue';
import UsersList from './views/UsersList.vue';
import LandingPage from "./components/LandingPage.vue";
import AirportsList from "./views/AirportsList.vue";
import RegisterAirport from "./views/RegisterAirport.vue";
import RegisterFlight from "./views/RegisterFlight.vue";

const router = createRouter({
    history: createWebHistory(),
    routes: [
        { path: "/", name: "Home", component: LandingPage },
        { path: "/login", name: "Login", component: LoginPage },
        { path: "/admin", children: 
            [ { path: 'flights', component: InternalLandingPageFlights },
              { path: 'users', component: InternalLandingPageUsers},
              { path: 'reports', component: InternalLandingPageReports},
              { path: 'airports', component: AirportsList },
              { path: 'register-airport', component: RegisterAirport },
              { path: 'register-flight', component: RegisterFlight },
              { path: 'consult-flights', component: InternalLandingPageConsultFlights },
              { path: 'list-users', component: UsersList },
            ], name: "Admin", component: InternalLandingPage},
    ],
});

createApp(App).use(router).mount("#app");
