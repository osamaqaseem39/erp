import React, { useEffect, useState } from "react";
import { useNavigate, useParams, Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { VerticalForm, FormInput } from "../../../../../components";
import { posSessionService, type CreatePOSSessionDto, type UpdatePOSSessionDto, type POSSessionDto } from "../services";
import { employeeService } from "../../../../hrm/entities/employee/services";
import { POS_SESSION_STATUS_OPTIONS } from "../types";
import * as yup from "yup";
import { yupResolver } from "@hookform/resolvers/yup";

const POSSessionForm: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const isEdit = !!id;

  const sessionSchema = yup.object({
    sessionNumber: yup.string().required("Session number is required"),
    cashierId: yup.string().uuid("Invalid cashier").required("Cashier is required"),
    terminalId: yup.string().uuid("Invalid terminal").required("Terminal is required"),
    startTime: yup.date().required("Start time is required"),
    openingBalance: yup.number().min(0, "Opening balance must be >= 0").required("Opening balance is required"),
    status: yup.string().oneOf(["Open", "Closed", "Suspended"], "Invalid status").required("Status is required"),
    notes: yup.string(),
  });

  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [employees, setEmployees] = useState<any[]>([]);
  const [session, setSession] = useState<POSSessionDto | null>(null);

  useEffect(() => {
    loadEmployees();
    if (isEdit && id) {
      loadSession(id);
    }
  }, [isEdit, id]);

  const loadSession = async (sessionId: string) => {
    try {
      const sessionData = await posSessionService.getById(sessionId);
      setSession(sessionData);
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load POS session");
    }
  };

  const loadEmployees = async () => {
    try {
      const data = await employeeService.getAll({ status: "Active" });
      setEmployees(data);
    } catch (err: any) {
      console.error("Failed to load employees", err);
    }
  };

  // Helper to format date for input (YYYY-MM-DDTHH:mm)
  const formatDateTimeForInput = (dateString?: string): string => {
    if (!dateString) return "";
    const date = new Date(dateString);
    if (isNaN(date.getTime())) return "";
    // Format as YYYY-MM-DDTHH:mm for datetime-local input
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, "0");
    const day = String(date.getDate()).padStart(2, "0");
    const hours = String(date.getHours()).padStart(2, "0");
    const minutes = String(date.getMinutes()).padStart(2, "0");
    return `${year}-${month}-${day}T${hours}:${minutes}`;
  };

  // Helper to format date for API (ISO string)
  const formatDateTimeForAPI = (dateString?: string): string | undefined => {
    if (!dateString) return undefined;
    const date = new Date(dateString);
    if (isNaN(date.getTime())) return undefined;
    return date.toISOString();
  };

  const onSubmit = async (data: any) => {
    setError(null);
    setLoading(true);
    try {
      if (isEdit && id) {
        const updateData: UpdatePOSSessionDto = {
          id,
          sessionNumber: data.sessionNumber,
          cashierId: data.cashierId,
          terminalId: data.terminalId,
          startTime: formatDateTimeForAPI(data.startTime),
          endTime: formatDateTimeForAPI(data.endTime),
          openingBalance: parseFloat(data.openingBalance) || 0,
          closingBalance: parseFloat(data.closingBalance) || 0,
          status: data.status,
          notes: data.notes || undefined,
        };
        await posSessionService.update(id, updateData);
      } else {
        const createData: CreatePOSSessionDto = {
          sessionNumber: data.sessionNumber,
          cashierId: data.cashierId,
          terminalId: data.terminalId,
          startTime: formatDateTimeForAPI(data.startTime) || new Date().toISOString(),
          openingBalance: parseFloat(data.openingBalance) || 0,
          status: data.status,
          notes: data.notes || undefined,
        };
        await posSessionService.create(createData);
      }
      navigate("/pos/sessions");
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || `Failed to ${isEdit ? "Save changes" : "Add POSSession"} POS session`);
    } finally {
      setLoading(false);
    }
  };

  return (
    <>
      <PageBreadcrumb title={isEdit ? "Edit POS Session" : "Add POS Session"} subName="POS" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center gap-2">
            <Link to="/pos/sessions" className="btn btn-sm btn-soft-secondary">
              <i className="mgc_arrow_left_line me-2"></i>Back
            </Link>
            <h4 className="card-title">{isEdit ? "Edit POS Session" : "Add POS Session"}</h4>
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
            resolver={yupResolver(sessionSchema)}
            defaultValues={session || {
              sessionNumber: "",
              cashierId: "",
              terminalId: "",
              startTime: formatDateTimeForInput(new Date().toISOString()),
              endTime: "",
              openingBalance: 0,
              closingBalance: 0,
              status: "Open",
              notes: "",
            }}
          >
            <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
              <FormInput
                label="Session Number"
                type="text"
                name="sessionNumber"
                placeholder="Enter session number"
                containerClass="mb-4"
                className="form-input"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                required
              />

              <FormInput
                label="Status"
                type="select"
                name="status"
                containerClass="mb-4"
                className="form-select"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                required
              >
                {POS_SESSION_STATUS_OPTIONS.map((option) => (
                  <option key={option.value} value={option.value}>
                    {option.label}
                  </option>
                ))}
              </FormInput>

              <FormInput
                label="Cashier"
                type="select"
                name="cashierId"
                containerClass="mb-4"
                className="form-select"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                required
              >
                <option value="">Select cashier</option>
                {employees.map((emp) => (
                  <option key={emp.id} value={emp.id}>
                    {emp.firstName} {emp.lastName} ({emp.employeeCode})
                  </option>
                ))}
              </FormInput>

              <FormInput
                label="Terminal ID"
                type="text"
                name="terminalId"
                placeholder="Enter terminal ID (GUID)"
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
                required
              />

              {isEdit && (
                <FormInput
                  label="End Time"
                  type="datetime-local"
                  name="endTime"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                />
              )}

              <FormInput
                label="Opening Balance"
                type="number"
                name="openingBalance"
                placeholder="0.00"
                containerClass="mb-4"
                className="form-input"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                step="0.01"
                min="0"
                required
              />

              {isEdit && (
                <FormInput
                  label="Closing Balance"
                  type="number"
                  name="closingBalance"
                  placeholder="0.00"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                  step="0.01"
                  min="0"
                />
              )}

              <div className="md:col-span-2">
                <FormInput
                  label="Notes"
                  type="textarea"
                  name="notes"
                  rows={4}
                  placeholder="Enter notes"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                />
              </div>
            </div>

            <div className="flex justify-end gap-2">
              <Link to="/pos/sessions" className="btn btn-soft-secondary">
                Cancel
              </Link>
              <button type="submit" className="btn btn-primary" disabled={loading}>
                {loading ? "Saving…" : isEdit ? "Save changes" : "Add POSSession"}
              </button>
            </div>
          </VerticalForm>
        </div>
      </div>
    </>
  );
};

export default POSSessionForm;

