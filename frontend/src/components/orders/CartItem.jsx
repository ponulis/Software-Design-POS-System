import React, { useState } from 'react';

export default function CartItem({ item, onUpdateQuantity, onRemove, onUpdateNotes }) {
  const [notes, setNotes] = useState(item.notes || '');
  const lineTotal = (item.quantity * item.price).toFixed(2);

  const handleQuantityChange = (delta) => {
    const newQuantity = Math.max(1, item.quantity + delta);
    onUpdateQuantity(newQuantity);
  };

  const handleNotesBlur = () => {
    onUpdateNotes(notes);
  };

  return (
    <div className="p-4 hover:bg-gray-50 transition">
      <div className="flex justify-between items-start mb-2">
        <div className="flex-1">
          <h4 className="font-medium text-gray-900">{item.name}</h4>
          {item.description && (
            <p className="text-sm text-gray-500 mt-1">{item.description}</p>
          )}
          <p className="text-sm font-semibold text-gray-700 mt-1">
            {item.price.toFixed(2)}€ each
          </p>
        </div>
        <button
          onClick={onRemove}
          className="text-red-600 hover:text-red-800 text-sm font-medium ml-4"
        >
          Remove
        </button>
      </div>

      <div className="flex items-center gap-4 mt-3">
        <div className="flex items-center gap-2">
          <button
            onClick={() => handleQuantityChange(-1)}
            className="w-8 h-8 rounded border border-gray-300 hover:bg-gray-100 flex items-center justify-center"
          >
            -
          </button>
          <span className="w-12 text-center font-medium">{item.quantity}</span>
          <button
            onClick={() => handleQuantityChange(1)}
            className="w-8 h-8 rounded border border-gray-300 hover:bg-gray-100 flex items-center justify-center"
          >
            +
          </button>
        </div>
        <div className="flex-1">
          <input
            type="text"
            placeholder="Add notes (optional)"
            value={notes}
            onChange={(e) => setNotes(e.target.value)}
            onBlur={handleNotesBlur}
            className="w-full px-3 py-1 text-sm border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>
        <div className="text-right">
          <p className="font-semibold text-gray-900">{lineTotal}€</p>
        </div>
      </div>
    </div>
  );
}
