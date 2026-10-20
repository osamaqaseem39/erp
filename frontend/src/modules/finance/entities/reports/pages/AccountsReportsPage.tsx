import React, { useEffect, useMemo, useState } from "react";
import { PageBreadcrumb } from "../../../../../components";
import financeReportService from "../services/financeReportService";
import { chartOfAccountService, type ChartOfAccountDto } from "../../chart-of-account/services";

type ReportKey =
  | "trial-balance"
  | "running-trial-balance"
  | "profit-loss"
  | "balance-sheet"
  | "general-ledger"
  | "vouchers"
  | "chart-of-accounts"
  | "daily-financial"
  | "cash-bank-vouchers"
  | "cash-bank-ledger"
  | "voucher-type-monthly"
  | "partner-balances"
  | "aged-receivables"
  | "aged-payables"
  | "cash-flow"
  | "budget-vs-actual"
  | "cheque-register"
  | "stock-average-rate";

const REPORTS: { key: ReportKey; label: string }[] = [
  { key: "trial-balance", label: "Trial Balance" },
  { key: "running-trial-balance", label: "Running Trial Balance" },
  { key: "profit-loss", label: "Income Statement" },
  { key: "balance-sheet", label: "Balance Sheet" },
  { key: "cash-flow", label: "Cash Flow" },
  { key: "budget-vs-actual", label: "Budget vs Actual" },
  { key: "cheque-register", label: "Cheque Register" },
  { key: "general-ledger", label: "Accounts Ledger" },
  { key: "vouchers", label: "Voucher Report" },
  { key: "chart-of-accounts", label: "Chart of Accounts" },
  { key: "daily-financial", label: "Daily Financial" },
  { key: "cash-bank-vouchers", label: "Cash/Bank Payment/Receipt Summary" },
  { key: "cash-bank-ledger", label: "Cash/Bank Ledger Summary" },
  { key: "voucher-type-monthly", label: "Voucher Type Monthly Summary" },
  { key: "partner-balances", label: "Customer / Supplier Summary" },
  { key: "aged-receivables", label: "Aged Receivables" },
  { key: "aged-payables", label: "Aged Payables" },
  { key: "stock-average-rate", label: "Item Stock Average Rate (Inventory)" },
];

function today() {
  return new Date().toISOString().slice(0, 10);
}

function monthStart() {
  const d = new Date();
  return new Date(d.getFullYear(), d.getMonth(), 1).toISOString().slice(0, 10);
}

function downloadJson(filename: string, data: unknown) {
  const blob = new Blob([JSON.stringify(data, null, 2)], { type: "application/json" });
  const url = URL.createObjectURL(blob);
  const a = document.createElement("a");
  a.href = url;
  a.download = filename;
  a.click();
  URL.revokeObjectURL(url);
}

function toCsv(rows: Record<string, unknown>[]): string {
  if (!rows.length) return "";
  const keys = Object.keys(rows[0]);
  const esc = (v: unknown) => `"${String(v ?? "").replace(/"/g, '""')}"`;
  return [keys.join(","), ...rows.map((r) => keys.map((k) => esc(r[k])).join(","))].join("\n");
}

function downloadCsv(filename: string, rows: Record<string, unknown>[]) {
  const blob = new Blob([toCsv(rows)], { type: "text/csv" });
  const url = URL.createObjectURL(blob);
  const a = document.createElement("a");
  a.href = url;
  a.download = filename;
  a.click();
  URL.revokeObjectURL(url);
}

const AccountsReportsPage: React.FC = () => {
  const [report, setReport] = useState<ReportKey>("trial-balance");
  const [fromDate, setFromDate] = useState(monthStart());
  const [toDate, setToDate] = useState(today());
  const [asOfDate, setAsOfDate] = useState(today());
  const [accountId, setAccountId] = useState("");
  const [itemId, setItemId] = useState("");
  const [journalType, setJournalType] = useState("");
  const [status, setStatus] = useState("");
  const [includeDetail, setIncludeDetail] = useState(true);
  const [accounts, setAccounts] = useState<ChartOfAccountDto[]>([]);
  const [data, setData] = useState<any>(null);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    chartOfAccountService.getAll({ postingOnly: true, isActive: true }).then(setAccounts).catch(() => undefined);
  }, []);

  const run = async () => {
    setLoading(true);
    setError(null);
    try {
      let result: any;
      switch (report) {
        case "trial-balance":
          result = await financeReportService.trialBalance(fromDate, toDate);
          break;
        case "running-trial-balance":
          result = await financeReportService.runningTrialBalance(fromDate, toDate);
          break;
        case "profit-loss":
          result = await financeReportService.profitLoss(fromDate, toDate);
          break;
        case "balance-sheet":
          result = await financeReportService.balanceSheet(asOfDate);
          break;
        case "general-ledger":
          if (!accountId) throw new Error("Select an account for the ledger.");
          result = await financeReportService.generalLedger(accountId, fromDate, toDate, true);
          break;
        case "vouchers":
          result = await financeReportService.vouchers(fromDate, toDate, journalType || undefined, status || undefined);
          break;
        case "chart-of-accounts":
          result = await financeReportService.chartOfAccounts(includeDetail);
          break;
        case "daily-financial":
          result = await financeReportService.dailyFinancial(asOfDate);
          break;
        case "cash-bank-vouchers":
          result = await financeReportService.cashBankVouchers(fromDate, toDate);
          break;
        case "cash-bank-ledger":
          result = await financeReportService.cashBankLedger(fromDate, toDate);
          break;
        case "voucher-type-monthly":
          result = await financeReportService.voucherTypeMonthly(fromDate, toDate);
          break;
        case "partner-balances":
          result = await financeReportService.partnerBalances(asOfDate);
          break;
        case "aged-receivables":
          result = await financeReportService.agedReceivables(asOfDate);
          break;
        case "aged-payables":
          result = await financeReportService.agedPayables(asOfDate);
          break;
        case "cash-flow":
          result = await financeReportService.cashFlow(fromDate, toDate);
          break;
        case "budget-vs-actual":
          result = await financeReportService.budgetVsActual(fromDate, toDate);
          break;
        case "cheque-register":
          result = await financeReportService.chequeRegister(
            fromDate,
            toDate,
            status || undefined,
            journalType || undefined
          );
          break;
        case "stock-average-rate":
          if (!itemId.trim()) throw new Error("Enter an inventory Item Id.");
          result = await financeReportService.stockAverageRate(itemId.trim(), undefined, asOfDate);
          break;
      }
      setData(result);
    } catch (err: any) {
      setData(null);
      setError(err.response?.data?.Error || err.message || "Report failed");
    } finally {
      setLoading(false);
    }
  };

  const csvRows = useMemo(() => {
    if (!data) return [];
    if (Array.isArray(data.lines)) return data.lines;
    if (Array.isArray(data.entries)) return data.entries;
    if (Array.isArray(data.revenue) || Array.isArray(data.expenses)) {
      return [
        ...(data.revenue || []).map((r: any) => ({ section: "Revenue", ...r })),
        ...(data.expenses || []).map((r: any) => ({ section: "Expense", ...r })),
      ];
    }
    if (data.assets || data.liabilities || data.equity) {
      return [
        ...(data.assets?.lines || []).map((r: any) => ({ section: "Assets", ...r })),
        ...(data.liabilities?.lines || []).map((r: any) => ({ section: "Liabilities", ...r })),
        ...(data.equity?.lines || []).map((r: any) => ({ section: "Equity", ...r })),
      ];
    }
    if (Array.isArray(data.customers) || Array.isArray(data.suppliers)) {
      return [
        ...(data.customers || []).map((r: any) => ({ section: "Customer", ...r })),
        ...(data.suppliers || []).map((r: any) => ({ section: "Supplier", ...r })),
      ];
    }
    if (Array.isArray(data.byType)) return data.byType;
    if (Array.isArray(data.buckets)) return data.buckets;
    if (Array.isArray(data.inflows) || Array.isArray(data.outflows)) {
      return [
        ...(data.inflows || []).map((r: any) => ({ section: "Inflow", ...r })),
        ...(data.outflows || []).map((r: any) => ({ section: "Outflow", ...r })),
      ];
    }
    return [];
  }, [data]);

  return (
    <>
      <PageBreadcrumb title="Accounts Reports" subName="Finance" />
      <div className="card mb-3">
        <div className="card-body">
          <div className="grid grid-cols-1 gap-3 md:grid-cols-4">
            <div>
              <label className="form-label">Report</label>
              <select className="form-select" value={report} onChange={(e) => setReport(e.target.value as ReportKey)}>
                {REPORTS.map((r) => (
                  <option key={r.key} value={r.key}>{r.label}</option>
                ))}
              </select>
            </div>
            {["balance-sheet", "daily-financial", "partner-balances", "aged-receivables", "aged-payables", "stock-average-rate"].includes(report) ? (
              <div>
                <label className="form-label">{report === "daily-financial" ? "Date" : "As of"}</label>
                <input type="date" className="form-input" value={asOfDate} onChange={(e) => setAsOfDate(e.target.value)} />
              </div>
            ) : report !== "chart-of-accounts" ? (
              <>
                <div>
                  <label className="form-label">From</label>
                  <input type="date" className="form-input" value={fromDate} onChange={(e) => setFromDate(e.target.value)} />
                </div>
                <div>
                  <label className="form-label">To</label>
                  <input type="date" className="form-input" value={toDate} onChange={(e) => setToDate(e.target.value)} />
                </div>
              </>
            ) : null}
            {report === "stock-average-rate" && (
              <div>
                <label className="form-label">Item Id</label>
                <input
                  type="text"
                  className="form-input"
                  placeholder="Inventory item GUID"
                  value={itemId}
                  onChange={(e) => setItemId(e.target.value)}
                />
              </div>
            )}
            {report === "general-ledger" && (
              <div>
                <label className="form-label">Account</label>
                <select className="form-select" value={accountId} onChange={(e) => setAccountId(e.target.value)}>
                  <option value="">Select posting account</option>
                  {accounts.map((a) => (
                    <option key={a.accountId} value={a.accountId}>{a.accountCode} — {a.accountName}</option>
                  ))}
                </select>
              </div>
            )}
            {(report === "vouchers" || report === "cheque-register") && (
              <>
                <div>
                  <label className="form-label">Type</label>
                  <select className="form-select" value={journalType} onChange={(e) => setJournalType(e.target.value)}>
                    <option value="">All</option>
                    <option value="CashPayment">Cash Payment</option>
                    <option value="CashReceipt">Cash Receipt</option>
                    <option value="BankPayment">Bank Payment</option>
                    <option value="BankReceipt">Bank Receipt</option>
                    {report === "vouchers" && <option value="Journal">Journal</option>}
                  </select>
                </div>
                <div>
                  <label className="form-label">{report === "cheque-register" ? "Cheque status" : "Status"}</label>
                  <select className="form-select" value={status} onChange={(e) => setStatus(e.target.value)}>
                    <option value="">All</option>
                    {report === "cheque-register" ? (
                      <>
                        <option value="Outstanding">Outstanding</option>
                        <option value="Cleared">Cleared</option>
                        <option value="Bounced">Bounced</option>
                        <option value="Cancelled">Cancelled</option>
                      </>
                    ) : (
                      <>
                        <option value="Draft">Draft</option>
                        <option value="Approved">Approved</option>
                        <option value="Posted">Posted</option>
                      </>
                    )}
                  </select>
                </div>
              </>
            )}
            {report === "chart-of-accounts" && (
              <div className="flex items-end">
                <label className="inline-flex items-center gap-2">
                  <input type="checkbox" checked={includeDetail} onChange={(e) => setIncludeDetail(e.target.checked)} />
                  Include detail / posting accounts
                </label>
              </div>
            )}
          </div>
          <div className="mt-3 flex gap-2 flex-wrap">
            <button type="button" className="btn btn-primary" disabled={loading} onClick={run}>
              {loading ? "Running…" : "Run report"}
            </button>
            {data && (
              <>
                <button type="button" className="btn btn-soft-secondary" onClick={() => downloadJson(`${report}.json`, data)}>
                  Export JSON
                </button>
                {csvRows.length > 0 && (
                  <button type="button" className="btn btn-soft-secondary" onClick={() => downloadCsv(`${report}.csv`, csvRows)}>
                    Export CSV
                  </button>
                )}
              </>
            )}
          </div>
        </div>
      </div>

      <div className="card">
        <div className="card-body">
          {error && <div className="alert alert-danger">{error}</div>}
          {!data && !error && <p className="text-muted">Choose a report and click Run.</p>}
          {data && report === "trial-balance" && (
            <>
              <p className="mb-2">Totals: Dr {data.totalDebit} / Cr {data.totalCredit}</p>
              <Table
                columns={["accountCode", "accountName", "accountType", "level", "debit", "credit", "balance"]}
                rows={data.lines || []}
              />
            </>
          )}
          {data && report === "running-trial-balance" && (
            <Table
              columns={["accountCode", "accountName", "openingBalance", "periodDebit", "periodCredit", "closingBalance"]}
              rows={data.lines || []}
            />
          )}
          {data && report === "profit-loss" && (
            <>
              <h5>Revenue ({data.totalRevenue})</h5>
              <Table columns={["accountCode", "accountName", "amount"]} rows={data.revenue || []} />
              <h5 className="mt-3">Expenses ({data.totalExpenses})</h5>
              <Table columns={["accountCode", "accountName", "amount"]} rows={data.expenses || []} />
              <p className="mt-3 font-semibold">Net income: {data.netIncome}</p>
            </>
          )}
          {data && report === "balance-sheet" && (
            <>
              <h5>Assets ({data.totalAssets})</h5>
              <Table columns={["accountCode", "accountName", "balance"]} rows={data.assets?.lines || []} />
              <h5 className="mt-3">Liabilities ({data.liabilities?.total})</h5>
              <Table columns={["accountCode", "accountName", "balance"]} rows={data.liabilities?.lines || []} />
              <h5 className="mt-3">Equity ({data.equity?.total})</h5>
              <Table columns={["accountCode", "accountName", "balance"]} rows={data.equity?.lines || []} />
              <p className="mt-3">Total L+E: {data.totalLiabilitiesAndEquity} · NI plug: {data.netIncomePlug}</p>
            </>
          )}
          {data && report === "general-ledger" && (
            <>
              <p>
                {data.accountCode} {data.accountName} — Opening {data.openingBalance} → Closing {data.closingBalance}
              </p>
              <Table
                columns={["entryDate", "journalNumber", "journalType", "debit", "credit", "narration", "runningBalance"]}
                rows={data.entries || []}
              />
            </>
          )}
          {data && report === "vouchers" && (
            <>
              <p>Count {data.count} · Dr {data.totalDebit} / Cr {data.totalCredit}</p>
              <Table
                columns={["journalNumber", "journalType", "journalDate", "status", "description", "totalDebit", "totalCredit"]}
                rows={data.lines || []}
              />
            </>
          )}
          {data && report === "chart-of-accounts" && (
            <CoaTree nodes={data.roots || []} />
          )}
          {data && report === "daily-financial" && (
            <>
              <p>
                Vouchers {data.voucherCount} · CPV {data.cashPayments} · CRV {data.cashReceipts} · BPV {data.bankPayments} · BRV {data.bankReceipts}
              </p>
              <Table columns={["journalType", "count", "totalDebit", "totalCredit"]} rows={data.byType || []} />
            </>
          )}
          {data && (report === "cash-bank-vouchers" || report === "voucher-type-monthly") && (
            <Table
              columns={report === "voucher-type-monthly"
                ? ["period", "journalType", "count", "totalDebit", "totalCredit"]
                : ["journalType", "count", "totalAmount", "totalDebit", "totalCredit"]}
              rows={data.lines || []}
            />
          )}
          {data && report === "cash-bank-ledger" && (
            <Table
              columns={["accountCode", "accountName", "openingBalance", "periodDebit", "periodCredit", "closingBalance"]}
              rows={data.lines || []}
            />
          )}
          {data && report === "partner-balances" && (
            <>
              <h5>Customers (AR {data.totalReceivables})</h5>
              <Table columns={["partnerId", "openDocuments", "totalInvoiced", "balance"]} rows={data.customers || []} />
              <h5 className="mt-3">Suppliers (AP {data.totalPayables})</h5>
              <Table columns={["partnerId", "openDocuments", "totalInvoiced", "balance"]} rows={data.suppliers || []} />
            </>
          )}
          {data && (report === "aged-receivables" || report === "aged-payables") && (
            <>
              <p>Total {data.totalBalance}</p>
              <Table columns={["label", "fromDays", "toDays", "count", "amount"]} rows={data.buckets || []} />
              <h5 className="mt-3">Documents</h5>
              <Table
                columns={["documentNumber", "partnerId", "dueDate", "daysOverdue", "bucket", "balance"]}
                rows={data.lines || []}
              />
            </>
          )}
          {data && report === "cash-flow" && (
            <>
              <p className="mb-2">
                Opening {data.openingCash} · In {data.totalInflows} · Out {data.totalOutflows} · Net {data.netChange} · Closing {data.closingCash}
              </p>
              <h5>Inflows</h5>
              <Table columns={["journalType", "count", "amount"]} rows={data.inflows || []} />
              <h5 className="mt-3">Outflows</h5>
              <Table columns={["journalType", "count", "amount"]} rows={data.outflows || []} />
            </>
          )}
          {data && report === "budget-vs-actual" && (
            <>
              <p className="mb-2">
                Budget {data.totalBudget} · Actual {data.totalActual} · Variance {data.totalVariance}
              </p>
              <Table
                columns={[
                  "budgetName",
                  "accountCode",
                  "accountName",
                  "budgetAmount",
                  "actualAmount",
                  "variance",
                  "variancePercent",
                  "status",
                ]}
                rows={data.lines || []}
              />
            </>
          )}
          {data && report === "stock-average-rate" && (
            <>
              <p className="mb-2">
                {data.itemCode} — {data.itemName} · Qty {data.quantityOnHand} · Avg rate {data.averageRate} · Value {data.totalValue}
                {data.costPriceUpdated ? " · Cost price updated" : ""}
              </p>
              <Table
                columns={["itemId", "itemCode", "quantityOnHand", "averageRate", "totalValue", "previousCostPrice", "ledgerEntryCount"]}
                rows={[data]}
              />
            </>
          )}
        </div>
      </div>
    </>
  );
};

function cell(row: any, key: string) {
  if (row[key] !== undefined && row[key] !== null) return row[key];
  const pascal = key.charAt(0).toUpperCase() + key.slice(1);
  if (row[pascal] !== undefined && row[pascal] !== null) return row[pascal];
  return "—";
}

const Table: React.FC<{ columns: string[]; rows: any[] }> = ({ columns, rows }) => (
  <div className="overflow-x-auto">
    <table className="table">
      <thead>
        <tr>
          {columns.map((c) => (
            <th key={c}>{c}</th>
          ))}
        </tr>
      </thead>
      <tbody>
        {rows.map((r, i) => (
          <tr key={i}>
            {columns.map((c) => (
              <td key={c}>{cell(r, c)}</td>
            ))}
          </tr>
        ))}
        {rows.length === 0 && (
          <tr><td colSpan={columns.length} className="text-center text-muted">No rows</td></tr>
        )}
      </tbody>
    </table>
  </div>
);

const CoaTree: React.FC<{ nodes: any[]; depth?: number }> = ({ nodes, depth = 0 }) => (
  <ul className={depth === 0 ? "list-none pl-0" : "list-none pl-4"}>
    {nodes.map((n) => (
      <li key={n.accountId} className="py-0.5">
        <span style={{ marginLeft: depth * 8 }}>
          {n.accountCode} — {n.accountName}
          <span className="text-muted text-sm"> (L{n.level}{n.isPostingAllowed ? ", posting" : ""})</span>
        </span>
        {n.children?.length > 0 && <CoaTree nodes={n.children} depth={depth + 1} />}
      </li>
    ))}
  </ul>
);

export default AccountsReportsPage;
