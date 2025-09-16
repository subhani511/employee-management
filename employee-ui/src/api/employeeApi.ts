// employee-ui/src/api/employeeApi.ts
import axios from "axios";
import type { Employee } from "../types/employee";

const base = import.meta.env.VITE_API_BASE ?? "http://localhost:5000/api";
const api = axios.create({
  baseURL: base,
  headers: { "Content-Type": "application/json" },
});

export const listEmployees = async (): Promise<Employee[]> => {
  const r = await api.get<Employee[]>("/employees");
  return r.data;
};

export const getEmployee = async (id: string): Promise<Employee> => {
  const r = await api.get<Employee>(`/employees/${id}`);
  return r.data;
};

export const createEmployee = async (
  payload: Partial<Employee>
): Promise<Employee> => {
  const r = await api.post<Employee>("/employees", payload);
  return r.data;
};

export const updateEmployee = async (
  id: string,
  payload: Partial<Employee>
): Promise<void> => {
  await api.put(`/employees/${id}`, payload);
};

export const deleteEmployee = async (id: string): Promise<void> => {
  await api.delete(`/employees/${id}`);
};
