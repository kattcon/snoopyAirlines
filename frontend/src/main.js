import { createApp } from 'vue';
import App from "./App.vue";
import { createRouter, createWebHistory } from "vue-router";
import LoginPage from "./components/LoginPage.vue";

const router = createRouter({
    history: createWebHistory(),
    routes: [
        { path: "/", name: "Home", component: App },
        { path: "/login", name: "Login", component: LoginPage },
    ],
});

createApp(App).use(router).mount("#app");
