using Slottet.Domain.Enums;

namespace Slottet.Domain.Entities;

public class Employee
{
    public int Id { get; set; } //primary key
    public string Name  { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role  { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}