import React, { useEffect, useState } from "react";
import { useNavigate, useParams, Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { VerticalForm, FormInput } from "../../../../../components";
import { employeeService, type CreateEmployeeDto, type UpdateEmployeeDto, type EmployeeDto } from "../services";
import { departmentService, type DepartmentDto } from "../../department/services";
import { 
  SALUTATION_OPTIONS, 
  GENDER_OPTIONS, 
  MARITAL_STATUS_OPTIONS, 
  EMPLOYMENT_TYPE_OPTIONS, 
  EMPLOYEE_STATUS_OPTIONS 
} from "../types";
import * as yup from "yup";
import { yupResolver } from "@hookform/resolvers/yup";

const EmployeeForm: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const isEdit = !!id;

  const employeeSchema = yup.object({
    employeeCode: yup.string().required("Employee code is required"),
    salutation: yup.string(),
    firstName: yup.string().required("First name is required"),
    lastName: yup.string().required("Last name is required"),
    preferredName: yup.string(),
    gender: yup.string(),
    dateOfBirth: yup.date().nullable().max(new Date(), "Date of birth cannot be in the future"),
    nationality: yup.string(),
    maritalStatus: yup.string(),
    jobTitle: yup.string(),
    departmentId: yup.string().uuid("Invalid department").nullable(),
    managerId: yup.string().uuid("Invalid manager").nullable(),
    employmentType: yup.string().oneOf(["FullTime", "PartTime", "Contract"], "Invalid employment type").required("Employment type is required"),
    workEmail: yup.string().email("Invalid email"),
    personalEmail: yup.string().email("Invalid email"),
    workPhone: yup.string(),
    mobilePhone: yup.string(),
    workLocationId: yup.string().uuid("Invalid location").nullable(),
    dateHired: yup.date().nullable(),
    dateTerminated: yup.date().nullable(),
    status: yup.string().oneOf(["Active", "Inactive", "Terminated"], "Invalid status").required("Status is required"),
    isRemote: yup.boolean(),
  });

  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [departments, setDepartments] = useState<DepartmentDto[]>([]);
  const [employees, setEmployees] = useState<EmployeeDto[]>([]);
  const [employee, setEmployee] = useState<EmployeeDto | null>(null);
  const [codeUnique, setCodeUnique] = useState<boolean>(true);

  useEffect(() => {
    loadDepartments();
    loadEmployees();
    if (isEdit && id) {
      loadEmployee(id);
    }
  }, [isEdit, id]);

  const loadEmployee = async (employeeId: string) => {
    try {
      const employeeData = await employeeService.getById(employeeId);
      setEmployee(employeeData);
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load employee");
    }
  };

  const loadDepartments = async () => {
    try {
      const data = await departmentService.getAll();
      setDepartments(data);
    } catch (err: any) {
      console.error("Failed to load departments", err);
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

  // Helper to format date for input (YYYY-MM-DD)
  const formatDateForInput = (dateString?: string): string => {
    if (!dateString) return "";
    const date = new Date(dateString);
    if (isNaN(date.getTime())) return "";
    return date.toISOString().split("T")[0];
  };

  // Helper to format date for API (ISO string)
  const formatDateForAPI = (dateString?: string): string | undefined => {
    if (!dateString) return undefined;
    const date = new Date(dateString);
    if (isNaN(date.getTime())) return undefined;
    return date.toISOString();
  };

  const onSubmit = async (data: any) => {
    setError(null);
    setLoading(true);
    try {
      // Validate employee code uniqueness
      if (!isEdit || data.employeeCode !== employee?.employeeCode) {
        const unique = await employeeService.isCodeUnique(data.employeeCode, id);
        if (!unique) {
          setError("Employee code already exists. Please use a different code.");
          setLoading(false);
          return;
        }
      }

      if (isEdit && id) {
        const updateData: UpdateEmployeeDto = {
          id,
          salutation: data.salutation || undefined,
          preferredName: data.preferredName || undefined,
          gender: data.gender || undefined,
          dateOfBirth: formatDateForAPI(data.dateOfBirth),
          nationality: data.nationality || undefined,
          maritalStatus: data.maritalStatus || undefined,
          jobTitle: data.jobTitle || undefined,
          departmentId: data.departmentId || undefined,
          managerId: data.managerId || undefined,
          employmentType: data.employmentType,
          workEmail: data.workEmail || undefined,
          personalEmail: data.personalEmail || undefined,
          workPhone: data.workPhone || undefined,
          mobilePhone: data.mobilePhone || undefined,
          workLocationId: data.workLocationId || undefined,
          dateHired: formatDateForAPI(data.dateHired),
          dateTerminated: formatDateForAPI(data.dateTerminated),
          status: data.status,
          isRemote: data.isRemote ?? false,
        };
        await employeeService.update(updateData);
      } else {
        const createData: CreateEmployeeDto = {
          employeeCode: data.employeeCode,
          salutation: data.salutation || undefined,
          firstName: data.firstName,
          lastName: data.lastName,
          preferredName: data.preferredName || undefined,
          gender: data.gender || undefined,
          dateOfBirth: formatDateForAPI(data.dateOfBirth),
          nationality: data.nationality || undefined,
          maritalStatus: data.maritalStatus || undefined,
          jobTitle: data.jobTitle || undefined,
          departmentId: data.departmentId || undefined,
          managerId: data.managerId || undefined,
          employmentType: data.employmentType,
          workEmail: data.workEmail || undefined,
          personalEmail: data.personalEmail || undefined,
          workPhone: data.workPhone || undefined,
          mobilePhone: data.mobilePhone || undefined,
          workLocationId: data.workLocationId || undefined,
          dateHired: formatDateForAPI(data.dateHired),
          status: data.status,
          isRemote: data.isRemote ?? false,
        };
        await employeeService.create(createData);
      }
      navigate("/hrm/employees");
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || `Failed to ${isEdit ? "Save changes" : "Add Employee"} employee`);
    } finally {
      setLoading(false);
    }
  };

  return (
    <>
      <PageBreadcrumb title={isEdit ? "Edit Employee" : "Add Employee"} subName="HRM" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center gap-2">
            <Link to="/hrm/employees" className="btn btn-sm btn-soft-secondary">
              <i className="mgc_arrow_left_line me-2"></i>Back
            </Link>
            <h4 className="card-title">{isEdit ? "Edit Employee" : "Add Employee"}</h4>
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
            resolver={yupResolver(employeeSchema)}
            defaultValues={employee ? {
              employeeCode: employee.employeeCode,
              salutation: employee.salutation || "",
              firstName: employee.firstName,
              lastName: employee.lastName,
              preferredName: employee.preferredName || "",
              gender: employee.gender || "",
              dateOfBirth: formatDateForInput(employee.dateOfBirth),
              nationality: employee.nationality || "",
              maritalStatus: employee.maritalStatus || "",
              jobTitle: employee.jobTitle || "",
              departmentId: employee.departmentId || "",
              managerId: employee.managerId || "",
              employmentType: employee.employmentType,
              workEmail: employee.workEmail || "",
              personalEmail: employee.personalEmail || "",
              workPhone: employee.workPhone || "",
              mobilePhone: employee.mobilePhone || "",
              workLocationId: employee.workLocationId || "",
              dateHired: formatDateForInput(employee.dateHired),
              dateTerminated: formatDateForInput(employee.dateTerminated),
              status: employee.status,
              isRemote: employee.isRemote,
            } : {
              employeeCode: "",
              salutation: "",
              firstName: "",
              lastName: "",
              preferredName: "",
              gender: "",
              dateOfBirth: "",
              nationality: "",
              maritalStatus: "",
              jobTitle: "",
              departmentId: "",
              managerId: "",
              employmentType: "FullTime",
              workEmail: "",
              personalEmail: "",
              workPhone: "",
              mobilePhone: "",
              workLocationId: "",
              dateHired: "",
              dateTerminated: "",
              status: "Active",
              isRemote: false,
            }}
          >
            {/* Basic Information Section */}
            <div className="mb-6">
              <h5 className="text-lg font-semibold mb-4">Basic Information</h5>
              <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
                <FormInput
                  label="Employee Code"
                  type="text"
                  name="employeeCode"
                  placeholder="Enter employee code"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                  required
                />

                <FormInput
                  label="Salutation"
                  type="select"
                  name="salutation"
                  containerClass="mb-4"
                  className="form-select"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                >
                  <option value="">Select salutation</option>
                  {SALUTATION_OPTIONS.map((option) => (
                    <option key={option.value} value={option.value}>
                      {option.label}
                    </option>
                  ))}
                </FormInput>

                <FormInput
                  label="First Name"
                  type="text"
                  name="firstName"
                  placeholder="Enter first name"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                  required
                />

                <FormInput
                  label="Last Name"
                  type="text"
                  name="lastName"
                  placeholder="Enter last name"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                  required
                />

                <FormInput
                  label="Preferred Name"
                  type="text"
                  name="preferredName"
                  placeholder="Enter preferred name"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                />

                <FormInput
                  label="Gender"
                  type="select"
                  name="gender"
                  containerClass="mb-4"
                  className="form-select"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                >
                  <option value="">Select gender</option>
                  {GENDER_OPTIONS.map((option) => (
                    <option key={option.value} value={option.value}>
                      {option.label}
                    </option>
                  ))}
                </FormInput>

                <FormInput
                  label="Date of Birth"
                  type="date"
                  name="dateOfBirth"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                />

                <FormInput
                  label="Nationality"
                  type="text"
                  name="nationality"
                  placeholder="Enter nationality"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                />

                <FormInput
                  label="Marital Status"
                  type="select"
                  name="maritalStatus"
                  containerClass="mb-4"
                  className="form-select"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                >
                  <option value="">Select marital status</option>
                  {MARITAL_STATUS_OPTIONS.map((option) => (
                    <option key={option.value} value={option.value}>
                      {option.label}
                    </option>
                  ))}
                </FormInput>
              </div>
            </div>

            {/* Employment Information Section */}
            <div className="mb-6">
              <h5 className="text-lg font-semibold mb-4">Employment Information</h5>
              <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
                <FormInput
                  label="Job Title"
                  type="text"
                  name="jobTitle"
                  placeholder="Enter job title"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                />

                <FormInput
                  label="Department"
                  type="select"
                  name="departmentId"
                  containerClass="mb-4"
                  className="form-select"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                >
                  <option value="">Select department</option>
                  {departments.map((dept) => (
                    <option key={dept.id} value={dept.id}>
                      {dept.name}
                    </option>
                  ))}
                </FormInput>

                <FormInput
                  label="Manager"
                  type="select"
                  name="managerId"
                  containerClass="mb-4"
                  className="form-select"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                >
                  <option value="">Select manager</option>
                  {employees
                    .filter((emp) => !id || emp.id !== id) // Exclude current employee
                    .map((emp) => (
                      <option key={emp.id} value={emp.id}>
                        {emp.firstName} {emp.lastName} ({emp.employeeCode})
                      </option>
                    ))}
                </FormInput>

                <FormInput
                  label="Employment Type"
                  type="select"
                  name="employmentType"
                  containerClass="mb-4"
                  className="form-select"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                  required
                >
                  {EMPLOYMENT_TYPE_OPTIONS.map((option) => (
                    <option key={option.value} value={option.value}>
                      {option.label}
                    </option>
                  ))}
                </FormInput>

                <FormInput
                  label="Date Hired"
                  type="date"
                  name="dateHired"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                />

                {isEdit && (
                  <FormInput
                    label="Date Terminated"
                    type="date"
                    name="dateTerminated"
                    containerClass="mb-4"
                    className="form-input"
                    labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                  />
                )}

                <FormInput
                  label="Status"
                  type="select"
                  name="status"
                  containerClass="mb-4"
                  className="form-select"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                  required
                >
                  {EMPLOYEE_STATUS_OPTIONS.map((option) => (
                    <option key={option.value} value={option.value}>
                      {option.label}
                    </option>
                  ))}
                </FormInput>

                <div className="mb-4">
                  <FormInput
                    label="Is Remote"
                    type="checkbox"
                    name="isRemote"
                    containerClass="flex items-center"
                    labelClassName="ms-2"
                    className="form-checkbox rounded"
                  />
                </div>
              </div>
            </div>

            {/* Contact Information Section */}
            <div className="mb-6">
              <h5 className="text-lg font-semibold mb-4">Contact Information</h5>
              <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
                <FormInput
                  label="Work Email"
                  type="email"
                  name="workEmail"
                  placeholder="Enter work email"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                />

                <FormInput
                  label="Personal Email"
                  type="email"
                  name="personalEmail"
                  placeholder="Enter personal email"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                />

                <FormInput
                  label="Work Phone"
                  type="text"
                  name="workPhone"
                  placeholder="Enter work phone"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                />

                <FormInput
                  label="Mobile Phone"
                  type="text"
                  name="mobilePhone"
                  placeholder="Enter mobile phone"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                />
              </div>
            </div>

            <div className="flex justify-end gap-2">
              <Link to="/hrm/employees" className="btn btn-soft-secondary">
                Cancel
              </Link>
              <button type="submit" className="btn btn-primary" disabled={loading}>
                {loading ? "Saving…" : isEdit ? "Save changes" : "Add Employee"}
              </button>
            </div>
          </VerticalForm>
        </div>
      </div>
    </>
  );
};

export default EmployeeForm;

