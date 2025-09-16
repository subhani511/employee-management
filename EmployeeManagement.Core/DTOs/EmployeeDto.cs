using System;

namespace EmployeeManagement.Core.DTOs
{
    /// <summary>
    /// Data Transfer Object used at the application/API boundary.
    /// Keeps domain model separate from transport/persistence concerns.
    /// </summary>
    public record EmployeeDto(
        Guid? Id,
        string FirstName,
        string LastName,
        string Email,
        DateTime DateOfHire,
        string EmployeeType,    // "FullTime" | "Contractor"
        decimal? AnnualSalary,  // for FullTime
        decimal? HourlyRate,    // for Contractor
        decimal? HoursPerMonth  // for Contractor
    );
}
