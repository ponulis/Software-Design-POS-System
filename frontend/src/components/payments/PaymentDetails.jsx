import React, { useState } from "react"; 
import PaymentButton from "./PaymentButton";
import OrderDetails from "./OrderDetails";
import CheckoutDetails from "./CheckoutDetails";

// Mock items. Each payment should have an order ID for it to be displayed.
const mockPayment = {
  items: [
    { title: "Notebook", subtitle: "Freshly roasted 500g bag", qty: 5, price: 2 },
    { title: "Notebook", subtitle: "Large size", qty: 3, price: 3 },
    { title: "Pen Set", subtitle: "Blue ink", qty: 12, price: 0.5 },
    { title: "Stapler", subtitle: "Heavy duty", qty: 1, price: 8 },
  ],
  subtotal: 5*2 + 3*3 + 12*0.5 + 1*8, // 10 + 9 + 6 + 8 = 33
  taxes: 3.3,   // example 10% tax
  discounts: 2, // example discount
  total: 34.3,  // subtotal + taxes - discounts
};

export default function PaymentDetails({
  payment = mockPayment,
}) {
  if (!payment) {
    return (
      <div className="flex items-center justify-center h-full text-gray-400">
        <p>Select a payment to precede</p>
      </div>
    );
  }

  const handleCancelPayment = () => {
    alert('Payment transaction cancelled.'); 
  };

  const handleProcessPayment = () => {
    alert('Payment is being processed.'); 
  };

  const { items, subtotal, taxes, discounts, total } = payment;
  const [selectedPaymentType, setSelectedPaymentType] = useState('Card');

  return (
    <>
      <div className="flex flex-col gap-6 rounded-xl p-6 items-center">

        <div className="flex flex-row gap-8 w-full justify-center">
          <PaymentButton onClick={() => setSelectedPaymentType('Card')} selected={selectedPaymentType === 'Card'}>PAYMENT BY CARD</PaymentButton>
          <PaymentButton onClick={() => setSelectedPaymentType('Cash')} selected={selectedPaymentType === 'Cash'}>PAYMENT BY CASH</PaymentButton>
          <PaymentButton onClick={() => setSelectedPaymentType('Gift Card')} selected={selectedPaymentType === 'Gift Card'}>PAYMENT BY GIFT CARD</PaymentButton>
        </div>

        <OrderDetails 
          paymentType={selectedPaymentType}
          items={items}
          subtotal={subtotal}
          taxes={taxes}
          discounts={discounts}
          total={total}
        />
        
        <CheckoutDetails paymentType={selectedPaymentType} total={total} items={items}/>
        
        <div className="flex flex-row gap-8 rounded-full justify-end w-full">
          <PaymentButton isImportant={false} onClick={handleCancelPayment}>CANCEL PAYMENT</PaymentButton>
          <PaymentButton isImportant={true} onClick={handleProcessPayment}>PROCESS PAYMENT</PaymentButton>
        </div>

      </div>

    </>
  );
}
