import React from "react";
import type { RoutesProps } from "../types";

const ComponentGallery = React.lazy(() => import("../../pages/dev/ComponentGallery"));

/** Development-only pages. Not registered in production builds. */
export const devRoutes: RoutesProps[] = [
  {
    path: "/dev/components",
    name: "UI kit gallery",
    element: <ComponentGallery />,
  },
];
