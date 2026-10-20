import SettingsCategoryPage from "../../core/pages/SettingsCategoryPage";

/** CoA setup: decimal places, max depth, posting-leaf-only, cost center rules. */
export default function FinanceSetupPage() {
  return (
    <SettingsCategoryPage
      category="finance"
      title="Account Setup Configuration"
      description="Value decimals (1–8), chart of accounts max depth, posting leaf-only, and voucher rules."
    />
  );
}
