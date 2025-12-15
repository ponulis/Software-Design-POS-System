import React, { useState, useEffect } from "react"; 
import PaymentButton from "./PaymentButton";
import OrderDetails from "./OrderDetails";
import CheckoutDetails from "./CheckoutDetails";
import ReceiptModal from "../receipts/ReceiptModal";
import { ordersApi } from "../../api/orders";

export default function PaymentDetails({ order }) {
  const [selectedPaymentType, setSelectedPaymentType] = useState('Card');
  const [orderDetails, setOrderDetails] = useState(null);
  const [loading, setLoading] = useState(false);

  // Fetch full order details if order ID is provided
  useEffect(() => {
    if (order?.id && !order.items) {
      setLoading(true);
      ordersApi.getById(order.id)
        .then((fullOrder) => {
          setOrderDetails(fullOrder);
        })
        .catch((err) => {
          console.error('Error fetching order details:', err);
          setOrderDetails(order); // Fallback to provided order
        })
        .finally(() => {
          setLoading(false);
        });
    } else if (order) {
      setOrderDetails(order);
    } else {
      setOrderDetails(null);
    }
  }, [order]);

  if (!orderDetails) {
    return (
      <div className="flex items-center justify-center h-full text-gray-400 min-h-[400px]">
        <div className="text-center">
          <p className="text-lg mb-2">No order selected</p>
          <p className="text-sm">Select an order from the list to view details</p>
        </div>
      </div>
    );
  }

  if (loading) {
    return (
      <div className="flex items-center justify-center h-full min-h-[400px]">
        <div className="text-center">
          <div className="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600 mb-4"></div>
          <p className="text-gray-600">Loading order details...</p>
        </div>
      </div>
    );
  }

  const handleCancelPayment = () => {
    if (window.confirm('Are you sure you want to cancel this payment?')) {
      // TODO: Implement cancel payment logic
      alert('Payment transaction cancelled.'); 
    }
  };

  const [paymentData, setPaymentData] = useState({
    cashReceived: null,
    giftCardCode: null,
    giftCardBalance: null,
  });
  const [processing, setProcessing] = useState(false);

  const handlePaymentDataChange = (data) => {
    setPaymentData(data);
  };

  const handleProcessPayment = async () => {
    if (!orderDetails) return;

    const numericTotal = parseFloat(orderDetails.total || 0);
    
    // Validate payment data based on payment type
    if (selectedPaymentType === 'Cash') {
      if (!paymentData.cashReceived || paymentData.cashReceived < numericTotal) {
        alert('Please enter sufficient cash amount');
        return;
      }
    } else if (selectedPaymentType === 'Gift Card') {
      if (!paymentData.giftCardCode || !paymentData.giftCardBalance) {
        alert('Please validate a gift card');
        return;
      }
      if (paymentData.giftCardBalance < numericTotal) {
        alert('Insufficient gift card balance');
        return;
      }
    }

    if (!window.confirm(`Process ${selectedPaymentType} payment of ${numericTotal.toFixed(2)}€?`)) {
      return;
    }

    setProcessing(true);
    try {
      const paymentRequest = {
        orderId: orderDetails.id,
        amount: numericTotal,
        method: selectedPaymentType === 'Gift Card' ? 'GiftCard' : selectedPaymentType,
        cashReceived: selectedPaymentType === 'Cash' ? paymentData.cashReceived : null,
        giftCardCode: selectedPaymentType === 'Gift Card' ? paymentData.giftCardCode : null,
      };

      const { paymentsApi } = await import('../../api/payments');
      const payment = await paymentsApi.create(paymentRequest);

      alert(`Payment processed successfully! Payment ID: ${payment.id}`);
      
      // Refresh order details
      const updatedOrder = await ordersApi.getById(orderDetails.id);
      setOrderDetails(updatedOrder);
      
      // Reset payment data
      setPaymentData({
        cashReceived: null,
        giftCardCode: null,
        giftCardBalance: null,
      });
    } catch (err) {
      const errorMessage = err.response?.data?.message || 'Failed to process payment';
      alert(`Error: ${errorMessage}`);
      console.error('Payment processing error:', err);
    } finally {
      setProcessing(false);
    }
  };

  // Convert order items to format expected by OrderDetails component
  const items = orderDetails.items?.map((item) => ({
    title: item.productName || `Product #${item.menuId}`,
    subtitle: item.notes || '',
    qty: item.quantity,
    price: parseFloat(item.price),
  })) || [];

  const subtotal = parseFloat(orderDetails.subTotal || 0);
  const taxes = parseFloat(orderDetails.tax || 0);
  const discounts = parseFloat(orderDetails.discount || 0);
  const total = parseFloat(orderDetails.total || 0);

  // Only show payment options if order is not already paid
  const canProcessPayment = orderDetails.status !== 'Paid' && orderDetails.status !== 'Cancelled';

  return (
    <div className="flex flex-col gap-6 rounded-xl p-6 items-center">
      {canProcessPayment && (
        <div className="flex flex-row gap-8 w-full justify-center">
          <PaymentButton 
            onClick={() => setSelectedPaymentType('Card')} 
            selected={selectedPaymentType === 'Card'}
          >
            PAYMENT BY CARD
          </PaymentButton>
          <PaymentButton 
            onClick={() => setSelectedPaymentType('Cash')} 
            selected={selectedPaymentType === 'Cash'}
          >
            PAYMENT BY CASH
          </PaymentButton>
          <PaymentButton 
            onClick={() => setSelectedPaymentType('Gift Card')} 
            selected={selectedPaymentType === 'Gift Card'}
          >
            PAYMENT BY GIFT CARD
          </PaymentButton>
        </div>
      )}

      <OrderDetails 
        paymentType={selectedPaymentType}
        items={items}
        subtotal={subtotal}
        taxes={taxes}
        discounts={discounts}
        total={total}
        orderStatus={orderDetails.status}
      />
      
      {canProcessPayment && (
        <>
          <CheckoutDetails paymentType={selectedPaymentType} total={total} items={items} orderId={orderDetails.id}/>
          
          <div className="flex flex-row gap-8 rounded-full justify-end w-full">
            <PaymentButton isImportant={false} onClick={handleCancelPayment}>
              CANCEL PAYMENT
            </PaymentButton>
            <PaymentButton isImportant={true} onClick={handleProcessPayment}>
              PROCESS PAYMENT
            </PaymentButton>
          </div>
        </>
      )}

      {!canProcessPayment && (
        <div className={`w-full p-4 rounded-lg text-center ${
          orderDetails.status === 'Paid' 
            ? 'bg-green-50 text-green-800 border border-green-200' 
            : 'bg-red-50 text-red-800 border border-red-200'
        }`}>
          <p className="font-semibold mb-3">
            Order is {orderDetails.status}
          </p>
          {orderDetails.status === 'Paid' && (
            <button
              onClick={() => setShowReceipt(true)}
              className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition text-sm"
            >
              View Receipt
            </button>
          )}
        </div>
      )}

      <ReceiptModal
        isOpen={showReceipt}
        orderId={orderDetails?.id}
        onClose={() => setShowReceipt(false)}
      />
    </div>
  );
}
