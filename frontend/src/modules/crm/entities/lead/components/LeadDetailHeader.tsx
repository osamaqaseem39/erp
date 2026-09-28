import React from "react";
import type { LeadDto } from "../types";
import {
  leadDisplayName,
  leadInitials,
  ownerInitials,
  priorityToRating,
} from "../leadHubUtils";

const StatusPill: React.FC<{ status?: string }> = ({ status }) => (
  <span className="inline-flex px-2 py-0.5 rounded-sm text-[11px] font-medium bg-[#EFF6FC] text-[#0078D4]">
    {status || "New"}
  </span>
);

interface LeadDetailHeaderProps {
  lead: LeadDto;
  saved?: boolean;
}

export const LeadDetailHeader: React.FC<LeadDetailHeaderProps> = ({ lead, saved }) => {
  const owner = lead.assignedToUserName || lead.assignedToName || "Unassigned";

  return (
    <div className="px-4 py-3 border-b border-[#EDEBE9] dark:border-[#3b3a39] bg-white dark:bg-[#1f1f1f]">
      <div className="flex flex-wrap items-start justify-between gap-4">
        <div className="flex items-start gap-3 min-w-0">
          <span className="flex h-11 w-11 shrink-0 items-center justify-center rounded-full bg-[#5C2D91] text-white text-sm font-semibold">
            {leadInitials(lead)}
          </span>
          <div className="min-w-0">
            <div className="flex items-center gap-2 flex-wrap mb-0.5">
              <h2 className="text-xl font-semibold text-[#323130] dark:text-[#f3f2f1] mb-0 truncate">
                {leadDisplayName(lead)}
              </h2>
              {saved && (
                <span className="inline-flex items-center gap-1 px-2 py-0.5 text-[11px] font-medium text-[#107C10] bg-[#DFF6DD] rounded-sm">
                  <i className="mgc_check_line text-xs" />
                  Saved
                </span>
              )}
            </div>
            <p className="text-xs text-[#8A8886] mb-0">Lead - {lead.leadCode}</p>
          </div>
        </div>

        <div className="flex flex-wrap items-start gap-6 text-sm">
          <Metric label="Lead Source" value={lead.leadSource || lead.source || "-"} />
          <Metric label="Rating" value={priorityToRating(lead.priority)} />
          <div>
            <p className="text-[11px] text-[#8A8886] mb-1 uppercase tracking-wide">Status</p>
            <StatusPill status={lead.status} />
          </div>
          <div>
            <p className="text-[11px] text-[#8A8886] mb-1 uppercase tracking-wide">Owner</p>
            <div className="flex items-center gap-2">
              <span className="flex h-7 w-7 items-center justify-center rounded-full bg-[#0078D4] text-[10px] font-semibold text-white">
                {ownerInitials(owner === "Unassigned" ? undefined : owner)}
              </span>
              <span className="text-[#323130] dark:text-[#f3f2f1] font-medium text-sm max-w-[160px] truncate">
                {owner}
              </span>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

const Metric: React.FC<{ label: string; value: string }> = ({ label, value }) => (
  <div>
    <p className="text-[11px] text-[#8A8886] mb-0.5 uppercase tracking-wide">{label}</p>
    <p className="text-[#323130] dark:text-[#f3f2f1] font-medium mb-0">{value}</p>
  </div>
);

export default LeadDetailHeader;
