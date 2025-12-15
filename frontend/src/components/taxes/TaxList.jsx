import React from 'react';

export default function TaxList({ taxes = [], onEdit, onDelete }) {
  const formatDate = (dateString) => {
    if (!dateString) return 'N/A';
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
    });
  };

  const isCurrentlyActive = (tax) => {
    if (!tax.isActive) return false;
    const now = new Date();
    const from = new Date(tax.effectiveFrom);
    const to = tax.effectiveTo ? new Date(tax.effectiveTo) : null;
    
    return now >= from && (!to || now <= to);
  };

  if (taxes.length === 0) {
    return (
      <div className="text-center py-12 text-gray-400">
        <p>No tax rules found.</p>
        <p className="text-sm mt-2">Click "Add Tax Rule" to create one.</p>
      </div>
    );
  }

  return (
    <div className="space-y-3">
      {taxes.map((tax) => (
        <div
          key={tax.id}
          className="p-4 rounded-lg border bg-white border-gray-200 hover:bg-gray-50 transition"
        >
          <div className="flex justify-between items-start">
            <div className="flex-1">
              <div className="flex items-center gap-2 mb-1">
                <h3 className="font-semibold text-gray-900">{tax.name}</h3>
                {isCurrentlyActive(tax) && (
                  <span className="px-2 py-0.5 text-xs bg-green-100 text-green-700 rounded">
                    Active
                  </span>
                )}
                {!tax.isActive && (
                  <span className="px-2 py-0.5 text-xs bg-gray-100 text-gray-700 rounded">
                    Inactive
                  </span>
                )}
              </div>
              <div className="text-sm text-gray-600 space-y-1">
                <p>Rate: <span className="font-medium">{tax.rate}%</span></p>
                <p>
                  Effective: {formatDate(tax.effectiveFrom)}
                  {tax.effectiveTo && ` - ${formatDate(tax.effectiveTo)}`}
                </p>
              </div>
            </div>
            <div className="flex gap-2 ml-4">
              <button
                onClick={(e) => {
                  e.stopPropagation();
                  onEdit(tax);
                }}
                className="px-3 py-1 text-sm bg-blue-100 text-blue-700 rounded hover:bg-blue-200 transition"
              >
                Edit
              </button>
              <button
                onClick={(e) => {
                  e.stopPropagation();
                  if (window.confirm(`Are you sure you want to delete "${tax.name}"?`)) {
                    onDelete(tax.id);
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
  );
}
