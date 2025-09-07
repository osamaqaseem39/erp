using ERP.Shared.DTOs;

namespace ERP.Shared.DTOs.HRM;

public class DepartmentDto : BaseDto
{
    public new Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ManagerId { get; set; }
    public Guid? ParentDepartmentId { get; set; }
    public new bool IsActive { get; set; } = true;
    public new string? CreatedBy { get; set; }
    public new string? UpdatedBy { get; set; }
}

public class CreateDepartmentDto : CreateDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ManagerId { get; set; }
    public Guid? ParentDepartmentId { get; set; }
    public new bool IsActive { get; set; } = true;
}

public class UpdateDepartmentDto : UpdateDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ManagerId { get; set; }
    public Guid? ParentDepartmentId { get; set; }
    public new bool IsActive { get; set; } = true;
}