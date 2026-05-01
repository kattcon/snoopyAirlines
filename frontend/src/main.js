import { createApp } from 'vue';
import App from "./App.vue";
import { createRouter, createWebHistory } from "vue-router";
import LoginPage from "./components/LoginPage.vue";
import LandingPage from "./components/LandingPage.vue";

const router = createRouter({
    history: createWebHistory(),
    routes: [
        { path: "/", name: "Home", component: LandingPage },
        { path: "/login", name: "Login", component: LoginPage },
    ],
});

createApp(App).use(router).mount("#app");
