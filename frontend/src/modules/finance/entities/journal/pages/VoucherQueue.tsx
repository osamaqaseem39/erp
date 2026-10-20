import React, { useEffect, useState } from "react";
import { Link, useLocation } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { journalService } from "../services";
import type { JournalDto } from "../types";

type Mode = "approve" | "closed" | "posted";

/** Approve = Draft; Closed = Approved; Posted = GL posted (unpost). */
const VoucherQueue: React.FC = () => {
  const location = useLocation();
  const mode: Mode = location.pathname.includes("/posted")
    ? "posted"
    : location.pathname.includes("/closed")
      ? "closed"
      : "approve";
  const status = mode === "approve" ? "Draft" : mode === "closed" ? "Approved" : "Posted";
  const title =
    mode === "approve" ? "Approve Voucher" : mode === "closed" ? "Closed Vouchers" : "Posted Vouchers";

  const [journals, setJournals] = useState<JournalDto[]>([]);
  const [selected, setSelected] = useState<JournalDto | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  const load = async () => {
    setError(null);
    try {
      const data = await journalService.getAll({ status, pageSize: 100 });
      setJournals(data.filter((j) => !j.journalType || true));
      setSelected(null);
    } catch (err: any) {
      setError(err.response?.data?.Error || err.message || "Failed to load");
    }
  };

  useEffect(() => {
    load();
  }, [status]);

  const handleApprove = async () => {
    if (!selected?.journalId && !selected?.id) return;
    if (!window.confirm("Are you sure you want to approve this voucher?")) return;
    setBusy(true);
    try {
      await journalService.approve((selected.journalId || selected.id)!);
      await load();
    } catch (err: any) {
      setError(err.response?.data?.error || err.response?.data?.Error || err.message || "Approve failed");
    } finally {
      setBusy(false);
    }
  };

  const handlePost = async () => {
    if (!selected?.journalId && !selected?.id) return;
    if (!window.confirm("Post this approved voucher to the general ledger?")) return;
    setBusy(true);
    try {
      await journalService.post((selected.journalId || selected.id)!);
      await load();
    } catch (err: any) {
      setError(err.response?.data?.Error || err.message || "Post failed");
    } finally {
      setBusy(false);
    }
  };

  const handleUnpost = async () => {
    if (!selected?.journalId && !selected?.id) return;
    if (!window.confirm("Unpost this voucher back to Draft? It will leave the ledger.")) return;
    setBusy(true);
    try {
      await journalService.unpost((selected.journalId || selected.id)!);
      await load();
    } catch (err: any) {
      setError(err.response?.data?.error || err.response?.data?.Error || err.message || "Unpost failed");
    } finally {
      setBusy(false);
    }
  };

  return (
    <>
      <PageBreadcrumb title={title} subName="Finance" />
      <div className="card">
        <div className="card-header flex justify-between items-center">
          <h4 className="card-title">{title}</h4>
          <div className="flex gap-2 flex-wrap">
            <Link to="/finance/vouchers/cash-payment" className="btn btn-soft-primary">New CPV</Link>
            <Link to="/finance/vouchers/approve" className="btn btn-soft-secondary">Approve</Link>
            <Link to="/finance/vouchers/closed" className="btn btn-soft-secondary">Closed</Link>
            <Link to="/finance/vouchers/posted" className="btn btn-soft-secondary">Posted</Link>
          </div>
        </div>
        <div className="card-body">
          {error && <div className="alert alert-danger">{error}</div>}
          <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
            <div className="overflow-x-auto">
              <table className="table">
                <thead>
                  <tr>
                    <th>Number</th>
                    <th>Type</th>
                    <th>Date</th>
                    <th>Debit</th>
                    <th>Status</th>
                  </tr>
                </thead>
                <tbody>
                  {journals.map((j) => {
                    const id = j.journalId || j.id;
                    const active = (selected?.journalId || selected?.id) === id;
                    return (
                      <tr
                        key={id}
                        className={active ? "bg-slate-100 cursor-pointer" : "cursor-pointer"}
                        onClick={async () => {
                          try {
                            const full = await journalService.getById(id!);
                            setSelected(full);
                          } catch {
                            setSelected(j);
                          }
                        }}
                      >
                        <td>{j.journalNumber || id?.slice(0, 8)}</td>
                        <td>{j.journalType}</td>
                        <td>{j.journalDate?.slice?.(0, 10) || j.journalDate}</td>
                        <td>{j.totalDebit?.toFixed?.(2) ?? j.totalDebit}</td>
                        <td>{j.status}</td>
                      </tr>
                    );
                  })}
                  {journals.length === 0 && (
                    <tr><td colSpan={5} className="text-center text-muted">No vouchers in this queue.</td></tr>
                  )}
                </tbody>
              </table>
            </div>
            <div className="border rounded p-3">
              <h5 className="mb-2">Details</h5>
              {!selected ? (
                <p className="text-muted">Select a voucher to view details and actions.</p>
              ) : (
                <>
                  <p><strong>No:</strong> {selected.journalNumber}</p>
                  <p><strong>Type:</strong> {selected.journalType}</p>
                  <p><strong>Date:</strong> {selected.journalDate?.toString?.().slice(0, 10)}</p>
                  <p><strong>Remarks:</strong> {selected.description || "—"}</p>
                  <p><strong>Paid by:</strong> {selected.paidBy || "—"} / <strong>Received by:</strong> {selected.receivedBy || "—"}</p>
                  <div className="overflow-x-auto my-3">
                    <table className="table table-sm">
                      <thead>
                        <tr><th>Account</th><th>Debit</th><th>Credit</th><th>Narration</th></tr>
                      </thead>
                      <tbody>
                        {(selected.journalEntries || []).map((e) => (
                          <tr key={e.entryId}>
                            <td>{e.accountCode || e.accountId?.slice?.(0, 8)}</td>
                            <td>{e.debit}</td>
                            <td>{e.credit}</td>
                            <td>{e.narration || "—"}</td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                  {mode === "approve" && selected.status === "Draft" && (
                    <button type="button" className="btn btn-success" disabled={busy} onClick={handleApprove}>
                      Approve
                    </button>
                  )}
                  {mode === "closed" && selected.status === "Approved" && (
                    <button type="button" className="btn btn-primary" disabled={busy} onClick={handlePost}>
                      Post to GL
                    </button>
                  )}
                  {mode === "posted" && selected.status === "Posted" && (
                    <button type="button" className="btn btn-warning" disabled={busy} onClick={handleUnpost}>
                      Unpost
                    </button>
                  )}
                </>
              )}
            </div>
          </div>
        </div>
      </div>
    </>
  );
};

export default VoucherQueue;
