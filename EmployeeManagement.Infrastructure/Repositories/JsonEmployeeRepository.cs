using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using EmployeeManagement.Core.Domain;
using EmployeeManagement.Core.Interfaces;

namespace EmployeeManagement.Infrastructure.Repositories
{
    public class JsonEmployeeRepository : IEmployeeRepository
    {
        private readonly string _filePath;
        private readonly object _fileLock = new();

        public JsonEmployeeRepository(string filePath = "employees_demo.json")
        {
            _filePath = Path.GetFullPath(filePath);
            EnsureFile();
        }

        private void EnsureFile()
        {
            var dir = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            if (!File.Exists(_filePath))
                File.WriteAllText(_filePath, "[]");
        }

        private List<PersistModel> LoadAll()
        {
            lock (_fileLock)
            {
                var json = File.ReadAllText(_filePath);
                return JsonSerializer.Deserialize<List<PersistModel>>(json) ?? new List<PersistModel>();
            }
        }

        private void SaveAll(List<PersistModel> models)
        {
            var json = JsonSerializer.Serialize(models, new JsonSerializerOptions { WriteIndented = true });
            lock (_fileLock)
                File.WriteAllText(_filePath, json);
        }

        public Task AddAsync(Employee employee)
        {
            var all = LoadAll();
            all.Add(PersistModel.FromEmployee(employee));
            SaveAll(all);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid id)
        {
            var all = LoadAll();
            var changed = all.RemoveAll(m => m.Id == id);
            if (changed > 0) SaveAll(all);
            return Task.CompletedTask;
        }

        public Task<Employee?> GetByIdAsync(Guid id)
        {
            var all = LoadAll();
            var m = all.FirstOrDefault(x => x.Id == id);
            return Task.FromResult(m?.ToEmployee());
        }

        public Task<IEnumerable<Employee>> ListAsync()
        {
            var all = LoadAll();
            var list = all.Select(m => m.ToEmployee()).ToList().AsEnumerable();
            return Task.FromResult(list);
        }

        public Task UpdateAsync(Employee employee)
        {
            var all = LoadAll();
            var idx = all.FindIndex(m => m.Id == employee.Id);
            if (idx < 0) throw new InvalidOperationException("Employee not found");
            all[idx] = PersistModel.FromEmployee(employee);
            SaveAll(all);
            return Task.CompletedTask;
        }

        // Internal persist model
        private class PersistModel
        {
            public Guid Id { get; set; }
            public string Type { get; set; } = "";
            public string FirstName { get; set; } = "";
            public string LastName { get; set; } = "";
            public string Email { get; set; } = "";
            public DateTime DateOfHire { get; set; }
            public decimal? AnnualSalary { get; set; }
            public decimal? HourlyRate { get; set; }
            public decimal? HoursPerMonth { get; set; }

            public Employee ToEmployee()
            {
                return Type switch
                {
                    "FullTime" => new FullTimeEmployee(
                        Id,
                        FirstName,
                        LastName,
                        Email,
                        DateOfHire,
                        AnnualSalary ?? 0m),
                    "Contractor" => new Contractor(
                        Id,
                        FirstName,
                        LastName,
                        Email,
                        DateOfHire,
                        HourlyRate ?? 0m,
                        HoursPerMonth ?? 0m),
                    _ => throw new InvalidOperationException($"Unknown employee type: {Type}")
                };
            }

            public static PersistModel FromEmployee(Employee e)
            {
                if (e is FullTimeEmployee ft)
                {
                    return new PersistModel
                    {
                        Id = ft.Id,
                        Type = "FullTime",
                        FirstName = ft.FirstName,
                        LastName = ft.LastName,
                        Email = ft.Email,
                        DateOfHire = ft.DateOfHire,
                        AnnualSalary = ft.AnnualSalary
                    };
                }

                if (e is Contractor c)
                {
                    return new PersistModel
                    {
                        Id = c.Id,
                        Type = "Contractor",
                        FirstName = c.FirstName,
                        LastName = c.LastName,
                        Email = c.Email,
                        DateOfHire = c.DateOfHire,
                        HourlyRate = c.HourlyRate,
                        HoursPerMonth = c.HoursPerMonth
                    };
                }

                throw new InvalidOperationException("Unsupported employee type");
            }
        }
    }
}
