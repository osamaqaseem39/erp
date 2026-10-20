import React, { useEffect, useState } from "react";
import { PageBreadcrumb } from "../../../../../components";
import accountDefaultService, { ACCOUNT_PURPOSES, type AccountDefaultDto } from "../services/accountDefaultService";
import { chartOfAccountService, type ChartOfAccountDto } from "../../chart-of-account/services";

const AccountDefaultsPage: React.FC = () => {
  const [defaults, setDefaults] = useState<Record<string, string>>({});
  const [accounts, setAccounts] = useState<ChartOfAccountDto[]>([]);
  const [message, setMessage] = useState<string | null>(null);

  useEffect(() => {
    (async () => {
      const [d, a] = await Promise.all([
        accountDefaultService.getAll(),
        chartOfAccountService.getAll({ postingOnly: true, isActive: true }),
      ]);
      setAccounts(a);
      const map: Record<string, string> = {};
      d.forEach((x: AccountDefaultDto) => {
        map[x.purpose] = x.accountId;
      });
      setDefaults(map);
    })().catch((err) => setMessage(err.message || "Load failed"));
  }, []);

  const save = async () => {
    try {
      const payload = ACCOUNT_PURPOSES.filter((p) => defaults[p]).map((purpose) => ({
        purpose,
        accountId: defaults[purpose],
        isActive: true,
      }));
      await accountDefaultService.upsert(payload);
      setMessage("Defaults saved.");
    } catch (err: any) {
      setMessage(err.response?.data?.Error || err.message || "Save failed");
    }
  };

  return (
    <>
      <PageBreadcrumb title="Default Accounts" subName="Finance" />
      <div className="card">
        <div className="card-header"><h4 className="card-title">System default posting accounts</h4></div>
        <div className="card-body">
          {message && <div className="alert alert-info">{message}</div>}
          <p className="text-sm text-muted mb-3">Map each purpose to a posting CoA account (e.g. Cash → Petty Cash).</p>
          <div className="space-y-3">
            {ACCOUNT_PURPOSES.map((purpose) => (
              <div key={purpose} className="grid grid-cols-1 gap-2 md:grid-cols-3 items-center">
                <label className="font-medium">{purpose}</label>
                <select
                  className="form-select md:col-span-2"
                  value={defaults[purpose] || ""}
                  onChange={(e) => setDefaults({ ...defaults, [purpose]: e.target.value })}
                >
                  <option value="">— not set —</option>
                  {accounts.map((a) => (
                    <option key={a.accountId} value={a.accountId}>
                      {a.accountCode} — {a.accountName}
                    </option>
                  ))}
                </select>
              </div>
            ))}
          </div>
          <button type="button" className="btn btn-primary mt-4" onClick={save}>Save defaults</button>
        </div>
      </div>
    </>
  );
};

export default AccountDefaultsPage;
