// src/router/index.ts
import { createRouter, createWebHistory } from 'vue-router';
import EmployeesPage from '../pages/Employees.vue';

const routes = [
  { path: '/', component: EmployeesPage },
  { path: '/employees', component: EmployeesPage },
];

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes,
});

export default router;

