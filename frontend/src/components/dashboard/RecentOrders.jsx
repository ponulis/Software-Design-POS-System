import React from 'react';
import { useNavigate } from 'react-router-dom';

export default function RecentOrders({ orders = [] }) {
  const navigate = useNavigate();

  const formatDate = (dateString) => {
    if (!dateString) return 'N/A';
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  const getStatusColor = (status) => {
    switch (status) {
      case 'Paid':
        return 'bg-green-100 text-green-700';
      case 'Placed':
        return 'bg-blue-100 text-blue-700';
      case 'Draft':
        return 'bg-gray-100 text-gray-700';
      case 'Cancelled':
        return 'bg-red-100 text-red-700';
      default:
        return 'bg-gray-100 text-gray-700';
    }
  };

  if (orders.length === 0) {
    return (
      <div className="bg-white rounded-lg shadow p-6 border border-gray-200">
        <h3 className="text-lg font-semibold text-gray-900 mb-4">Recent Orders</h3>
        <p className="text-center text-gray-400 py-8">No recent orders</p>
      </div>
    );
  }

  return (
    <div className="bg-white rounded-lg shadow border border-gray-200">
      <div className="p-4 border-b border-gray-200 flex justify-between items-center">
        <h3 className="text-lg font-semibold text-gray-900">Recent Orders</h3>
        <button
          onClick={() => navigate('/payments')}
          className="text-sm text-blue-600 hover:text-blue-800 font-medium"
        >
          View All
        </button>
      </div>
      <div className="divide-y divide-gray-200">
        {orders.slice(0, 5).map((order) => (
          <div
            key={order.id}
            onClick={() => navigate(`/payments`)}
            className="p-4 hover:bg-gray-50 cursor-pointer transition"
          >
            <div className="flex justify-between items-start">
              <div>
                <p className="font-medium text-gray-900">Order #{order.id}</p>
                <p className="text-sm text-gray-600 mt-1">
                  {formatDate(order.createdAt)} • Spot {order.spotId}
                </p>
                {order.createdByName && (
                  <p className="text-xs text-gray-500 mt-1">By {order.createdByName}</p>
                )}
              </div>
              <div className="text-right">
                <p className="font-semibold text-gray-900">{order.total.toFixed(2)}€</p>
                <span className={`inline-block mt-1 px-2 py-1 text-xs rounded font-medium ${getStatusColor(order.status)}`}>
                  {order.status}
                </span>
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
