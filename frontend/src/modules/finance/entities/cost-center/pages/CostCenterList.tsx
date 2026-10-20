import React, { useEffect, useState } from "react";
import { PageBreadcrumb } from "../../../../../components";
import costCenterService, { type CostCenterDto } from "../services/costCenterService";

const CostCenterList: React.FC = () => {
  const [items, setItems] = useState<CostCenterDto[]>([]);
  const [form, setForm] = useState({ costCenterCode: "", costCenterName: "", description: "" });
  const [error, setError] = useState<string | null>(null);

  const load = async () => {
    try {
      setItems(await costCenterService.getAll());
    } catch (err: any) {
      setError(err.response?.data?.Error || err.message || "Failed to load");
    }
  };

  useEffect(() => {
    load();
  }, []);

  const onCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await costCenterService.create({ ...form, isActive: true });
      setForm({ costCenterCode: "", costCenterName: "", description: "" });
      load();
    } catch (err: any) {
      alert(err.response?.data?.error || err.message || "Create failed");
    }
  };

  return (
    <>
      <PageBreadcrumb title="Cost Centers" subName="Finance" />
      <div className="card">
        <div className="card-header"><h4 className="card-title">Cost Centers</h4></div>
        <div className="card-body">
          {error && <div className="alert alert-danger">{error}</div>}
          <form onSubmit={onCreate} className="mb-4 grid grid-cols-1 gap-3 md:grid-cols-4">
            <input className="form-input" placeholder="Code" required value={form.costCenterCode} onChange={(e) => setForm({ ...form, costCenterCode: e.target.value })} />
            <input className="form-input" placeholder="Name" required value={form.costCenterName} onChange={(e) => setForm({ ...form, costCenterName: e.target.value })} />
            <input className="form-input" placeholder="Description" value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} />
            <button type="submit" className="btn btn-primary">Add</button>
          </form>
          <table className="table">
            <thead>
              <tr><th>Code</th><th>Name</th><th>Description</th><th>Active</th></tr>
            </thead>
            <tbody>
              {items.map((c) => (
                <tr key={c.costCenterId || c.id}>
                  <td>{c.costCenterCode}</td>
                  <td>{c.costCenterName}</td>
                  <td>{c.description || "—"}</td>
                  <td>{c.isActive ? "Yes" : "No"}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </>
  );
};

export default CostCenterList;
