import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useSelector } from 'react-redux';
import { RootState } from '../../redux/store';

interface Module {
  id: string;
  name: string;
  description: string;
  icon: string;
  color: string;
  path: string;
  enabled: boolean;
}

const ModuleSelection: React.FC = () => {
  const navigate = useNavigate();
  const user = useSelector((state: RootState) => state.Auth.user);

  const modules: Module[] = [
    {
      id: 'core',
      name: 'Core / Settings',
      description: 'System settings, configuration, and core information',
      icon: '⚙️',
      color: 'bg-gray-500',
      path: '/core/settings',
      enabled: true,
    },
    {
      id: 'admin',
      name: 'Admin',
      description: 'User management, roles, and system administration',
      icon: '👤',
      color: 'bg-purple-500',
      path: '/erp/admin/users',
      enabled: true,
    },
    {
      id: 'hrm',
      name: 'HRM',
      description: 'Human Resource Management - Employees, Payroll, Leave',
      icon: '👥',
      color: 'bg-blue-500',
      path: '/hrm/dashboard',
      enabled: true,
    },
    {
      id: 'finance',
      name: 'Finance',
      description: 'Financial management, Invoices, Payments, Accounts',
      icon: '💰',
      color: 'bg-green-500',
      path: '/finance/dashboard',
      enabled: true,
    },
    {
      id: 'inventory',
      name: 'Inventory',
      description: 'Inventory and warehouse management',
      icon: '📦',
      color: 'bg-orange-500',
      path: '/inventory/dashboard',
      enabled: true,
    },
    {
      id: 'crm',
      name: 'CRM',
      description: 'Customer Relationship Management - Leads, Opportunities',
      icon: '🤝',
      color: 'bg-indigo-500',
      path: '/crm/dashboard',
      enabled: true,
    },
    {
      id: 'manufacturing',
      name: 'Manufacturing',
      description: 'Manufacturing and production management',
      icon: '🏭',
      color: 'bg-yellow-500',
      path: '/manufacturing/dashboard',
      enabled: true,
    },
    {
      id: 'procurement',
      name: 'Procurement',
      description: 'Procurement and vendor management',
      icon: '🛒',
      color: 'bg-teal-500',
      path: '/procurement/dashboard',
      enabled: true,
    },
    {
      id: 'projects',
      name: 'Projects',
      description: 'Project management and tracking',
      icon: '📊',
      color: 'bg-pink-500',
      path: '/projects/dashboard',
      enabled: true,
    },
    {
      id: 'reports',
      name: 'Reports',
      description: 'Reporting and analytics',
      icon: '📈',
      color: 'bg-red-500',
      path: '/reports/dashboard',
      enabled: true,
    },
  ];

  const handleModuleClick = (module: Module) => {
    if (module.enabled) {
      navigate(module.path);
    }
  };

  return (
    <div className="min-h-screen bg-gray-50 dark:bg-gray-900 py-12 px-4 sm:px-6 lg:px-8">
      <div className="max-w-7xl mx-auto">
        {/* Header */}
        <div className="text-center mb-12">
          <h1 className="text-4xl font-bold text-gray-900 dark:text-white mb-4">
            Welcome to ERP System
          </h1>
          {user && (
            <p className="text-lg text-gray-600 dark:text-gray-400">
              Welcome back, <span className="font-semibold">{user.username || user.email}</span>
            </p>
          )}
          <p className="text-gray-500 dark:text-gray-500 mt-2">
            Select a module to get started
          </p>
        </div>

        {/* Module Grid */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
          {modules.map((module) => (
            <div
              key={module.id}
              onClick={() => handleModuleClick(module)}
              className={`
                relative bg-white dark:bg-gray-800 rounded-lg shadow-md hover:shadow-xl 
                transition-all duration-300 cursor-pointer transform hover:-translate-y-1
                border border-gray-200 dark:border-gray-700
                ${module.enabled ? 'opacity-100' : 'opacity-50 cursor-not-allowed'}
              `}
            >
              <div className="p-6">
                {/* Icon */}
                <div className={`w-16 h-16 ${module.color} rounded-lg flex items-center justify-center mb-4 text-3xl`}>
                  {module.icon}
                </div>

                {/* Module Name */}
                <h3 className="text-xl font-semibold text-gray-900 dark:text-white mb-2">
                  {module.name}
                </h3>

                {/* Description */}
                <p className="text-sm text-gray-600 dark:text-gray-400 mb-4">
                  {module.description}
                </p>

                {/* Status Badge */}
                {module.enabled ? (
                  <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200">
                    Available
                  </span>
                ) : (
                  <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-300">
                    Coming Soon
                  </span>
                )}

                {/* Hover Arrow */}
                <div className="absolute top-4 right-4 opacity-0 group-hover:opacity-100 transition-opacity">
                  <svg
                    className="w-6 h-6 text-gray-400"
                    fill="none"
                    stroke="currentColor"
                    viewBox="0 0 24 24"
                  >
                    <path
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      strokeWidth={2}
                      d="M9 5l7 7-7 7"
                    />
                  </svg>
                </div>
              </div>
            </div>
          ))}
        </div>

        {/* Footer Info */}
        <div className="mt-12 text-center">
          <p className="text-sm text-gray-500 dark:text-gray-400">
            Need help? Contact your system administrator
          </p>
        </div>
      </div>
    </div>
  );
};

export default ModuleSelection;

