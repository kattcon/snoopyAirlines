import { createApp } from 'vue';
import App from "./App.vue";
import { createRouter, createWebHistory } from "vue-router";
import LoginPage from "./components/LoginPage.vue";
import InternalLandingPage from './components/InternalLandingPage.vue';
import InternalLandingPageFlights from './views/InternalLandingPageFlights.vue';
import InternalLandingPageReports from './views/InternalLandingPageReports.vue';
import InternalLandingPageUsers from './views/InternalLandingPageUsers.vue';
import LandingPage from "./components/LandingPage.vue";
import AirportsList from "./components/AirportsList.vue";
import RegisterAirport from "./components/RegisterAirport.vue";

const router = createRouter({
    history: createWebHistory(),
    routes: [
        { path: "/", name: "Home", component: LandingPage },
        { path: "/login", name: "Login", component: LoginPage },
        { path: "/admin", children: 
            [ { path: 'flights', component: InternalLandingPageFlights },
              { path: 'users', component: InternalLandingPageUsers},
              { path: 'reports', component: InternalLandingPageReports},
            ], name: "Admin", component: InternalLandingPage},
        { path: "/airports", name: "AirportsList", component: AirportsList },
        { path: "/register-airport", name: "RegisterAirport", component: RegisterAirport },
    ],
});

createApp(App).use(router).mount("#app");
