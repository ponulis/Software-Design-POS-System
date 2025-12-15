import React from 'react';
import CartItem from './CartItem';

export default function Cart({ items, onUpdateQuantity, onRemoveItem, onUpdateNotes }) {
  if (!items || items.length === 0) {
    return (
      <div className="bg-white rounded-lg border border-gray-200 p-6 text-center text-gray-400">
        <p>Cart is empty</p>
        <p className="text-sm mt-2">Add products to create an order</p>
      </div>
    );
  }

  return (
    <div className="bg-white rounded-lg border border-gray-200">
      <div className="p-4 border-b border-gray-200">
        <h3 className="font-semibold text-gray-800">Order Items ({items.length})</h3>
      </div>
      <div className="divide-y divide-gray-200">
        {items.map((item, index) => (
          <CartItem
            key={index}
            item={item}
            onUpdateQuantity={(quantity) => onUpdateQuantity(index, quantity)}
            onRemove={() => onRemoveItem(index)}
            onUpdateNotes={(notes) => onUpdateNotes(index, notes)}
          />
        ))}
      </div>
    </div>
  );
}
