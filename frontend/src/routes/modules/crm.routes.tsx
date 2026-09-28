import React from "react";
import type { RoutesProps } from "../types";

const ActivityForm = React.lazy(() => import("../../modules/crm/entities/activity/pages/ActivityForm"));
const ActivityList = React.lazy(() => import("../../modules/crm/entities/activity/pages/ActivityList"));
const CampaignForm = React.lazy(() => import("../../modules/crm/entities/campaign/pages/CampaignForm"));
const CampaignList = React.lazy(() => import("../../modules/crm/entities/campaign/pages/CampaignList"));
const ContactForm = React.lazy(() => import("../../modules/crm/entities/contact/pages/ContactForm"));
const ContactList = React.lazy(() => import("../../modules/crm/entities/contact/pages/ContactList"));
const CrmDashboard = React.lazy(() => import("../../pages/crm/CrmDashboard"));
const CustomerDetail = React.lazy(() => import("../../modules/crm/entities/customer/pages/CustomerDetail"));
const CustomerForm = React.lazy(() => import("../../modules/crm/entities/customer/pages/CustomerForm"));
const CustomerGroupForm = React.lazy(() => import("../../modules/crm/entities/customer-group/pages/CustomerGroupForm"));
const CustomerGroupList = React.lazy(() => import("../../modules/crm/entities/customer-group/pages/CustomerGroupList"));
const CustomerList = React.lazy(() => import("../../modules/crm/entities/customer/pages/CustomerList"));
const LeadForm = React.lazy(() => import("../../modules/crm/entities/lead/pages/LeadForm"));
const LeadsPage = React.lazy(() => import("../../modules/crm/entities/lead/pages/LeadsPage"));
const OpportunityForm = React.lazy(() => import("../../modules/crm/entities/opportunity/pages/OpportunityForm"));
const OpportunityList = React.lazy(() => import("../../modules/crm/entities/opportunity/pages/OpportunityList"));
const PriceListForm = React.lazy(() => import("../../modules/crm/entities/price-list/pages/PriceListForm"));
const PriceListList = React.lazy(() => import("../../modules/crm/entities/price-list/pages/PriceListList"));
const QuoteForm = React.lazy(() => import("../../modules/crm/entities/quote/pages/QuoteForm"));
const QuoteList = React.lazy(() => import("../../modules/crm/entities/quote/pages/QuoteList"));
const ReturnOrderForm = React.lazy(() => import("../../modules/crm/entities/return-order/pages/ReturnOrderForm"));
const ReturnOrderList = React.lazy(() => import("../../modules/crm/entities/return-order/pages/ReturnOrderList"));
const SalesOrderForm = React.lazy(() => import("../../modules/crm/entities/sales-order/pages/SalesOrderForm"));
const SalesOrderList = React.lazy(() => import("../../modules/crm/entities/sales-order/pages/SalesOrderList"));
const SalesTerritoryForm = React.lazy(() => import("../../modules/crm/entities/sales-territory/pages/SalesTerritoryForm"));
const SalesTerritoryList = React.lazy(() => import("../../modules/crm/entities/sales-territory/pages/SalesTerritoryList"));

export const crmRoutes: RoutesProps = {
  path: "/crm",
  name: "CRM",
  icon: "briefcase",
  header: "Customer Relationship Management",
  children: [
    {
      path: "/crm/dashboard",
      name: "CRM Dashboard",
      element: <CrmDashboard />,
    },
    // Customers
    {
      path: "/crm/customers",
      name: "Customers",
      element: <CustomerList />,
    },
    {
      path: "/crm/customers/new",
      name: "Add Customer",
      element: <CustomerForm />,
    },
    {
      path: "/crm/customers/:id",
      name: "Customer",
      element: <CustomerDetail />,
    },
    {
      path: "/crm/customers/:id/edit",
      name: "Edit Customer",
      element: <CustomerForm />,
    },
    // Contacts
    {
      path: "/crm/contacts",
      name: "Contacts",
      element: <ContactList />,
    },
    {
      path: "/crm/contacts/new",
      name: "Add Contact",
      element: <ContactForm />,
    },
    {
      path: "/crm/contacts/:id/edit",
      name: "Edit Contact",
      element: <ContactForm />,
    },
    // Leads
    {
      path: "/crm/leads",
      name: "Leads",
      element: <LeadsPage />,
    },
    {
      path: "/crm/leads/new",
      name: "Add Lead",
      element: <LeadForm />,
    },
    {
      path: "/crm/leads/:id/edit",
      name: "Edit Lead",
      element: <LeadForm />,
    },
    // Opportunities
    {
      path: "/crm/opportunities",
      name: "Opportunities",
      element: <OpportunityList />,
    },
    {
      path: "/crm/opportunities/new",
      name: "Add Opportunity",
      element: <OpportunityForm />,
    },
    {
      path: "/crm/opportunities/:id/edit",
      name: "Edit Opportunity",
      element: <OpportunityForm />,
    },
    // Quotes
    {
      path: "/crm/quotes",
      name: "Quotes",
      element: <QuoteList />,
    },
    {
      path: "/crm/quotes/new",
      name: "Add Quote",
      element: <QuoteForm />,
    },
    {
      path: "/crm/quotes/:id/edit",
      name: "Edit Quote",
      element: <QuoteForm />,
    },
    // Sales Orders
    {
      path: "/crm/sales-orders",
      name: "Sales Orders",
      element: <SalesOrderList />,
    },
    {
      path: "/crm/sales-orders/new",
      name: "Add Sales Order",
      element: <SalesOrderForm />,
    },
    {
      path: "/crm/sales-orders/:id/edit",
      name: "Edit Sales Order",
      element: <SalesOrderForm />,
    },
    // Activities
    {
      path: "/crm/activities",
      name: "Activities",
      element: <ActivityList />,
    },
    {
      path: "/crm/activities/new",
      name: "Add Activity",
      element: <ActivityForm />,
    },
    {
      path: "/crm/activities/:id/edit",
      name: "Edit Activity",
      element: <ActivityForm />,
    },
    // Campaigns
    {
      path: "/crm/campaigns",
      name: "Campaigns",
      element: <CampaignList />,
    },
    {
      path: "/crm/campaigns/new",
      name: "Add Campaign",
      element: <CampaignForm />,
    },
    {
      path: "/crm/campaigns/:id/edit",
      name: "Edit Campaign",
      element: <CampaignForm />,
    },
    // Customer Groups
    {
      path: "/crm/customer-groups",
      name: "Customer Groups",
      element: <CustomerGroupList />,
    },
    {
      path: "/crm/customer-groups/new",
      name: "Add Customer Group",
      element: <CustomerGroupForm />,
    },
    {
      path: "/crm/customer-groups/:id/edit",
      name: "Edit Customer Group",
      element: <CustomerGroupForm />,
    },
    // Price Lists
    {
      path: "/crm/price-lists",
      name: "Price Lists",
      element: <PriceListList />,
    },
    {
      path: "/crm/price-lists/new",
      name: "Add Price List",
      element: <PriceListForm />,
    },
    {
      path: "/crm/price-lists/:id/edit",
      name: "Edit Price List",
      element: <PriceListForm />,
    },
    // Sales Territories
    {
      path: "/crm/sales-territories",
      name: "Sales Territories",
      element: <SalesTerritoryList />,
    },
    {
      path: "/crm/sales-territories/new",
      name: "Add Sales Territory",
      element: <SalesTerritoryForm />,
    },
    {
      path: "/crm/sales-territories/:id/edit",
      name: "Edit Sales Territory",
      element: <SalesTerritoryForm />,
    },
    // Return Orders
    {
      path: "/crm/return-orders",
      name: "Return Orders",
      element: <ReturnOrderList />,
    },
    {
      path: "/crm/return-orders/new",
      name: "Add Return Order",
      element: <ReturnOrderForm />,
    },
    {
      path: "/crm/return-orders/:id/edit",
      name: "Edit Return Order",
      element: <ReturnOrderForm />,
    },
  ],
};
