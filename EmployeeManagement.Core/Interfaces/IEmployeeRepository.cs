using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EmployeeManagement.Core.Domain;

namespace EmployeeManagement.Core.Interfaces
{
    /// <summary>
    /// Repository abstraction for Employee persistence.
    /// Implementations can be JSON file, EF Core, in-memory, etc.
    /// Keeps the application code (services) decoupled from persistence details.
    /// </summary>
    public interface IEmployeeRepository
    {
        /// <summary>Get employee by id or null if not found</summary>
        Task<Employee?> GetByIdAsync(Guid id);

        /// <summary>List all employees</summary>
        Task<IEnumerable<Employee>> ListAsync();

        /// <summary>Add a new employee</summary>
        Task AddAsync(Employee employee);

        /// <summary>Update existing employee (throws if not found)</summary>
        Task UpdateAsync(Employee employee);

        /// <summary>Delete by id (no-op if not found)</summary>
        Task DeleteAsync(Guid id);
    }
}
