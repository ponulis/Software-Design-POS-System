import React from 'react';

export default function DiscountList({ discounts = [], onEdit, onDelete }) {
  const formatDate = (dateString) => {
    if (!dateString) return 'N/A';
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
    });
  };

  const isCurrentlyValid = (discount) => {
    if (!discount.isActive) return false;
    const now = new Date();
    const from = discount.validFrom ? new Date(discount.validFrom) : null;
    const to = discount.validTo ? new Date(discount.validTo) : null;
    
    if (from && now < from) return false;
    if (to && now > to) return false;
    return true;
  };

  const formatValue = (discount) => {
    if (discount.type === 'Percentage') {
      return `${discount.value}%`;
    }
    return `${discount.value.toFixed(2)}€`;
  };

  if (discounts.length === 0) {
    return (
      <div className="text-center py-12 text-gray-400">
        <p>No discounts found.</p>
        <p className="text-sm mt-2">Click "Add Discount" to create one.</p>
      </div>
    );
  }

  return (
    <div className="bg-white rounded-lg shadow">
      <div className="p-4 border-b border-gray-200">
        <h3 className="font-semibold text-gray-800">Discounts ({discounts.length})</h3>
      </div>
      <div className="divide-y divide-gray-200">
        {discounts.map((discount) => (
          <div
            key={discount.id}
            className="p-4 hover:bg-gray-50 transition"
          >
            <div className="flex justify-between items-start">
              <div className="flex-1">
                <div className="flex items-center gap-2 mb-1">
                  <h3 className="font-semibold text-gray-900">{discount.name}</h3>
                  {isCurrentlyValid(discount) && (
                    <span className="px-2 py-0.5 text-xs bg-green-100 text-green-700 rounded">
                      Active
                    </span>
                  )}
                  {!discount.isActive && (
                    <span className="px-2 py-0.5 text-xs bg-gray-100 text-gray-700 rounded">
                      Inactive
                    </span>
                  )}
                  <span className={`px-2 py-0.5 text-xs rounded ${
                    discount.type === 'Percentage' 
                      ? 'bg-blue-100 text-blue-700' 
                      : 'bg-purple-100 text-purple-700'
                  }`}>
                    {discount.type}
                  </span>
                </div>
                {discount.description && (
                  <p className="text-sm text-gray-600 mb-2">{discount.description}</p>
                )}
                <div className="text-sm text-gray-600 space-y-1">
                  <p>Value: <span className="font-medium">{formatValue(discount)}</span></p>
                  {(discount.validFrom || discount.validTo) && (
                    <p>
                      Valid: {discount.validFrom ? formatDate(discount.validFrom) : 'Always'} - {discount.validTo ? formatDate(discount.validTo) : 'No expiry'}
                    </p>
                  )}
                </div>
              </div>
              <div className="flex gap-2 ml-4">
                <button
                  onClick={() => onEdit(discount)}
                  className="px-3 py-1 text-sm bg-blue-100 text-blue-700 rounded hover:bg-blue-200 transition"
                >
                  Edit
                </button>
                <button
                  onClick={() => {
                    if (window.confirm(`Are you sure you want to delete "${discount.name}"?`)) {
                      onDelete(discount.id);
                    }
                  }}
                  className="px-3 py-1 text-sm bg-red-100 text-red-700 rounded hover:bg-red-200 transition"
                >
                  Delete
                </button>
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
