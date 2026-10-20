import React, { useEffect, useState } from "react";
import { useNavigate, Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { journalService } from "../services";
import { chartOfAccountService, type ChartOfAccountDto } from "../../chart-of-account/services";
import costCenterService, { type CostCenterDto } from "../../cost-center/services/costCenterService";
import accountDefaultService from "../../account-default/services/accountDefaultService";
import type { CreateCashPaymentLineDto } from "../types";

export type VoucherKind = "CashPayment" | "CashReceipt" | "BankPayment" | "BankReceipt";

const META: Record<
  VoucherKind,
  {
    title: string;
    accountLabel: string;
    defaultPurpose: "Cash" | "BankClearing";
    offsetLabel: string;
    isPayment: boolean;
    showCheque: boolean;
  }
> = {
  CashPayment: {
    title: "Cash Payment Voucher (CPV)",
    accountLabel: "Cash account",
    defaultPurpose: "Cash",
    offsetLabel: "Debit lines (expense / personal)",
    isPayment: true,
    showCheque: false,
  },
  CashReceipt: {
    title: "Cash Receipt Voucher (CRV)",
    accountLabel: "Cash account",
    defaultPurpose: "Cash",
    offsetLabel: "Credit lines (income / receivable)",
    isPayment: false,
    showCheque: false,
  },
  BankPayment: {
    title: "Bank Payment Voucher (BPV)",
    accountLabel: "Bank account (CoA)",
    defaultPurpose: "BankClearing",
    offsetLabel: "Debit lines (expense / payable)",
    isPayment: true,
    showCheque: true,
  },
  BankReceipt: {
    title: "Bank Receipt Voucher (BRV)",
    accountLabel: "Bank account (CoA)",
    defaultPurpose: "BankClearing",
    offsetLabel: "Credit lines (income / receivable)",
    isPayment: false,
    showCheque: true,
  },
};

interface Props {
  kind: VoucherKind;
}

const TypedVoucherForm: React.FC<Props> = ({ kind }) => {
  const meta = META[kind];
  const navigate = useNavigate();
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [headerAccounts, setHeaderAccounts] = useState<ChartOfAccountDto[]>([]);
  const [postingAccounts, setPostingAccounts] = useState<ChartOfAccountDto[]>([]);
  const [costCenters, setCostCenters] = useState<CostCenterDto[]>([]);
  const [cashOrBankAccountId, setCashOrBankAccountId] = useState("");
  const [journalDate, setJournalDate] = useState(new Date().toISOString().slice(0, 10));
  const [referenceNo, setReferenceNo] = useState("");
  const [chequeNumber, setChequeNumber] = useState("");
  const [paidBy, setPaidBy] = useState("");
  const [receivedBy, setReceivedBy] = useState("");
  const [description, setDescription] = useState("");
  const [lines, setLines] = useState<CreateCashPaymentLineDto[]>([
    { accountId: "", amount: 0, narration: "", costCenterId: "" },
  ]);

  useEffect(() => {
    (async () => {
      const [posting, centers, defaults] = await Promise.all([
        chartOfAccountService.getAll({ postingOnly: true, isActive: true }),
        costCenterService.getAll(true),
        accountDefaultService.getAll(),
      ]);
      setPostingAccounts(posting);
      setHeaderAccounts(posting.filter((a) => a.accountType === "Asset"));
      setCostCenters(centers);
      const def = defaults.find((d) => d.purpose === meta.defaultPurpose);
      if (def?.accountId) setCashOrBankAccountId(def.accountId);
    })().catch((err) => setError(err.message || "Failed to load masters"));
  }, [meta.defaultPurpose]);

  const total = lines.reduce((s, l) => s + (Number(l.amount) || 0), 0);

  const onSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    if (!cashOrBankAccountId) {
      setError(`Select the ${meta.accountLabel.toLowerCase()}.`);
      return;
    }
    const offsetLines = lines.filter((l) => l.accountId && Number(l.amount) > 0);
    if (offsetLines.length === 0) {
      setError("Add at least one offset line.");
      return;
    }
    setLoading(true);
    try {
      const payload: Record<string, unknown> = {
        journalType: kind,
        journalDate,
        referenceNo: referenceNo || undefined,
        description: description || undefined,
        paidBy: paidBy || undefined,
        receivedBy: receivedBy || undefined,
        chequeNumber: chequeNumber || undefined,
        cashOrBankAccountId,
        offsetLines: offsetLines.map((l) => ({
          accountId: l.accountId,
          amount: Number(l.amount),
          narration: l.narration,
          costCenterId: l.costCenterId || undefined,
        })),
      };
      if (meta.isPayment) {
        payload.debitLines = payload.offsetLines;
      } else {
        payload.creditLines = payload.offsetLines;
      }
      await journalService.create(payload as any);
      navigate("/finance/vouchers/approve");
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.error || err.message || "Save failed");
    } finally {
      setLoading(false);
    }
  };

  return (
    <>
      <PageBreadcrumb title={meta.title} subName="Finance" />
      <div className="card">
        <div className="card-header flex justify-between">
          <h4 className="card-title">{meta.title}</h4>
          <Link to="/finance/vouchers/approve" className="btn btn-soft-secondary">
            Approve queue
          </Link>
        </div>
        <div className="card-body">
          {error && <div className="alert alert-danger">{error}</div>}
          <form onSubmit={onSubmit}>
            <div className="grid grid-cols-1 gap-3 md:grid-cols-3 mb-4">
              <div>
                <label className="form-label">{meta.accountLabel}</label>
                <select
                  className="form-select"
                  required
                  value={cashOrBankAccountId}
                  onChange={(e) => setCashOrBankAccountId(e.target.value)}
                >
                  <option value="">Select…</option>
                  {headerAccounts.map((a) => (
                    <option key={a.accountId} value={a.accountId}>
                      {a.accountCode} — {a.accountName}
                    </option>
                  ))}
                </select>
              </div>
              <div>
                <label className="form-label">Voucher date</label>
                <input
                  type="date"
                  className="form-input"
                  required
                  value={journalDate}
                  onChange={(e) => setJournalDate(e.target.value)}
                />
              </div>
              <div>
                <label className="form-label">Reference no</label>
                <input className="form-input" value={referenceNo} onChange={(e) => setReferenceNo(e.target.value)} />
              </div>
              {meta.showCheque && (
                <div>
                  <label className="form-label">Cheque number</label>
                  <input className="form-input" value={chequeNumber} onChange={(e) => setChequeNumber(e.target.value)} />
                </div>
              )}
              <div>
                <label className="form-label">Paid by</label>
                <input className="form-input" value={paidBy} onChange={(e) => setPaidBy(e.target.value)} />
              </div>
              <div>
                <label className="form-label">Received by</label>
                <input className="form-input" value={receivedBy} onChange={(e) => setReceivedBy(e.target.value)} />
              </div>
              <div>
                <label className="form-label">{meta.isPayment ? "Credit (auto)" : "Debit (auto)"}</label>
                <input className="form-input" readOnly value={total.toFixed(2)} />
              </div>
              <div className="md:col-span-3">
                <label className="form-label">Remarks</label>
                <input
                  className="form-input"
                  value={description}
                  onChange={(e) => setDescription(e.target.value)}
                />
              </div>
            </div>

            <h5 className="mb-2">{meta.offsetLabel}</h5>
            {lines.map((line, idx) => (
              <div key={idx} className="grid grid-cols-1 gap-2 md:grid-cols-12 mb-2 items-end">
                <div className="md:col-span-4">
                  <label className="form-label">Account</label>
                  <select
                    className="form-select"
                    value={line.accountId}
                    onChange={(e) => {
                      const next = [...lines];
                      next[idx] = { ...next[idx], accountId: e.target.value };
                      setLines(next);
                    }}
                  >
                    <option value="">Select…</option>
                    {postingAccounts.map((a) => (
                      <option key={a.accountId} value={a.accountId}>
                        {a.accountCode} — {a.accountName}
                      </option>
                    ))}
                  </select>
                </div>
                <div className="md:col-span-2">
                  <label className="form-label">Amount</label>
                  <input
                    type="number"
                    step="0.01"
                    className="form-input"
                    value={line.amount || ""}
                    onChange={(e) => {
                      const next = [...lines];
                      next[idx] = { ...next[idx], amount: parseFloat(e.target.value) || 0 };
                      setLines(next);
                    }}
                  />
                </div>
                <div className="md:col-span-3">
                  <label className="form-label">Cost center</label>
                  <select
                    className="form-select"
                    value={line.costCenterId || ""}
                    onChange={(e) => {
                      const next = [...lines];
                      next[idx] = { ...next[idx], costCenterId: e.target.value };
                      setLines(next);
                    }}
                  >
                    <option value="">Optional</option>
                    {costCenters.map((c) => (
                      <option key={c.costCenterId || c.id} value={c.costCenterId || c.id}>
                        {c.costCenterCode} — {c.costCenterName}
                      </option>
                    ))}
                  </select>
                </div>
                <div className="md:col-span-3">
                  <label className="form-label">Narration</label>
                  <input
                    className="form-input"
                    value={line.narration || ""}
                    onChange={(e) => {
                      const next = [...lines];
                      next[idx] = { ...next[idx], narration: e.target.value };
                      setLines(next);
                    }}
                  />
                </div>
              </div>
            ))}
            <button
              type="button"
              className="btn btn-soft-secondary mb-3"
              onClick={() => setLines([...lines, { accountId: "", amount: 0, narration: "", costCenterId: "" }])}
            >
              Add line
            </button>
            <div>
              <button type="submit" className="btn btn-primary" disabled={loading}>
                {loading ? "Saving…" : "Save voucher (Draft)"}
              </button>
            </div>
          </form>
        </div>
      </div>
    </>
  );
};

export default TypedVoucherForm;
