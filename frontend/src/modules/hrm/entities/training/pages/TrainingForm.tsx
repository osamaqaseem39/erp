import React, { useEffect, useState } from "react";
import { useNavigate, useParams, Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { VerticalForm, FormInput } from "../../../../../components";
import { trainingService, type CreateTrainingDto, type UpdateTrainingDto, type TrainingDto } from "../services";
import { employeeService } from "../../employee/services";
import { TRAINING_TYPE_OPTIONS, TRAINING_STATUS_OPTIONS } from "../types";
import * as yup from "yup";
import { yupResolver } from "@hookform/resolvers/yup";

const TrainingForm: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const isEdit = !!id;

  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [training, setTraining] = useState<TrainingDto | null>(null);
  const [employees, setEmployees] = useState<any[]>([]);

  useEffect(() => {
    loadEmployees();
    if (isEdit && id) {
      loadTraining(id);
    }
  }, [isEdit, id]);

  const loadTraining = async (trainingId: string) => {
    try {
      const trainingData = await trainingService.getById(trainingId);
      setTraining(trainingData);
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load training");
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

  const calculateDuration = (startDate: string, endDate: string): number => {
    if (!startDate || !endDate) return 0;
    const start = new Date(startDate);
    const end = new Date(endDate);
    const diffTime = Math.abs(end.getTime() - start.getTime());
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
    return diffDays;
  };

  const trainingSchema = yup.object({
    title: yup.string().required("Title is required"),
    description: yup.string().required("Description is required"),
    trainingType: yup.string().required("Training type is required"),
    startDate: yup.string().required("Start date is required"),
    endDate: yup.string().required("End date is required"),
    duration: yup.number().min(1, "Duration must be at least 1 day").required("Duration is required"),
    status: yup.string(),
    instructorId: yup.string().uuid("Invalid instructor").nullable(),
    location: yup.string().required("Location is required"),
    cost: yup.number().min(0, "Cost must be >= 0"),
    maxParticipants: yup.number().min(1, "Max participants must be at least 1").required("Max participants is required"),
    notes: yup.string(),
    isActive: yup.boolean(),
  });

  const onSubmit = async (data: any) => {
    setError(null);
    
    // Validate end date is after start date
    if (new Date(data.endDate) < new Date(data.startDate)) {
      setError("End date must be after start date");
      return;
    }

    // Calculate duration if not provided
    const duration = data.duration || calculateDuration(data.startDate, data.endDate);

    setLoading(true);
    try {
      if (isEdit && id) {
        const updateData: UpdateTrainingDto = {
          id,
          title: data.title,
          description: data.description,
          trainingType: data.trainingType,
          startDate: data.startDate,
          endDate: data.endDate,
          duration,
          status: data.status || "Scheduled",
          instructorId: data.instructorId || undefined,
          instructor: data.instructor || undefined,
          location: data.location,
          cost: parseFloat(data.cost) || 0,
          maxParticipants: parseInt(data.maxParticipants) || 1,
          notes: data.notes || undefined,
          isActive: data.isActive ?? true,
        };
        await trainingService.update(id, updateData);
      } else {
        const createData: CreateTrainingDto = {
          title: data.title,
          description: data.description,
          trainingType: data.trainingType,
          startDate: data.startDate,
          endDate: data.endDate,
          duration,
          status: data.status || "Scheduled",
          instructorId: data.instructorId || undefined,
          instructor: data.instructor || undefined,
          location: data.location,
          cost: parseFloat(data.cost) || 0,
          maxParticipants: parseInt(data.maxParticipants) || 1,
          notes: data.notes || undefined,
          isActive: data.isActive ?? true,
        };
        await trainingService.create(createData);
      }
      navigate("/hrm/trainings");
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to save training");
    } finally {
      setLoading(false);
    }
  };

  const defaultValues = isEdit && training
    ? {
        title: training.title || "",
        description: training.description || "",
        trainingType: training.trainingType || "",
        startDate: training.startDate ? training.startDate.split('T')[0] : new Date().toISOString().split('T')[0],
        endDate: training.endDate ? training.endDate.split('T')[0] : new Date().toISOString().split('T')[0],
        duration: training.duration || 1,
        status: training.status || "Scheduled",
        instructorId: training.instructorId || "",
        instructor: training.instructor || "",
        location: training.location || "",
        cost: training.cost || 0,
        maxParticipants: training.maxParticipants || 1,
        notes: training.notes || "",
        isActive: training.isActive ?? true,
      }
    : {
        title: "",
        description: "",
        trainingType: "",
        startDate: new Date().toISOString().split('T')[0],
        endDate: new Date(Date.now() + 7 * 24 * 60 * 60 * 1000).toISOString().split('T')[0], // 7 days from now
        duration: 1,
        status: "Scheduled",
        instructorId: "",
        instructor: "",
        location: "",
        cost: 0,
        maxParticipants: 1,
        notes: "",
        isActive: true,
      };

  return (
    <>
      <PageBreadcrumb title={isEdit ? "Edit Training" : "Add Training"} subName="HRM" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center gap-2">
            <Link to="/hrm/trainings" className="btn btn-sm btn-soft-secondary">
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
            resolver={yupResolver(trainingSchema)}
            defaultValues={defaultValues}
          >
            <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
              <FormInput
                label="Title"
                type="text"
                name="title"
                placeholder="Enter training title"
                containerClass="mb-4"
                className="form-input"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                required
              />

              <FormInput
                label="Training Type"
                type="select"
                name="trainingType"
                containerClass="mb-4"
                className="form-select"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                required
              >
                <option value="">Select training type</option>
                {TRAINING_TYPE_OPTIONS.map((option) => (
                  <option key={option.value} value={option.value}>
                    {option.label}
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
                label="Duration (Days)"
                type="number"
                name="duration"
                placeholder="1"
                containerClass="mb-4"
                className="form-input"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                step="1"
                min="1"
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
                {TRAINING_STATUS_OPTIONS.map((option) => (
                  <option key={option.value} value={option.value}>
                    {option.label}
                  </option>
                ))}
              </FormInput>

              <FormInput
                label="Instructor"
                type="select"
                name="instructorId"
                containerClass="mb-4"
                className="form-select"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
              >
                <option value="">Select instructor</option>
                {employees.map((employee) => (
                  <option key={employee.id} value={employee.id}>
                    {employee.employeeCode} - {employee.firstName} {employee.lastName}
                  </option>
                ))}
              </FormInput>

              <FormInput
                label="Location"
                type="text"
                name="location"
                placeholder="Enter location"
                containerClass="mb-4"
                className="form-input"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                required
              />

              <FormInput
                label="Cost"
                type="number"
                name="cost"
                placeholder="0.00"
                containerClass="mb-4"
                className="form-input"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                step="0.01"
                min="0"
              />

              <FormInput
                label="Max Participants"
                type="number"
                name="maxParticipants"
                placeholder="1"
                containerClass="mb-4"
                className="form-input"
                labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                step="1"
                min="1"
                required
              />

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
                  placeholder="Enter training description"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                  required
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
              <Link to="/hrm/trainings" className="btn btn-soft-secondary">
                Cancel
              </Link>
              <button type="submit" className="btn btn-primary" disabled={loading}>
                {loading ? "Saving…" : isEdit ? "Save changes" : "Add Training"}
              </button>
            </div>
          </VerticalForm>
        </div>
      </div>
    </>
  );
};

export default TrainingForm;

