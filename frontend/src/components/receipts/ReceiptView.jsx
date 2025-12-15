import React, { useState, useEffect } from 'react';
import { ordersApi } from '../../api/orders';
import LoadingSpinner from '../LoadingSpinner';
import { useToast } from '../../context/ToastContext';
import { getErrorMessage } from '../../utils/errorHandler';

export default function ReceiptView({ orderId, onClose }) {
  const [receipt, setReceipt] = useState(null);
  const [loading, setLoading] = useState(true);
  const { error: showErrorToast } = useToast();

  useEffect(() => {
    const fetchReceipt = async () => {
      try {
        setLoading(true);
        const data = await ordersApi.getReceipt(orderId);
        setReceipt(data);
      } catch (err) {
        console.error('Error fetching receipt:', err);
        showErrorToast(getErrorMessage(err));
      } finally {
        setLoading(false);
      }
    };

    if (orderId) {
      fetchReceipt();
    }
  }, [orderId, showErrorToast]);

  const formatDate = (dateString) => {
    if (!dateString) return 'N/A';
    const date = new Date(dateString);
    return date.toLocaleString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  const formatCurrency = (amount) => {
    return `€${amount.toFixed(2)}`;
  };

  if (loading) {
    return (
      <div className="p-8">
        <LoadingSpinner text="Loading receipt..." />
      </div>
    );
  }

  if (!receipt) {
    return (
      <div className="p-8 text-center text-gray-500">
        <p>Receipt not available</p>
      </div>
    );
  }

  return (
    <div className="bg-white rounded-lg shadow-lg p-6 max-w-2xl mx-auto">
      {/* Header */}
      <div className="text-center mb-6 pb-4 border-b border-gray-200">
        <h2 className="text-2xl font-bold text-gray-900">{receipt.businessName}</h2>
        {receipt.businessDescription && (
          <p className="text-sm text-gray-600 mt-1">{receipt.businessDescription}</p>
        )}
        <div className="mt-2 text-xs text-gray-500">
          <p>{receipt.businessAddress}</p>
          {receipt.businessPhone && <p>Phone: {receipt.businessPhone}</p>}
          {receipt.businessEmail && <p>Email: {receipt.businessEmail}</p>}
        </div>
      </div>

      {/* Order Info */}
      <div className="mb-4 pb-4 border-b border-gray-200">
        <div className="flex justify-between items-start">
          <div>
            <p className="text-sm text-gray-600">Order #{receipt.orderNumber || receipt.orderId}</p>
            <p className="text-xs text-gray-500 mt-1">{formatDate(receipt.orderDate)}</p>
          </div>
          <span className={`px-3 py-1 text-xs rounded font-medium ${
            receipt.status === 'Paid' ? 'bg-green-100 text-green-700' :
            receipt.status === 'Cancelled' ? 'bg-red-100 text-red-700' :
            'bg-gray-100 text-gray-700'
          }`}>
            {receipt.status}
          </span>
        </div>
        {receipt.createdByName && (
          <p className="text-xs text-gray-500 mt-2">Served by: {receipt.createdByName}</p>
        )}
      </div>

      {/* Items */}
      <div className="mb-4">
        <h3 className="text-sm font-semibold text-gray-700 mb-2">Items</h3>
        <div className="space-y-2">
          {receipt.items && receipt.items.length > 0 ? (
            receipt.items.map((item, index) => (
              <div key={index} className="flex justify-between text-sm">
                <div className="flex-1">
                  <p className="font-medium text-gray-900">{item.name}</p>
                  <p className="text-xs text-gray-500">
                    {item.quantity} × {formatCurrency(item.unitPrice)}
                  </p>
                </div>
                <p className="font-medium text-gray-900">{formatCurrency(item.totalPrice)}</p>
              </div>
            ))
          ) : (
            <p className="text-sm text-gray-500">No items</p>
          )}
        </div>
      </div>

      {/* Totals */}
      <div className="mb-4 pt-4 border-t border-gray-200 space-y-2">
        <div className="flex justify-between text-sm">
          <span className="text-gray-600">Subtotal</span>
          <span className="text-gray-900">{formatCurrency(receipt.subTotal)}</span>
        </div>
        {receipt.discount > 0 && (
          <div className="flex justify-between text-sm">
            <span className="text-gray-600">Discount</span>
            <span className="text-green-600">-{formatCurrency(receipt.discount)}</span>
          </div>
        )}
        {receipt.tax > 0 && (
          <div className="flex justify-between text-sm">
            <span className="text-gray-600">Tax</span>
            <span className="text-gray-900">{formatCurrency(receipt.tax)}</span>
          </div>
        )}
        <div className="flex justify-between text-base font-bold pt-2 border-t border-gray-200">
          <span>Total</span>
          <span>{formatCurrency(receipt.total)}</span>
        </div>
      </div>

      {/* Payments */}
      {receipt.payments && receipt.payments.length > 0 && (
        <div className="mb-4 pt-4 border-t border-gray-200">
          <h3 className="text-sm font-semibold text-gray-700 mb-2">Payments</h3>
          <div className="space-y-2">
            {receipt.payments.map((payment, index) => (
              <div key={index} className="flex justify-between text-sm">
                <div>
                  <p className="font-medium text-gray-900">{payment.method}</p>
                  <p className="text-xs text-gray-500">{formatDate(payment.paidAt)}</p>
                  {payment.transactionId && (
                    <p className="text-xs text-gray-500">Transaction: {payment.transactionId}</p>
                  )}
                </div>
                <p className="font-medium text-gray-900">{formatCurrency(payment.amount)}</p>
              </div>
            ))}
          </div>
          <div className="mt-3 pt-2 border-t border-gray-200">
            <div className="flex justify-between text-sm">
              <span className="text-gray-600">Total Paid</span>
              <span className="font-medium text-gray-900">{formatCurrency(receipt.totalPaid)}</span>
            </div>
            {receipt.remainingBalance > 0 && (
              <div className="flex justify-between text-sm mt-1">
                <span className="text-gray-600">Remaining Balance</span>
                <span className="font-medium text-red-600">{formatCurrency(receipt.remainingBalance)}</span>
              </div>
            )}
          </div>
        </div>
      )}

      {/* Footer */}
      <div className="mt-6 pt-4 border-t border-gray-200 text-center text-xs text-gray-500">
        <p>Thank you for your business!</p>
      </div>

      {/* Actions */}
      {onClose && (
        <div className="mt-6 flex justify-end">
          <button
            onClick={onClose}
            className="px-4 py-2 bg-gray-200 text-gray-700 rounded-lg hover:bg-gray-300 transition"
          >
            Close
          </button>
        </div>
      )}
    </div>
  );
}
