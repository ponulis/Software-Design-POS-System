import React, { useState } from "react"; 
import SummaryRow from "./SummaryRow";

export default function CashCheckout({total}) {

    const [cashReceived, setCashReceived] = useState('');

    const handleInputChange = (event) => {
        const value = event.target.value.replace(/[^0-9.]/g, '');
        setCashReceived(value);
    };

    const inputAsNumber = parseFloat(cashReceived) || 0;
    
    const amountDue = (total - inputAsNumber).toFixed(2);
    const changeDue = Math.max(0, inputAsNumber - total).toFixed(2);    
    
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
                <label htmlFor="price" className="block text-sm/6 text-xs">
                    Cash received
                </label>
                <div className="mt-2">
                    <div className="flex items-center rounded-md bg-white pl-2 outline-1 outline-gray-300 gap-2">
                        <div className="shrink-0 text-base text-gray-400 select-none ">€</div>
                        <input
                            id="price"
                            name="price"
                            type="text"
                            placeholder="0.00"
                            className="focus:outline-none"
                            value={cashReceived}
                            onChange={handleInputChange}
                        />
                    </div>
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