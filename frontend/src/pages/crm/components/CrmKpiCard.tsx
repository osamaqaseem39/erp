import React from "react";
import { Link } from "react-router-dom";
import Tippy from "@tippyjs/react";
import "tippy.js/dist/tippy.css";
import { CrmSparkline } from "./CrmDashboardCharts";

const ACCENTS = {
  primary: { bar: "#0078D4", spark: "#0078D4", icon: "#0078D4" },
  success: { bar: "#107C10", spark: "#107C10", icon: "#107C10" },
  info: { bar: "#038387", spark: "#038387", icon: "#038387" },
  warning: { bar: "#CA5010", spark: "#CA5010", icon: "#CA5010" },
  danger: { bar: "#D13438", spark: "#D13438", icon: "#D13438" },
  secondary: { bar: "#5C2D91", spark: "#5C2D91", icon: "#5C2D91" },
} as const;

export type KpiVariant = keyof typeof ACCENTS;

interface CrmKpiCardProps {
  label: string;
  value: string | number;
  sublabel?: string;
  icon: string;
  variant: KpiVariant;
  link: string;
  sparkData: number[];
  tooltip?: string;
}

export const CrmKpiCard: React.FC<CrmKpiCardProps> = ({
  label,
  value,
  sublabel,
  icon,
  variant,
  link,
  sparkData,
  tooltip,
}) => {
  const accent = ACCENTS[variant];

  return (
    <Tippy content={tooltip ?? `View ${label}`} animation="shift-away" theme="translucent">
      <Link
        to={link}
        className="group block bg-white dark:bg-[#1f1f1f] border border-[#EDEBE9] dark:border-[#3b3a39] rounded-sm hover:border-[#C8C6C4] dark:hover:border-[#605E5C] hover:shadow-[0_1.6px_3.6px_rgba(0,0,0,0.13),0_0.3px_0.9px_rgba(0,0,0,0.11)] transition-shadow duration-150 overflow-hidden"
      >
        <div className="h-[3px] w-full" style={{ backgroundColor: accent.bar }} />
        <div className="px-4 py-3.5">
          <div className="flex items-start justify-between gap-3">
            <div className="min-w-0">
              <p className="text-[11px] font-normal text-[#605E5C] dark:text-[#a19f9d] mb-0.5 leading-tight">
                {label}
              </p>
              <p className="text-[28px] font-semibold text-[#323130] dark:text-[#f3f2f1] leading-none tracking-tight mb-0">
                {value}
              </p>
              {sublabel && (
                <p className="text-[11px] text-[#0078D4] dark:text-[#4ba0e8] mt-1 mb-0">{sublabel}</p>
              )}
            </div>
            <span
              className="flex h-9 w-9 shrink-0 items-center justify-center rounded-sm bg-[#EFF6FC] dark:bg-[#243a5e]"
              style={{ color: accent.icon }}
            >
              <i className={`${icon} text-lg`} />
            </span>
          </div>
          <div className="mt-2 h-10 opacity-80 group-hover:opacity-100 transition-opacity">
            <CrmSparkline data={sparkData} color={accent.spark} />
          </div>
        </div>
      </Link>
    </Tippy>
  );
};
