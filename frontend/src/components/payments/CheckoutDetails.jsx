import CashCheckout from "./CashCheckout";
import GiftCardCheckout from "./GiftCardCheckout";
import CardCheckout from "./CardCheckout";

export default function CheckoutDetails({ paymentType, total, items}) {
    
    if (paymentType === "Card")
    {
        return <CardCheckout total={total} items={items} />;
    }
    else if (paymentType === "Cash") 
    {
        return <CashCheckout total={total} items={items} />;
    }
    else if (paymentType === "Gift Card")
    {
        return <GiftCardCheckout total={total} items={items} />;
    }

    return null;
}