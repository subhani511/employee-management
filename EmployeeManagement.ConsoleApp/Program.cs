// EmployeeManagement.ConsoleApp/Program.cs
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using EmployeeManagement.Core.DTOs;
using EmployeeManagement.Core.Domain;
using EmployeeManagement.Core.Interfaces;
using EmployeeManagement.Infrastructure.Repositories;
using EmployeeManagement.Application.Services;

var builder = WebApplication.CreateBuilder(args);

// Allow Vite dev server (change origin(s) as needed)
const string DevCorsPolicy = "DevCors";
builder.Services.AddCors(options =>
{
    options.AddPolicy(DevCorsPolicy, policy =>
    {
        policy.WithOrigins("http://localhost:5173") // Vite dev origin
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Register services
// Use a JSON file relative to the working folder for persistence
var jsonPath = "employees_demo.json";
builder.Services.AddSingleton<IEmployeeRepository>(_ => new JsonEmployeeRepository(jsonPath));
builder.Services.AddSingleton<EmployeeService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseCors(DevCorsPolicy);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.MapGet("/api/employees", async (EmployeeService svc) =>
{
    var list = await svc.ListAsync();
    return Results.Ok(list.Select(MapToDto));
});

app.MapGet("/api/employees/search", async (string? name, EmployeeService svc) =>
{
    if (string.IsNullOrWhiteSpace(name)) return Results.BadRequest("Query parameter 'name' is required.");
    var found = await svc.SearchByNameAsync(name);
    return Results.Ok(found.Select(MapToDto));
});

app.MapGet("/api/employees/{id:guid}", async (Guid id, EmployeeService svc) =>
{
    var e = await svc.GetByIdAsync(id);
    return e is null ? Results.NotFound() : Results.Ok(MapToDto(e));
});

app.MapPost("/api/employees", async (EmployeeDto dto, EmployeeService svc) =>
{
    try
    {
        var created = await svc.CreateAsync(dto);
        var createdDto = MapToDto(created);
        return Results.Created($"/api/employees/{created.Id}", createdDto);
    }
    catch (ArgumentException aex)
    {
        return Results.BadRequest(aex.Message);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

app.MapPut("/api/employees/{id:guid}", async (Guid id, EmployeeDto dto, EmployeeService svc) =>
{
    try
    {
        // Ensure types match (changing concrete type is not supported in this simple API)
        var existing = await svc.GetByIdAsync(id);
        if (existing is null) return Results.NotFound();

        // If client tried to change EmployeeType, reject to keep domain invariants simple
        var existingType = existing is FullTimeEmployee ? "FullTime" : "Contractor";
        if (!string.Equals(existingType, dto.EmployeeType, StringComparison.OrdinalIgnoreCase))
            return Results.BadRequest("Changing employee type is not supported. Delete and recreate with new type.");

        await svc.UpdateAsync(id, emp =>
        {
            emp.SetName(dto.FirstName, dto.LastName);
            emp.SetEmail(dto.Email);
            emp.SetDateOfHire(dto.DateOfHire);

            if (emp is FullTimeEmployee ft && dto.AnnualSalary.HasValue)
            {
                ft.SetAnnualSalary(dto.AnnualSalary.Value);
            }
            else if (emp is Contractor c && dto.HourlyRate.HasValue && dto.HoursPerMonth.HasValue)
            {
                c.SetRateAndHours(dto.HourlyRate.Value, dto.HoursPerMonth.Value);
            }
        });

        var updated = await svc.GetByIdAsync(id);
        return Results.Ok(MapToDto(updated!));
    }
    catch (ArgumentException aex)
    {
        return Results.BadRequest(aex.Message);
    }
    catch (InvalidOperationException)
    {
        return Results.NotFound();
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

app.MapDelete("/api/employees/{id:guid}", async (Guid id, EmployeeService svc) =>
{
    await svc.DeleteAsync(id);
    return Results.NoContent();
});

app.Run();

// Helper mapping: Domain -> DTO
static EmployeeDto MapToDto(Employee e)
{
    if (e is FullTimeEmployee ft)
    {
        return new EmployeeDto(
            ft.Id,
            ft.FirstName,
            ft.LastName,
            ft.Email,
            ft.DateOfHire,
            "FullTime",
            ft.AnnualSalary,
            null,
            null
        );
    }
    else if (e is Contractor c)
    {
        return new EmployeeDto(
            c.Id,
            c.FirstName,
            c.LastName,
            c.Email,
            c.DateOfHire,
            "Contractor",
            null,
            c.HourlyRate,
            c.HoursPerMonth
        );
    }
    else
    {
        // fallback (shouldn't happen)
        return new EmployeeDto(e.Id, e.FirstName, e.LastName, e.Email, e.DateOfHire, "FullTime", 0m, null, null);
    }
}
