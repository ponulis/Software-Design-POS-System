import { useState } from 'react';
import { useDashboard } from '../hooks/useDashboard';
import MetricCard from '../components/dashboard/MetricCard';
import RecentOrders from '../components/dashboard/RecentOrders';
import RecentAppointments from '../components/dashboard/RecentAppointments';

export default function Home() {
  const [dateRange, setDateRange] = useState({ startDate: null, endDate: null });
  const { dashboardData, loading, error, refreshDashboard } = useDashboard(
    dateRange.startDate,
    dateRange.endDate
  );

  if (loading && !dashboardData) {
    return (
      <div className="min-h-screen bg-gray-100 p-6 flex items-center justify-center">
        <div className="text-center">
          <div className="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600 mb-4"></div>
          <p className="text-gray-600">Loading dashboard...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-100 p-6">
      <div className="max-w-7xl mx-auto">
        <div className="mb-6 flex justify-between items-center">
          <div>
            <h2 className="text-sm font-semibold text-gray-500 uppercase mb-1">
              Dashboard
            </h2>
            <h1 className="text-2xl font-bold text-gray-900">Overview</h1>
          </div>
          <div className="flex gap-2">
            <input
              type="date"
              value={dateRange.startDate ? dateRange.startDate.toISOString().split('T')[0] : ''}
              onChange={(e) => setDateRange({
                ...dateRange,
                startDate: e.target.value ? new Date(e.target.value) : null
              })}
              className="px-3 py-2 border border-gray-300 rounded-lg text-sm"
            />
            <input
              type="date"
              value={dateRange.endDate ? dateRange.endDate.toISOString().split('T')[0] : ''}
              onChange={(e) => setDateRange({
                ...dateRange,
                endDate: e.target.value ? new Date(e.target.value) : null
              })}
              className="px-3 py-2 border border-gray-300 rounded-lg text-sm"
            />
            <button
              onClick={refreshDashboard}
              className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition text-sm"
            >
              Refresh
            </button>
          </div>
        </div>

        {error && (
          <div className="mb-4 p-4 bg-red-50 border border-red-200 rounded-lg">
            <p className="text-red-800 text-sm">{error}</p>
          </div>
        )}

        {dashboardData && (
          <>
            {/* Revenue Metrics */}
            <div className="mb-6">
              <h3 className="text-lg font-semibold text-gray-900 mb-4">Revenue</h3>
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
                <MetricCard
                  title="Today"
                  value={`${dashboardData.todayRevenue.toFixed(2)}€`}
                  subtitle="Revenue today"
                />
                <MetricCard
                  title="This Week"
                  value={`${dashboardData.thisWeekRevenue.toFixed(2)}€`}
                  subtitle="Revenue this week"
                />
                <MetricCard
                  title="This Month"
                  value={`${dashboardData.thisMonthRevenue.toFixed(2)}€`}
                  subtitle="Revenue this month"
                />
                <MetricCard
                  title="Total Revenue"
                  value={`${dashboardData.totalRevenue.toFixed(2)}€`}
                  subtitle="All time revenue"
                />
              </div>
            </div>

            {/* Order Metrics */}
            <div className="mb-6">
              <h3 className="text-lg font-semibold text-gray-900 mb-4">Orders</h3>
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
                <MetricCard
                  title="Today"
                  value={dashboardData.todayOrders}
                  subtitle="Orders today"
                />
                <MetricCard
                  title="This Week"
                  value={dashboardData.thisWeekOrders}
                  subtitle="Orders this week"
                />
                <MetricCard
                  title="Pending"
                  value={dashboardData.pendingOrders}
                  subtitle="Pending orders"
                />
                <MetricCard
                  title="Total Orders"
                  value={dashboardData.totalOrders}
                  subtitle="All time orders"
                />
              </div>
            </div>

            {/* Payment Methods Breakdown */}
            {dashboardData.paymentMethods && (
              <div className="mb-6">
                <h3 className="text-lg font-semibold text-gray-900 mb-4">Payment Methods</h3>
                <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                  <MetricCard
                    title="Cash"
                    value={`${dashboardData.paymentMethods.cashTotal.toFixed(2)}€`}
                    subtitle={`${dashboardData.paymentMethods.cashCount} transactions`}
                  />
                  <MetricCard
                    title="Card"
                    value={`${dashboardData.paymentMethods.cardTotal.toFixed(2)}€`}
                    subtitle={`${dashboardData.paymentMethods.cardCount} transactions`}
                  />
                  <MetricCard
                    title="Gift Card"
                    value={`${dashboardData.paymentMethods.giftCardTotal.toFixed(2)}€`}
                    subtitle={`${dashboardData.paymentMethods.giftCardCount} transactions`}
                  />
                </div>
              </div>
            )}

            {/* Appointments Metrics */}
            <div className="mb-6">
              <h3 className="text-lg font-semibold text-gray-900 mb-4">Appointments</h3>
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
                <MetricCard
                  title="Today"
                  value={dashboardData.todayAppointments}
                  subtitle="Appointments today"
                />
                <MetricCard
                  title="Upcoming"
                  value={dashboardData.upcomingAppointments}
                  subtitle="Upcoming appointments"
                />
                <MetricCard
                  title="Completed"
                  value={dashboardData.completedAppointments}
                  subtitle="Completed appointments"
                />
                <MetricCard
                  title="Total"
                  value={dashboardData.totalAppointments}
                  subtitle="All appointments"
                />
              </div>
            </div>

            {/* Recent Data */}
            <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
              <RecentOrders orders={dashboardData.recentOrders || []} />
              <RecentAppointments appointments={dashboardData.recentAppointments || []} />
            </div>
          </>
        )}
      </div>
    </div>
  );
}
