import React from "react";
import type { RoutesProps } from "../types";

const DepartmentForm = React.lazy(() => import("../../modules/hrm/entities/department/pages/DepartmentForm"));
const DepartmentList = React.lazy(() => import("../../modules/hrm/entities/department/pages/DepartmentList"));
const EmployeeForm = React.lazy(() => import("../../modules/hrm/entities/employee/pages/EmployeeForm"));
const EmployeeList = React.lazy(() => import("../../modules/hrm/entities/employee/pages/EmployeeList"));
const LeaveRequestForm = React.lazy(() => import("../../modules/hrm/entities/leave-request/pages/LeaveRequestForm"));
const LeaveRequestList = React.lazy(() => import("../../modules/hrm/entities/leave-request/pages/LeaveRequestList"));
const PayrollForm = React.lazy(() => import("../../modules/hrm/entities/payroll/pages/PayrollForm"));
const PayrollList = React.lazy(() => import("../../modules/hrm/entities/payroll/pages/PayrollList"));
const PerformanceReviewForm = React.lazy(() => import("../../modules/hrm/entities/performance-review/pages/PerformanceReviewForm"));
const PerformanceReviewList = React.lazy(() => import("../../modules/hrm/entities/performance-review/pages/PerformanceReviewList"));
const TimeEntryForm = React.lazy(() => import("../../modules/hrm/entities/time-tracking/pages/TimeEntryForm"));
const TimeEntryList = React.lazy(() => import("../../modules/hrm/entities/time-tracking/pages/TimeEntryList"));
const TrainingForm = React.lazy(() => import("../../modules/hrm/entities/training/pages/TrainingForm"));
const TrainingList = React.lazy(() => import("../../modules/hrm/entities/training/pages/TrainingList"));

export const hrmRoutes: RoutesProps = {
  path: "/hrm",
  name: "HRM",
  icon: "users",
  header: "Human Resources",
  children: [
    // Employees
    {
      path: "/hrm/employees",
      name: "Employees",
      element: <EmployeeList />,
    },
    {
      path: "/hrm/employees/new",
      name: "Add Employee",
      element: <EmployeeForm />,
    },
    {
      path: "/hrm/employees/:id/edit",
      name: "Edit Employee",
      element: <EmployeeForm />,
    },
    // Departments
    {
      path: "/hrm/departments",
      name: "Departments",
      element: <DepartmentList />,
    },
    {
      path: "/hrm/departments/new",
      name: "Add Department",
      element: <DepartmentForm />,
    },
    {
      path: "/hrm/departments/:id/edit",
      name: "Edit Department",
      element: <DepartmentForm />,
    },
    // Payrolls
    {
      path: "/hrm/payrolls",
      name: "Payrolls",
      element: <PayrollList />,
    },
    {
      path: "/hrm/payrolls/new",
      name: "Add Payroll",
      element: <PayrollForm />,
    },
    {
      path: "/hrm/payrolls/:id/edit",
      name: "Edit Payroll",
      element: <PayrollForm />,
    },
    // Leave Requests
    {
      path: "/hrm/leave-requests",
      name: "Leave Requests",
      element: <LeaveRequestList />,
    },
    {
      path: "/hrm/leave-requests/new",
      name: "Add Leave Request",
      element: <LeaveRequestForm />,
    },
    {
      path: "/hrm/leave-requests/:id/edit",
      name: "Edit Leave Request",
      element: <LeaveRequestForm />,
    },
    // Trainings
    {
      path: "/hrm/trainings",
      name: "Trainings",
      element: <TrainingList />,
    },
    {
      path: "/hrm/trainings/new",
      name: "Add Training",
      element: <TrainingForm />,
    },
    {
      path: "/hrm/trainings/:id/edit",
      name: "Edit Training",
      element: <TrainingForm />,
    },
    // Performance Reviews
    {
      path: "/hrm/performance-reviews",
      name: "Performance Reviews",
      element: <PerformanceReviewList />,
    },
    {
      path: "/hrm/performance-reviews/new",
      name: "Add Performance Review",
      element: <PerformanceReviewForm />,
    },
    {
      path: "/hrm/performance-reviews/:id/edit",
      name: "Edit Performance Review",
      element: <PerformanceReviewForm />,
    },
    // Time Entries
    {
      path: "/hrm/time-entries",
      name: "Time Entries",
      element: <TimeEntryList />,
    },
    {
      path: "/hrm/time-entries/new",
      name: "Add Time Entry",
      element: <TimeEntryForm />,
    },
    {
      path: "/hrm/time-entries/:id/edit",
      name: "Edit Time Entry",
      element: <TimeEntryForm />,
    },
  ],
};
