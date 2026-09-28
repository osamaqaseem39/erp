import React from "react";
import {
  daysInStage,
  getActiveBpfStageIndex,
  getBpfPriorStageSummary,
  getBpfStageFields,
  LEAD_BPF_STAGES,
} from "../leadHubUtils";
import type { LeadDto } from "../types";

interface LeadBusinessProcessFlowProps {
  lead: LeadDto;
}

const StageFields: React.FC<{ fields: { label: string; value: string }[]; compact?: boolean }> = ({
  fields,
  compact,
}) => {
  if (fields.length === 0) return null;
  return (
    <ul className={`mb-0 mt-1.5 space-y-0.5 ${compact ? "max-h-[52px] overflow-hidden" : ""}`}>
      {fields.map((f) => (
        <li key={`${f.label}-${f.value}`} className="text-[11px] text-[#605E5C] truncate" title={`${f.label}: ${f.value}`}>
          <span className="text-[#8A8886]">{f.label}:</span> {f.value}
        </li>
      ))}
    </ul>
  );
};

export const LeadBusinessProcessFlow: React.FC<LeadBusinessProcessFlowProps> = ({ lead }) => {
  const activeIndex = getActiveBpfStageIndex(lead.status);
  const days = daysInStage(lead);
  const priorSummary = getBpfPriorStageSummary(lead, activeIndex);

  return (
    <div className="px-4 py-2 border-b border-[#EDEBE9] dark:border-[#3b3a39] bg-[#FAF9F8] dark:bg-[#292827]">
      <p className="text-[11px] text-[#605E5C] mb-2 font-medium">Lead to Opportunity Sales Process</p>
      <div className="flex items-stretch gap-0 overflow-x-auto">
        {LEAD_BPF_STAGES.map((stage, index) => {
          const isActive = index === activeIndex;
          const isPast = index < activeIndex;
          const isFuture = index > activeIndex;
          const stageFields = !isFuture ? getBpfStageFields(lead, index) : [];

          return (
            <div
              key={stage.id}
              className={`relative flex-1 min-w-[140px] px-4 py-2.5 border-r border-[#EDEBE9] dark:border-[#3b3a39] last:border-r-0 ${
                isActive ? "bg-[#EFF6FC]" : isPast ? "bg-[#FAF9F8]" : "bg-white dark:bg-[#1f1f1f]"
              }`}
            >
              <div className="flex items-start gap-2">
                <span
                  className={`mt-0.5 flex h-6 w-6 shrink-0 items-center justify-center rounded-full text-[11px] font-semibold ${
                    isActive
                      ? "bg-[#0078D4] text-white"
                      : isPast
                        ? "bg-[#107C10] text-white"
                        : "bg-[#EDEBE9] text-[#8A8886]"
                  }`}
                >
                  {isFuture ? (
                    <i className="mgc_lock_line text-xs" />
                  ) : isPast ? (
                    <i className="mgc_check_line text-xs" />
                  ) : (
                    index + 1
                  )}
                </span>
                <div className="min-w-0 flex-1">
                  <p
                    className={`text-sm font-semibold mb-0 truncate ${
                      isActive ? "text-[#0078D4]" : isFuture ? "text-[#8A8886]" : "text-[#323130] dark:text-[#f3f2f1]"
                    }`}
                  >
                    {stage.label}
                  </p>
                  {isActive && (
                    <p className="text-[11px] text-[#605E5C] mb-0 mt-0.5">
                      Active for {days} {days === 1 ? "day" : "days"}
                    </p>
                  )}
                  {isPast && <StageFields fields={stageFields} compact />}
                  {isActive && (
                    <>
                      <StageFields fields={stageFields} />
                      {priorSummary.length > 0 && (
                        <p className="text-[10px] text-[#8A8886] mb-0 mt-1.5 uppercase tracking-wide">From prior steps</p>
                      )}
                      <StageFields fields={priorSummary} compact />
                    </>
                  )}
                </div>
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
};

export default LeadBusinessProcessFlow;
