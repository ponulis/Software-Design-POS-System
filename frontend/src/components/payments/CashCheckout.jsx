import React, { useState } from "react"; 
import SummaryRow from "./SummaryRow";

export default function CashCheckout({ total, onCashReceivedChange }) {
    const [cashReceived, setCashReceived] = useState('');

    const handleInputChange = (event) => {
        const value = event.target.value.replace(/[^0-9.]/g, '');
        setCashReceived(value);
        
        // Notify parent component of cash received amount
        if (onCashReceivedChange) {
            const numericValue = parseFloat(value) || 0;
            onCashReceivedChange(numericValue);
        }
    };

    const inputAsNumber = parseFloat(cashReceived) || 0;
    const numericTotal = parseFloat(total) || 0;
    
    const amountDue = Math.max(0, numericTotal - inputAsNumber).toFixed(2);
    const changeDue = Math.max(0, inputAsNumber - numericTotal).toFixed(2);
    
    const isValid = inputAsNumber >= numericTotal;
    
    return (
        <div className="bg-gray-50 rounded-xl p-6 w-full flex flex-col gap-4">
            <div>
                <h1 className="text-xl font-bold mb-1 text-left">
                    Cash Checkout
                </h1>
                <p className="text-xs mb text-left">
                    Track the amount collected and the change to return.
                </p>
            </div>

            <div>
                <label htmlFor="cash-received" className="block text-sm/6 text-xs">
                    Cash received
                </label>
                <div className="mt-2">
                    <div className={`flex items-center rounded-md bg-white pl-2 outline-1 outline-gray-300 gap-2 ${
                        cashReceived && !isValid ? 'outline-red-500' : ''
                    }`}>
                        <div className="shrink-0 text-base text-gray-400 select-none">€</div>
                        <input
                            id="cash-received"
                            name="cash-received"
                            type="text"
                            placeholder="0.00"
                            className="focus:outline-none w-full"
                            value={cashReceived}
                            onChange={handleInputChange}
                        />
                    </div>
                    {cashReceived && !isValid && (
                        <p className="mt-1 text-xs text-red-600">
                            Insufficient cash. Need at least {numericTotal.toFixed(2)}€
                        </p>
                    )}
                </div>
            </div>
            <div className="flex items-center rounded-md bg-gray-100 px-2 outline-1 outline-gray-300 gap-2">
                <div className="w-full">
                    <SummaryRow label="Amount due" value={amountDue} />
                    <SummaryRow label="Change due" value={changeDue} />
                </div>
            </div>
        </div>
    );    
}