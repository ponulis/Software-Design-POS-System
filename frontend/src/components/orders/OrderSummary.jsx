import React from 'react';

export default function OrderSummary({ subtotal, tax, discount, total }) {
  return (
    <div className="bg-gray-50 rounded-lg border border-gray-200 p-4">
      <h3 className="font-semibold text-gray-800 mb-3">Order Summary</h3>
      <div className="space-y-2">
        <div className="flex justify-between text-sm">
          <span className="text-gray-600">Subtotal:</span>
          <span className="font-medium">{subtotal.toFixed(2)}€</span>
        </div>
        {discount > 0 && (
          <div className="flex justify-between text-sm">
            <span className="text-gray-600">Discount:</span>
            <span className="font-medium text-green-600">-{discount.toFixed(2)}€</span>
          </div>
        )}
        {tax > 0 && (
          <div className="flex justify-between text-sm">
            <span className="text-gray-600">Tax:</span>
            <span className="font-medium">{tax.toFixed(2)}€</span>
          </div>
        )}
        <div className="flex justify-between text-lg font-bold pt-2 border-t border-gray-300">
          <span>Total:</span>
          <span>{total.toFixed(2)}€</span>
        </div>
      </div>
    </div>
  );
}
