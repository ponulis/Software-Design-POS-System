import CashCheckout from "./CashCheckout";
import GiftCardCheckout from "./GiftCardCheckout";
import CardCheckout from "./CardCheckout";

export default function CheckoutDetails({ paymentType, total, items, orderId, onPaymentDataChange }) {
    
    const handleCashReceivedChange = (cashReceived) => {
        if (onPaymentDataChange) {
            onPaymentDataChange({
                cashReceived,
                giftCardCode: null,
                giftCardBalance: null,
                cardDetails: null,
            });
        }
    };

    const handleGiftCardChange = (code, balance) => {
        if (onPaymentDataChange) {
            onPaymentDataChange({
                cashReceived: null,
                giftCardCode: code,
                giftCardBalance: balance,
                cardDetails: null,
            });
        }
    };

    // Render appropriate checkout component based on payment type
    if (paymentType === "Card")
    {
        return <CardCheckout total={total} items={items} orderId={orderId} onPaymentDataChange={onPaymentDataChange} />;
    }
    else if (paymentType === "Cash") 
    {
        return <CashCheckout total={total} onCashReceivedChange={handleCashReceivedChange} />;
    }
    else if (paymentType === "Gift Card")
    {
        return <GiftCardCheckout total={total} onGiftCardChange={handleGiftCardChange} />;
    }

    // Fallback: show message if payment type is not selected
    return (
        <div className="bg-gray-50 rounded-xl p-6 w-full flex flex-col gap-4">
            <div className="text-center text-gray-500">
                <p className="text-sm">Please select a payment method above</p>
            </div>
        </div>
    );
}