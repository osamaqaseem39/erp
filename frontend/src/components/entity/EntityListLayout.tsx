import React from "react";
import PageBreadcrumb from "../PageBreadcrumb";
import AddEntityButton from "./AddEntityButton";

interface EntityListLayoutProps {
  title: string;
  moduleName: string;
  entityName: string;
  addUrl: string;
  children: React.ReactNode;
  toolbar?: React.ReactNode;
  error?: string | null;
  headerExtra?: React.ReactNode;
  showBreadcrumb?: boolean;
}

const EntityListLayout: React.FC<EntityListLayoutProps> = ({
  title,
  moduleName,
  entityName,
  addUrl,
  children,
  toolbar,
  error,
  headerExtra,
  showBreadcrumb = true,
}) => {
  const isCrm = moduleName === "CRM";

  return (
    <>
      {showBreadcrumb && (
        <PageBreadcrumb title={title} name={moduleName} breadCrumbItems={[moduleName, title]} />
      )}
      {isCrm ? (
        <div className="flex flex-wrap items-center justify-between gap-3 mb-4">
          <h1 className="text-[24px] font-semibold text-[#323130] dark:text-[#f3f2f1] mb-0 leading-tight">
            {title}
          </h1>
          <div className="flex items-center gap-2">
            {headerExtra}
            <AddEntityButton to={addUrl} entityName={entityName} />
          </div>
        </div>
      ) : null}
      <div className={isCrm ? "card crm-entity-list" : "card"}>
        {!isCrm ? (
          <div className="card-header border-b border-gray-100 dark:border-gray-700">
            <div className="flex flex-wrap items-center justify-between gap-3">
              <h4 className="card-title mb-0">{title}</h4>
              <div className="flex items-center gap-2">
                {headerExtra}
                <AddEntityButton to={addUrl} entityName={entityName} />
              </div>
            </div>
          </div>
        ) : null}
        <div className="card-body">
          {error && (
            <div className="alert alert-danger mb-4" role="alert">
              {error}
            </div>
          )}
          {toolbar && <div className="mb-4">{toolbar}</div>}
          {children}
        </div>
      </div>
    </>
  );
};

export default EntityListLayout;
