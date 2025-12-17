import React, { useState } from "react";
import { useToast } from "../../context/ToastContext";
import SplitPayment from "./SplitPayment";
import CardDetailsInput from "./CardDetailsInput";

export default function CardCheckout({ total, items, orderId, onPaymentDataChange, isSplitPaymentMode = false }) {
  const { error: showErrorToast, success: showSuccessToast } = useToast();
  
  const [isSplitPaymentEnabled, setIsSplitPaymentEnabled] = useState(false);
  const [cardDetails, setCardDetails] = useState(null);
  const [cardError, setCardError] = useState(null);

  const numericTotal = parseFloat(total);
  const formattedTotal = isNaN(total) ? total : numericTotal.toFixed(2);

  // Handle card details change from CardDetailsInput component
  const handleCardDetailsChange = (details) => {
    setCardDetails(details);
    setCardError(null);

    // Notify parent component when card details are valid
    if (onPaymentDataChange && details.isValid) {
      onPaymentDataChange({
        cardDetails: {
          cardNumber: details.cardNumber,
          expiryMonth: details.expiryMonth,
          expiryYear: details.expiryYear,
          cvv: details.cvv,
          cardholderName: details.cardholderName,
        },
        cashReceived: null,
        giftCardCode: null,
        giftCardBalance: null,
        paymentIntentId: null,
        clientSecret: null,
      });
    } else if (onPaymentDataChange && !details.isValid) {
      // Clear payment data if card details are invalid
      onPaymentDataChange({
        cardDetails: null,
        cashReceived: null,
        giftCardCode: null,
        giftCardBalance: null,
        paymentIntentId: null,
        clientSecret: null,
      });
    }
  };

  const handleSplitToggle = (e) => {
    setIsSplitPaymentEnabled(e.target.checked);
    if (e.target.checked) {
      setCardDetails(null);
    }
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

      <div className="space-y-4">
        <div className="p-4 border border-gray-300 rounded-lg bg-white">
          <CardDetailsInput onCardDetailsChange={handleCardDetailsChange} />
          {cardError && (
            <p className="mt-2 text-sm text-red-600">{cardError}</p>
          )}
        </div>

        {cardDetails && cardDetails.isValid && (
          <div className="mt-4 p-2 bg-green-50 border border-green-200 rounded-lg">
            <p className="text-xs text-green-700">✓ Card details entered. Ready to process payment.</p>
          </div>
        )}
      </div>
    </div>
  );
}
