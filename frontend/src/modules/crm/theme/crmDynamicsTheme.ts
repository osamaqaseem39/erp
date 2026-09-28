/** Microsoft Dynamics 365 Sales / Fluent UI palette */
export const D365 = {
  blue: "#0078D4",
  blueDark: "#005A9E",
  blueLight: "#2B88D8",
  blueMuted: "#C7E0F4",
  teal: "#038387",
  green: "#107C10",
  orange: "#CA5010",
  purple: "#5C2D91",
  magenta: "#881798",
  bg: "#F3F2F1",
  bgAlt: "#FAF9F8",
  surface: "#FFFFFF",
  border: "#EDEBE9",
  borderStrong: "#C8C6C4",
  text: "#323130",
  textSecondary: "#605E5C",
  textMuted: "#8A8886",
  hover: "#F3F2F1",
  selected: "#EFF6FC",
  navBg: "#FFFFFF",
  navActiveBar: "#0078D4",
} as const;

export const D365_CHART_COLORS = [
  D365.blue,
  D365.teal,
  D365.purple,
  D365.orange,
  D365.green,
  D365.magenta,
  D365.blueLight,
  "#004578",
];

export const d365Card =
  "bg-white dark:bg-[#1f1f1f] border border-[#EDEBE9] dark:border-[#3b3a39] rounded-sm shadow-none";

export const d365CardHeader =
  "flex items-center justify-between gap-2 px-4 py-3 border-b border-[#EDEBE9] dark:border-[#3b3a39]";

export const d365Title = "text-[15px] font-semibold text-[#323130] dark:text-[#f3f2f1] mb-0";

export const d365Subtitle = "text-xs text-[#605E5C] dark:text-[#a19f9d] mb-0";

export const isCrmPath = (pathname: string) => pathname.startsWith("/crm");
