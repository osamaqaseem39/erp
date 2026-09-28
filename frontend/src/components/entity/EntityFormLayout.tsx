import React from "react";
import { Link } from "react-router-dom";
import PageBreadcrumb from "../PageBreadcrumb";
import { addLabel, editLabel } from "./labels";

interface EntityFormLayoutProps {
  moduleName: string;
  entityName: string;
  isEdit: boolean;
  subtitle?: string;
  backUrl: string;
  fetching?: boolean;
  error?: string | null;
  children: React.ReactNode;
  maxWidth?: "default" | "wide";
  showBreadcrumb?: boolean;
}

/** @deprecated Legacy layout kept until its module is migrated. Use PageHeader + FormSection + FormFooter from components/erp. */
const EntityFormLayout: React.FC<EntityFormLayoutProps> = ({
  moduleName,
  entityName,
  isEdit,
  subtitle,
  backUrl,
  fetching = false,
  error,
  children,
  maxWidth = "default",
  showBreadcrumb = true,
}) => {
  const pageTitle = isEdit ? editLabel(entityName) : addLabel(entityName);
  const widthClass = maxWidth === "wide" ? "max-w-6xl" : "max-w-5xl";
  const isCrm = moduleName === "CRM";

  if (fetching) {
    return (
      <>
        {showBreadcrumb && (
          <PageBreadcrumb title={pageTitle} name={moduleName} breadCrumbItems={[moduleName, pageTitle]} />
        )}
        <div className={`card ${widthClass} mx-auto`}>
          <div className="card-body flex items-center justify-center py-16">
            <div className="spinner-border text-primary" role="status">
              <span className="visually-hidden">Loading...</span>
            </div>
          </div>
        </div>
      </>
    );
  }

  return (
    <>
      {showBreadcrumb && (
        <PageBreadcrumb title={pageTitle} name={moduleName} breadCrumbItems={[moduleName, pageTitle]} />
      )}
      {isCrm && (
        <div className="flex flex-wrap items-center gap-3 mb-4">
          <Link
            to={backUrl}
            className="inline-flex items-center gap-1 px-2 py-1 text-sm text-[#323130] hover:bg-[#F3F2F1] rounded-sm transition-colors"
          >
            <i className="mgc_arrow_left_line" aria-hidden />
            Back
          </Link>
          <div>
            <h1 className="text-[24px] font-semibold text-[#323130] dark:text-[#f3f2f1] mb-0 leading-tight">
              {pageTitle}
            </h1>
            {subtitle && (
              <p className="text-sm text-[#605E5C] dark:text-[#a19f9d] mt-0.5 mb-0">{subtitle}</p>
            )}
          </div>
        </div>
      )}
      <div className={`card ${widthClass} mx-auto ${isCrm ? "crm-entity-form" : ""}`}>
        {!isCrm && (
          <div className="card-header border-b border-gray-100 dark:border-gray-700">
            <div className="flex flex-wrap items-center gap-3">
              <Link to={backUrl} className="btn btn-sm btn-soft-secondary">
                <i className="mgc_arrow_left_line me-1" aria-hidden />
                Back
              </Link>
              <div>
                <h4 className="card-title mb-0">{pageTitle}</h4>
                {subtitle && (
                  <p className="text-sm text-gray-500 dark:text-gray-400 mt-0.5 mb-0">{subtitle}</p>
                )}
              </div>
            </div>
          </div>
        )}
        <div className="card-body">
          {error && (
            <div className="alert alert-danger mb-5" role="alert">
              {error}
            </div>
          )}
          {children}
        </div>
      </div>
    </>
  );
};

export default EntityFormLayout;
