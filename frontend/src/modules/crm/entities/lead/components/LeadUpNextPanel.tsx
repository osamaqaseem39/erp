import React from "react";
import { Link } from "react-router-dom";
import type { ActivityDto } from "../../activity/types";
import { formatLeadDate } from "../leadHubUtils";
import { d365Card } from "../../../theme/crmDynamicsTheme";

interface LeadUpNextPanelProps {
  leadId: string;
  leadEmail?: string;
  activities: ActivityDto[];
  loading: boolean;
  onCompleteActivity?: (activityId: string) => void;
  actionLoading?: boolean;
}

export const LeadUpNextPanel: React.FC<LeadUpNextPanelProps> = ({
  leadId,
  leadEmail,
  activities,
  loading,
  onCompleteActivity,
  actionLoading = false,
}) => {
  const upcoming = [...activities]
    .filter((a) => a.status !== "Completed" && a.status !== "Cancelled")
    .sort((a, b) => new Date(a.startDate).getTime() - new Date(b.startDate).getTime())
    .slice(0, 5);

  return (
    <div className={d365Card}>
      <div className="px-4 py-3 border-b border-[#EDEBE9]">
        <h3 className="text-[15px] font-semibold text-[#323130] mb-0">Up next</h3>
        <p className="text-xs text-[#605E5C] mb-0 mt-0.5">New lead nurturing</p>
      </div>
      <div className="p-4">
        {loading ? (
          <p className="text-sm text-[#605E5C] mb-0">Loading activities…</p>
        ) : upcoming.length === 0 ? (
          <div className="text-center py-6">
            <p className="text-sm text-[#605E5C] mb-3">No upcoming steps</p>
            <Link
              to={`/crm/activities/new?leadId=${leadId}`}
              className="text-sm text-[#0078D4] hover:underline font-medium"
            >
              Schedule activity
            </Link>
          </div>
        ) : (
          <ol className="space-y-0 mb-0">
            {upcoming.map((act, idx) => {
              const isEmail =
                (act.activityType || "").toLowerCase().includes("email") ||
                (act.subject || "").toLowerCase().includes("mail");
              return (
                <li
                  key={act.id}
                  className="relative pl-8 pb-6 last:pb-0 border-l-2 border-[#EDEBE9] ml-3 last:border-transparent"
                >
                  <span className="absolute -left-[9px] top-0 flex h-4 w-4 items-center justify-center rounded-full bg-[#0078D4] text-[10px] text-white font-semibold">
                    {idx * 2 + 1}
                  </span>
                  <p className="text-sm font-semibold text-[#323130] mb-0.5">{act.subject}</p>
                  <p className="text-xs text-[#605E5C] mb-1">
                    Due by {formatLeadDate(act.startDate)} · {act.activityType || "Task"}
                  </p>
                  {act.description && (
                    <p className="text-xs text-[#605E5C] mb-2 line-clamp-2">{act.description}</p>
                  )}
                  <div className="flex flex-wrap gap-2">
                    {isEmail && leadEmail && (
                      <a
                        href={`mailto:${leadEmail}`}
                        className="inline-flex items-center gap-1 px-2.5 py-1 text-xs font-medium text-[#0078D4] border border-[#0078D4] rounded-sm hover:bg-[#EFF6FC]"
                      >
                        Email
                      </a>
                    )}
                    {onCompleteActivity && act.id ? (
                      <button
                        type="button"
                        disabled={actionLoading}
                        onClick={() => onCompleteActivity(act.id!)}
                        className="inline-flex items-center gap-1 px-2.5 py-1 text-xs font-medium text-[#323130] border border-[#8A8886] rounded-sm hover:bg-[#F3F2F1] disabled:opacity-50"
                      >
                        Mark complete
                      </button>
                    ) : (
                      <Link
                        to={`/crm/activities/${act.id}/edit`}
                        className="inline-flex items-center gap-1 px-2.5 py-1 text-xs font-medium text-[#323130] border border-[#8A8886] rounded-sm hover:bg-[#F3F2F1]"
                      >
                        Edit
                      </Link>
                    )}
                  </div>
                  {idx < upcoming.length - 1 && (
                    <p className="text-[11px] text-[#8A8886] mt-4 mb-0 italic">Wait for 5 days</p>
                  )}
                </li>
              );
            })}
          </ol>
        )}
      </div>
    </div>
  );
};

export default LeadUpNextPanel;
