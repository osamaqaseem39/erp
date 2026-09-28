import React, { useEffect, useState } from "react";
import { useNavigate, useParams, Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { VerticalForm, FormInput } from "../../../../../components";
import { projectService, type CreateProjectDto, type UpdateProjectDto, type ProjectDto } from "../services";
import { customerService } from "../../../../crm/entities/customer/services";
import { userService } from "../../../../core/entities/user/services";
import { PROJECT_STATUS_OPTIONS, PROJECT_PRIORITY_OPTIONS } from "../types";
import * as yup from "yup";
import { yupResolver } from "@hookform/resolvers/yup";

const ProjectForm: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const isEdit = !!id;

  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [project, setProject] = useState<ProjectDto | null>(null);
  const [customers, setCustomers] = useState<any[]>([]);
  const [users, setUsers] = useState<any[]>([]);

  useEffect(() => {
    loadCustomers();
    loadUsers();
    if (isEdit && id) {
      loadProject(id);
    }
  }, [isEdit, id]);

  const loadProject = async (projectId: string) => {
    try {
      const projectData = await projectService.getById(projectId);
      setProject(projectData);
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load project");
    }
  };

  const loadCustomers = async () => {
    try {
      const data = await customerService.getAll();
      setCustomers(data);
    } catch (err: any) {
      console.error("Failed to load customers", err);
    }
  };

  const loadUsers = async () => {
    try {
      const data = await userService.getAll();
      setUsers(data);
    } catch (err: any) {
      console.error("Failed to load users", err);
    }
  };

  const projectSchema = yup.object({
    projectCode: yup.string().required("Project code is required"),
    projectName: yup.string().required("Project name is required"),
    description: yup.string(),
    clientId: yup.string().uuid("Invalid client").nullable(),
    projectManagerId: yup.string().uuid("Invalid project manager").nullable(),
    startDate: yup.string(),
    endDate: yup.string(),
    status: yup.string(),
    priority: yup.string(),
    budget: yup.number().min(0, "Budget must be >= 0"),
    notes: yup.string(),
  });

  const onSubmit = async (data: any) => {
    setError(null);
    setLoading(true);
    try {
      if (isEdit && id) {
        const updateData: UpdateProjectDto = {
          id,
          projectCode: data.projectCode,
          projectName: data.projectName,
          description: data.description || undefined,
          clientId: data.clientId || undefined,
          projectManagerId: data.projectManagerId || undefined,
          startDate: data.startDate || undefined,
          endDate: data.endDate || undefined,
          status: data.status || "Planning",
          priority: data.priority || "Medium",
          budget: data.budget ? parseFloat(data.budget) : undefined,
          notes: data.notes || undefined,
        };
        await projectService.update(id, updateData);
      } else {
        const createData: CreateProjectDto = {
          projectCode: data.projectCode,
          projectName: data.projectName,
          description: data.description || undefined,
          clientId: data.clientId || undefined,
          projectManagerId: data.projectManagerId || undefined,
          startDate: data.startDate || undefined,
          endDate: data.endDate || undefined,
          status: data.status || "Planning",
          priority: data.priority || "Medium",
          budget: data.budget ? parseFloat(data.budget) : undefined,
          notes: data.notes || undefined,
        };
        await projectService.create(createData);
      }
      navigate("/projects");
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to save project");
    } finally {
      setLoading(false);
    }
  };

  const defaultValues = isEdit && project
    ? {
        projectCode: project.projectCode || "",
        projectName: project.projectName || "",
        description: project.description || "",
        clientId: project.clientId || "",
        projectManagerId: project.projectManagerId || "",
        startDate: project.startDate ? project.startDate.split('T')[0] : "",
        endDate: project.endDate ? project.endDate.split('T')[0] : "",
        status: project.status || "Planning",
        priority: project.priority || "Medium",
        budget: project.budget || 0,
        notes: project.notes || "",
      }
    : {
        projectCode: "",
        projectName: "",
        description: "",
        clientId: "",
        projectManagerId: "",
        startDate: "",
        endDate: "",
        status: "Planning",
        priority: "Medium",
        budget: 0,
        notes: "",
      };

  return (
    <>
      <PageBreadcrumb title={isEdit ? "Edit Project" : "Add Project"} subName="Projects" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center gap-2">
            <Link to="/projects" className="btn btn-sm btn-soft-secondary">
              <i className="mgc_arrow_left_line me-2"></i>Back
            </Link>
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
            resolver={yupResolver(projectSchema)}
            defaultValues={defaultValues}
          >
            <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
              <FormInput
                label="Project Code"
                type="text"
                name="projectCode"
                placeholder="Enter project code"
                containerClass="mb-4"
                className="form-input"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                required
              />

              <FormInput
                label="Project Name"
                type="text"
                name="projectName"
                placeholder="Enter project name"
                containerClass="mb-4"
                className="form-input"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                required
              />

              <div className="md:col-span-2">
                <FormInput
                  label="Description"
                  type="textarea"
                  name="description"
                  rows={3}
                  placeholder="Enter project description"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                />
              </div>

              <FormInput
                label="Client"
                type="select"
                name="clientId"
                containerClass="mb-4"
                className="form-select"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
              >
                <option value="">Select client</option>
                {customers.map((customer) => (
                  <option key={customer.id} value={customer.id}>
                    {customer.customerCode} - {customer.customerName}
                  </option>
                ))}
              </FormInput>

              <FormInput
                label="Project Manager"
                type="select"
                name="projectManagerId"
                containerClass="mb-4"
                className="form-select"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
              >
                <option value="">Select project manager</option>
                {users.map((user) => (
                  <option key={user.id} value={user.id}>
                    {user.userName || user.email}
                  </option>
                ))}
              </FormInput>

              <FormInput
                label="Start Date"
                type="date"
                name="startDate"
                containerClass="mb-4"
                className="form-input"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
              />

              <FormInput
                label="End Date"
                type="date"
                name="endDate"
                containerClass="mb-4"
                className="form-input"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
              />

              <FormInput
                label="Status"
                type="select"
                name="status"
                containerClass="mb-4"
                className="form-select"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
              >
                {PROJECT_STATUS_OPTIONS.map((option) => (
                  <option key={option.value} value={option.value}>
                    {option.label}
                  </option>
                ))}
              </FormInput>

              <FormInput
                label="Priority"
                type="select"
                name="priority"
                containerClass="mb-4"
                className="form-select"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
              >
                {PROJECT_PRIORITY_OPTIONS.map((option) => (
                  <option key={option.value} value={option.value}>
                    {option.label}
                  </option>
                ))}
              </FormInput>

              <FormInput
                label="Budget"
                type="number"
                name="budget"
                placeholder="0.00"
                containerClass="mb-4"
                className="form-input"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                step="0.01"
                min="0"
              />

              <div className="md:col-span-2">
                <FormInput
                  label="Notes"
                  type="textarea"
                  name="notes"
                  rows={4}
                  placeholder="Additional notes"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                />
              </div>

              {isEdit && project && (
                <>
                  <FormInput
                    label="Actual Cost"
                    type="number"
                    name="actualCost"
                    value={project.actualCost || 0}
                    containerClass="mb-4"
                    className="form-input"
                    labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                    disabled
                  />

                  <FormInput
                    label="Progress"
                    type="number"
                    name="progress"
                    value={project.progress || 0}
                    containerClass="mb-4"
                    className="form-input"
                    labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                    disabled
                  />

                  <FormInput
                    label="Actual Start Date"
                    type="text"
                    name="actualStartDate"
                    value={project.actualStartDate ? new Date(project.actualStartDate).toLocaleString() : "-"}
                    containerClass="mb-4"
                    className="form-input"
                    labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                    disabled
                  />

                  <FormInput
                    label="Actual End Date"
                    type="text"
                    name="actualEndDate"
                    value={project.actualEndDate ? new Date(project.actualEndDate).toLocaleString() : "-"}
                    containerClass="mb-4"
                    className="form-input"
                    labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                    disabled
                  />
                </>
              )}
            </div>

            <div className="flex justify-end gap-3 mt-6">
              <Link to="/projects" className="btn btn-soft-secondary">
                Cancel
              </Link>
              <button type="submit" className="btn btn-primary" disabled={loading}>
                {loading ? "Saving…" : isEdit ? "Save changes" : "Add Project"}
              </button>
            </div>
          </VerticalForm>
        </div>
      </div>
    </>
  );
};

export default ProjectForm;

