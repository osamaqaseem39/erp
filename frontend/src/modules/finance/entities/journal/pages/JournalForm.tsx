import React, { useEffect, useState } from "react";
import { useNavigate, useParams, Link } from "react-router-dom";
import { PageBreadcrumb } from "../../../../../components";
import { VerticalForm, FormInput } from "../../../../../components";
import { journalService, type CreateJournalDto, type UpdateJournalDto, type JournalDto, type JournalEntryDto, type CreateJournalEntryDto } from "../services";
import { chartOfAccountService } from "../../chart-of-account/services";
import { currencyService } from "../../currency/services";
import { JOURNAL_TYPE_OPTIONS, JOURNAL_STATUS_OPTIONS } from "../types";
import * as yup from "yup";
import { yupResolver } from "@hookform/resolvers/yup";

const JournalForm: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const isEdit = !!id;

  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [journal, setJournal] = useState<JournalDto | null>(null);
  const [entries, setEntries] = useState<CreateJournalEntryDto[]>([]);
  const [accounts, setAccounts] = useState<any[]>([]);
  const [currencies, setCurrencies] = useState<any[]>([]);

  useEffect(() => {
    loadAccounts();
    loadCurrencies();
    if (isEdit && id) {
      loadJournal(id);
    } else {
      // Initialize with two empty entries (debit and credit)
      setEntries([
        {
          journalId: "",
          accountId: "",
          accountCode: "",
          accountName: "",
          debit: 0,
          credit: 0,
          narration: "",
        },
        {
          journalId: "",
          accountId: "",
          accountCode: "",
          accountName: "",
          debit: 0,
          credit: 0,
          narration: "",
        },
      ]);
    }
  }, [isEdit, id]);

  const loadJournal = async (journalId: string) => {
    try {
      const journalData = await journalService.getById(journalId);
      setJournal(journalData);
      if (journalData.journalEntries && journalData.journalEntries.length > 0) {
        setEntries(journalData.journalEntries.map(entry => ({
          journalId: entry.journalId,
          accountId: entry.accountId,
          accountCode: entry.accountCode,
          accountName: entry.accountName,
          costCenterId: entry.costCenterId,
          projectId: entry.projectId,
          debit: entry.debit,
          credit: entry.credit,
          narration: entry.narration || "",
        })));
      }
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to load journal");
    }
  };

  const loadAccounts = async () => {
    try {
      const data = await chartOfAccountService.getAll({ isActive: true });
      setAccounts(data);
    } catch (err: any) {
      console.error("Failed to load accounts", err);
    }
  };

  const loadCurrencies = async () => {
    try {
      const data = await currencyService.getActive();
      setCurrencies(data);
    } catch (err: any) {
      console.error("Failed to load currencies", err);
    }
  };

  const addEntry = () => {
    setEntries([...entries, {
      journalId: id || "",
      accountId: "",
      accountCode: "",
      accountName: "",
      debit: 0,
      credit: 0,
      narration: "",
    }]);
  };

  const removeEntry = (index: number) => {
    const newEntries = entries.filter((_, i) => i !== index);
    setEntries(newEntries);
  };

  const updateEntry = (index: number, field: string, value: any) => {
    const newEntries = [...entries];
    (newEntries[index] as any)[field] = value;
    
    // If account is selected, update accountCode and accountName
    if (field === "accountId" && value) {
      const account = accounts.find(acc => acc.accountId === value || acc.id === value);
      if (account) {
        newEntries[index].accountCode = account.accountCode || "";
        newEntries[index].accountName = account.accountName || "";
      }
    }
    
    // Ensure numeric fields are numbers
    if (field === "debit" || field === "credit") {
      (newEntries[index] as any)[field] = parseFloat(value) || 0;
      // Ensure only one of debit or credit has value
      if (field === "debit" && parseFloat(value) > 0) {
        newEntries[index].credit = 0;
      } else if (field === "credit" && parseFloat(value) > 0) {
        newEntries[index].debit = 0;
      }
    }
    
    setEntries(newEntries);
  };

  const calculateTotals = () => {
    const totalDebit = entries.reduce((sum, entry) => sum + (entry.debit || 0), 0);
    const totalCredit = entries.reduce((sum, entry) => sum + (entry.credit || 0), 0);
    return { totalDebit, totalCredit, isBalanced: Math.abs(totalDebit - totalCredit) < 0.01 };
  };

  const journalSchema = yup.object({
    journalType: yup.string().required("Journal type is required"),
    referenceNo: yup.string(),
    journalDate: yup.string().required("Journal date is required"),
    description: yup.string(),
    currencyCode: yup.string(),
    exchangeRate: yup.number().min(0, "Exchange rate must be >= 0"),
    status: yup.string(),
  });

  const onSubmit = async (data: any) => {
    setError(null);
    
    // Validate entries
    if (entries.length < 2) {
      setError("At least two entries are required");
      return;
    }

    const totals = calculateTotals();
    if (!totals.isBalanced) {
      setError(`Journal entries are not balanced. Total Debit: ${totals.totalDebit.toFixed(2)}, Total Credit: ${totals.totalCredit.toFixed(2)}`);
      return;
    }

    setLoading(true);
    try {
      const entriesData = entries.map(entry => ({
        ...entry,
        journalId: id || "",
      }));

      if (isEdit && id) {
        const updateData: UpdateJournalDto = {
          journalId: id,
          journalType: data.journalType,
          referenceNo: data.referenceNo || undefined,
          journalDate: data.journalDate,
          description: data.description || undefined,
          currencyCode: data.currencyCode || undefined,
          exchangeRate: data.exchangeRate ? parseFloat(data.exchangeRate) : undefined,
          status: data.status || "Draft",
        };
        await journalService.update(id, updateData);
        // TODO: Update entries separately if needed
      } else {
        const createData: CreateJournalDto = {
          journalType: data.journalType,
          referenceNo: data.referenceNo || undefined,
          journalDate: data.journalDate,
          description: data.description || undefined,
          currencyCode: data.currencyCode || undefined,
          exchangeRate: data.exchangeRate ? parseFloat(data.exchangeRate) : undefined,
          status: data.status || "Draft",
          journalEntries: entriesData,
        };
        await journalService.create(createData);
      }
      navigate("/finance/journals");
    } catch (err: any) {
      setError(err.response?.data?.Error || err.response?.data?.message || "Failed to save journal");
    } finally {
      setLoading(false);
    }
  };

  const totals = calculateTotals();
  const defaultValues = isEdit && journal
    ? {
        journalType: journal.journalType || "",
        referenceNo: journal.referenceNo || "",
        journalDate: journal.journalDate ? journal.journalDate.split('T')[0] : new Date().toISOString().split('T')[0],
        description: journal.description || "",
        currencyCode: journal.currencyCode || "",
        exchangeRate: journal.exchangeRate || 1,
        status: journal.status || "Draft",
      }
    : {
        journalType: "",
        referenceNo: "",
        journalDate: new Date().toISOString().split('T')[0],
        description: "",
        currencyCode: "",
        exchangeRate: 1,
        status: "Draft",
      };

  return (
    <>
      <PageBreadcrumb title={isEdit ? "Edit Journal" : "Add Journal"} subName="Finance" />
      <div className="card">
        <div className="card-header">
          <div className="flex items-center gap-2">
            <Link to="/finance/journals" className="btn btn-sm btn-soft-secondary">
              <i className="mgc_arrow_left_line me-2"></i>Back
            </Link>
          </div>
        </div>
        <div className="card-body">
          {error && (
            <div className={`alert ${error.includes("balanced") ? "alert-warning" : "alert-danger"}`} role="alert">
              {error}
            </div>
          )}

          <VerticalForm
            onSubmit={onSubmit}
            resolver={yupResolver(journalSchema)}
            defaultValues={defaultValues}
          >
            {/* Journal Header */}
            <div className="mb-6">
              <h5 className="text-lg font-semibold mb-4">Journal Information</h5>
              <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
                <FormInput
                  label="Journal Type"
                  type="select"
                  name="journalType"
                  containerClass="mb-4"
                  className="form-select"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                  required
                >
                  <option value="">Select journal type</option>
                  {JOURNAL_TYPE_OPTIONS.map((option) => (
                    <option key={option.value} value={option.value}>
                      {option.label}
                    </option>
                  ))}
                </FormInput>

                <FormInput
                  label="Journal Date"
                  type="date"
                  name="journalDate"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                  required
                />

                <FormInput
                  label="Reference No"
                  type="text"
                  name="referenceNo"
                  placeholder="Enter reference number"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                />

                <FormInput
                  label="Status"
                  type="select"
                  name="status"
                  containerClass="mb-4"
                  className="form-select"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                >
                  {JOURNAL_STATUS_OPTIONS.map((option) => (
                    <option key={option.value} value={option.value}>
                      {option.label}
                    </option>
                  ))}
                </FormInput>

                <FormInput
                  label="Currency"
                  type="select"
                  name="currencyCode"
                  containerClass="mb-4"
                  className="form-select"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                >
                  <option value="">Select currency</option>
                  {currencies.map((curr) => (
                    <option key={curr.id} value={curr.code}>
                      {curr.code} - {curr.name}
                    </option>
                  ))}
                </FormInput>

                <FormInput
                  label="Exchange Rate"
                  type="number"
                  name="exchangeRate"
                  placeholder="1.00"
                  containerClass="mb-4"
                  className="form-input"
                  labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                  step="0.0001"
                  min="0"
                />

                <div className="md:col-span-2">
                  <FormInput
                    label="Description"
                    type="textarea"
                    name="description"
                    rows={3}
                    placeholder="Enter journal description"
                    containerClass="mb-4"
                    className="form-input"
                    labelClassName="block text-sm font-medium text-gray-600 dark:text-gray-200 mb-2"
                  />
                </div>
              </div>
            </div>

            {/* Journal Entries */}
            <div className="mb-6">
              <div className="flex items-center justify-between mb-4">
                <h5 className="text-lg font-semibold">Journal Entries</h5>
                <button
                  type="button"
                  onClick={addEntry}
                  className="btn btn-sm btn-primary"
                >
                  <i className="mgc_add_line me-2"></i>Add Entry
                </button>
              </div>

              <div className="table-responsive">
                <table className="table">
                  <thead>
                    <tr>
                      <th>Account</th>
                      <th>Debit</th>
                      <th>Credit</th>
                      <th>Narration</th>
                      <th>Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    {entries.map((entry, index) => (
                      <tr key={index}>
                        <td>
                          <select
                            className="form-select form-select-sm"
                            value={entry.accountId}
                            onChange={(e) => updateEntry(index, "accountId", e.target.value)}
                            required
                          >
                            <option value="">Select account</option>
                            {accounts.map((acc) => (
                              <option key={acc.accountId || acc.id} value={acc.accountId || acc.id}>
                                {acc.accountCode} - {acc.accountName}
                              </option>
                            ))}
                          </select>
                        </td>
                        <td>
                          <input
                            type="number"
                            className="form-control form-control-sm"
                            value={entry.debit || 0}
                            onChange={(e) => updateEntry(index, "debit", e.target.value)}
                            step="0.01"
                            min="0"
                            disabled={entry.credit > 0}
                          />
                        </td>
                        <td>
                          <input
                            type="number"
                            className="form-control form-control-sm"
                            value={entry.credit || 0}
                            onChange={(e) => updateEntry(index, "credit", e.target.value)}
                            step="0.01"
                            min="0"
                            disabled={entry.debit > 0}
                          />
                        </td>
                        <td>
                          <input
                            type="text"
                            className="form-control form-control-sm"
                            value={entry.narration || ""}
                            onChange={(e) => updateEntry(index, "narration", e.target.value)}
                            placeholder="Narration"
                          />
                        </td>
                        <td>
                          {entries.length > 2 && (
                            <button
                              type="button"
                              onClick={() => removeEntry(index)}
                              className="btn btn-sm btn-soft-danger"
                            >
                              <i className="mgc_delete_line"></i>
                            </button>
                          )}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                  <tfoot>
                    <tr className="font-bold">
                      <td>Total</td>
                      <td>{totals.totalDebit.toFixed(2)}</td>
                      <td>{totals.totalCredit.toFixed(2)}</td>
                      <td colSpan={2}>
                        {totals.isBalanced ? (
                          <span className="badge badge-pill badge-soft-success">Balanced</span>
                        ) : (
                          <span className="badge bg-danger">
                            Difference: {(totals.totalDebit - totals.totalCredit).toFixed(2)}
                          </span>
                        )}
                      </td>
                    </tr>
                  </tfoot>
                </table>
              </div>
            </div>

            <div className="flex justify-end gap-3 mt-6">
              <Link to="/finance/journals" className="btn btn-soft-secondary">
                Cancel
              </Link>
              <button type="submit" className="btn btn-primary" disabled={loading || !totals.isBalanced}>
                {loading ? "Saving…" : isEdit ? "Save changes" : "Add Journal"}
              </button>
            </div>
          </VerticalForm>
        </div>
      </div>
    </>
  );
};

export default JournalForm;

