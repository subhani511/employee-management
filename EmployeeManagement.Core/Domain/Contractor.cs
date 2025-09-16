using System;

namespace EmployeeManagement.Core.Domain
{
    /// <summary>
    /// Contractor paid by hourly rate and hours per month.
    /// </summary>
    public class Contractor : Employee
    {
        public decimal HourlyRate { get; private set; }
        public decimal HoursPerMonth { get; private set; }

        // Normal constructor (new employee)
        public Contractor(string firstName, string lastName, string email, DateTime dateOfHire, decimal hourlyRate, decimal hoursPerMonth)
            : base(firstName, lastName, email, dateOfHire)
        {
            SetRateAndHours(hourlyRate, hoursPerMonth);
        }

        // Reconstruction constructor (preserve Id)
        public Contractor(Guid id, string firstName, string lastName, string email, DateTime dateOfHire, decimal hourlyRate, decimal hoursPerMonth)
            : base(id, firstName, lastName, email, dateOfHire)
        {
            SetRateAndHours(hourlyRate, hoursPerMonth);
        }

        public void SetRateAndHours(decimal rate, decimal hours)
        {
            if (rate < 0) throw new ArgumentException("Hourly rate must be non-negative.", nameof(rate));
            if (hours < 0) throw new ArgumentException("Hours per month must be non-negative.", nameof(hours));

            HourlyRate = decimal.Round(rate, 2);
            HoursPerMonth = decimal.Round(hours, 2);
        }

        public override decimal GetMonthlyPay() => decimal.Round(HourlyRate * HoursPerMonth, 2);
    }
}
