import React, { useEffect, useState } from "react";
import { useNavigate, useParams, Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { VerticalForm, FormInput } from "../../../../../components";
import { timeTrackingService, type CreateTimeEntryDto, type UpdateTimeEntryDto, type TimeEntryDto } from "../services";
import { employeeService } from "../../employee/services";
import { TIME_ENTRY_STATUS_OPTIONS } from "../types";
import * as yup from "yup";
import { yupResolver } from "@hookform/resolvers/yup";

const TimeEntryForm: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const isEdit = !!id;

  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [timeEntry, setTimeEntry] = useState<TimeEntryDto | null>(null);
  const [employees, setEmployees] = useState<any[]>([]);

  useEffect(() => {
    loadEmployees();
    if (isEdit && id) {
      loadTimeEntry(id);
    }
  }, [isEdit, id]);

  const loadTimeEntry = async (timeEntryId: string) => {
    try {
      const timeEntryData = await timeTrackingService.getById(timeEntryId);
      setTimeEntry(timeEntryData);
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load time entry");
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

  const calculateHours = (startTime: string, endTime: string): number => {
    if (!startTime || !endTime) return 0;
    const start = new Date(startTime);
    const end = new Date(endTime);
    const diffTime = Math.abs(end.getTime() - start.getTime());
    const diffHours = diffTime / (1000 * 60 * 60);
    return parseFloat(diffHours.toFixed(2));
  };

  const timeEntrySchema = yup.object({
    employeeId: yup.string().uuid("Invalid employee").required("Employee is required"),
    workDate: yup.string().required("Work date is required"),
    startTime: yup.string(),
    endTime: yup.string(),
    hoursWorked: yup.number().min(0, "Hours worked must be >= 0"),
    description: yup.string(),
    status: yup.string(),
    isBillable: yup.boolean(),
    hourlyRate: yup.number().min(0, "Hourly rate must be >= 0"),
    notes: yup.string(),
  });

  const onSubmit = async (data: any) => {
    setError(null);
    
    // Calculate hours if start and end times are provided
    let hoursWorked = data.hoursWorked;
    if (data.startTime && data.endTime) {
      hoursWorked = calculateHours(data.startTime, data.endTime);
      if (new Date(data.endTime) < new Date(data.startTime)) {
        setError("End time must be after start time");
        return;
      }
    }

    setLoading(true);
    try {
      if (isEdit && id) {
        const updateData: UpdateTimeEntryDto = {
          id,
          employeeId: data.employeeId,
          projectId: data.projectId || undefined,
          taskId: data.taskId || undefined,
          startTime: data.startTime || undefined,
          endTime: data.endTime || undefined,
          workDate: data.workDate,
          hoursWorked: hoursWorked || 0,
          description: data.description || undefined,
          status: data.status || "Draft",
          isBillable: data.isBillable || false,
          hourlyRate: data.hourlyRate ? parseFloat(data.hourlyRate) : undefined,
          notes: data.notes || undefined,
        };
        await timeTrackingService.update(id, updateData);
      } else {
        const createData: CreateTimeEntryDto = {
          employeeId: data.employeeId,
          projectId: data.projectId || undefined,
          taskId: data.taskId || undefined,
          startTime: data.startTime || undefined,
          endTime: data.endTime || undefined,
          workDate: data.workDate,
          hoursWorked: hoursWorked || 0,
          description: data.description || undefined,
          status: data.status || "Draft",
          isBillable: data.isBillable || false,
          hourlyRate: data.hourlyRate ? parseFloat(data.hourlyRate) : undefined,
          notes: data.notes || undefined,
        };
        await timeTrackingService.create(createData);
      }
      navigate("/hrm/time-entries");
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to save time entry");
    } finally {
      setLoading(false);
    }
  };

  const formatDateTimeForInput = (dateString?: string): string => {
    if (!dateString) return "";
    const date = new Date(dateString);
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, "0");
    const day = String(date.getDate()).padStart(2, "0");
    const hours = String(date.getHours()).padStart(2, "0");
    const minutes = String(date.getMinutes()).padStart(2, "0");
    return `${year}-${month}-${day}T${hours}:${minutes}`;
  };

  const defaultValues = isEdit && timeEntry
    ? {
        employeeId: timeEntry.employeeId || "",
        projectId: timeEntry.projectId || "",
        taskId: timeEntry.taskId || "",
        workDate: timeEntry.workDate ? timeEntry.workDate.split('T')[0] : new Date().toISOString().split('T')[0],
        startTime: formatDateTimeForInput(timeEntry.startTime),
        endTime: formatDateTimeForInput(timeEntry.endTime),
        hoursWorked: timeEntry.hoursWorked || 0,
        description: timeEntry.description || "",
        status: timeEntry.status || "Draft",
        isBillable: timeEntry.isBillable || false,
        hourlyRate: timeEntry.hourlyRate || 0,
        notes: timeEntry.notes || "",
      }
    : {
        employeeId: "",
        projectId: "",
        taskId: "",
        workDate: new Date().toISOString().split('T')[0],
        startTime: "",
        endTime: "",
        hoursWorked: 0,
        description: "",
        status: "Draft",
        isBillable: false,
        hourlyRate: 0,
        notes: "",
      };

  return (
    <>
      <PageBreadcrumb title={isEdit ? "Edit Time Entry" : "Add Time Entry"} subName="HRM" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center gap-2">
            <Link to="/hrm/time-entries" className="btn btn-sm btn-soft-secondary">
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
            resolver={yupResolver(timeEntrySchema)}
            defaultValues={defaultValues}
          >
            <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
              <FormInput
                label="Employee"
                type="select"
                name="employeeId"
                containerClass="mb-4"
                className="form-select"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                required
              >
                <option value="">Select employee</option>
                {employees.map((employee) => (
                  <option key={employee.id} value={employee.id}>
                    {employee.employeeCode} - {employee.firstName} {employee.lastName}
                  </option>
                ))}
              </FormInput>

              <FormInput
                label="Work Date"
                type="date"
                name="workDate"
                containerClass="mb-4"
                className="form-input"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                required
              />

              <FormInput
                label="Start Time"
                type="datetime-local"
                name="startTime"
                containerClass="mb-4"
                className="form-input"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
              />

              <FormInput
                label="End Time"
                type="datetime-local"
                name="endTime"
                containerClass="mb-4"
                className="form-input"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
              />

              <FormInput
                label="Hours Worked"
                type="number"
                name="hoursWorked"
                placeholder="0.00"
                containerClass="mb-4"
                className="form-input"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                step="0.25"
                min="0"
              />

              <FormInput
                label="Status"
                type="select"
                name="status"
                containerClass="mb-4"
                className="form-select"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
              >
                {TIME_ENTRY_STATUS_OPTIONS.map((option) => (
                  <option key={option.value} value={option.value}>
                    {option.label}
                  </option>
                ))}
              </FormInput>

              <FormInput
                label="Hourly Rate"
                type="number"
                name="hourlyRate"
                placeholder="0.00"
                containerClass="mb-4"
                className="form-input"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                step="0.01"
                min="0"
              />

              <div className="mb-4">
                <FormInput
                  label="Billable"
                  type="checkbox"
                  name="isBillable"
                  containerClass="flex items-center"
                  labelClassName="ms-2"
                  className="form-checkbox rounded"
                />
              </div>

              {isEdit && timeEntry && timeEntry.totalAmount && (
                <div className="md:col-span-2">
                  <div className="bg-gray-50 dark:bg-gray-800 p-4 rounded">
                    <div className="text-sm text-gray-600 dark:text-gray-400">Total Amount</div>
                    <div className="text-lg font-semibold text-green-600">
                      {(timeEntry.totalAmount || 0).toFixed(2)}
                    </div>
                  </div>
                </div>
              )}

              <div className="md:col-span-2">
                <FormInput
                  label="Description"
                  type="textarea"
                  name="description"
                  rows={3}
                  placeholder="Work description"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                />
              </div>

              <div className="md:col-span-2">
                <FormInput
                  label="Notes"
                  type="textarea"
                  name="notes"
                  rows={3}
                  placeholder="Additional notes"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                />
              </div>
            </div>

            <div className="flex justify-end gap-3 mt-6">
              <Link to="/hrm/time-entries" className="btn btn-soft-secondary">
                Cancel
              </Link>
              <button type="submit" className="btn btn-primary" disabled={loading}>
                {loading ? "Saving…" : isEdit ? "Save changes" : "Add Time Entry"}
              </button>
            </div>
          </VerticalForm>
        </div>
      </div>
    </>
  );
};

export default TimeEntryForm;

