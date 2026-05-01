import { createApp } from 'vue';
import App from "./App.vue";
import { createRouter, createWebHistory } from "vue-router";
import LoginPage from "./components/LoginPage.vue";
import RegisterAirport from "./components/RegisterAirport.vue";

const router = createRouter({
    history: createWebHistory(),
    routes: [
        { path: "/", name: "Home", component: App },
        { path: "/login", name: "Login", component: LoginPage },
        { path: "/register-airport", name: "RegisterAirport", component: RegisterAirport },
    ],
});

createApp(App).use(router).mount("#app");
