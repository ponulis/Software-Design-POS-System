import React, { useState, useEffect } from "react";
import { useStripe, useElements, CardElement } from "@stripe/react-stripe-js";
import { stripeApi } from "../../api/stripe";
import SplitPayment from "./SplitPayment";

export default function CardCheckout({ total, items, orderId, onPaymentDataChange }) {
  const stripe = useStripe();
  const elements = useElements();
  const { error: showErrorToast } = useToast();
  
  const [isSplitPaymentEnabled, setIsSplitPaymentEnabled] = useState(false);
  const [paymentIntent, setPaymentIntent] = useState(null);
  const [loading, setLoading] = useState(false);
  const [cardError, setCardError] = useState(null);

  const numericTotal = parseFloat(total);
  const formattedTotal = isNaN(total) ? total : numericTotal.toFixed(2);

  // Create payment intent when component mounts or orderId changes
  useEffect(() => {
    if (orderId && !isSplitPaymentEnabled && !paymentIntent) {
      createPaymentIntent();
    }
  }, [orderId, isSplitPaymentEnabled]);

  const createPaymentIntent = async () => {
    if (!orderId) return;

    try {
      setLoading(true);
      setCardError(null);
      
      const response = await stripeApi.createPaymentIntent({
        orderId: orderId,
        amount: numericTotal,
        currency: 'eur', // EUR for European businesses
      });

      setPaymentIntent(response);
      
      // Notify parent component
      // Notify parent component
      if (onPaymentDataChange) {
        onPaymentDataChange({
          paymentIntentId: response.paymentIntentId,
          clientSecret: response.clientSecret,
          cashReceived: null,
          giftCardCode: null,
          giftCardBalance: null,
        });
      }
    } catch (err) {
      console.error('Error creating payment intent:', err);
      setCardError(err.response?.data?.message || 'Failed to initialize payment');
    } finally {
      setLoading(false);
    }
  };

  const handleCardChange = (event) => {
    if (event.error) {
      setCardError(event.error.message);
    } else {
      setCardError(null);
    }
  };

  const handleSplitToggle = (e) => {
    setIsSplitPaymentEnabled(e.target.checked);
    if (e.target.checked) {
      setPaymentIntent(null);
    } else {
      createPaymentIntent();
    }
  };

  const cardElementOptions = {
    style: {
      base: {
        fontSize: '16px',
        color: '#424770',
        '::placeholder': {
          color: '#aab7c4',
        },
      },
      invalid: {
        color: '#9e2146',
      },
    },
    hidePostalCode: false,
  };

  if (isSplitPaymentEnabled) {
    return (
      <div className="bg-gray-50 rounded-xl p-6 w-full flex flex-col gap-4">
        <div>
          <h1 className="text-xl font-bold mb-1 text-left">
            Split Payment
          </h1>
          <p className="text-xs mb text-left">
            Decide if the order needs splitting before selecting people or items.
          </p>
        </div>

        <div className="flex items-center rounded-md bg-gray-100 px-2 h-8 outline-1 outline-gray-300 gap-2">
          <input 
            id="default-checkbox" 
            type="checkbox" 
            className="w-4 h-4 border border-default-medium rounded-xs bg-neutral-secondary-medium" 
            checked={isSplitPaymentEnabled} 
            onChange={handleSplitToggle}
          />
          <p className="flex justify-between text-xs">
            Enable split payment
          </p>
        </div>

        <SplitPayment total={formattedTotal} items={items} />
      </div>
    );
  }

  return (
    <div className="bg-gray-50 rounded-xl p-6 w-full flex flex-col gap-4">
      <div>
        <h1 className="text-xl font-bold mb-1 text-left">
          Card Payment
        </h1>
        <p className="text-xs mb-2 text-left text-gray-600">
          Enter your card details to complete the payment of {formattedTotal}€
        </p>
      </div>

      <div className="flex items-center rounded-md bg-gray-100 px-2 h-8 outline-1 outline-gray-300 gap-2 mb-2">
        <input 
          id="split-checkbox" 
          type="checkbox" 
          className="w-4 h-4 border border-default-medium rounded-xs bg-neutral-secondary-medium" 
          checked={isSplitPaymentEnabled} 
          onChange={handleSplitToggle}
        />
        <p className="flex justify-between text-xs">
          Enable split payment
        </p>
      </div>

      {loading ? (
        <div className="flex items-center justify-center p-8">
          <div className="inline-block animate-spin rounded-full h-6 w-6 border-b-2 border-blue-600"></div>
          <span className="ml-2 text-sm text-gray-600">Setting up payment...</span>
        </div>
      ) : paymentIntent ? (
        <div className="space-y-4">
          <div className="p-4 border border-gray-300 rounded-lg bg-white">
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Card Details
            </label>
            <CardElement
              options={cardElementOptions}
              onChange={handleCardChange}
            />
            {cardError && (
              <p className="mt-2 text-sm text-red-600">{cardError}</p>
            )}
          </div>

          {onPaymentDataChange && (
            <div className="text-xs text-gray-500">
              Payment intent created. Card details ready for processing.
            </div>
          )}
        </div>
      ) : (
        <div className="p-4 border border-yellow-300 rounded-lg bg-yellow-50">
          <p className="text-sm text-yellow-800">
            Unable to initialize payment. Please try again.
          </p>
          <button
            onClick={createPaymentIntent}
            className="mt-2 px-4 py-2 bg-yellow-600 text-white rounded-lg hover:bg-yellow-700 transition text-sm"
          >
            Retry
          </button>
        </div>
      )}
    </div>
  );
}
