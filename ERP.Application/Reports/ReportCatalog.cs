namespace ERP.Application.Reports;

/// <summary>Single source of truth for built-in report codes seeded per tenant.</summary>
public static class ReportCatalog
{
    public readonly record struct Def(string Code, string Name, string Category, string Description, string ReportType = "Operational");

    public static IReadOnlyList<Def> All { get; } =
    [
        // Sales (existing + ops leftovers)
        new("SALE_ORDER", "Sale Order", "Sales", "Sales orders by period"),
        new("SALE_INVOICE", "Sale Invoice", "Sales", "Sales invoices by period"),
        new("CREDIT_NOTE", "Credit Note", "Sales", "Sales credit notes by period"),
        new("CUSTOMER_INFO", "Customer Information Report", "Sales", "Customer master dump"),
        new("PENDING_SALE_ORDER", "Pending Sale Order", "Sales", "Open sales orders not invoiced"),
        new("SALE_INVOICE_SUMMARY", "Sale Invoice Summary", "Sales", "Aggregated SI totals by status"),
        new("SALE_SUMMARY_ITEM", "Sale Summary Item", "Sales", "Totals by item"),
        new("SALE_SUMMARY_CUSTOMER_ITEM", "Sale Summary Customer-Item", "Sales", "Customer × item matrix"),
        new("SALE_SUMMARY_ITEM_CUSTOMER", "Sale Summary Item-Customer", "Sales", "Item × customer matrix"),
        new("SALE_ITEM_RATE_DETAIL", "Sale Item Rate Detail", "Sales", "Historical rates by item/customer"),
        new("SALE_ORDER_SCHEDULE", "Sale Order Schedule", "Sales", "SO lines by required/delivery date"),
        new("SALE_PENDING_FOR_PRODUCTION", "Pending Sale Order for Production", "Sales", "SO lines awaiting MFG release"),
        new("SALE_AGING", "Sale Aging", "Sales", "AR aging from finance invoices"),
        new("DISPATCH_ADVICE", "Dispatch Advice", "Sales", "Delivery / dispatch notes by period"),
        new("SALE_RETURN", "Sale Return", "Sales", "Customer return orders by period"),

        // Purchase
        new("PURCHASE_ORDER_SUMMARY", "Purchase Order Summary", "Purchase", "PO totals by status"),
        new("PURCHASE_ORDER", "Purchase Order", "Purchase", "Purchase orders by period"),
        new("PURCHASE_INVOICE_SUMMARY", "Purchase Invoice Summary", "Purchase", "PI totals by period"),
        new("GOODS_RECEIPT_SUMMARY", "Goods Receipt Summary", "Purchase", "GRN headers by period"),
        new("PURCHASE_DEBIT_NOTE", "Purchase Debit Note", "Purchase", "Debit notes by period"),
        new("PURCHASE_RETURN_SUMMARY", "Purchase Return Summary", "Purchase", "Purchase returns by period"),
        new("LANDED_COST_SUMMARY", "Landed Cost Summary", "Purchase", "Landed cost vouchers by period"),
        new("VENDOR_INFO", "Vendor Information", "Purchase", "Vendor master dump"),
        new("PURCHASE_REQUISITION", "Purchase Requisition", "Purchase", "PR list by period"),
        new("RFQ_SUMMARY", "RFQ Summary", "Purchase", "Requests for quotation by period"),
        new("VENDOR_CONTRACT", "Vendor Contract", "Purchase", "Active vendor price contracts"),

        // Inventory
        new("STOCK_ON_HAND", "Stock On Hand", "Inventory", "Warehouse stock balances"),
        new("STOCK_LEDGER", "Stock Ledger", "Inventory", "Stock movements by item/period"),
        new("STOCK_MOVEMENT_SUMMARY", "Stock Movement Summary", "Inventory", "Transfers and adjustments by period"),
        new("STOCK_AVERAGE_RATE", "Stock Average Rate", "Inventory", "Weighted average cost by item"),
        new("STOCK_RESERVATION", "Stock Reservation", "Inventory", "Open stock reservations"),
        new("CYCLE_COUNT_VARIANCE", "Cycle Count Variance", "Inventory", "Cycle count variances"),
        new("STOCK_VALUATION", "Stock Valuation", "Inventory", "Qty × cost valuation by warehouse"),
        new("REORDER_ALERT", "Reorder Alert", "Inventory", "Items at or below reorder point"),
        new("BATCH_EXPIRY", "Batch Expiry", "Inventory", "Batches with expiry dates"),
        new("SERIAL_NUMBER_LIST", "Serial Number List", "Inventory", "Serialized inventory status"),
        new("ITEM_KIT_LIST", "Item Kit List", "Inventory", "Kits and component counts"),

        // Finance
        new("TRIAL_BALANCE", "Trial Balance", "Finance", "Trial balance", "Financial"),
        new("RUNNING_TRIAL_BALANCE", "Running Trial Balance", "Finance", "Running trial balance", "Financial"),
        new("PROFIT_LOSS", "Profit and Loss", "Finance", "Income statement", "Financial"),
        new("BALANCE_SHEET", "Balance Sheet", "Finance", "Balance sheet", "Financial"),
        new("GENERAL_LEDGER", "General Ledger", "Finance", "Account ledger movements", "Financial"),
        new("VOUCHER_LIST", "Voucher Report", "Finance", "Journals/vouchers by period", "Financial"),
        new("CHART_OF_ACCOUNTS", "Chart of Accounts", "Finance", "Chart of accounts listing", "Financial"),
        new("DAILY_FINANCIAL", "Daily Financial", "Finance", "Daily financial snapshot", "Financial"),
        new("CASH_BANK_VOUCHERS", "Cash Bank Vouchers", "Finance", "Cash/bank payment receipt summary", "Financial"),
        new("CASH_BANK_LEDGER", "Cash Bank Ledger", "Finance", "Cash/bank ledger summary", "Financial"),
        new("VOUCHER_TYPE_MONTHLY", "Voucher Type Monthly", "Finance", "Voucher type monthly summary", "Financial"),
        new("PARTNER_BALANCES", "Partner Balances", "Finance", "Customer/supplier balances", "Financial"),
        new("AGED_RECEIVABLES", "Aged Receivables", "Finance", "AR aging", "Financial"),
        new("AGED_PAYABLES", "Aged Payables", "Finance", "AP aging", "Financial"),
        new("CASH_FLOW", "Cash Flow", "Finance", "Cash flow statement", "Financial"),
        new("BUDGET_VS_ACTUAL", "Budget vs Actual", "Finance", "Budget versus actual", "Financial"),
        new("CHEQUE_REGISTER", "Cheque Register", "Finance", "Cheque register", "Financial"),
        new("PAYMENT_REGISTER", "Payment Register", "Finance", "Customer/supplier payments by period", "Financial"),
        new("TAX_SUMMARY", "Tax Summary", "Finance", "Tax transactions by period", "Financial"),
        new("FIXED_ASSET_REGISTER", "Fixed Asset Register", "Finance", "Fixed assets and book values", "Financial"),
        new("DEPRECIATION_SCHEDULE", "Depreciation Schedule", "Finance", "Depreciation entries by period", "Financial"),
        new("EXPENSE_CLAIM_SUMMARY", "Expense Claim Summary", "Finance", "Employee expense claims", "Financial"),
        new("BANK_TRANSACTION", "Bank Transaction", "Finance", "Imported bank statement lines", "Financial"),
        new("OPENING_BALANCE", "Opening Balance", "Finance", "Fiscal year opening balances", "Financial"),

        // CRM
        new("LEAD_PIPELINE", "Lead Pipeline", "CRM", "Leads by status/owner"),
        new("OPPORTUNITY_PIPELINE", "Opportunity Pipeline", "CRM", "Opportunities by stage"),
        new("QUOTE_LIST", "Quote List", "CRM", "Quotations by period"),
        new("QUOTE_CONVERSION", "Quote Conversion", "CRM", "Quotes to sales order conversion"),
        new("CAMPAIGN_ACTIVITY", "Campaign Activity", "CRM", "Campaigns and linked activities"),
        new("SALE_ENQUIRY_SUMMARY", "Sale Enquiry Summary", "CRM", "Packaging enquiries by period"),
        new("SALE_PRECOST_SUMMARY", "Sale Pre-Cost Summary", "CRM", "Pre-costs by period"),

        // Manufacturing
        new("PRODUCTION_ORDER_STATUS", "Production Order Status", "Manufacturing", "Production orders by status"),
        new("BOM_USAGE", "BOM Usage", "Manufacturing", "BOM headers and component counts"),
        new("PRODUCTION_SCRAP", "Production Scrap", "Manufacturing", "Scrap by order/item"),
        new("WIP_SUMMARY", "WIP Summary", "Manufacturing", "Open production / WIP"),
        new("WORK_CENTER_LOAD", "Work Center Load", "Manufacturing", "Orders by work center"),
        new("ROUTING_LIST", "Routing List", "Manufacturing", "Routings and operation counts"),

        // Projects
        new("PROJECT_STATUS", "Project Status", "Projects", "Active projects and progress"),
        new("PROJECT_TIMESHEET", "Project Timesheet", "Projects", "Timesheet hours by project"),
        new("PROJECT_BURN", "Project Burn", "Projects", "Budget vs hours/cost"),
        new("MILESTONE_DUE", "Milestone Due", "Projects", "Upcoming and overdue milestones"),

        // HR
        new("EMPLOYEE_ROSTER", "Employee Roster", "HR", "Active employees by department"),
        new("LEAVE_BALANCE", "Leave Balance", "HR", "Leave balances by type"),
        new("LEAVE_UTILIZATION", "Leave Utilization", "HR", "Leave requests by period"),
        new("ATTENDANCE_SUMMARY", "Attendance Summary", "HR", "Time entry aggregates"),
        new("PAYROLL_SUMMARY", "Payroll Summary", "HR", "Payroll runs and totals"),
        new("EMPLOYEE_ADVANCE", "Employee Advance", "HR", "Open advances and recoveries"),
        new("BENEFIT_ENROLLMENT", "Benefit Enrollment", "HR", "Active benefit enrollments"),
        new("TRAINING_SUMMARY", "Training Summary", "HR", "Trainings and enrollments"),
        new("RECRUITMENT_PIPELINE", "Recruitment Pipeline", "HR", "Job applications by status"),

        // POS
        new("POS_SESSION_SUMMARY", "POS Session Summary", "POS", "POS sessions and totals"),
        new("POS_TENDER_SUMMARY", "POS Tender Summary", "POS", "POS payments by tender"),
        new("POS_ITEM_SALES", "POS Item Sales", "POS", "Items sold via POS"),
    ];

    /// <summary>Maps ReportCode to permission resource segment, e.g. SALE_ORDER → saleOrder.</summary>
    public static string PermissionResource(string reportCode)
    {
        var parts = reportCode.Trim().Split('_', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return "unknown";
        return parts[0].ToLowerInvariant() + string.Concat(parts.Skip(1).Select(p =>
            char.ToUpperInvariant(p[0]) + p[1..].ToLowerInvariant()));
    }

    public static string ViewPermission(string reportCode) =>
        $"reports.{PermissionResource(reportCode)}.view";
}
