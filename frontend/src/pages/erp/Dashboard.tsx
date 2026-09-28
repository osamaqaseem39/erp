import React, { useEffect, useState } from "react";
import { PageBreadcrumb } from "../../components";
// Use Core module services to avoid duplication
import { userService } from "../../modules/core/entities/user/services";
import { roleService } from "../../modules/core/entities/role/services";
import { tenantService } from "../../modules/core/entities/tenant/services";

const Dashboard = () => {
  const [stats, setStats] = useState([
    {
      title: 'Total Tenants',
      value: '0',
      icon: 'mgc_building_2_line',
      color: 'bg-primary',
      change: '+0%',
    },
    {
      title: 'Active Users',
      value: '0',
      icon: 'mgc_user_3_line',
      color: 'bg-success',
      change: '+0%',
    },
    {
      title: 'Total Roles',
      value: '0',
      icon: 'mgc_user_group_line',
      color: 'bg-info',
      change: '+0%',
    },
    {
      title: 'System Status',
      value: 'Active',
      icon: 'mgc_check_circle_line',
      color: 'bg-success',
      change: 'All Systems',
    },
  ]);

  useEffect(() => {
    loadStats();
  }, []);

  const loadStats = async () => {
    try {
      const [tenants, users, roles] = await Promise.all([
        tenantService.getAll().catch(() => []),
        userService.getAll(undefined, true).catch(() => []),
        roleService.getAll(true).catch(() => []),
      ]);

      setStats([
        {
          title: 'Total Tenants',
          value: tenants.length.toString(),
          icon: 'mgc_building_2_line',
          color: 'bg-primary',
          change: '+0%',
        },
        {
          title: 'Active Users',
          value: users.length.toString(),
          icon: 'mgc_user_3_line',
          color: 'bg-success',
          change: '+0%',
        },
        {
          title: 'Total Roles',
          value: roles.length.toString(),
          icon: 'mgc_user_group_line',
          color: 'bg-info',
          change: '+0%',
        },
        {
          title: 'System Status',
          value: 'Active',
          icon: 'mgc_check_circle_line',
          color: 'bg-success',
          change: 'All Systems',
        },
      ]);
    } catch (error) {
      console.error('Failed to load dashboard stats', error);
    }
  };

  return (
    <>
      <PageBreadcrumb title="Dashboard" subName="ERP" />
      <div className="grid grid-cols-1 gap-6 md:grid-cols-2 lg:grid-cols-4">
        {stats.map((stat, index) => (
          <div key={index} className="card">
            <div className="card-body">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-gray-500 dark:text-gray-400 mb-1">{stat.title}</p>
                  <h3 className="text-2xl font-semibold">{stat.value}</h3>
                  <p className="text-sm text-success mt-1">
                    <i className="mgc_arrow_up_line"></i> {stat.change} from last month
                  </p>
                </div>
                <div className={`${stat.color} text-white p-3 rounded-lg`}>
                  <i className={`${stat.icon} text-2xl`}></i>
                </div>
              </div>
            </div>
          </div>
        ))}
      </div>

      <div className="grid grid-cols-1 gap-6 lg:grid-cols-2 mt-6">
        <div className="card">
          <div className="card-header">
            <h4 className="card-title">Quick Actions</h4>
          </div>
          <div className="card-body">
            <div className="grid grid-cols-2 gap-4">
              <a href="/erp/admin/users/new" className="btn btn-soft-primary">
                <i className="mgc_user_3_line me-2"></i>Add User
              </a>
              <a href="/erp/admin/roles/new" className="btn btn-soft-info">
                <i className="mgc_user_group_line me-2"></i>Add Role
              </a>
              <a href="/erp/tenants/new" className="btn btn-soft-warning">
                <i className="mgc_building_2_line me-2"></i>Add Tenant
              </a>
              <a href="/erp/admin/system-settings" className="btn btn-soft-success">
                <i className="mgc_settings_3_line me-2"></i>Settings
              </a>
            </div>
          </div>
        </div>
        <div className="card">
          <div className="card-header">
            <h4 className="card-title">System Information</h4>
          </div>
          <div className="card-body">
            <div className="space-y-3">
              <div className="flex justify-between items-center">
                <span className="text-gray-600 dark:text-gray-400">Core Module</span>
                <span className="badge badge-pill badge-soft-success">Active</span>
              </div>
              <div className="flex justify-between items-center">
                <span className="text-gray-600 dark:text-gray-400">Admin Module</span>
                <span className="badge badge-pill badge-soft-success">Active</span>
              </div>
              <div className="flex justify-between items-center">
                <span className="text-gray-600 dark:text-gray-400">Multi-tenancy</span>
                <span className="badge badge-pill badge-soft-success">Enabled</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </>
  );
};

export default Dashboard;

