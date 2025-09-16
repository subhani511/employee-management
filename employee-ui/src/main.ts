import { createApp } from "vue";
import App from "./App.vue";
import router from "./router";
import "./style.css";

// 👇 import notyf
import { Notyf } from "notyf";
import "notyf/notyf.min.css";

const app = createApp(App);

// 👇 create Notyf instance and provide globally
const notyf = new Notyf({
  duration: 3000,
  position: { x: "right", y: "top" },
});
app.provide("notyf", notyf);

app.use(router).mount("#app");
