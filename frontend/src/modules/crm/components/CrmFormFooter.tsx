import React from "react";
import { Link } from "react-router-dom";
import { addSubmitLabel, saveChangesLabel } from "../../../components/entity/labels";

interface CrmFormFooterProps {
  cancelUrl: string;
  loading: boolean;
  isEdit: boolean;
  entityName?: string;
  createLabel?: string;
  updateLabel?: string;
}

/** @deprecated Legacy layout kept until its module is migrated. Use FormFooter from components/erp. */
const cmdBtnSecondary =
  "inline-flex items-center justify-center px-4 py-1.5 text-sm font-medium text-[#323130] bg-white border border-[#8A8886] hover:bg-[#F3F2F1] rounded-sm transition-colors disabled:opacity-50";

const cmdBtnPrimary =
  "inline-flex items-center justify-center min-w-[120px] px-4 py-1.5 text-sm font-medium text-white bg-[#0078D4] hover:bg-[#106EBE] rounded-sm transition-colors disabled:opacity-50";

const CrmFormFooter: React.FC<CrmFormFooterProps> = ({
  cancelUrl,
  loading,
  isEdit,
  entityName = "Record",
  createLabel,
  updateLabel = saveChangesLabel,
}) => (
  <div className="crm-form-footer flex flex-wrap items-center justify-end gap-2 pt-4 mt-2 border-t border-[#EDEBE9] dark:border-[#3b3a39] -mx-4 -mb-4 px-4 py-3 bg-[#FAF9F8] dark:bg-[#292827]">
    <Link to={cancelUrl} className={cmdBtnSecondary}>
      Cancel
    </Link>
    <button type="submit" className={cmdBtnPrimary} disabled={loading}>
      {loading ? (
        <>
          <span
            className="inline-block h-4 w-4 border-2 border-white border-t-transparent rounded-full animate-spin me-2"
            role="status"
            aria-hidden
          />
          Saving…
        </>
      ) : isEdit ? (
        updateLabel
      ) : (
        createLabel ?? addSubmitLabel(entityName)
      )}
    </button>
  </div>
);

export default CrmFormFooter;
