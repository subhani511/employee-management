<!-- src/pages/Employees.vue -->
<template>
  <div class="app-container">
    <header class="app-header">
      <div class="brand">
        <div class="logo">EM</div>
        <div>
          <div class="app-title">Employee Management</div>
          <div class="app-sub">Manage employees — fast, bold & clear</div>
        </div>
      </div>
      <div style="margin-left: auto" class="small-muted">Connected</div>
    </header>

    <div class="columns">
      <section class="col-left">
        <div class="h2">Employees</div>

        <div v-if="loading" class="small-muted">Loading employees...</div>

        <div class="employee-list" v-else-if="employees && employees.length">
          <article v-for="e in employees" :key="e.id" class="employee-card">
            <div class="employee-meta">
              <div class="avatar">
                {{ e.firstName?.charAt(0) ?? ""
                }}{{ e.lastName?.charAt(0) ?? "" }}
              </div>
              <div class="employee-info">
                <div class="employee-name">
                  {{ e.firstName }} {{ e.lastName }}
                </div>
                <div class="employee-detail">
                  {{ e.employeeType }} • {{ formatDate(e.dateOfHire) }}
                </div>
              </div>
            </div>

            <div class="actions">
              <button class="btn btn-ghost" @click="startEdit(e)">Edit</button>
              <button class="btn btn-danger" @click="doDelete(e.id)">
                Delete
              </button>
            </div>
          </article>
        </div>

        <div v-else class="small-muted">
          No employees yet. Create one from the form.
        </div>
      </section>

      <aside class="col-right">
        <div class="form-card">
          <div class="h3">
            {{ editing ? "Edit Employee" : "Create Employee" }}
          </div>
          <EmployeeForm :initial="editing" @saved="onSaved" />
        </div>
      </aside>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, onMounted, inject } from "vue";
import EmployeeForm from "../components/EmployeeForm.vue";
import { listEmployees, deleteEmployee } from "../api/employeeApi";
import type { Employee } from "../types/employee";
import type { Notyf } from "notyf";

// reactive state
const employees = ref<Employee[]>([]);
const loading = ref(true);
const error = ref<string | null>(null);
const editing = ref<Employee | null>(null);

// inject Notyf (may be undefined if not provided)
const notyf = inject("notyf") as Notyf | undefined;

async function load() {
  loading.value = true;
  error.value = null;
  try {
    employees.value = await listEmployees();
  } catch (err: any) {
    error.value = err?.message ?? "Failed to fetch";
    console.error("Failed to load employees", err);
    notyf?.error(`Failed to load employees: ${error.value}`);
  } finally {
    loading.value = false;
  }
}

onMounted(() => {
  load();
});

function startEdit(e: Employee) {
  editing.value = e;
}

async function doDelete(id: string) {
  if (!confirm("Delete employee?")) return;
  try {
    await deleteEmployee(id);
    notyf?.success("Employee deleted");
    await load();
  } catch (err: any) {
    console.error("Delete failed", err);
    notyf?.error("Delete failed: " + (err?.message ?? err));
    alert("Delete failed: " + (err?.message ?? err));
  }
}

function onSaved() {
  editing.value = null;
  // reload list after a save
  load();
}

function formatDate(s?: string) {
  if (!s) return "";
  try {
    return new Date(s).toLocaleDateString();
  } catch {
    return s;
  }
}
</script>

<style scoped>
/* keep minimal page-specific adjustments; global styles are in src/style.css */
.columns {
  display: flex;
  gap: 16px;
}
.col-left {
  flex: 1;
}
.col-right {
  width: 420px;
}
</style>
