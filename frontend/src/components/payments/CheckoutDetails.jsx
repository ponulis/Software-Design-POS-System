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
            });
        }
    };

    const handleGiftCardChange = (code, balance) => {
        if (onPaymentDataChange) {
            onPaymentDataChange({
                cashReceived: null,
                giftCardCode: code,
                giftCardBalance: balance,
            });
        }
    };

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

    return null;
}