using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmployeeManagement.Core.Domain;
using EmployeeManagement.Core.DTOs;
using EmployeeManagement.Core.Interfaces;

namespace EmployeeManagement.Application.Services
{
    public class EmployeeService
    {
        private readonly IEmployeeRepository _repo;

        public EmployeeService(IEmployeeRepository repo)
        {
            _repo = repo;
        }

        public async Task<Employee> CreateAsync(EmployeeDto dto)
        {
            if (dto.EmployeeType == "FullTime")
            {
                var salary = dto.AnnualSalary ?? throw new ArgumentException("AnnualSalary required for FullTime");
                var emp = new FullTimeEmployee(dto.FirstName, dto.LastName, dto.Email, dto.DateOfHire, salary);
                await _repo.AddAsync(emp);
                return emp;
            }
            else if (dto.EmployeeType == "Contractor")
            {
                var rate = dto.HourlyRate ?? throw new ArgumentException("HourlyRate required for Contractor");
                var hours = dto.HoursPerMonth ?? throw new ArgumentException("HoursPerMonth required for Contractor");
                var emp = new Contractor(dto.FirstName, dto.LastName, dto.Email, dto.DateOfHire, rate, hours);
                await _repo.AddAsync(emp);
                return emp;
            }
            else
            {
                throw new ArgumentException("Unknown EmployeeType");
            }
        }

        public Task<IEnumerable<Employee>> ListAsync() => _repo.ListAsync();

        public Task<Employee?> GetByIdAsync(Guid id) => _repo.GetByIdAsync(id);

        public async Task UpdateAsync(Guid id, Action<Employee> updateAction)
        {
            var e = await _repo.GetByIdAsync(id) ?? throw new InvalidOperationException("Not found");
            updateAction(e);
            await _repo.UpdateAsync(e);
        }

        public Task DeleteAsync(Guid id) => _repo.DeleteAsync(id);

        public async Task<IEnumerable<Employee>> SearchByNameAsync(string nameFragment)
        {
            var all = (await _repo.ListAsync()).ToList();
            return all.Where(e => (e.FirstName + " " + e.LastName).Contains(nameFragment, StringComparison.OrdinalIgnoreCase));
        }
    }
}
