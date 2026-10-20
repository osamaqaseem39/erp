# Standard ERP Default Reports Pack

Default operational/financial reports expected in trade/accounting ERPs. Seeded via `ReportCatalog` and runnable from `/reports`.

**Count:** ~85 catalog codes (module coverage, not one report per entity).

## Sales / CRM / POS
| Code | Purpose |
|------|---------|
| SALE_ORDER, SALE_INVOICE, CREDIT_NOTE | Document lists |
| DISPATCH_ADVICE | Delivery notes |
| SALE_RETURN | Customer returns |
| CUSTOMER_INFO, PENDING_SALE_ORDER, SALE_* summaries / schedule / aging | Ops analytics |
| QUOTE_LIST, QUOTE_CONVERSION, LEAD/OPPORTUNITY/CAMPAIGN, enquiry/pre-cost | CRM |
| POS_SESSION / TENDER / ITEM_SALES | POS |

## Purchase
| Code | Purpose |
|------|---------|
| VENDOR_INFO, VENDOR_CONTRACT | Party / contracts |
| PURCHASE_REQUISITION, RFQ_SUMMARY | Upstream P2P |
| PURCHASE_ORDER(+summary), GOODS_RECEIPT, PURCHASE_INVOICE, RETURN, DEBIT_NOTE, LANDED_COST | P2P docs |

## Inventory
| Code | Purpose |
|------|---------|
| STOCK_ON_HAND, STOCK_VALUATION, STOCK_LEDGER, STOCK_MOVEMENT, STOCK_AVERAGE_RATE | Core stock |
| REORDER_ALERT, STOCK_RESERVATION, CYCLE_COUNT_VARIANCE | Control |
| BATCH_EXPIRY, SERIAL_NUMBER_LIST, ITEM_KIT_LIST | Traceability / kits |

## Finance
| Code | Purpose |
|------|---------|
| TRIAL_BALANCE, RUNNING_TB, P&L, BS, CASH_FLOW, GL, COA, VOUCHERS | Statements |
| AGED_RECEIVABLES/PAYABLES, PARTNER_BALANCES, PAYMENT_REGISTER | AR/AP |
| CASH_BANK_*, CHEQUE_REGISTER, BANK_TRANSACTION, BUDGET_VS_ACTUAL | Treasury |
| TAX_SUMMARY, FIXED_ASSET_REGISTER, DEPRECIATION_SCHEDULE, EXPENSE_CLAIM, OPENING_BALANCE | Compliance / FA |

## Manufacturing / Projects / HR
| Code | Purpose |
|------|---------|
| PRODUCTION_ORDER, BOM, ROUTING, SCRAP, WIP, WORK_CENTER | Shop floor |
| PROJECT_STATUS, TIMESHEET, BURN, MILESTONE_DUE | Projects |
| EMPLOYEE_ROSTER, LEAVE_*, ATTENDANCE, PAYROLL, ADVANCE, BENEFIT, TRAINING, RECRUITMENT | HR |

## Intentionally not default
- Core/Admin platform tables (users, roles, tenants) — use Admin screens  
- Every line/child entity as its own report  
- Jurisdiction-specific GST/VAT packs (extend TAX_SUMMARY when needed)

See also: [REPORTS_COMPLETION_PLAN.md](./REPORTS_COMPLETION_PLAN.md)
