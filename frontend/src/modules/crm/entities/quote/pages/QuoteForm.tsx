import React, { useEffect, useState } from "react";
import { getCompanyId } from "../../../../core/hooks/useCompanyId";
import { useNavigate, useParams, useSearchParams } from "react-router-dom";
import { FormInput } from "../../../../../components";
import {
  CrmFormLayout,
  CrmFormFooter,
  FormSection,
  CRM_LABEL_CLASS,
  CRM_INPUT_CLASS,
  CRM_SELECT_CLASS,
  CRM_FIELD_WRAP,
  CRM_TEXTAREA_CLASS,
  optionalString,
  optionalUuid,
} from "../../../components";
import { quoteService } from "../services";
import type { CreateQuoteDto, UpdateQuoteDto, CreateQuoteLineDto } from "../types";
import { customerService } from "../../customer/services";
import { opportunityService } from "../../opportunity/services";
import { itemService } from "../../../../inventory/entities/item/services";
import { currencyService } from "../../../../finance/entities/currency/services";
import { userService } from "../../../../core/entities/user/services";
import { QUOTE_STATUS_OPTIONS } from "../types";
import * as yup from "yup";
import { yupResolver } from "@hookform/resolvers/yup";
import { FormProvider, useForm } from "react-hook-form";

const quoteSchema = yup.object({
  quoteNumber: optionalString(),
  customerId: optionalUuid(),
  opportunityId: optionalUuid(),
  quoteDate: optionalString(),
  validUntil: optionalString(),
  currency: optionalString(),
  status: optionalString(),
  notes: optionalString(),
  terms: optionalString(),
  assignedToUserId: optionalUuid(),
});

type QuoteFormValues = yup.InferType<typeof quoteSchema>;

const defaultValues: QuoteFormValues = {
  quoteNumber: "",
  customerId: "",
  opportunityId: "",
  quoteDate: new Date().toISOString().split("T")[0],
  validUntil: new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString().split("T")[0],
  currency: "USD",
  status: "Draft",
  notes: "",
  terms: "",
  assignedToUserId: "",
};

const QuoteForm: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const [searchParams] = useSearchParams();
  const opportunityIdParam = searchParams.get("opportunityId");
  const isEdit = !!id;

  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [fetching, setFetching] = useState(isEdit);
  const [lines, setLines] = useState<CreateQuoteLineDto[]>([]);
  const [customers, setCustomers] = useState<any[]>([]);
  const [opportunities, setOpportunities] = useState<any[]>([]);
  const [items, setItems] = useState<any[]>([]);
  const [currencies, setCurrencies] = useState<any[]>([]);
  const [users, setUsers] = useState<any[]>([]);

  const methods = useForm<QuoteFormValues>({
    defaultValues: {
      ...defaultValues,
      opportunityId: opportunityIdParam || "",
    },
    resolver: yupResolver(quoteSchema),
    mode: "onBlur",
  });

  const { handleSubmit, reset, setValue } = methods;

  useEffect(() => {
    void loadCustomers();
    void loadOpportunities();
    void loadItems();
    void loadCurrencies();
    void loadUsers();
    if (isEdit && id) {
      void loadQuote(id);
    } else {
      setLines([
        {
          itemId: "",
          description: "",
          quantity: 1,
          unitPrice: 0,
          discountPercent: 0,
          taxRate: 0,
        },
      ]);
      if (opportunityIdParam) {
        void prefillFromOpportunity(opportunityIdParam);
      }
    }
  }, [isEdit, id, opportunityIdParam]);

  const prefillFromOpportunity = async (opportunityId: string) => {
    try {
      const opp = await opportunityService.getById(opportunityId);
      setValue("opportunityId", opportunityId);
      if (opp.customerId) setValue("customerId", opp.customerId);
      if (opp.currency) setValue("currency", opp.currency);
      if (opp.assignedToUserId) setValue("assignedToUserId", opp.assignedToUserId);
      if (opp.notes || opp.description) setValue("notes", opp.notes || opp.description || "");
    } catch {
      setValue("opportunityId", opportunityId);
    }
  };

  const loadQuote = async (quoteId: string) => {
    setFetching(true);
    setError(null);
    try {
      const quoteData = await quoteService.getById(quoteId);
      reset({
        quoteNumber: quoteData.quoteNumber ?? "",
        customerId: quoteData.customerId ?? "",
        opportunityId: quoteData.opportunityId ?? "",
        quoteDate: quoteData.quoteDate
          ? quoteData.quoteDate.split("T")[0]
          : new Date().toISOString().split("T")[0],
        validUntil: quoteData.validUntil ? quoteData.validUntil.split("T")[0] : "",
        currency: quoteData.currency ?? "USD",
        status: quoteData.status ?? "Draft",
        notes: quoteData.notes ?? "",
        terms: quoteData.terms ?? "",
        assignedToUserId: quoteData.assignedToUserId ?? "",
      });
      if (quoteData.quoteLines && quoteData.quoteLines.length > 0) {
        setLines(
          quoteData.quoteLines.map((line) => ({
            itemId: line.itemId,
            description: line.description || "",
            quantity: line.quantity,
            unitPrice: line.unitPrice,
            discountPercent: line.discountPercent || 0,
            taxRate: line.taxRate || 0,
          }))
        );
      }
    } catch (err: unknown) {
      const e = err as { response?: { data?: { Error?: string; message?: string } } };
      setError(e.response?.data?.Error || e.response?.data?.message || "Failed to load quote");
    } finally {
      setFetching(false);
    }
  };

  const loadCustomers = async () => {
    try {
      const data = await customerService.getAll();
      setCustomers(data);
    } catch (err: unknown) {
      console.error("Failed to load customers", err);
    }
  };

  const loadOpportunities = async () => {
    try {
      const data = await opportunityService.getAll();
      setOpportunities(data);
    } catch (err: unknown) {
      console.error("Failed to load opportunities", err);
    }
  };

  const loadItems = async () => {
    try {
      const companyId = getCompanyId();
      const data = await itemService.getAll({ companyId });
      setItems(data);
    } catch (err: unknown) {
      console.error("Failed to load items", err);
    }
  };

  const loadCurrencies = async () => {
    try {
      const data = await currencyService.getActive();
      setCurrencies(data);
    } catch (err: unknown) {
      console.error("Failed to load currencies", err);
    }
  };

  const loadUsers = async () => {
    try {
      const data = await userService.getAll();
      setUsers(data);
    } catch (err: unknown) {
      console.error("Failed to load users", err);
    }
  };

  const addLine = () => {
    setLines([
      ...lines,
      {
        itemId: "",
        description: "",
        quantity: 1,
        unitPrice: 0,
        discountPercent: 0,
        taxRate: 0,
      },
    ]);
  };

  const removeLine = (index: number) => {
    setLines(lines.filter((_, i) => i !== index));
  };

  const updateLine = (index: number, field: string, value: any) => {
    const newLines = [...lines];
    (newLines[index] as any)[field] = value;

    if (field === "itemId" && value) {
      const item = items.find((item) => item.id === value);
      if (item) {
        newLines[index].description = item.itemName || "";
        newLines[index].unitPrice = item.sellingPrice || 0;
      }
    }

    if (
      field === "quantity" ||
      field === "unitPrice" ||
      field === "taxRate" ||
      field === "discountPercent"
    ) {
      (newLines[index] as any)[field] = parseFloat(value) || 0;
    }

    setLines(newLines);
  };

  const calculateTotals = () => {
    let subTotal = 0;
    let totalTax = 0;

    lines.forEach((line) => {
      const lineSubTotal = line.quantity * line.unitPrice;
      const discountAmount = lineSubTotal * ((line.discountPercent || 0) / 100);
      const lineAfterDiscount = lineSubTotal - discountAmount;
      const lineTax = lineAfterDiscount * ((line.taxRate || 0) / 100);

      subTotal += lineAfterDiscount;
      totalTax += lineTax;
    });

    return {
      subTotal,
      taxAmount: totalTax,
      totalAmount: subTotal + totalTax,
    };
  };

  const onSubmit = async (data: QuoteFormValues) => {
    setError(null);
    setLoading(true);
    try {
      const totals = calculateTotals();

      if (isEdit && id) {
        const updateData: UpdateQuoteDto = {
          id,
          quoteNumber: data.quoteNumber || undefined,
          customerId: data.customerId || undefined,
          opportunityId: data.opportunityId || undefined,
          quoteDate: data.quoteDate || undefined,
          validUntil: data.validUntil || undefined,
          currency: data.currency || "USD",
          subTotal: totals.subTotal,
          taxAmount: totals.taxAmount,
          totalAmount: totals.totalAmount,
          status: data.status || "Draft",
          notes: data.notes || undefined,
          terms: data.terms || undefined,
          assignedToUserId: data.assignedToUserId || undefined,
        };
        await quoteService.update(id, updateData);
      } else {
        const createData: CreateQuoteDto = {
          quoteNumber: data.quoteNumber || undefined,
          customerId: data.customerId || undefined,
          opportunityId: data.opportunityId || undefined,
          quoteDate: data.quoteDate || undefined,
          validUntil: data.validUntil || undefined,
          currency: data.currency || "USD",
          subTotal: totals.subTotal,
          taxAmount: totals.taxAmount,
          totalAmount: totals.totalAmount,
          status: data.status || "Draft",
          notes: data.notes || undefined,
          terms: data.terms || undefined,
          assignedToUserId: data.assignedToUserId || undefined,
          quoteLines: lines,
        };
        await quoteService.create(createData);
      }
      navigate("/crm/quotes");
    } catch (err: unknown) {
      const e = err as { response?: { data?: { Error?: string; message?: string } } };
      setError(e.response?.data?.Error || e.response?.data?.message || "Failed to save quote");
    } finally {
      setLoading(false);
    }
  };

  const totals = calculateTotals();

  return (
    <CrmFormLayout
      breadcrumbTitle={isEdit ? "Edit Quote" : "Add Quote"}
      title={isEdit ? "Edit Quote" : "Add Quote"}
      subtitle={isEdit ? "Update quote details and line items" : "Prepare a sales quote for a customer"}
      backUrl="/crm/quotes"
      fetching={fetching}
      error={error}
      maxWidth="wide"
    >
      <FormProvider {...methods}>
        <form onSubmit={handleSubmit(onSubmit)} className="crm-entity-form-fields" noValidate>
          <FormSection title="Quote information" description="Header details and assignment.">
            <FormInput
              label="Quote number"
              type="text"
              name="quoteNumber"
              placeholder="Auto-generated if empty"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_INPUT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
            />
            <FormInput
              label="Customer"
              type="select"
              name="customerId"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_SELECT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
            >
              <option value="">Select customer</option>
              {customers.map((customer) => (
                <option key={customer.id} value={customer.id}>
                  {customer.customerName} ({customer.companyName})
                </option>
              ))}
            </FormInput>
            <FormInput
              label="Opportunity"
              type="select"
              name="opportunityId"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_SELECT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
            >
              <option value="">Select opportunity</option>
              {opportunities.map((opp) => (
                <option key={opp.id} value={opp.id}>
                  {opp.opportunityCode || opp.title || opp.opportunityName}
                </option>
              ))}
            </FormInput>
            <FormInput
              label="Assigned to"
              type="select"
              name="assignedToUserId"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_SELECT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
            >
              <option value="">Select user</option>
              {users.map((user) => (
                <option key={user.id} value={user.id}>
                  {user.username || user.email}
                </option>
              ))}
            </FormInput>
            <FormInput
              label="Quote date"
              type="date"
              name="quoteDate"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_INPUT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
            />
            <FormInput
              label="Valid until"
              type="date"
              name="validUntil"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_INPUT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
            />
            <FormInput
              label="Currency"
              type="select"
              name="currency"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_SELECT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
            >
              <option value="">Select currency</option>
              {currencies.map((curr) => (
                <option key={curr.id} value={curr.code}>
                  {curr.code} - {curr.name}
                </option>
              ))}
            </FormInput>
            <FormInput
              label="Status"
              type="select"
              name="status"
              containerClass={CRM_FIELD_WRAP}
              className={CRM_SELECT_CLASS}
              labelClassName={CRM_LABEL_CLASS}
            >
              {QUOTE_STATUS_OPTIONS.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </FormInput>
            <div className="sm:col-span-2">
              <FormInput
                label="Terms"
                type="textarea"
                name="terms"
                rows={2}
                placeholder="Payment terms"
                containerClass={CRM_FIELD_WRAP}
                className={CRM_TEXTAREA_CLASS}
                labelClassName={CRM_LABEL_CLASS}
              />
            </div>
            <div className="sm:col-span-2">
              <FormInput
                label="Notes"
                type="textarea"
                name="notes"
                rows={3}
                placeholder="Additional notes"
                containerClass={CRM_FIELD_WRAP}
                className={CRM_TEXTAREA_CLASS}
                labelClassName={CRM_LABEL_CLASS}
              />
            </div>
          </FormSection>

          <FormSection title="Line items" description="Products and pricing." columns={1}>
            <div className="flex items-center justify-end mb-2">
              <button type="button" onClick={addLine} className="btn btn-sm btn-primary">
                <i className="mgc_add_line me-2" />
                Add line
              </button>
            </div>
            <div className="table-responsive">
              <table className="table">
                <thead>
                  <tr>
                    <th>Item</th>
                    <th>Description</th>
                    <th>Quantity</th>
                    <th>Unit price</th>
                    <th>Discount %</th>
                    <th>Tax rate %</th>
                    <th>Line total</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {lines.map((line, index) => {
                    const lineSubTotal = line.quantity * line.unitPrice;
                    const discountAmount = lineSubTotal * ((line.discountPercent || 0) / 100);
                    const lineAfterDiscount = lineSubTotal - discountAmount;
                    const lineTax = lineAfterDiscount * ((line.taxRate || 0) / 100);
                    const lineTotal = lineAfterDiscount + lineTax;

                    return (
                      <tr key={index}>
                        <td>
                          <select
                            className="form-select form-select-sm"
                            value={line.itemId}
                            onChange={(e) => updateLine(index, "itemId", e.target.value)}
                          >
                            <option value="">Select item</option>
                            {items.map((item) => (
                              <option key={item.id} value={item.id}>
                                {item.itemCode} - {item.itemName}
                              </option>
                            ))}
                          </select>
                        </td>
                        <td>
                          <input
                            type="text"
                            className="form-control form-control-sm"
                            value={line.description || ""}
                            onChange={(e) => updateLine(index, "description", e.target.value)}
                            placeholder="Description"
                          />
                        </td>
                        <td>
                          <input
                            type="number"
                            className="form-control form-control-sm"
                            value={line.quantity}
                            onChange={(e) => updateLine(index, "quantity", e.target.value)}
                            step="0.01"
                            min="0"
                          />
                        </td>
                        <td>
                          <input
                            type="number"
                            className="form-control form-control-sm"
                            value={line.unitPrice}
                            onChange={(e) => updateLine(index, "unitPrice", e.target.value)}
                            step="0.01"
                            min="0"
                          />
                        </td>
                        <td>
                          <input
                            type="number"
                            className="form-control form-control-sm"
                            value={line.discountPercent || 0}
                            onChange={(e) => updateLine(index, "discountPercent", e.target.value)}
                            step="0.01"
                            min="0"
                            max="100"
                          />
                        </td>
                        <td>
                          <input
                            type="number"
                            className="form-control form-control-sm"
                            value={line.taxRate || 0}
                            onChange={(e) => updateLine(index, "taxRate", e.target.value)}
                            step="0.01"
                            min="0"
                            max="100"
                          />
                        </td>
                        <td>{lineTotal.toFixed(2)}</td>
                        <td>
                          {lines.length > 1 && (
                            <button
                              type="button"
                              onClick={() => removeLine(index)}
                              className="btn btn-sm btn-soft-danger"
                            >
                              <i className="mgc_delete_line" />
                            </button>
                          )}
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
                <tfoot>
                  <tr className="font-bold">
                    <td colSpan={6}>Subtotal</td>
                    <td>{totals.subTotal.toFixed(2)}</td>
                    <td />
                  </tr>
                  <tr className="font-bold">
                    <td colSpan={6}>Tax</td>
                    <td>{totals.taxAmount.toFixed(2)}</td>
                    <td />
                  </tr>
                  <tr className="font-bold text-lg">
                    <td colSpan={6}>Total</td>
                    <td>{totals.totalAmount.toFixed(2)}</td>
                    <td />
                  </tr>
                </tfoot>
              </table>
            </div>
          </FormSection>

          <CrmFormFooter
            cancelUrl="/crm/quotes"
            loading={loading}
            isEdit={isEdit}
            createLabel="Add quote"
          />
        </form>
      </FormProvider>
    </CrmFormLayout>
  );
};

export default QuoteForm;
