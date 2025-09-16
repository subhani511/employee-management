using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using EmployeeManagement.Application.Services;
using EmployeeManagement.Core.DTOs;
using EmployeeManagement.Core.Domain;
using EmployeeManagement.Core.Interfaces;

namespace EmployeeManagement.Tests
{
    // Tiny in-memory repository used only for tests
    class InMemoryRepo : IEmployeeRepository
    {
        private readonly List<Employee> _list = new();

        public Task AddAsync(Employee employee)
        {
            _list.Add(employee);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid id)
        {
            _list.RemoveAll(e => e.Id == id);
            return Task.CompletedTask;
        }

        public Task<Employee?> GetByIdAsync(Guid id)
        {
            var e = _list.FirstOrDefault(x => x.Id == id);
            return Task.FromResult<Employee?>(e);
        }

        public Task<IEnumerable<Employee>> ListAsync()
        {
            return Task.FromResult<IEnumerable<Employee>>(_list.ToList());
        }

        public Task UpdateAsync(Employee employee)
        {
            var idx = _list.FindIndex(e => e.Id == employee.Id);
            if (idx >= 0) _list[idx] = employee;
            else throw new InvalidOperationException("Not found");
            return Task.CompletedTask;
        }
    }

    public class EmployeeServiceTests
    {
        [Fact]
        public async Task CreateFullTimeEmployee_IsStoredAndMonthlyPayCorrect()
        {
            var repo = new InMemoryRepo();
            var svc = new EmployeeService(repo);

            var dto = new EmployeeDto(null, "Test", "FullTime", "ft@test.com", DateTime.UtcNow, "FullTime", 60000m, null, null);
            var created = await svc.CreateAsync(dto);

            var fetched = await svc.GetByIdAsync(created.Id);
            Assert.NotNull(fetched);
            Assert.IsType<FullTimeEmployee>(fetched);
            var expectedMonthly = decimal.Round(60000m / 12m, 2);
            Assert.Equal(expectedMonthly, fetched!.GetMonthlyPay());
        }

        [Fact]
        public async Task CreateContractor_IsStoredAndMonthlyPayCorrect()
        {
            var repo = new InMemoryRepo();
            var svc = new EmployeeService(repo);

            var dto = new EmployeeDto(null, "Test", "Contractor", "c@test.com", DateTime.UtcNow, "Contractor", null, 50m, 160m);
            var created = await svc.CreateAsync(dto);

            var fetched = await svc.GetByIdAsync(created.Id);
            Assert.NotNull(fetched);
            Assert.IsType<Contractor>(fetched);
            var expectedMonthly = decimal.Round(50m * 160m, 2);
            Assert.Equal(expectedMonthly, fetched!.GetMonthlyPay());
        }

        [Fact]
        public async Task UpdateEmployee_ModifiesStoredEntity()
        {
            var repo = new InMemoryRepo();
            var svc = new EmployeeService(repo);

            var dto = new EmployeeDto(null, "Upd", "User", "u@test.com", DateTime.UtcNow, "FullTime", 48000m, null, null);
            var created = await svc.CreateAsync(dto);

            // Update salary
            await svc.UpdateAsync(created.Id, emp =>
            {
                if (emp is FullTimeEmployee ft) ft.SetAnnualSalary(72000m);
            });

            var fetched = await svc.GetByIdAsync(created.Id);
            Assert.NotNull(fetched);
            var expectedMonthly = decimal.Round(72000m / 12m, 2);
            Assert.Equal(expectedMonthly, fetched!.GetMonthlyPay());
        }

        [Fact]
        public async Task DeleteEmployee_RemovesEntity()
        {
            var repo = new InMemoryRepo();
            var svc = new EmployeeService(repo);

            var dto = new EmployeeDto(null, "To", "Delete", "d@test.com", DateTime.UtcNow, "Contractor", null, 30m, 100m);
            var created = await svc.CreateAsync(dto);

            var before = (await svc.ListAsync()).ToList();
            Assert.Contains(before, e => e.Id == created.Id);

            await svc.DeleteAsync(created.Id);

            var after = (await svc.ListAsync()).ToList();
            Assert.DoesNotContain(after, e => e.Id == created.Id);
        }
    }
}
