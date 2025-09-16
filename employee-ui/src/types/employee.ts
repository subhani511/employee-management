// src/types/employee.ts
export type EmployeeType = "FullTime" | "Contractor";

export interface Employee {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  dateOfHire: string; // ISO date string
  employeeType: EmployeeType;
  annualSalary?: number | null;
  hourlyRate?: number | null;
  hoursPerMonth?: number | null;
}
