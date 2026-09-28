import React from "react";
import type { RoutesProps } from "../types";

const ProjectForm = React.lazy(() => import("../../modules/projects/entities/project/pages/ProjectForm"));
const ProjectList = React.lazy(() => import("../../modules/projects/entities/project/pages/ProjectList"));

export const projectsRoutes: RoutesProps = {
  path: "/projects",
  name: "Projects",
  icon: "briefcase",
  header: "Project Management",
  children: [
    // Projects
    {
      path: "/projects",
      name: "Projects",
      element: <ProjectList />,
    },
    {
      path: "/projects/new",
      name: "Add Project",
      element: <ProjectForm />,
    },
    {
      path: "/projects/:id/edit",
      name: "Edit Project",
      element: <ProjectForm />,
    },
  ],
};
