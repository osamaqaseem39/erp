import React, { useEffect, useState } from "react";
import { useNavigate, useParams, Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { VerticalForm, FormInput } from "../../../../../components";
import { departmentService, type CreateDepartmentDto, type UpdateDepartmentDto, type DepartmentDto } from "../services";
import { employeeService, type EmployeeDto } from "../../employee/services";
import * as yup from "yup";
import { yupResolver } from "@hookform/resolvers/yup";

const DepartmentForm: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const isEdit = !!id;

  const departmentSchema = yup.object({
    name: yup.string().required("Department name is required"),
    description: yup.string(),
    managerId: yup.string().uuid("Invalid manager").nullable(),
    parentDepartmentId: yup.string().uuid("Invalid parent department").nullable(),
    isActive: yup.boolean(),
  });

  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [employees, setEmployees] = useState<EmployeeDto[]>([]);
  const [departments, setDepartments] = useState<DepartmentDto[]>([]);
  const [department, setDepartment] = useState<DepartmentDto | null>(null);

  useEffect(() => {
    loadEmployees();
    loadDepartments();
    if (isEdit && id) {
      loadDepartment(id);
    }
  }, [isEdit, id]);

  const loadDepartment = async (departmentId: string) => {
    try {
      const departmentData = await departmentService.getById(departmentId);
      setDepartment(departmentData);
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load department");
    }
  };

  const loadEmployees = async () => {
    try {
      const data = await employeeService.getAll();
      setEmployees(data);
    } catch (err: any) {
      console.error("Failed to load employees", err);
    }
  };

  const loadDepartments = async () => {
    try {
      const data = await departmentService.getAll();
      // Exclude current department from parent options
      setDepartments(data.filter((dept) => !id || dept.id !== id));
    } catch (err: any) {
      console.error("Failed to load departments", err);
    }
  };

  const onSubmit = async (data: any) => {
    setError(null);
    setLoading(true);
    try {
      if (isEdit && id) {
        const updateData: UpdateDepartmentDto = {
          id,
          name: data.name,
          description: data.description || undefined,
          managerId: data.managerId || undefined,
          parentDepartmentId: data.parentDepartmentId || undefined,
          isActive: data.isActive ?? true,
        };
        await departmentService.update(id, updateData);
      } else {
        const createData: CreateDepartmentDto = {
          name: data.name,
          description: data.description || undefined,
          managerId: data.managerId || undefined,
          parentDepartmentId: data.parentDepartmentId || undefined,
          isActive: data.isActive ?? true,
        };
        await departmentService.create(createData);
      }
      navigate("/hrm/departments");
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || `Failed to ${isEdit ? "Save changes" : "Add Department"} department`);
    } finally {
      setLoading(false);
    }
  };

  return (
    <>
      <PageBreadcrumb title={isEdit ? "Edit Department" : "Add Department"} subName="HRM" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center gap-2">
            <Link to="/hrm/departments" className="btn btn-sm btn-soft-secondary">
              <i className="mgc_arrow_left_line me-2"></i>Back
            </Link>
            <h4 className="card-title">{isEdit ? "Edit Department" : "Add Department"}</h4>
          </div>
        </div>
        <div className="card-body">
          {error && (
            <div className="alert alert-danger" role="alert">
              {error}
            </div>
          )}

          <VerticalForm
            onSubmit={onSubmit}
            resolver={yupResolver(departmentSchema)}
            defaultValues={department || {
              name: "",
              description: "",
              managerId: "",
              parentDepartmentId: "",
              isActive: true,
            }}
          >
            <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
              <FormInput
                label="Department Name"
                type="text"
                name="name"
                placeholder="Enter department name"
                containerClass="mb-4"
                className="form-input"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                required
              />

              <FormInput
                label="Manager"
                type="select"
                name="managerId"
                containerClass="mb-4"
                className="form-select"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
              >
                <option value="">Select manager</option>
                {employees.map((emp) => (
                  <option key={emp.id} value={emp.id}>
                    {emp.firstName} {emp.lastName} ({emp.employeeCode})
                  </option>
                ))}
              </FormInput>

              <FormInput
                label="Parent Department"
                type="select"
                name="parentDepartmentId"
                containerClass="mb-4"
                className="form-select"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
              >
                <option value="">Select parent department</option>
                {departments.map((dept) => (
                  <option key={dept.id} value={dept.id}>
                    {dept.name}
                  </option>
                ))}
              </FormInput>

              <div className="mb-4">
                <FormInput
                  label="Active"
                  type="checkbox"
                  name="isActive"
                  containerClass="flex items-center"
                  labelClassName="ms-2"
                  className="form-checkbox rounded"
                />
              </div>

              <div className="md:col-span-2">
                <FormInput
                  label="Description"
                  type="textarea"
                  name="description"
                  rows={4}
                  placeholder="Enter department description"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                />
              </div>
            </div>

            <div className="flex justify-end gap-2">
              <Link to="/hrm/departments" className="btn btn-soft-secondary">
                Cancel
              </Link>
              <button type="submit" className="btn btn-primary" disabled={loading}>
                {loading ? "Saving…" : isEdit ? "Save changes" : "Add Department"}
              </button>
            </div>
          </VerticalForm>
        </div>
      </div>
    </>
  );
};

export default DepartmentForm;

