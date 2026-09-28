import React, { useEffect, useMemo, useState } from "react";
import { Link } from "react-router-dom";
import { Tab } from "@headlessui/react";
import SimpleBar from "simplebar-react";
import "simplebar-react/dist/simplebar.min.css";
import { PageBreadcrumb } from "../../components";
import { customerService, type CustomerDto } from "../../modules/crm/entities/customer/services";
import { contactService, type ContactDto } from "../../modules/crm/entities/contact/services";
import { leadService, type LeadDto } from "../../modules/crm/entities/lead/services";
import { opportunityService, type OpportunityDto } from "../../modules/crm/entities/opportunity/services";
import { quoteService, type QuoteDto } from "../../modules/crm/entities/quote/services";
import { salesOrderService, type SalesOrderDto } from "../../modules/crm/entities/sales-order/services";
import { activityService } from "../../modules/crm/entities/activity/services";
import type { ActivityDto } from "../../modules/crm/entities/activity/types";
import { campaignService } from "../../modules/crm/entities/campaign/services";
import type { CampaignDto } from "../../modules/crm/entities/campaign/types";
import {
  calculateWinRate,
  countRecordsByMonth,
  groupCountByField,
  sumNumericField,
} from "./crmDashboardAnalytics";
import { CrmKpiCard } from "./components/CrmKpiCard";
import {
  CrmGrowthChart,
  CrmPipelineBar,
  CrmRadialGauge,
  CrmStageDonut,
} from "./components/CrmDashboardCharts";
import { d365Card, d365CardHeader, d365Subtitle, d365Title, D365 } from "../../modules/crm/theme/crmDynamicsTheme";

const PREVIEW_ROWS = 6;

const StatusBadge: React.FC<{ status?: string }> = ({ status }) => {
  if (!status) return <span className="text-[#8A8886]">—</span>;
  const normalized = status.toLowerCase();
  const className =
    normalized.includes("active") || normalized.includes("won") || normalized.includes("accepted")
      ? "inline-flex px-2 py-0.5 rounded-sm text-[11px] font-medium bg-[#DFF6DD] text-[#107C10]"
      : normalized.includes("draft") || normalized.includes("new") || normalized.includes("open")
        ? "inline-flex px-2 py-0.5 rounded-sm text-[11px] font-medium bg-[#EFF6FC] text-[#0078D4]"
        : normalized.includes("lost") || normalized.includes("rejected") || normalized.includes("cancel")
          ? "inline-flex px-2 py-0.5 rounded-sm text-[11px] font-medium bg-[#FDE7E9] text-[#A4262C]"
          : normalized.includes("inactive") || normalized.includes("hold")
            ? "inline-flex px-2 py-0.5 rounded-sm text-[11px] font-medium bg-[#FFF4CE] text-[#8A6116]"
            : "inline-flex px-2 py-0.5 rounded-sm text-[11px] font-medium bg-[#F3F2F1] text-[#605E5C]";
  return <span className={className}>{status}</span>;
};

const EmptyPanel: React.FC<{ message: string; createLink: string; createLabel: string }> = ({
  message,
  createLink,
  createLabel,
}) => (
  <div className="flex flex-col items-center justify-center py-16 text-center">
    <div className="h-14 w-14 rounded-sm bg-[#F3F2F1] dark:bg-[#292827] flex items-center justify-center mb-4">
      <i className="mgc_inbox_line text-2xl text-[#8A8886]" />
    </div>
    <p className="font-semibold text-[15px] text-[#323130] dark:text-[#f3f2f1] mb-1">{message}</p>
    <p className="text-xs text-[#605E5C] dark:text-[#a19f9d] mb-4">Create your first record to see it here.</p>
    <Link
      to={createLink}
      className="inline-flex items-center gap-1.5 px-4 py-1.5 text-sm font-medium text-white bg-[#0078D4] hover:bg-[#106EBE] rounded-sm transition-colors"
    >
      <i className="mgc_add_line text-base" />
      {createLabel}
    </Link>
  </div>
);

const CrmDashboard: React.FC = () => {
  const [customers, setCustomers] = useState<CustomerDto[]>([]);
  const [contacts, setContacts] = useState<ContactDto[]>([]);
  const [leads, setLeads] = useState<LeadDto[]>([]);
  const [opportunities, setOpportunities] = useState<OpportunityDto[]>([]);
  const [quotes, setQuotes] = useState<QuoteDto[]>([]);
  const [salesOrders, setSalesOrders] = useState<SalesOrderDto[]>([]);
  const [activities, setActivities] = useState<ActivityDto[]>([]);
  const [campaigns, setCampaigns] = useState<CampaignDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    void loadDashboard();
  }, []);

  const loadDashboard = async () => {
    setLoading(true);
    setError(null);
    try {
      const [
        customersData,
        contactsData,
        leadsData,
        opportunitiesData,
        quotesData,
        ordersData,
        activitiesData,
        campaignsData,
      ] = await Promise.all([
        customerService.getAll(),
        contactService.getAll(),
        leadService.getAll(),
        opportunityService.getAll(),
        quoteService.getAll(),
        salesOrderService.getAll(),
        activityService.getAll(),
        campaignService.getAll(),
      ]);

      setCustomers(customersData);
      setContacts(contactsData);
      setLeads(leadsData);
      setOpportunities(opportunitiesData);
      setQuotes(quotesData);
      setSalesOrders(ordersData);
      setActivities(activitiesData);
      setCampaigns(campaignsData);
    } catch (err: unknown) {
      const e = err as { response?: { data?: { error?: string; Error?: string } } };
      setError(e.response?.data?.Error || e.response?.data?.error || "Failed to load CRM dashboard");
    } finally {
      setLoading(false);
    }
  };

  const formatDate = (value?: string) => {
    if (!value) return "—";
    return new Date(value).toLocaleDateString();
  };

  const formatCurrency = (amount?: number, currency?: string) => {
    if (amount == null) return "—";
    const code = currency?.trim() || "USD";
    return new Intl.NumberFormat(undefined, { style: "currency", currency: code }).format(amount);
  };

  const customerTrend = useMemo(() => countRecordsByMonth(customers), [customers]);
  const leadTrend = useMemo(() => countRecordsByMonth(leads), [leads]);
  const opportunityTrend = useMemo(() => countRecordsByMonth(opportunities), [opportunities]);

  const stageBreakdown = useMemo(
    () => groupCountByField(opportunities, "stage", "No stage"),
    [opportunities]
  );
  const leadStatusBreakdown = useMemo(
    () => groupCountByField(leads, "status", "No status"),
    [leads]
  );

  const pipelineValue = useMemo(
    () =>
      sumNumericField(opportunities, ["estimatedValue", "amount", "actualValue"]),
    [opportunities]
  );
  const winRate = useMemo(() => calculateWinRate(opportunities), [opportunities]);

  const upcomingActivities = useMemo(
    () =>
      [...activities]
        .filter((a) => a.startDate)
        .sort((a, b) => new Date(a.startDate!).getTime() - new Date(b.startDate!).getTime())
        .slice(0, 10),
    [activities]
  );

  const quickActions = [
    { title: "Add Customer", path: "/crm/customers/new", icon: "mgc_group_line", variant: "btn-soft-primary" },
    { title: "Add Lead", path: "/crm/leads/new", icon: "mgc_user_add_line", variant: "btn-soft-success" },
    { title: "Add Opportunity", path: "/crm/opportunities/new", icon: "mgc_target_line", variant: "btn-soft-info" },
    { title: "Add Quote", path: "/crm/quotes/new", icon: "mgc_file_line", variant: "btn-soft-warning" },
    { title: "Add Sales Order", path: "/crm/sales-orders/new", icon: "mgc_shopping_cart_1_line", variant: "btn-soft-danger" },
    { title: "Add Activity", path: "/crm/activities/new", icon: "mgc_calendar_line", variant: "btn-soft-secondary" },
  ];

  const browseLinks = [
    { title: "Contacts", path: "/crm/contacts", count: contacts.length },
    { title: "Campaigns", path: "/crm/campaigns", count: campaigns.length },
    { title: "Customer Groups", path: "/crm/customer-groups" },
    { title: "Price Lists", path: "/crm/price-lists" },
    { title: "Territories", path: "/crm/sales-territories" },
    { title: "Returns", path: "/crm/return-orders" },
  ];

  const tableClass =
    "w-full text-sm mb-0 [&_th]:text-left [&_th]:text-[11px] [&_th]:font-semibold [&_th]:text-[#605E5C] [&_th]:uppercase [&_th]:tracking-wide [&_th]:px-4 [&_th]:py-2 [&_th]:bg-[#FAF9F8] [&_th]:border-b [&_th]:border-[#EDEBE9] [&_td]:px-4 [&_td]:py-2.5 [&_td]:text-[#323130] [&_td]:border-b [&_td]:border-[#EDEBE9] [&_tbody_tr:hover]:bg-[#F3F2F1]";

  const entityTabs = useMemo(
    () => [
      {
        title: "Customers",
        count: customers.length,
        viewAll: "/crm/customers",
        create: "/crm/customers/new",
        createLabel: "Add customer",
        empty: "No customers yet",
        content:
          customers.length === 0 ? (
            <EmptyPanel message="No customers yet" createLink="/crm/customers/new" createLabel="Add customer" />
          ) : (
            <table className={tableClass}>
              <thead>
                <tr>
                  <th>Code</th>
                  <th>Name</th>
                  <th>Status</th>
                </tr>
              </thead>
              <tbody>
                {customers.slice(0, PREVIEW_ROWS).map((row) => (
                  <tr key={row.id}>
                    <td className="text-gray-500">{row.customerCode}</td>
                    <td>
                      <Link to={`/crm/customers/${row.id}/edit`} className="font-medium text-[#0078D4] hover:text-[#106EBE] hover:underline">
                        {row.customerName}
                      </Link>
                    </td>
                    <td>
                      <StatusBadge status={row.status} />
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          ),
      },
      {
        title: "Leads",
        count: leads.length,
        viewAll: "/crm/leads",
        create: "/crm/leads/new",
        createLabel: "Add lead",
        empty: "No leads yet",
        content:
          leads.length === 0 ? (
            <EmptyPanel message="No leads yet" createLink="/crm/leads/new" createLabel="Add lead" />
          ) : (
            <table className={tableClass}>
              <thead>
                <tr>
                  <th>Code</th>
                  <th>Name</th>
                  <th>Status</th>
                </tr>
              </thead>
              <tbody>
                {leads.slice(0, PREVIEW_ROWS).map((row) => (
                  <tr key={row.id}>
                    <td className="text-gray-500">{row.leadCode}</td>
                    <td>
                      <Link to={`/crm/leads/${row.id}/edit`} className="font-medium text-[#0078D4] hover:text-[#106EBE] hover:underline">
                        {row.firstName} {row.lastName}
                      </Link>
                    </td>
                    <td>
                      <StatusBadge status={row.status} />
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          ),
      },
      {
        title: "Opportunities",
        count: opportunities.length,
        viewAll: "/crm/opportunities",
        create: "/crm/opportunities/new",
        createLabel: "Add opportunity",
        empty: "No opportunities yet",
        content:
          opportunities.length === 0 ? (
            <EmptyPanel
              message="No opportunities yet"
              createLink="/crm/opportunities/new"
              createLabel="Add opportunity"
            />
          ) : (
            <table className={tableClass}>
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Stage</th>
                  <th>Value</th>
                </tr>
              </thead>
              <tbody>
                {opportunities.slice(0, PREVIEW_ROWS).map((row) => (
                  <tr key={row.id}>
                    <td>
                      <Link to={`/crm/opportunities/${row.id}/edit`} className="font-medium text-[#0078D4] hover:text-[#106EBE] hover:underline">
                        {row.opportunityName || row.title}
                      </Link>
                    </td>
                    <td>{row.stage || "—"}</td>
                    <td className="font-medium">
                      {formatCurrency(row.estimatedValue ?? row.amount, row.currency)}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          ),
      },
      {
        title: "Quotes",
        count: quotes.length,
        viewAll: "/crm/quotes",
        create: "/crm/quotes/new",
        createLabel: "Add Quote",
        empty: "No quotes yet",
        content:
          quotes.length === 0 ? (
            <EmptyPanel message="No quotes yet" createLink="/crm/quotes/new" createLabel="Add Quote" />
          ) : (
            <table className={tableClass}>
              <thead>
                <tr>
                  <th>Number</th>
                  <th>Customer</th>
                  <th>Status</th>
                  <th>Total</th>
                </tr>
              </thead>
              <tbody>
                {quotes.slice(0, PREVIEW_ROWS).map((row) => (
                  <tr key={row.id}>
                    <td>
                      <Link to={`/crm/quotes/${row.id}/edit`} className="font-medium text-[#0078D4] hover:text-[#106EBE] hover:underline">
                        {row.quoteNumber}
                      </Link>
                    </td>
                    <td>{row.customerName || "—"}</td>
                    <td>
                      <StatusBadge status={row.status} />
                    </td>
                    <td className="font-medium">{formatCurrency(row.totalAmount, row.currency)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          ),
      },
      {
        title: "Orders",
        count: salesOrders.length,
        viewAll: "/crm/sales-orders",
        create: "/crm/sales-orders/new",
        createLabel: "Add Sales Order",
        empty: "No orders yet",
        content:
          salesOrders.length === 0 ? (
            <EmptyPanel message="No sales orders yet" createLink="/crm/sales-orders/new" createLabel="Add Sales Order" />
          ) : (
            <table className={tableClass}>
              <thead>
                <tr>
                  <th>Number</th>
                  <th>Customer</th>
                  <th>Status</th>
                  <th>Total</th>
                </tr>
              </thead>
              <tbody>
                {salesOrders.slice(0, PREVIEW_ROWS).map((row) => (
                  <tr key={row.id}>
                    <td>
                      <Link to={`/crm/sales-orders/${row.id}/edit`} className="font-medium text-[#0078D4] hover:text-[#106EBE] hover:underline">
                        {row.orderNumber || row.salesOrderNumber}
                      </Link>
                    </td>
                    <td>{row.customerName || "—"}</td>
                    <td>
                      <StatusBadge status={row.status} />
                    </td>
                    <td className="font-medium">{formatCurrency(row.totalAmount, row.currency)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          ),
      },
      {
        title: "Contacts",
        count: contacts.length,
        viewAll: "/crm/contacts",
        create: "/crm/contacts/new",
        createLabel: "Add contact",
        empty: "No contacts yet",
        content:
          contacts.length === 0 ? (
            <EmptyPanel message="No contacts yet" createLink="/crm/contacts/new" createLabel="Add contact" />
          ) : (
            <table className={tableClass}>
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Customer</th>
                  <th>Email</th>
                </tr>
              </thead>
              <tbody>
                {contacts.slice(0, PREVIEW_ROWS).map((row) => (
                  <tr key={row.id}>
                    <td>
                      <Link to={`/crm/contacts/${row.id}/edit`} className="font-medium text-[#0078D4] hover:text-[#106EBE] hover:underline">
                        {row.fullName || `${row.firstName} ${row.lastName}`}
                      </Link>
                    </td>
                    <td>{row.customerName || "—"}</td>
                    <td className="text-gray-500">{row.email || "—"}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          ),
      },
      {
        title: "Activities",
        count: activities.length,
        viewAll: "/crm/activities",
        create: "/crm/activities/new",
        createLabel: "Schedule activity",
        empty: "No activities yet",
        content:
          activities.length === 0 ? (
            <EmptyPanel
              message="No activities yet"
              createLink="/crm/activities/new"
              createLabel="Schedule activity"
            />
          ) : (
            <table className={tableClass}>
              <thead>
                <tr>
                  <th>Subject</th>
                  <th>Type</th>
                  <th>Status</th>
                  <th>Date</th>
                </tr>
              </thead>
              <tbody>
                {activities.slice(0, PREVIEW_ROWS).map((row) => (
                  <tr key={row.id}>
                    <td>
                      <Link to={`/crm/activities/${row.id}/edit`} className="font-medium text-[#0078D4] hover:text-[#106EBE] hover:underline">
                        {row.subject}
                      </Link>
                    </td>
                    <td>{row.activityType || row.type || "—"}</td>
                    <td>
                      <StatusBadge status={row.status} />
                    </td>
                    <td className="text-gray-500">{formatDate(row.startDate)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          ),
      },
      {
        title: "Campaigns",
        count: campaigns.length,
        viewAll: "/crm/campaigns",
        create: "/crm/campaigns/new",
        createLabel: "Add Campaign",
        empty: "No campaigns yet",
        content:
          campaigns.length === 0 ? (
            <EmptyPanel
              message="No campaigns yet"
              createLink="/crm/campaigns/new"
              createLabel="Add Campaign"
            />
          ) : (
            <table className={tableClass}>
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Type</th>
                  <th>Status</th>
                  <th>Budget</th>
                </tr>
              </thead>
              <tbody>
                {campaigns.slice(0, PREVIEW_ROWS).map((row) => (
                  <tr key={row.id}>
                    <td>
                      <Link to={`/crm/campaigns/${row.id}/edit`} className="font-medium text-[#0078D4] hover:text-[#106EBE] hover:underline">
                        {row.campaignName}
                      </Link>
                    </td>
                    <td>{row.campaignType || "—"}</td>
                    <td>
                      <StatusBadge status={row.status} />
                    </td>
                    <td className="font-medium">{formatCurrency(row.budget, row.currency)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          ),
      },
    ],
    [customers, leads, opportunities, quotes, salesOrders, contacts, activities, campaigns]
  );

  return (
    <div
      className="crm-dynamics -mx-4 -mt-2 px-4 pb-6 min-h-full font-['Segoe_UI','Segoe_UI_Web',system-ui,sans-serif]"
      style={{ backgroundColor: D365.bg }}
    >
      <PageBreadcrumb title="Sales | Dashboard" subName="CRM" />

      <div className="flex flex-wrap items-start justify-between gap-4 mb-5">
        <div>
          <h1 className="text-[28px] font-semibold text-[#323130] dark:text-[#f3f2f1] mb-0 leading-tight">
            Sales overview
          </h1>
          <p className="text-sm text-[#605E5C] dark:text-[#a19f9d] mt-1 mb-0">
            Monitor pipeline health, activities, and key records
          </p>
        </div>
        <div className="flex flex-wrap gap-2">
          <Link
            to="/crm/leads/new"
            className="inline-flex items-center gap-1.5 px-3 py-1.5 text-sm font-medium text-[#323130] bg-white border border-[#8A8886] hover:bg-[#F3F2F1] rounded-sm transition-colors"
          >
            <i className="mgc_add_line" />
            New lead
          </Link>
          <Link
            to="/crm/opportunities/new"
            className="inline-flex items-center gap-1.5 px-3 py-1.5 text-sm font-medium text-white bg-[#0078D4] hover:bg-[#106EBE] rounded-sm transition-colors"
          >
            <i className="mgc_add_line" />
            New opportunity
          </Link>
        </div>
      </div>

      {error && (
        <div
          className="mb-5 px-4 py-3 rounded-sm border border-[#F1BBBC] bg-[#FDE7E9] text-[#A4262C] text-sm"
          role="alert"
        >
          {error}
        </div>
      )}

      <div className="grid grid-cols-1 gap-3 sm:grid-cols-2 xl:grid-cols-4 mb-5">
        {loading ? (
          Array.from({ length: 4 }).map((_, i) => (
            <div key={i} className={`${d365Card} animate-pulse`}>
              <div className="h-[100px]" />
            </div>
          ))
        ) : (
          <>
            <CrmKpiCard
              label="Customers"
              value={customers.length}
              icon="mgc_group_line"
              variant="primary"
              link="/crm/customers"
              sparkData={customerTrend.counts}
              tooltip="Total active customer accounts"
            />
            <CrmKpiCard
              label="Leads"
              value={leads.length}
              icon="mgc_user_add_line"
              variant="success"
              link="/crm/leads"
              sparkData={leadTrend.counts}
              tooltip="Leads in your pipeline"
            />
            <CrmKpiCard
              label="Opportunities"
              value={opportunities.length}
              icon="mgc_target_line"
              variant="info"
              link="/crm/opportunities"
              sparkData={opportunityTrend.counts}
              tooltip="Open sales opportunities"
            />
            <CrmKpiCard
              label="Pipeline value"
              value={formatCurrency(pipelineValue)}
              sublabel={`${winRate}% win rate`}
              icon="mgc_currency_dollar_line"
              variant="warning"
              link="/crm/opportunities"
              sparkData={opportunityTrend.counts}
              tooltip="Estimated value across opportunities"
            />
          </>
        )}
      </div>

      <div className="grid grid-cols-1 gap-4 xl:grid-cols-12 mb-5">
        <div className={`xl:col-span-7 ${d365Card}`}>
          <div className={`${d365CardHeader} flex-wrap`}>
            <div>
              <h4 className={d365Title}>Revenue & growth</h4>
              <p className={d365Subtitle}>New records — last 6 months</p>
            </div>
            <span className="inline-flex px-2 py-0.5 text-[11px] font-medium rounded-sm bg-[#EFF6FC] text-[#0078D4]">
              Live data
            </span>
          </div>
          <div className="p-4">
            {loading ? (
              <div className="h-[320px] flex items-center justify-center">
                <div className="inline-block h-8 w-8 border-2 border-[#0078D4] border-t-transparent rounded-full animate-spin" role="status" />
              </div>
            ) : (
              <CrmGrowthChart
                labels={customerTrend.labels}
                customers={customerTrend.counts}
                leads={leadTrend.counts}
                opportunities={opportunityTrend.counts}
              />
            )}
          </div>
        </div>

        <div className={`xl:col-span-5 ${d365Card}`}>
          <div className={d365CardHeader}>
            <h4 className={d365Title}>Pipeline by stage</h4>
          </div>
          <div className="p-4">
            {loading ? (
              <div className="h-[280px] flex items-center justify-center">
                <div className="inline-block h-8 w-8 border-2 border-[#0078D4] border-t-transparent rounded-full animate-spin" role="status" />
              </div>
            ) : stageBreakdown.series.length === 0 ? (
              <p className="text-center text-[#605E5C] py-16 mb-0 text-sm">No opportunity stages yet</p>
            ) : (
              <CrmStageDonut labels={stageBreakdown.labels} series={stageBreakdown.series} />
            )}
          </div>
        </div>
      </div>

      <div className="grid grid-cols-1 gap-4 xl:grid-cols-12 mb-5">
        <div className={`xl:col-span-4 ${d365Card}`}>
          <div className={d365CardHeader}>
            <h4 className={d365Title}>Win rate</h4>
          </div>
          <div className="p-4">
            {loading ? (
              <div className="h-[260px] animate-pulse bg-[#EDEBE9] dark:bg-[#3b3a39] rounded-sm" />
            ) : (
              <CrmRadialGauge value={winRate} label="Closed won" />
            )}
          </div>
        </div>

        <div className={`xl:col-span-4 ${d365Card}`}>
          <div className={d365CardHeader}>
            <h4 className={d365Title}>Lead status</h4>
          </div>
          <div className="p-4">
            {loading ? (
              <div className="h-[260px] animate-pulse bg-[#EDEBE9] dark:bg-[#3b3a39] rounded-sm" />
            ) : leadStatusBreakdown.series.length === 0 ? (
              <p className="text-center text-[#605E5C] py-16 mb-0 text-sm">No leads yet</p>
            ) : (
              <CrmPipelineBar labels={leadStatusBreakdown.labels} series={leadStatusBreakdown.series} />
            )}
          </div>
        </div>

        <div className={`xl:col-span-4 ${d365Card}`}>
          <div className={`${d365CardHeader}`}>
            <h4 className={d365Title}>My activities</h4>
            <Link to="/crm/activities" className="text-sm text-[#0078D4] hover:underline font-medium">
              See all
            </Link>
          </div>
          <div className="p-0">
            <SimpleBar className="px-4" style={{ maxHeight: 280 }}>
              {loading ? (
                <p className="text-center text-[#605E5C] py-8 text-sm">Loading...</p>
              ) : upcomingActivities.length === 0 ? (
                <div className="py-8 text-center">
                  <p className="text-[#605E5C] mb-3 text-sm">No scheduled activities</p>
                  <Link to="/crm/activities/new" className="inline-flex items-center text-sm font-medium text-[#0078D4] hover:underline">
                    Schedule one
                  </Link>
                </div>
              ) : (
                <ul className="space-y-0 divide-y divide-[#EDEBE9] dark:divide-[#3b3a39]">
                  {upcomingActivities.map((activity) => (
                    <li key={activity.id} className="py-3 flex gap-3">
                      <span className="flex h-8 w-8 shrink-0 items-center justify-center rounded-sm bg-[#EFF6FC] text-[#0078D4]">
                        <i className="mgc_calendar_line" />
                      </span>
                      <div className="min-w-0 flex-1">
                        <Link
                          to={`/crm/activities/${activity.id}/edit`}
                          className="font-medium text-[#323130] dark:text-[#f3f2f1] hover:text-[#0078D4] hover:underline truncate block text-sm"
                        >
                          {activity.subject}
                        </Link>
                        <p className="text-[11px] text-[#605E5C] mb-0">
                          {formatDate(activity.startDate)} · {activity.activityType || activity.type || "Activity"}
                        </p>
                      </div>
                      <StatusBadge status={activity.status} />
                    </li>
                  ))}
                </ul>
              )}
            </SimpleBar>
          </div>
        </div>
      </div>

      <div className="grid grid-cols-1 gap-4 xl:grid-cols-12">
        <div className={`xl:col-span-8 ${d365Card}`}>
          <div className={d365CardHeader}>
            <h4 className={d365Title}>Recent records</h4>
          </div>
          <div className="p-0">
            <Tab.Group>
              <Tab.List className="flex flex-wrap gap-0 px-4 border-b border-[#EDEBE9] dark:border-[#3b3a39] bg-[#FAF9F8] dark:bg-[#292827]">
                {entityTabs.map((tab) => (
                  <Tab
                    key={tab.title}
                    className={({ selected }) =>
                      `px-4 py-2.5 text-sm font-medium transition-colors outline-none border-b-2 -mb-px ${
                        selected
                          ? "text-[#0078D4] border-[#0078D4] bg-white dark:bg-[#1f1f1f]"
                          : "text-[#605E5C] border-transparent hover:text-[#323130] hover:border-[#C8C6C4]"
                      }`
                    }
                  >
                    {tab.title}
                    <span className="ms-1.5 inline-flex min-w-[1.25rem] justify-center rounded-sm bg-[#EDEBE9] dark:bg-[#3b3a39] px-1.5 text-[11px] text-[#605E5C]">
                      {tab.count}
                    </span>
                  </Tab>
                ))}
              </Tab.List>
              <Tab.Panels>
                {entityTabs.map((tab) => (
                  <Tab.Panel key={tab.title} className="p-0">
                    <div className="flex items-center justify-end gap-2 px-4 py-2.5 border-b border-[#EDEBE9] dark:border-[#3b3a39] bg-[#FAF9F8] dark:bg-[#292827]">
                      <Link to={tab.viewAll} className="inline-flex items-center px-3 py-1 text-sm font-medium text-[#323130] bg-white border border-[#8A8886] hover:bg-[#F3F2F1] rounded-sm transition-colors">
                        View all
                      </Link>
                      <Link to={tab.create} className="inline-flex items-center gap-1 px-3 py-1 text-sm font-medium text-white bg-[#0078D4] hover:bg-[#106EBE] rounded-sm transition-colors">
                        <i className="mgc_add_line me-1" />
                        {tab.createLabel}
                      </Link>
                    </div>
                    <div className="table-responsive min-h-[240px]">
                      {loading ? (
                        <div className="flex items-center justify-center py-16">
                          <div className="inline-block h-8 w-8 border-2 border-[#0078D4] border-t-transparent rounded-full animate-spin" role="status" />
                        </div>
                      ) : (
                        tab.content
                      )}
                    </div>
                  </Tab.Panel>
                ))}
              </Tab.Panels>
            </Tab.Group>
          </div>
        </div>

        <div className="xl:col-span-4 space-y-6">
          <div className={d365Card}>
            <div className={d365CardHeader}>
              <h4 className={d365Title}>Quick create</h4>
            </div>
            <div className="p-3">
              <div className="grid grid-cols-2 gap-2">
                {quickActions.map((action) => (
                  <Link key={action.path} to={action.path} className="inline-flex items-center gap-1.5 px-3 py-2 text-sm font-medium text-[#323130] bg-white border border-[#EDEBE9] hover:bg-[#F3F2F1] hover:border-[#C8C6C4] rounded-sm transition-colors">
                    <i className={`${action.icon} me-1`} />
                    {action.title}
                  </Link>
                ))}
              </div>
            </div>
          </div>

          <div className={d365Card}>
            <div className={d365CardHeader}>
              <h4 className={d365Title}>Related areas</h4>
            </div>
            <div className="p-3 pt-1">
              <div className="flex flex-col gap-1">
                {browseLinks.map((link) => (
                  <Link
                    key={link.path}
                    to={link.path}
                    className="flex items-center justify-between px-3 py-2 rounded-sm hover:bg-[#F3F2F1] dark:hover:bg-[#292827] transition-colors border-b border-[#EDEBE9] dark:border-[#3b3a39] last:border-0"
                  >
                    <span className="text-sm font-medium">{link.title}</span>
                    {"count" in link && link.count != null && (
                      <span className="inline-flex min-w-[1.25rem] justify-center px-1.5 text-[11px] font-medium rounded-sm bg-[#EFF6FC] text-[#0078D4]">
                        {link.count}
                      </span>
                    )}
                  </Link>
                ))}
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default CrmDashboard;
