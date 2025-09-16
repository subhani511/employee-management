using System;

namespace EmployeeManagement.Core.Domain
{
    /// <summary>
    /// Full-time employee with an annual salary.
    /// </summary>
    public class FullTimeEmployee : Employee
    {
        public decimal AnnualSalary { get; private set; }

        // Normal constructor (new employee)
        public FullTimeEmployee(string firstName, string lastName, string email, DateTime dateOfHire, decimal annualSalary)
            : base(firstName, lastName, email, dateOfHire)
        {
            SetAnnualSalary(annualSalary);
        }

        // Reconstruction constructor (preserve Id)
        public FullTimeEmployee(Guid id, string firstName, string lastName, string email, DateTime dateOfHire, decimal annualSalary)
            : base(id, firstName, lastName, email, dateOfHire)
        {
            SetAnnualSalary(annualSalary);
        }

        public void SetAnnualSalary(decimal salary)
        {
            if (salary < 0) throw new ArgumentException("Annual salary must be non-negative.", nameof(salary));
            AnnualSalary = decimal.Round(salary, 2);
        }

        public override decimal GetMonthlyPay() => decimal.Round(AnnualSalary / 12m, 2);
    }
}
