<!-- src/components/EmployeeForm.vue -->
<template>
  <form @submit.prevent="submit" style="display: grid; gap: 12px">
    <div class="form-row">
      <input
        class="input"
        v-model="form.firstName"
        placeholder="First name"
        required
      />
      <input
        class="input"
        v-model="form.lastName"
        placeholder="Last name"
        required
      />
    </div>

    <input
      class="input"
      v-model="form.email"
      placeholder="Email"
      type="email"
      required
    />
    <input class="input" v-model="form.dateOfHire" type="date" />

    <select class="input" v-model="form.employeeType">
      <option value="FullTime">FullTime</option>
      <option value="Contractor">Contractor</option>
    </select>

    <div v-if="form.employeeType === 'FullTime'">
      <input
        class="input"
        v-model.number="form.annualSalary"
        placeholder="Annual salary"
        type="number"
      />
    </div>

    <div v-else class="form-row">
      <input
        class="input"
        v-model.number="form.hourlyRate"
        placeholder="Hourly rate"
        type="number"
      />
      <input
        class="input"
        v-model.number="form.hoursPerMonth"
        placeholder="Hours per month"
        type="number"
      />
    </div>

    <div style="display: flex; gap: 10px; margin-top: 6px">
      <button class="btn btn-primary" type="submit" :disabled="saving">
        <span v-if="saving">{{ form.id ? "Saving..." : "Creating..." }}</span>
        <span v-else>{{ form.id ? "Save" : "Create" }}</span>
      </button>

      <button
        v-if="initial"
        type="button"
        class="btn btn-ghost"
        @click="cancel"
        :disabled="saving"
      >
        Cancel
      </button>
    </div>
  </form>
</template>

<script lang="ts" setup>
import { reactive, watch, computed, inject, ref } from "vue";
import { createEmployee, updateEmployee } from "../api/employeeApi";
import type { Employee } from "../types/employee";
import type { Notyf } from "notyf";

interface FormModel {
  id?: string | null;
  firstName: string;
  lastName: string;
  email: string;
  dateOfHire?: string;
  employeeType: "FullTime" | "Contractor";
  annualSalary?: number | null;
  hourlyRate?: number | null;
  hoursPerMonth?: number | null;
}

const props = defineProps<{ initial?: Employee | null }>();
const emit = defineEmits<{
  (e: "saved"): void;
}>();

const initial = computed(() => props.initial ?? null);

const form = reactive<FormModel>({
  id: null,
  firstName: "",
  lastName: "",
  email: "",
  dateOfHire: new Date().toISOString().slice(0, 10),
  employeeType: "FullTime",
  annualSalary: undefined,
  hourlyRate: undefined,
  hoursPerMonth: undefined,
});

watch(
  initial,
  (v) => {
    if (!v) {
      Object.assign(form, {
        id: null,
        firstName: "",
        lastName: "",
        email: "",
        dateOfHire: new Date().toISOString().slice(0, 10),
        employeeType: "FullTime",
        annualSalary: undefined,
        hourlyRate: undefined,
        hoursPerMonth: undefined,
      });
      return;
    }
    Object.assign(form, {
      id: v.id,
      firstName: v.firstName,
      lastName: v.lastName,
      email: v.email,
      dateOfHire:
        v.dateOfHire?.slice(0, 10) ?? new Date().toISOString().slice(0, 10),
      employeeType: v.employeeType,
      annualSalary: v.annualSalary ?? undefined,
      hourlyRate: v.hourlyRate ?? undefined,
      hoursPerMonth: v.hoursPerMonth ?? undefined,
    });
  },
  { immediate: true }
);

// notyf injection (may be undefined if not provided)
const notyf = inject("notyf") as Notyf | undefined;

// saving state to disable the form during network calls
const saving = ref(false);

async function submit() {
  const payload = {
    firstName: form.firstName,
    lastName: form.lastName,
    email: form.email,
    dateOfHire: form.dateOfHire,
    employeeType: form.employeeType,
    annualSalary:
      form.employeeType === "FullTime" ? form.annualSalary ?? 0 : null,
    hourlyRate:
      form.employeeType === "Contractor" ? form.hourlyRate ?? 0 : null,
    hoursPerMonth:
      form.employeeType === "Contractor" ? form.hoursPerMonth ?? 0 : null,
  };

  saving.value = true;
  try {
    if (form.id) {
      // edit existing
      await updateEmployee(form.id, payload);
      notyf?.success("Employee updated");
    } else {
      // create new
      await createEmployee(payload);
      notyf?.success("Employee created");

      // reset form after successful create
      Object.assign(form, {
        id: null,
        firstName: "",
        lastName: "",
        email: "",
        dateOfHire: new Date().toISOString().slice(0, 10),
        employeeType: "FullTime",
        annualSalary: undefined,
        hourlyRate: undefined,
        hoursPerMonth: undefined,
      });
    }

    emit("saved");
  } catch (err: any) {
    console.error("Save failed", err);
    notyf?.error("Save failed: " + (err?.message ?? "Unknown error"));
    alert("Save failed: " + (err?.message ?? err));
  } finally {
    saving.value = false;
  }
}

function cancel() {
  Object.assign(form, {
    id: null,
    firstName: "",
    lastName: "",
    email: "",
    dateOfHire: new Date().toISOString().slice(0, 10),
    employeeType: "FullTime",
    annualSalary: undefined,
    hourlyRate: undefined,
    hoursPerMonth: undefined,
  });
  emit("saved");
}
</script>

<style scoped>
/* keep styles minimal — global styles applied in src/style.css */
</style>
