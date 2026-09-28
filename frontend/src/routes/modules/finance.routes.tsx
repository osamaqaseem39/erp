import React from "react";
import type { RoutesProps } from "../types";

const BankForm = React.lazy(() => import("../../modules/finance/entities/bank/pages/BankForm"));
const BankList = React.lazy(() => import("../../modules/finance/entities/bank/pages/BankList"));
const BillForm = React.lazy(() => import("../../modules/finance/entities/bill/pages/BillForm"));
const BillList = React.lazy(() => import("../../modules/finance/entities/bill/pages/BillList"));
const ChartOfAccountForm = React.lazy(() => import("../../modules/finance/entities/chart-of-account/pages/ChartOfAccountForm"));
const ChartOfAccountList = React.lazy(() => import("../../modules/finance/entities/chart-of-account/pages/ChartOfAccountList"));
const CurrencyForm = React.lazy(() => import("../../modules/finance/entities/currency/pages/CurrencyForm"));
const CurrencyList = React.lazy(() => import("../../modules/finance/entities/currency/pages/CurrencyList"));
const InvoiceForm = React.lazy(() => import("../../modules/finance/entities/invoice/pages/InvoiceForm"));
const InvoiceList = React.lazy(() => import("../../modules/finance/entities/invoice/pages/InvoiceList"));
const JournalForm = React.lazy(() => import("../../modules/finance/entities/journal/pages/JournalForm"));
const JournalList = React.lazy(() => import("../../modules/finance/entities/journal/pages/JournalList"));
const PaymentForm = React.lazy(() => import("../../modules/finance/entities/payment/pages/PaymentForm"));
const PaymentList = React.lazy(() => import("../../modules/finance/entities/payment/pages/PaymentList"));

export const financeRoutes: RoutesProps = {
  path: "/finance",
  name: "Finance",
  icon: "dollar-sign",
  header: "Finance & Accounting",
  children: [
    // Chart of Accounts
    {
      path: "/finance/chart-of-accounts",
      name: "Chart of Accounts",
      element: <ChartOfAccountList />,
    },
    {
      path: "/finance/chart-of-accounts/new",
      name: "Add Chart of Account",
      element: <ChartOfAccountForm />,
    },
    {
      path: "/finance/chart-of-accounts/:id/edit",
      name: "Edit Chart of Account",
      element: <ChartOfAccountForm />,
    },
    // Currencies
    {
      path: "/finance/currencies",
      name: "Currencies",
      element: <CurrencyList />,
    },
    {
      path: "/finance/currencies/new",
      name: "Add Currency",
      element: <CurrencyForm />,
    },
    {
      path: "/finance/currencies/:id/edit",
      name: "Edit Currency",
      element: <CurrencyForm />,
    },
    // Journals
    {
      path: "/finance/journals",
      name: "Journals",
      element: <JournalList />,
    },
    {
      path: "/finance/journals/new",
      name: "Add Journal",
      element: <JournalForm />,
    },
    {
      path: "/finance/journals/:id/edit",
      name: "Edit Journal",
      element: <JournalForm />,
    },
    // Invoices
    {
      path: "/finance/invoices",
      name: "Invoices",
      element: <InvoiceList />,
    },
    {
      path: "/finance/invoices/new",
      name: "Add Invoice",
      element: <InvoiceForm />,
    },
    {
      path: "/finance/invoices/:id/edit",
      name: "Edit Invoice",
      element: <InvoiceForm />,
    },
    // Bills
    {
      path: "/finance/bills",
      name: "Bills",
      element: <BillList />,
    },
    {
      path: "/finance/bills/new",
      name: "Add Bill",
      element: <BillForm />,
    },
    {
      path: "/finance/bills/:id/edit",
      name: "Edit Bill",
      element: <BillForm />,
    },
    // Payments
    {
      path: "/finance/payments",
      name: "Payments",
      element: <PaymentList />,
    },
    {
      path: "/finance/payments/new",
      name: "Add Payment",
      element: <PaymentForm />,
    },
    {
      path: "/finance/payments/:id/edit",
      name: "Edit Payment",
      element: <PaymentForm />,
    },
    // Banks
    {
      path: "/finance/banks",
      name: "Banks",
      element: <BankList />,
    },
    {
      path: "/finance/banks/new",
      name: "Add Bank",
      element: <BankForm />,
    },
    {
      path: "/finance/banks/:id/edit",
      name: "Edit Bank",
      element: <BankForm />,
    },
  ],
};
