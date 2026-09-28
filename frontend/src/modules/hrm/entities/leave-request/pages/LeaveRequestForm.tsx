import React, { useEffect, useState } from "react";
import { useNavigate, useParams, Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { VerticalForm, FormInput } from "../../../../../components";
import { leaveRequestService, type CreateLeaveRequestDto, type UpdateLeaveRequestDto, type LeaveRequestDto } from "../services";
import { employeeService } from "../../employee/services";
import { LEAVE_REQUEST_STATUS_OPTIONS } from "../types";
import * as yup from "yup";
import { yupResolver } from "@hookform/resolvers/yup";

const LeaveRequestForm: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const isEdit = !!id;

  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [leaveRequest, setLeaveRequest] = useState<LeaveRequestDto | null>(null);
  const [employees, setEmployees] = useState<any[]>([]);
  const [leaveTypes] = useState([
    { id: "1", name: "Annual Leave" },
    { id: "2", name: "Sick Leave" },
    { id: "3", name: "Personal Leave" },
    { id: "4", name: "Maternity Leave" },
    { id: "5", name: "Paternity Leave" },
    { id: "6", name: "Unpaid Leave" },
  ]);

  useEffect(() => {
    loadEmployees();
    if (isEdit && id) {
      loadLeaveRequest(id);
    }
  }, [isEdit, id]);

  const loadLeaveRequest = async (leaveRequestId: string) => {
    try {
      const leaveRequestData = await leaveRequestService.getById(leaveRequestId);
      setLeaveRequest(leaveRequestData);
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load leave request");
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

  const calculateDays = (startDate: string, endDate: string): number => {
    if (!startDate || !endDate) return 0;
    const start = new Date(startDate);
    const end = new Date(endDate);
    const diffTime = Math.abs(end.getTime() - start.getTime());
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24)) + 1; // +1 to include both start and end days
    return diffDays;
  };

  const leaveRequestSchema = yup.object({
    employeeId: yup.string().uuid("Invalid employee").required("Employee is required"),
    leaveTypeId: yup.string().required("Leave type is required"),
    startDate: yup.string().required("Start date is required"),
    endDate: yup.string().required("End date is required"),
    daysRequested: yup.number().min(0.5, "Days requested must be at least 0.5").required("Days requested is required"),
    reason: yup.string(),
    status: yup.string(),
    notes: yup.string(),
  });

  const onSubmit = async (data: any) => {
    setError(null);
    
    // Validate end date is after start date
    if (new Date(data.endDate) < new Date(data.startDate)) {
      setError("End date must be after start date");
      return;
    }

    // Calculate days if not provided
    const daysRequested = data.daysRequested || calculateDays(data.startDate, data.endDate);

    setLoading(true);
    try {
      if (isEdit && id) {
        const updateData: UpdateLeaveRequestDto = {
          id,
          employeeId: data.employeeId,
          leaveTypeId: data.leaveTypeId,
          startDate: data.startDate,
          endDate: data.endDate,
          daysRequested,
          reason: data.reason || undefined,
          status: data.status || "Pending",
          notes: data.notes || undefined,
        };
        await leaveRequestService.update(id, updateData);
      } else {
        const createData: CreateLeaveRequestDto = {
          employeeId: data.employeeId,
          leaveTypeId: data.leaveTypeId,
          startDate: data.startDate,
          endDate: data.endDate,
          daysRequested,
          reason: data.reason || undefined,
          status: data.status || "Pending",
          notes: data.notes || undefined,
          appliedAt: new Date().toISOString(),
        };
        await leaveRequestService.create(createData);
      }
      navigate("/hrm/leave-requests");
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to save leave request");
    } finally {
      setLoading(false);
    }
  };

  const defaultValues = isEdit && leaveRequest
    ? {
        employeeId: leaveRequest.employeeId || "",
        leaveTypeId: leaveRequest.leaveTypeId || "",
        startDate: leaveRequest.startDate ? leaveRequest.startDate.split('T')[0] : new Date().toISOString().split('T')[0],
        endDate: leaveRequest.endDate ? leaveRequest.endDate.split('T')[0] : new Date().toISOString().split('T')[0],
        daysRequested: leaveRequest.daysRequested || 0,
        reason: leaveRequest.reason || "",
        status: leaveRequest.status || "Pending",
        notes: leaveRequest.notes || "",
      }
    : {
        employeeId: "",
        leaveTypeId: "",
        startDate: new Date().toISOString().split('T')[0],
        endDate: new Date().toISOString().split('T')[0],
        daysRequested: 0,
        reason: "",
        status: "Pending",
        notes: "",
      };

  return (
    <>
      <PageBreadcrumb title={isEdit ? "Edit Leave Request" : "Add Leave Request"} subName="HRM" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center gap-2">
            <Link to="/hrm/leave-requests" className="btn btn-sm btn-soft-secondary">
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
            resolver={yupResolver(leaveRequestSchema)}
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
                label="Leave Type"
                type="select"
                name="leaveTypeId"
                containerClass="mb-4"
                className="form-select"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                required
              >
                <option value="">Select leave type</option>
                {leaveTypes.map((type) => (
                  <option key={type.id} value={type.id}>
                    {type.name}
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
                required
              />

              <FormInput
                label="End Date"
                type="date"
                name="endDate"
                containerClass="mb-4"
                className="form-input"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                required
              />

              <FormInput
                label="Days Requested"
                type="number"
                name="daysRequested"
                placeholder="0.5"
                containerClass="mb-4"
                className="form-input"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                step="0.5"
                min="0.5"
                required
              />

              <FormInput
                label="Status"
                type="select"
                name="status"
                containerClass="mb-4"
                className="form-select"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
              >
                {LEAVE_REQUEST_STATUS_OPTIONS.map((option) => (
                  <option key={option.value} value={option.value}>
                    {option.label}
                  </option>
                ))}
              </FormInput>

              <div className="md:col-span-2">
                <FormInput
                  label="Reason"
                  type="textarea"
                  name="reason"
                  rows={3}
                  placeholder="Reason for leave"
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
              <Link to="/hrm/leave-requests" className="btn btn-soft-secondary">
                Cancel
              </Link>
              <button type="submit" className="btn btn-primary" disabled={loading}>
                {loading ? "Saving…" : isEdit ? "Save changes" : "Add Leave Request"}
              </button>
            </div>
          </VerticalForm>
        </div>
      </div>
    </>
  );
};

export default LeaveRequestForm;

