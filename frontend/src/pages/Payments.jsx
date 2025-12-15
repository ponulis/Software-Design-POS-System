import { useState } from "react";
import PaymentsList from "../components/payments/PaymentsList";
import { useOrders } from "../hooks/useOrders";
import PaymentDetails from "../components/payments/PaymentDetails";

export default function Payments() {
  const {
    orders,
    selectedOrder,
    loading,
    error,
    selectOrder,
    refreshOrders,
  } = useOrders();

  const [filters, setFilters] = useState({
    status: '',
    startDate: '',
    endDate: '',
  });

  const handleFilterChange = (key, value) => {
    const newFilters = { ...filters, [key]: value };
    setFilters(newFilters);
    // TODO: Apply filters to API call
  };

  if (loading && orders.length === 0) {
    return (
      <div className="min-h-screen bg-gray-100 p-6 flex items-center justify-center">
        <div className="text-center">
          <div className="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600 mb-4"></div>
          <p className="text-gray-600">Loading orders...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-100 p-6 flex gap-6">
      <div className="w-2/3 bg-white shadow rounded-xl p-6">
        <div className="flex justify-between items-center mb-6">
          <div>
            <h2 className="text-sm font-semibold text-gray-500 uppercase mb-1">
              Orders & Payments
            </h2>
            <h1 className="text-2xl font-bold">Orders</h1>
          </div>
          <button
            onClick={refreshOrders}
            className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition"
          >
            Refresh
          </button>
        </div>

        {error && (
          <div className="mb-4 p-4 bg-red-50 border border-red-200 rounded-lg">
            <p className="text-red-800 text-sm">{error}</p>
          </div>
        )}

        {/* Filter controls */}
        <div className="mb-4 flex gap-4">
          <select
            value={filters.status}
            onChange={(e) => handleFilterChange('status', e.target.value)}
            className="px-3 py-2 border border-gray-300 rounded-lg text-sm"
          >
            <option value="">All Statuses</option>
            <option value="Draft">Draft</option>
            <option value="Placed">Placed</option>
            <option value="Paid">Paid</option>
            <option value="Cancelled">Cancelled</option>
          </select>
          <input
            type="date"
            value={filters.startDate}
            onChange={(e) => handleFilterChange('startDate', e.target.value)}
            className="px-3 py-2 border border-gray-300 rounded-lg text-sm"
            placeholder="Start Date"
          />
          <input
            type="date"
            value={filters.endDate}
            onChange={(e) => handleFilterChange('endDate', e.target.value)}
            className="px-3 py-2 border border-gray-300 rounded-lg text-sm"
            placeholder="End Date"
          />
        </div>

        <PaymentsList
          orders={orders}
          selected={selectedOrder}
          onSelect={selectOrder}
        />
      </div>

      <div className="w-1/3 bg-white shadow rounded-xl p-6">
        <PaymentDetails order={selectedOrder} />
      </div>
    </div>
  );
}
