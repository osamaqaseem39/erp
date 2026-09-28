import React from "react";
import type { RoutesProps } from "../types";

import TicketsApp from "../../pages/apps/Tickets";
const Accordions = React.lazy(() => import("../../pages/ui/Accordions"));
const Alerts = React.lazy(() => import("../../pages/ui/Alerts"));
const Animation = React.lazy(() => import("../../pages/extended/Animation"));
const Avatars = React.lazy(() => import("../../pages/ui/Avatars"));
const Badges = React.lazy(() => import("../../pages/ui/Badges"));
const BasicTables = React.lazy(() => import("../../pages/tables/BasicTables"));
const Breadcrumb = React.lazy(() => import("../../pages/ui/Breadcrumb"));
const Buttons = React.lazy(() => import("../../pages/ui/Buttons"));
const CalendarApp = React.lazy(() => import("../../pages/apps/Calendar"));
const Cards = React.lazy(() => import("../../pages/ui/Cards"));
const Chart = React.lazy(() => import("../../pages/ui/Chart"));
const Collapse = React.lazy(() => import("../../pages/ui/Collapse"));
const DataTables = React.lazy(() => import("../../pages/tables/DataTables"));
const Dismissible = React.lazy(() => import("../../pages/ui/Dismissible"));
const Dropdowns = React.lazy(() => import("../../pages/ui/Dropdowns"));
const Editor = React.lazy(() => import("../../pages/forms/Editor"));
const Error404Alt = React.lazy(() => import("../../pages/error/Error404Alt"));
const FAQs = React.lazy(() => import("../../pages/extra/FAQs"));
const FeatherIcons = React.lazy(() => import("../../pages/ui/icons/FeatherIcons"));
const FileManagerApp = React.lazy(() => import("../../pages/apps/FileManager"));
const FileUploads = React.lazy(() => import("../../pages/forms/FileUploads"));
const FormElements = React.lazy(() => import("../../pages/forms/FormElements"));
const FormLayout = React.lazy(() => import("../../pages/forms/FormLayout"));
const FormSelect = React.lazy(() => import("../../pages/forms/Select"));
const Gallery = React.lazy(() => import("../../pages/extra/Gallery"));
const GoogleMaps = React.lazy(() => import("../../pages/ui/maps/GoogleMaps"));
const Invoice = React.lazy(() => import("../../pages/extra/Invoice"));
const KanbanApp = React.lazy(() => import("../../pages/apps/Kanban"));
const Lightbox = React.lazy(() => import("../../pages/extended/Lightbox"));
const ListGroup = React.lazy(() => import("../../pages/ui/ListGroup"));
const Masks = React.lazy(() => import("../../pages/forms/Masks"));
const MaterialSymbolIcons = React.lazy(() => import("../../pages/ui/icons/MaterialSymbolIcons"));
const MingCuteIcons = React.lazy(() => import("../../pages/ui/icons/MingCuteIcons"));
const Modals = React.lazy(() => import("../../pages/ui/Modals"));
const NestableList = React.lazy(() => import("../../pages/extended/NestableList"));
const Offcanvas = React.lazy(() => import("../../pages/ui/Offcanvas"));
const Pickers = React.lazy(() => import("../../pages/forms/Pickers"));
const Player = React.lazy(() => import("../../pages/extended/Player"));
const Popovers = React.lazy(() => import("../../pages/ui/Popovers"));
const Pricing = React.lazy(() => import("../../pages/extra/Pricing"));
const Progress = React.lazy(() => import("../../pages/ui/Progress"));
const ProjectAppList = React.lazy(() => import("../../pages/apps/Project/List"));
const ProjectCreate = React.lazy(() => import("../../pages/apps/Project/Create"));
const ProjectDetail = React.lazy(() => import("../../pages/apps/Project/Detail"));
const Range = React.lazy(() => import("../../pages/forms/Range"));
const Ratings = React.lazy(() => import("../../pages/extended/Ratings"));
const Ratio = React.lazy(() => import("../../pages/ui/Ratio"));
const Scrollbar = React.lazy(() => import("../../pages/extended/Scrollbar"));
const Skeleton = React.lazy(() => import("../../pages/ui/Skeleton"));
const Spinners = React.lazy(() => import("../../pages/ui/Spinners"));
const Starter = React.lazy(() => import("../../pages/extra/Starter"));
const SweetAlert = React.lazy(() => import("../../pages/extended/SweetAlert"));
const Swiper = React.lazy(() => import("../../pages/extended/Swiper"));
const Tabs = React.lazy(() => import("../../pages/ui/Tabs"));
const Timeline = React.lazy(() => import("../../pages/extra/TimeLine"));
const TippyTooltip = React.lazy(() => import("../../pages/extended/TippyTooltip"));
const Tooltips = React.lazy(() => import("../../pages/ui/Tooltips"));
const TourPage = React.lazy(() => import("../../pages/extended/TourPage"));
const Typography = React.lazy(() => import("../../pages/ui/Typography"));
const Validation = React.lazy(() => import("../../pages/forms/Validation"));
const VectorMaps = React.lazy(() => import("../../pages/ui/maps/VectorMaps"));

export const calendarAppRoutes: RoutesProps = {
  path: "/apps/calendar",
  name: "Calendar",
  roles: ["Admin"],
  icon: "calendar",
  element: <CalendarApp />,
  header: "Apps",
};

export const ticketsAppRoutes: RoutesProps = {
  path: "/apps/tickets",
  name: "Tickets",
  roles: ["Admin"],
  icon: "tickets",
  element: <TicketsApp />,
  header: "Apps",
};

export const fileAppRoutes: RoutesProps = {
  path: "/apps/file-manager",
  name: "File Manager",
  roles: ["Admin"],
  icon: "filemanager",
  element: <FileManagerApp />,
  header: "Apps",
};

export const kanbanAppRoutes: RoutesProps = {
  path: "/apps/kanban",
  name: "Kanban Board",
  roles: ["Admin"],
  icon: "kanban",
  element: <KanbanApp />,
  header: "Apps",
};

export const projectAppRoutes: RoutesProps = {
  path: "/apps/project",
  name: "Project",
  roles: ["Admin"],
  icon: "project",
  children: [
    {
      path: '/apps/project/list',
      name: 'ProjectList',
      element: <ProjectAppList />,
    },
    {
      path: '/apps/project/detail',
      name: 'ProjectDetail',
      element: <ProjectDetail />,
    },
    {
      path: '/apps/project/create',
      name: 'ProjectCreate',
      element: <ProjectCreate />,
    },
  ]
};

export const customPagesRoutes: RoutesProps = {
  path: "/pages",
  name: "Pages",
  icon: "pages",
  header: "Custom",
  children: [
    {
      path: "/pages/starter",
      name: "Starter",
      element: <Starter />,
    },
    {
      path: "/pages/timeline",
      name: "Timeline",
      element: <Timeline />,
    },
    {
      path: "/pages/invoice",
      name: "Invoice",
      element: <Invoice />,
    },
    {
      path: "/pages/gallery",
      name: "Gallery",
      element: <Gallery />,
    },
    {
      path: "/pages/faqs",
      name: "FAQs",
      element: <FAQs />,
    },
    {
      path: "/pages/pricing",
      name: "Pricing",
      element: <Pricing />,
    },
    {
      path: "/error-404-alt",
      name: "Error - 404-alt",
      element: <Error404Alt />,
    },
  ],
};

export const tableRoutes: RoutesProps = {
  path: "/tables",
  name: "Tables",
  icon: "table",
  header: "Elements",
  children: [
    {
      path: "/ui/tables/basic-tables",
      name: "Basic Tables",
      element: <BasicTables />,
    },
    {
      path: "/ui/tables/data-tables",
      name: "Data Tables",
      element: <DataTables />,
    },
  ]
}

export const uiRoutes: RoutesProps = {
  path: "/ui",
  name: "Components",
  icon: "pocket",
  header: "Elements",
  children: [
    {
      path: "/ui/components",
      name: "Components",
      children: [
        {
          path: "/ui/accordions",
          name: "Accordions",
          element: <Accordions />,
        },
        {
          path: "/ui/alerts",
          name: "Alerts",
          element: <Alerts />,
        },
        {
          path: "/ui/avatars",
          name: "Avatars",
          element: <Avatars />,
        },
        {
          path: "/ui/buttons",
          name: "Buttons",
          element: <Buttons />,
        },
        {
          path: "/ui/badges",
          name: "Badges",
          element: <Badges />,
        },
        {
          path: "/ui/breadcrumb",
          name: "Breadcrumb",
          element: <Breadcrumb />,
        },
        {
          path: "/ui/cards",
          name: "Cards",
          element: <Cards />,
        },
        {
          path: "/ui/collapse",
          name: "Collapse",
          element: <Collapse />,
        },
        {
          path: "/ui/dismissible",
          name: "Dismissible",
          element: <Dismissible />,
        },
        {
          path: "/ui/dropdowns",
          name: "Dropdowns",
          element: <Dropdowns />,
        },
        {
          path: "/ui/progress",
          name: "Progress",
          element: <Progress />,
        },
        {
          path: "/ui/skeleton",
          name: "Skeleton",
          element: <Skeleton />,
        },
        {
          path: "/ui/spinners",
          name: "Spinners",
          element: <Spinners />,
        },
        {
          path: "/ui/list-group",
          name: "List Group",
          element: <ListGroup />,
        },
        {
          path: "/ui/ratio",
          name: "Ratio",
          element: <Ratio />,
        },
        {
          path: "/ui/tab",
          name: "Tab",
          element: <Tabs />,
        },
        {
          path: "/ui/modals",
          name: "Modals",
          element: <Modals />,
        },
        {
          path: "/ui/offcanvas",
          name: "Offcanvas",
          element: <Offcanvas />,
        },
        {
          path: "/ui/popovers",
          name: "Popovers",
          element: <Popovers />,
        },
        {
          path: "/ui/tooltips",
          name: "Tooltips",
          element: <Tooltips />,
        },
        {
          path: "/ui/typography",
          name: "Typography",
          element: <Typography />,
        },
      ],
    },
    {
      path: '/extended',
      name: 'Extended',
      children: [
        {
          path: "/extended/swiper",
          name: "Swiper",
          element: <Swiper />,
        },
        {
          path: "/extended/nestable-list",
          name: "Nestable List",
          element: <NestableList />,
        },
        {
          path: "/extended/ratings",
          name: "Ratings",
          element: <Ratings />,
        },
        {
          path: "/extended/animation",
          name: "Animation",
          element: <Animation />,
        },
        {
          path: "/extended/player",
          name: "Player",
          element: <Player />,
        },
        {
          path: "/extended/scrollbar",
          name: "Scrollbar",
          element: <Scrollbar />,
        },
        {
          path: "/extended/sweet-alert",
          name: "Sweet Alert",
          element: <SweetAlert />,
        },
        {
          path: "/extended/tour",
          name: "Tourpage",
          element: <TourPage />,
        },
        {
          path: "/extended/tooltippy",
          name: "Tippy Tooltip",
          element: <TippyTooltip />,
        },
        {
          path: "/extended/lightbox",
          name: "Lightbox",
          element: <Lightbox />,
        },
      ],
    },
    {
      path: '/ui/forms',
      name: 'Forms',
      children: [
        {
          path: "/ui/forms/form-elements",
          name: "Form Elements",
          element: <FormElements />,
        },
        {
          path: "/ui/forms/select",
          name: "Select",
          element: <FormSelect />,
        },
        {
          path: "/ui/forms/range",
          name: "Range",
          element: <Range />,
        },
        {
          path: "/ui/forms/pickers",
          name: "Pickers",
          element: <Pickers />,
        },
        {
          path: "/ui/forms/masks",
          name: "Masks",
          element: <Masks />,
        },
        {
          path: "/ui/forms/editor",
          name: "Editor",
          element: <Editor />,
        },
        {
          path: "/ui/forms/file-upload",
          name: "File Uploads",
          element: <FileUploads />,
        },
        {
          path: "/ui/forms/validation",
          name: "Validation",
          element: <Validation />,
        },
        {
          path: "/ui/forms/form-layout",
          name: "Form Layout",
          element: <FormLayout />,
        },
      ],
    },
    {
      path: '/ui/icons',
      name: 'Icons',
      children: [
        {
          path: "/ui/icons/mingcute",
          name: "Mingcute",
          element: <MingCuteIcons />,
        },
        {
          path: "/ui/icons/feather",
          name: "Feather",
          element: <FeatherIcons />,
        },
        {
          path: "/ui/icons/material",
          name: "Material Symbols",
          element: <MaterialSymbolIcons />,
        },
      ],
    },
    {
      path: '/ui/maps',
      name: 'Maps',
      children: [
        {
          path: "/ui/maps/vector-maps",
          name: "Vector Maps",
          element: <VectorMaps />,
        },
        {
          path: "/ui/maps/google-maps",
          name: "Google Maps",
          element: <GoogleMaps />,
        },
      ],
    },
  ],
};

export const chartRoutes: RoutesProps = {
  path: "/ui/chart",
  name: "Chart",
  roles: ["Admin"],
  icon: "Chart",
  element: <Chart />,
  header: "Elements",
};
