import React from "react";
import { Link } from "react-router-dom";
import { addSubmitLabel, saveChangesLabel } from "./labels";

interface EntityFormFooterProps {
  cancelUrl: string;
  loading: boolean;
  isEdit: boolean;
  entityName: string;
  createLabel?: string;
  updateLabel?: string;
}

const EntityFormFooter: React.FC<EntityFormFooterProps> = ({
  cancelUrl,
  loading,
  isEdit,
  entityName,
  createLabel,
  updateLabel = saveChangesLabel,
}) => (
  <div className="flex flex-wrap items-center justify-end gap-2 pt-4 mt-2 border-t border-gray-100 dark:border-gray-700">
    <Link to={cancelUrl} className="btn btn-soft-secondary">
      Cancel
    </Link>
    <button type="submit" className="btn btn-primary min-w-[140px]" disabled={loading}>
      {loading ? (
        <>
          <span className="spinner-border spinner-border-sm me-2" role="status" aria-hidden />
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

export default EntityFormFooter;
