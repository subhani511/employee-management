using System;

namespace EmployeeManagement.Core.Domain
{
    /// <summary>
    /// Base domain entity for employees.
    /// Includes protected overload to allow reconstruction with a known Id (persistence).
    /// </summary>
    public abstract class Employee
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string Email { get; private set; }
        public DateTime DateOfHire { get; private set; }

        // Default constructor used when creating a new employee (generates Id)
        protected Employee(string firstName, string lastName, string email, DateTime dateOfHire)
        {
            SetName(firstName, lastName);
            SetEmail(email);
            SetDateOfHire(dateOfHire);
        }

        // Reconstruction constructor used by repositories to preserve Id
        protected Employee(Guid id, string firstName, string lastName, string email, DateTime dateOfHire)
            : this(firstName, lastName, email, dateOfHire)
        {
            Id = id;
        }

        public void SetName(string firstName, string lastName)
        {
            if (string.IsNullOrWhiteSpace(firstName)) throw new ArgumentException("First name is required.", nameof(firstName));
            if (string.IsNullOrWhiteSpace(lastName)) throw new ArgumentException("Last name is required.", nameof(lastName));

            FirstName = firstName.Trim();
            LastName = lastName.Trim();
        }

        public void SetEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@")) throw new ArgumentException("Invalid email.", nameof(email));
            Email = email.Trim();
        }

        public void SetDateOfHire(DateTime doh)
        {
            if (doh > DateTime.UtcNow) throw new ArgumentException("Date of hire cannot be in the future.", nameof(doh));
            DateOfHire = doh;
        }

        public abstract decimal GetMonthlyPay();

        public override string ToString()
        {
            return $"{FirstName} {LastName} (Id: {Id}) - {Email} - Hired: {DateOfHire:yyyy-MM-dd} - MonthlyPay: {GetMonthlyPay():C}";
        }
    }
}
