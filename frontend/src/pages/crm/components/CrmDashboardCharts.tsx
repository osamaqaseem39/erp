import React, { useMemo } from "react";
import type { ApexOptions } from "apexcharts";
import ReactApexChart from "react-apexcharts";
import { D365, D365_CHART_COLORS } from "../../../modules/crm/theme/crmDynamicsTheme";

const chartBase: ApexOptions = {
  chart: {
    toolbar: { show: false },
    fontFamily: "'Segoe UI', 'Segoe UI Web', system-ui, sans-serif",
  },
  grid: {
    borderColor: "#EDEBE9",
    strokeDashArray: 0,
    xaxis: { lines: { show: false } },
    yaxis: { lines: { show: true } },
  },
  dataLabels: { enabled: false },
  xaxis: {
    labels: { style: { colors: D365.textSecondary, fontSize: "11px" } },
    axisBorder: { show: false },
    axisTicks: { show: false },
  },
  yaxis: {
    labels: { style: { colors: D365.textSecondary, fontSize: "11px" } },
  },
  legend: {
    fontSize: "12px",
    fontWeight: 400,
    labels: { colors: D365.text },
    markers: { size: 6, strokeWidth: 0 },
  },
  tooltip: {
    theme: "light",
    style: { fontSize: "12px" },
  },
};

interface GrowthChartProps {
  labels: string[];
  customers: number[];
  leads: number[];
  opportunities: number[];
}

export const CrmGrowthChart: React.FC<GrowthChartProps> = ({
  labels,
  customers,
  leads,
  opportunities,
}) => {
  const options = useMemo<ApexOptions>(
    () => ({
      ...chartBase,
      chart: { ...chartBase.chart, height: 300, type: "area" },
      colors: [D365.blue, D365.teal, D365.purple],
      stroke: { curve: "smooth", width: 2 },
      fill: {
        type: "gradient",
        gradient: { shadeIntensity: 0.5, opacityFrom: 0.25, opacityTo: 0.02, stops: [0, 95, 100] },
      },
      xaxis: { ...chartBase.xaxis, categories: labels },
      yaxis: {
        ...chartBase.yaxis,
        labels: { ...chartBase.yaxis?.labels, formatter: (v) => `${Math.round(v)}` },
      },
      legend: { ...chartBase.legend, position: "top", horizontalAlign: "left" },
      series: [
        { name: "Customers", data: customers },
        { name: "Leads", data: leads },
        { name: "Opportunities", data: opportunities },
      ],
    }),
    [labels, customers, leads, opportunities]
  );

  return (
    <ReactApexChart
      className="apex-charts"
      options={options}
      series={options.series}
      type="area"
      height={300}
    />
  );
};

interface StageDonutProps {
  labels: string[];
  series: number[];
}

export const CrmStageDonut: React.FC<StageDonutProps> = ({ labels, series }) => {
  const options = useMemo<ApexOptions>(
    () => ({
      ...chartBase,
      chart: { ...chartBase.chart, height: 280, type: "donut" },
      labels,
      colors: D365_CHART_COLORS,
      legend: { ...chartBase.legend, position: "bottom", horizontalAlign: "center" },
      plotOptions: {
        pie: {
          donut: {
            size: "68%",
            labels: {
              show: true,
              name: { fontSize: "12px", color: D365.textSecondary },
              value: { fontSize: "22px", fontWeight: 600, color: D365.text },
              total: {
                show: true,
                label: "Total",
                color: D365.textSecondary,
                fontSize: "12px",
                formatter: (w) => `${w.globals.seriesTotals.reduce((a: number, b: number) => a + b, 0)}`,
              },
            },
          },
        },
      },
      series,
    }),
    [labels, series]
  );

  return (
    <ReactApexChart
      className="apex-charts"
      options={options}
      series={series}
      type="donut"
      height={280}
    />
  );
};

interface RadialGaugeProps {
  value: number;
  label: string;
}

export const CrmRadialGauge: React.FC<RadialGaugeProps> = ({ value, label }) => {
  const options = useMemo<ApexOptions>(
    () => ({
      chart: {
        height: 260,
        type: "radialBar",
        fontFamily: "'Segoe UI', 'Segoe UI Web', system-ui, sans-serif",
      },
      plotOptions: {
        radialBar: {
          hollow: { size: "65%" },
          track: { background: "#EDEBE9", strokeWidth: "100%" },
          dataLabels: {
            name: { fontSize: "12px", color: D365.textSecondary, offsetY: 20 },
            value: {
              fontSize: "26px",
              fontWeight: 600,
              color: D365.text,
              offsetY: -6,
              formatter: (v) => `${Math.round(Number(v))}%`,
            },
          },
        },
      },
      colors: [D365.blue],
      labels: [label],
      series: [value],
    }),
    [value, label]
  );

  return (
    <ReactApexChart
      className="apex-charts"
      options={options}
      series={[value]}
      type="radialBar"
      height={260}
    />
  );
};

interface SparklineProps {
  data: number[];
  color: string;
}

export const CrmSparkline: React.FC<SparklineProps> = ({ data, color }) => {
  const options = useMemo<ApexOptions>(
    () => ({
      chart: {
        type: "area",
        height: 40,
        sparkline: { enabled: true },
        animations: { enabled: true },
      },
      stroke: { curve: "smooth", width: 1.5 },
      fill: {
        type: "gradient",
        gradient: { shadeIntensity: 0.5, opacityFrom: 0.3, opacityTo: 0.02 },
      },
      colors: [color],
      tooltip: { enabled: false },
      series: [{ data }],
    }),
    [data, color]
  );

  return <ReactApexChart options={options} series={[{ data }]} type="area" height={40} />;
};

interface PipelineBarProps {
  labels: string[];
  series: number[];
}

export const CrmPipelineBar: React.FC<PipelineBarProps> = ({ labels, series }) => {
  const options = useMemo<ApexOptions>(
    () => ({
      ...chartBase,
      chart: { ...chartBase.chart, height: 260, type: "bar" },
      plotOptions: {
        bar: { horizontal: true, borderRadius: 2, barHeight: "60%" },
      },
      colors: [D365.blue],
      xaxis: { ...chartBase.xaxis, categories: labels },
      series: [{ name: "Records", data: series }],
    }),
    [labels, series]
  );

  return (
    <ReactApexChart
      className="apex-charts"
      options={options}
      series={options.series}
      type="bar"
      height={260}
    />
  );
};
