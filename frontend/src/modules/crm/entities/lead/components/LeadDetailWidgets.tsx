import React from "react";
import type { LeadDto } from "../types";
import { d365Card } from "../../../theme/crmDynamicsTheme";

interface LeadDetailWidgetsProps {
  lead: LeadDto;
}

export const LeadDetailWidgets: React.FC<LeadDetailWidgetsProps> = ({ lead }) => (
  <div className="space-y-4">
    <Widget title="Who knows whom">
      <p className="text-sm text-[#605E5C] mb-0 text-center py-4">No connections found</p>
    </Widget>

    <Widget title="Assistant">
      <p className="text-sm text-[#605E5C] mb-0 text-center py-4">No notifications or suggestions</p>
    </Widget>

    <Widget title="Relationship health">
      {lead.leadScore != null && lead.leadScore > 0 ? (
        <div className="py-2 text-center">
          <p className="text-2xl font-semibold text-[#0078D4] mb-0">{lead.leadScore}</p>
          <p className="text-xs text-[#605E5C] mb-0">Lead score out of 100</p>
        </div>
      ) : (
        <p className="text-sm text-[#605E5C] mb-0 text-center py-4">
          Add activities and complete nurturing steps to build relationship health.
        </p>
      )}
    </Widget>
  </div>
);

const Widget: React.FC<{ title: string; children: React.ReactNode }> = ({ title, children }) => (
  <div className={d365Card}>
    <div className="px-4 py-3 border-b border-[#EDEBE9]">
      <h3 className="text-[15px] font-semibold text-[#323130] mb-0">{title}</h3>
    </div>
    <div className="p-4">{children}</div>
  </div>
);

export default LeadDetailWidgets;
