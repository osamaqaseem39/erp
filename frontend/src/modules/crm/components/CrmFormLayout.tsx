import React from "react";
import { Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../components";
import { D365 } from "../theme/crmDynamicsTheme";
import { addLabel, editLabel } from "../../../components/entity/labels";

interface CrmFormLayoutProps {
  breadcrumbTitle: string;
  title: string;
  subtitle?: string;
  backUrl: string;
  fetching?: boolean;
  error?: string | null;
  children: React.ReactNode;
  maxWidth?: "default" | "wide";
  showBreadcrumb?: boolean;
}

function parseEntityFromTitles(breadcrumbTitle: string, title: string): { entityName: string; isEdit: boolean } {
  const isEdit = breadcrumbTitle.startsWith("Edit");
  const source = breadcrumbTitle.replace(/^(Edit|Add|Create New|Create)\s+/i, "").trim() || title;
  const entityName = source.replace(/^(Edit|Add|Create New|Create)\s+/i, "").trim();
  return { entityName: entityName || "Record", isEdit };
}

/** @deprecated Legacy layout kept until its module is migrated. Use PageHeader + FormSection + FormFooter from components/erp. */
const CrmFormLayout: React.FC<CrmFormLayoutProps> = ({
  breadcrumbTitle,
  title,
  subtitle,
  backUrl,
  fetching = false,
  error,
  children,
  maxWidth = "default",
  showBreadcrumb = true,
}) => {
  const { entityName, isEdit } = parseEntityFromTitles(breadcrumbTitle, title);
  const pageTitle = isEdit ? editLabel(entityName) : addLabel(entityName);
  const widthClass = maxWidth === "wide" ? "max-w-6xl" : "max-w-5xl";

  if (fetching) {
    return (
      <div
        className="crm-dynamics crm-entity-form-page -mx-4 -mt-2 px-4 pb-6 min-h-[320px]"
        style={{ backgroundColor: D365.bg }}
      >
        {showBreadcrumb && (
          <PageBreadcrumb title={pageTitle} name="CRM" breadCrumbItems={["CRM", pageTitle]} />
        )}
        <div className={`${widthClass} mx-auto bg-white border border-[#EDEBE9] rounded-sm p-16 flex justify-center`}>
          <div
            className="h-8 w-8 border-2 border-[#0078D4] border-t-transparent rounded-full animate-spin"
            role="status"
          />
        </div>
      </div>
    );
  }

  return (
    <div
      className="crm-dynamics crm-entity-form-page -mx-4 -mt-2 px-4 pb-8 font-['Segoe_UI','Segoe_UI_Web',system-ui,sans-serif]"
      style={{ backgroundColor: D365.bg }}
    >
      {showBreadcrumb && (
        <PageBreadcrumb title={pageTitle} name="CRM" breadCrumbItems={["CRM", pageTitle]} />
      )}

      <div className={`${widthClass} mx-auto`}>
        <div className="flex flex-wrap items-start gap-3 mb-4">
          <Link
            to={backUrl}
            className="inline-flex items-center gap-1 px-2 py-1.5 text-sm font-medium text-[#323130] hover:bg-[#F3F2F1] rounded-sm transition-colors mt-1"
          >
            <i className="mgc_arrow_left_line text-base" aria-hidden />
            Back
          </Link>
          <div className="min-w-0 flex-1">
            <h1 className="text-[24px] font-semibold text-[#323130] dark:text-[#f3f2f1] mb-0 leading-tight">
              {pageTitle}
            </h1>
            {subtitle && (
              <p className="text-sm text-[#605E5C] dark:text-[#a19f9d] mt-1 mb-0">{subtitle}</p>
            )}
          </div>
        </div>

        <div className="bg-white dark:bg-[#1f1f1f] border border-[#EDEBE9] dark:border-[#3b3a39] rounded-sm shadow-none crm-entity-form">
          {error && (
            <div
              className="mx-4 mt-4 px-4 py-3 rounded-sm border border-[#F1BBBC] bg-[#FDE7E9] text-[#A4262C] text-sm"
              role="alert"
            >
              {error}
            </div>
          )}
          <div className="p-4">{children}</div>
        </div>
      </div>
    </div>
  );
};

export default CrmFormLayout;
