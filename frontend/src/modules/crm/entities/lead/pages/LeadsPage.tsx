import React, { useRef, useState } from "react";
import { useSearchParams } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { D365 } from "../../../theme/crmDynamicsTheme";
import { LeadsViewBar } from "../components/LeadsViewBar";
import { LeadQuickCreatePanel } from "../components/LeadQuickCreatePanel";
import LeadHub from "./LeadHub";
import LeadList, { type LeadListHandle } from "./LeadList";

const LeadsPage: React.FC = () => {
  const [searchParams, setSearchParams] = useSearchParams();
  const view = searchParams.get("view") === "list" ? "list" : "hub";
  const [quickCreateOpen, setQuickCreateOpen] = useState(false);
  const [selectionCount, setSelectionCount] = useState(0);
  const listRef = useRef<LeadListHandle>(null);

  const handleCreated = async (leadId: string) => {
    if (view === "list") {
      await listRef.current?.reload();
      return;
    }
    const next = new URLSearchParams(searchParams);
    next.delete("view");
    next.set("id", leadId);
    setSearchParams(next);
  };

  return (
    <div
      className="crm-dynamics -mx-4 -mt-2 min-h-[calc(100vh-120px)] font-['Segoe_UI','Segoe_UI_Web',system-ui,sans-serif]"
      style={{ backgroundColor: D365.bg }}
    >
      <PageBreadcrumb title="Sales | Leads" name="CRM" breadCrumbItems={["CRM", "Leads"]} />

      <div className="mx-1 bg-white dark:bg-[#1f1f1f] border border-[#EDEBE9] dark:border-[#3b3a39] rounded-sm shadow-none">
        <LeadsViewBar
          view={view}
          onNew={() => setQuickCreateOpen(true)}
          onDelete={() => void listRef.current?.deleteSelected()}
          deleteDisabled={view !== "list" || selectionCount === 0}
        />
        {view === "list" ? (
          <LeadList ref={listRef} onSelectionChange={setSelectionCount} />
        ) : (
          <LeadHub onQuickCreate={() => setQuickCreateOpen(true)} />
        )}
      </div>

      <LeadQuickCreatePanel
        open={quickCreateOpen}
        onClose={() => setQuickCreateOpen(false)}
        onCreated={handleCreated}
      />
    </div>
  );
};

export default LeadsPage;
