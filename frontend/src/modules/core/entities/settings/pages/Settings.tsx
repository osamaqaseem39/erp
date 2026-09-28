import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { PageBreadcrumb } from '../../../../../components';

interface SettingsSection {
  id: string;
  title: string;
  description: string;
  icon: string;
  path: string;
}

const Settings: React.FC = () => {
  const navigate = useNavigate();
  const [activeSection, setActiveSection] = useState<string>('general');

  const settingsSections: SettingsSection[] = [
    {
      id: 'users',
      title: 'User Management',
      description: 'Manage users and their access',
      icon: '👤',
      path: '/core/settings/users',
    },
    {
      id: 'roles',
      title: 'Role Management',
      description: 'Manage roles and permissions (RBAC)',
      icon: '🔐',
      path: '/core/settings/roles',
    },
    {
      id: 'tenants',
      title: 'Tenant Management',
      description: 'Multi-tenant configuration and management',
      icon: '🏘️',
      path: '/core/settings/tenants',
    },
    {
      id: 'system-settings',
      title: 'System Settings',
      description: 'System configuration and preferences',
      icon: '⚙️',
      path: '/core/settings/system-settings',
    },
    {
      id: 'audit-logs',
      title: 'Audit Logs',
      description: 'View system audit trail',
      icon: '📋',
      path: '/core/settings/audit-logs',
    },
    {
      id: 'system-logs',
      title: 'System Logs',
      description: 'View system logs and errors',
      icon: '📊',
      path: '/core/settings/system-logs',
    },
  ];

  const handleSectionClick = (section: SettingsSection) => {
    navigate(section.path);
  };

  return (
    <>
      <PageBreadcrumb title="Settings" />
      <div className="container-fluid">
        <div className="grid grid-cols-1 lg:grid-cols-4 gap-6">
          {/* Settings Sidebar */}
          <div className="lg:col-span-1">
            <div className="bg-white dark:bg-gray-800 rounded-lg shadow">
              <div className="p-4 border-b border-gray-200 dark:border-gray-700">
                <h2 className="text-xl font-semibold text-gray-900 dark:text-white">
                  Settings
                </h2>
              </div>
              <nav className="p-2">
                {settingsSections.map((section) => (
                  <button
                    key={section.id}
                    onClick={() => handleSectionClick(section)}
                    className="w-full text-left px-4 py-3 rounded-lg mb-1 transition-colors text-gray-700 dark:text-gray-300 hover:bg-gray-100 dark:hover:bg-gray-700"
                  >
                    <div className="flex items-center">
                      <span className="text-xl mr-3">{section.icon}</span>
                      <div>
                        <div className="font-medium">{section.title}</div>
                        <div className="text-xs opacity-75">{section.description}</div>
                      </div>
                    </div>
                  </button>
                ))}
              </nav>
            </div>
          </div>

          {/* Settings Content */}
          <div className="lg:col-span-3">
            <div className="bg-white dark:bg-gray-800 rounded-lg shadow p-6">
              <h3 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">
                Core Settings
              </h3>
              <p className="text-gray-600 dark:text-gray-400 mb-4">
                Select a section from the sidebar to manage different aspects of the system.
              </p>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                {settingsSections.map((section) => (
                  <div
                    key={section.id}
                    onClick={() => handleSectionClick(section)}
                    className="p-4 border rounded-lg cursor-pointer hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors"
                  >
                    <div className="flex items-center mb-2">
                      <span className="text-2xl mr-3">{section.icon}</span>
                      <h4 className="font-semibold text-gray-900 dark:text-white">{section.title}</h4>
                    </div>
                    <p className="text-sm text-gray-600 dark:text-gray-400">{section.description}</p>
                  </div>
                ))}
              </div>
            </div>
          </div>
        </div>
      </div>
    </>
  );
};

export default Settings;

