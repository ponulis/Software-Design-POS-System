import React from 'react';

export default function MetricCard({ title, value, subtitle, icon, trend, trendValue }) {
  return (
    <div className="bg-white rounded-lg shadow p-6 border border-gray-200">
      <div className="flex items-center justify-between mb-2">
        <h3 className="text-sm font-medium text-gray-600 uppercase">{title}</h3>
        {icon && <div className="text-gray-400">{icon}</div>}
      </div>
      <div className="mt-2">
        <p className="text-3xl font-bold text-gray-900">{value}</p>
        {subtitle && (
          <p className="text-sm text-gray-500 mt-1">{subtitle}</p>
        )}
        {trend && trendValue && (
          <div className={`mt-2 text-xs flex items-center ${
            trend === 'up' ? 'text-green-600' : trend === 'down' ? 'text-red-600' : 'text-gray-600'
          }`}>
            <span>{trend === 'up' ? '↑' : trend === 'down' ? '↓' : '→'}</span>
            <span className="ml-1">{trendValue}</span>
          </div>
        )}
      </div>
    </div>
  );
}
