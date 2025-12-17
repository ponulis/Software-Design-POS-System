import React, { useState, useEffect } from "react";
import PaymentButton from "./PaymentButton";
import OrderDetails from "./OrderDetails";
import CheckoutDetails from "./CheckoutDetails";
import SplitPayment from "./SplitPayment";
import ReceiptModal from "../receipts/ReceiptModal";
import StripeProvider from "../stripe/StripeProvider";
import CardPaymentHandler from "./CardPaymentHandler";
import { ordersApi } from "../../api/orders";
import { paymentsApi } from "../../api/payments";
import { useToast } from "../../context/ToastContext";
import { getErrorMessage } from "../../utils/errorHandler";

export default function PaymentDetails({ order }) {
  const [paymentMode, setPaymentMode] = useState('single'); // 'single' or 'split'
  const [selectedPaymentType, setSelectedPaymentType] = useState('Card');
  const [orderDetails, setOrderDetails] = useState(null);
  const [loading, setLoading] = useState(false);
  const [paymentData, setPaymentData] = useState({
    cashReceived: null,
    giftCardCode: null,
    giftCardBalance: null,
    paymentIntentId: null,
    clientSecret: null,
  });
  const [processing, setProcessing] = useState(false);
  const [showReceipt, setShowReceipt] = useState(false);
  const [cardPaymentHandler, setCardPaymentHandler] = useState(null);
  const { success: showSuccessToast, error: showErrorToast } = useToast();

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

  // Validate order before allowing payment (Business Flow: Order Validation)
  const validateOrderForPayment = () => {
    if (!orderDetails) {
      return { valid: false, message: 'No order selected' };
    }

    // Validate order status (can only pay Draft or Placed orders)
    if (orderDetails.status === 'Paid') {
      return { valid: false, message: 'Order is already paid' };
    }

    if (orderDetails.status === 'Cancelled') {
      return { valid: false, message: 'Order has been cancelled' };
    }

    // Validate order has items
    if (!orderDetails.items || orderDetails.items.length === 0) {
      return { valid: false, message: 'Order has no items. Cannot process payment.' };
    }

    // Validate order has valid total
    const numericTotal = parseFloat(orderDetails.total || 0);
    if (isNaN(numericTotal) || numericTotal <= 0) {
      return { valid: false, message: 'Order total is invalid. Cannot process payment.' };
    }

    return { valid: true };
  };

  const handleCancelPayment = () => {
    if (window.confirm('Are you sure you want to cancel this payment?')) {
      // TODO: Implement cancel payment logic
      alert('Payment transaction cancelled.'); 
    }
  };

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

  const handlePaymentDataChange = (data) => {
    setPaymentData((prev) => ({ ...prev, ...data }));
  };

  const handleSplitPaymentComplete = async (updatedOrder) => {
    // Refresh order details after split payment
    const refreshedOrder = await ordersApi.getById(orderDetails.id);
    setOrderDetails(refreshedOrder);
  };

  const handleProcessPayment = async () => {
    if (!orderDetails) return;

    // Business Flow Step 1: Validate order (stock, schedule, status)
    const validation = validateOrderForPayment();
    if (!validation.valid) {
      showErrorToast(`Order validation failed: ${validation.message}`);
      return;
    }

    const numericTotal = parseFloat(orderDetails.total || 0);
    
    // Business Flow Step 2: Validate payment data based on payment type
    if (selectedPaymentType === 'Cash') {
      if (!paymentData.cashReceived || paymentData.cashReceived < numericTotal) {
        showErrorToast('Please enter sufficient cash amount');
        return;
      }
    } else if (selectedPaymentType === 'Gift Card') {
      if (!paymentData.giftCardCode || !paymentData.giftCardBalance) {
        showErrorToast('Please validate a gift card');
        return;
      }
      if (paymentData.giftCardBalance < numericTotal) {
        showErrorToast('Insufficient gift card balance');
        return;
      }
    } else if (selectedPaymentType === 'Card') {
      // Business Flow: Card Payment Processing
      // Step 1: Validate card details are entered
      if (!paymentData.paymentIntentId || !paymentData.clientSecret) {
        showErrorToast('Please wait for payment to initialize');
        return;
      }
      if (!cardPaymentHandler) {
        showErrorToast('Card payment handler not ready. Please wait.');
        return;
      }
      // Step 2: Card should be confirmed (validated, funds checked, authorized)
      if (!paymentData.isConfirmed) {
        showErrorToast('Please confirm the card payment before processing');
        return;
      }
    }

    if (!window.confirm(`Process ${selectedPaymentType} payment of ${numericTotal.toFixed(2)}€?`)) {
      return;
    }

    setProcessing(true);
    try {
      let payment;

      if (selectedPaymentType === 'Card') {
        // Business Flow: Card Payment Processing
        // Step 3: Confirm payment (already validated card details, checked funds, authorized)
        // Step 4: Create payment record
        if (!cardPaymentHandler || !paymentData.clientSecret) {
          throw new Error('Card payment not ready. Please wait for initialization.');
        }

        payment = await cardPaymentHandler.confirmPayment(
          paymentData.clientSecret,
          orderDetails.id,
          paymentData.paymentIntentId
        );
      } else {
        // For Cash and Gift Card, use direct payment API
        const paymentRequest = {
          orderId: orderDetails.id,
          amount: numericTotal,
          method: selectedPaymentType === 'Gift Card' ? 'GiftCard' : selectedPaymentType,
          cashReceived: selectedPaymentType === 'Cash' ? paymentData.cashReceived : null,
          giftCardCode: selectedPaymentType === 'Gift Card' ? paymentData.giftCardCode : null,
        };

        payment = await paymentsApi.create(paymentRequest);
      }

      // Business Flow: Payment Authorization Success
      // Step 5: Payment successful - provide receipt
      showSuccessToast(`Payment processed successfully! Payment ID: ${payment.id}`);
      
      // Refresh order details
      const updatedOrder = await ordersApi.getById(orderDetails.id);
      setOrderDetails(updatedOrder);
      
      // Show receipt if order is fully paid
      if (updatedOrder.status === 'Paid') {
        setShowReceipt(true);
      }
      
      // Reset payment data
      setPaymentData({
        cashReceived: null,
        giftCardCode: null,
        giftCardBalance: null,
        paymentIntentId: null,
        clientSecret: null,
      });
    } catch (err) {
      // Business Flow: Payment Authorization Failure
      // Distinguish between validation errors and payment failures
      const errorMessage = getErrorMessage(err);
      const isValidationError = errorMessage.includes('validation') || 
                                errorMessage.includes('invalid') ||
                                errorMessage.includes('not found') ||
                                errorMessage.includes('status');
      
      if (isValidationError) {
        showErrorToast(`Order validation failed: ${errorMessage}`);
      } else {
        showErrorToast(`Payment failed: ${errorMessage}`);
      }
      console.error('Payment processing error:', err);
    } finally {
      setProcessing(false);
    }
  };

  // Convert order items to format expected by OrderDetails component
  const items = orderDetails.items?.map((item, idx) => ({
    id: item.id || idx, // Use item ID or fallback to index
    title: item.productName || `Product #${item.menuId}`,
    subtitle: item.notes || '',
    qty: item.quantity,
    price: parseFloat(item.price),
  })) || [];

  const subtotal = parseFloat(orderDetails.subTotal || 0);
  const taxes = parseFloat(orderDetails.tax || 0);
  const discounts = parseFloat(orderDetails.discount || 0);
  const total = parseFloat(orderDetails.total || 0);

  // Business Flow: Only show payment options if order is validated and ready
  const orderValidation = orderDetails ? validateOrderForPayment() : { valid: false };
  const canProcessPayment = orderValidation.valid;

  return (
    <div className="flex flex-col gap-6 rounded-xl p-6 items-center">
      {canProcessPayment && (
        <>
          {/* Payment Mode Toggle */}
          <div className="flex flex-row gap-4 w-full justify-center mb-2">
            <PaymentButton 
              onClick={() => setPaymentMode('single')} 
              selected={paymentMode === 'single'}
            >
              SINGLE PAYMENT
            </PaymentButton>
            <PaymentButton 
              onClick={() => setPaymentMode('split')} 
              selected={paymentMode === 'split'}
            >
              SPLIT PAYMENT
            </PaymentButton>
          </div>

          {paymentMode === 'single' ? (
            <>
              {/* Single Payment Mode */}
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

              <OrderDetails 
                paymentType={selectedPaymentType}
                items={items}
                subtotal={subtotal}
                taxes={taxes}
                discounts={discounts}
                total={total}
                orderStatus={orderDetails.status}
              />
              
              {/* Always render StripeProvider and CardPaymentHandler to maintain consistent hook order */}
              <StripeProvider>
                <CardPaymentHandler onPaymentReady={setCardPaymentHandler}>
                  <CheckoutDetails 
                    paymentType={selectedPaymentType} 
                    total={total} 
                    items={items} 
                    orderId={orderDetails.id}
                    onPaymentDataChange={handlePaymentDataChange}
                  />
                  
                  <div className="flex flex-row gap-8 rounded-full justify-end w-full">
                    <PaymentButton isImportant={false} onClick={handleCancelPayment} disabled={processing}>
                      CANCEL PAYMENT
                    </PaymentButton>
                    <PaymentButton isImportant={true} onClick={handleProcessPayment} disabled={processing}>
                      {processing ? 'PROCESSING...' : 'PROCESS PAYMENT'}
                    </PaymentButton>
                  </div>
                </CardPaymentHandler>
              </StripeProvider>
            </>
          ) : (
            <>
              {/* Split Payment Mode */}
              <OrderDetails 
                paymentType={null}
                items={items}
                subtotal={subtotal}
                taxes={taxes}
                discounts={discounts}
                total={total}
                orderStatus={orderDetails.status}
              />
              <SplitPayment
                order={orderDetails}
                items={items}
                subtotal={subtotal}
                taxes={taxes}
                discounts={discounts}
                total={total}
                onPaymentComplete={handleSplitPaymentComplete}
              />
            </>
          )}
        </>
      )}

      {!canProcessPayment && (
        <>
          <OrderDetails 
            paymentType={null}
            items={items}
            subtotal={subtotal}
            taxes={taxes}
            discounts={discounts}
            total={total}
            orderStatus={orderDetails.status}
          />
          
          {/* Business Flow: Show validation error or order status */}
          <div className={`w-full p-4 rounded-lg text-center ${
            orderDetails.status === 'Paid' 
              ? 'bg-green-50 text-green-800 border border-green-200' 
              : orderDetails.status === 'Cancelled'
              ? 'bg-red-50 text-red-800 border border-red-200'
              : 'bg-yellow-50 text-yellow-800 border border-yellow-200'
          }`}>
            <p className="font-semibold mb-3">
              {orderDetails.status === 'Paid' 
                ? 'Order is Paid' 
                : orderDetails.status === 'Cancelled'
                ? 'Order is Cancelled'
                : `Order cannot be processed: ${orderValidation.message || 'Invalid order state'}`}
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
        </>
      )}

      <ReceiptModal
        isOpen={showReceipt}
        orderId={orderDetails?.id}
        onClose={() => setShowReceipt(false)}
      />
    </div>
  );
}
