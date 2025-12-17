import React from "react";

export default function PaymentsList({ orders = [], selected, onSelect }) {
  const formatDate = (dateString) => {
    if (!dateString) return 'N/A';
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
    });
  };

  const formatTime = (dateString) => {
    if (!dateString) return '';
    const date = new Date(dateString);
    return date.toLocaleTimeString('en-US', {
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  const getStatusColor = (status) => {
    switch (status) {
      case 'Paid':
        return 'bg-green-100 text-green-700';
      case 'Draft':
        return 'bg-gray-100 text-gray-700';
      case 'Pending':
        return 'bg-yellow-100 text-yellow-700';
      case 'Placed':
        return 'bg-blue-100 text-blue-700';
      case 'Cancelled':
        return 'bg-red-100 text-red-700';
      default:
        return 'bg-gray-100 text-gray-700';
    }
  };

  return (
    <div className="space-y-4">
      {orders.length === 0 ? (
        <div className="text-center py-12 text-gray-400">
          <p>No orders found.</p>
        </div>
      ) : (
        orders.map((order) => (
          <div
            key={order.id}
            onClick={() => onSelect(order)}
            className={`grid grid-cols-5 items-center p-4 rounded-lg border cursor-pointer transition hover:bg-blue-50 
              ${selected?.id === order.id ? "ring-2 ring-blue-400 bg-blue-100" : ""} 
              bg-white border-blue-100`}
          >
            <div>
              <p className="font-semibold text-gray-800">{formatDate(order.createdAt)}</p>
              <p className="text-xs text-gray-500">{formatTime(order.createdAt)}</p>
            </div>
            <p className="font-medium">{order.total?.toFixed(2) || '0.00'}€</p>
            <p className="text-sm text-gray-600">
              {order.items?.length || 0} {order.items?.length === 1 ? 'item' : 'items'}
            </p>
            <p className="text-sm text-gray-600">Order #{order.id}</p>
            <span
              className={`px-3 py-1 text-sm rounded-lg font-medium ${getStatusColor(order.status)}`}
            >
              {order.status || 'Draft'}
            </span>
          </div>
        ))
      )}
    </div>
  );
}
