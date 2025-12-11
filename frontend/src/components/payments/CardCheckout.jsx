import React, { useState } from "react"; 
import SplitPayment from "./SplitPayment";

export default function CardCheckout({total, items}) {

    const [isSplitPaymentEnabled, setIsSplitPaymentEnabled] = useState(false);

    const numericTotal = parseFloat(total);
    
    const formattedTotal = isNaN(total) 
        ? total 
        : numericTotal.toFixed(2);

    const handleSplitToggle = (e) => {
        setIsSplitPaymentEnabled(e.target.checked);
    };

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
                    value={isSplitPaymentEnabled}
                >
                </input>
                <p className="flex justify-between text-xs">
                    Enable split payment
                </p>
            </div>

            {isSplitPaymentEnabled ? (
                <SplitPayment total={formattedTotal} items={items} />
            ) : (
                <div className="flex items-center rounded-md bg-gray-100 px-2 h-8 outline-1 outline-dashed outline-gray-300 gap-2">
                    <p className="flex justify-between text-xs">
                        Split is off. Charge the full {formattedTotal}€ to a single card payment.
                    </p>
                </div>
            )}

        </div>
    );    
        
}