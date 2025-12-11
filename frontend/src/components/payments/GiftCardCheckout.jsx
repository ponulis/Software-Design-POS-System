import React, { useState } from "react"; 
import SummaryRow from "./SummaryRow";

// The logic is not really implemented because I think it needs further consideration 
export default function GiftCardCheckout({total}) {

    return (
        <div className="bg-gray-50 rounded-xl p-6 w-full flex flex-col gap-4">
            <div>
                <h1 className="text-xl font-bold mb-1 text-left">
                    Gift Card Redemption
                </h1>
                <p className="text-xs mb text-left">
                    Validate the gift card and confirm the remaining balance.
                </p>
            </div>

            <div>
                <label htmlFor="code" className="block text-sm/6 text-xs">
                    Gift card code
                </label>
                <div className="mt-2">
                    <div className="flex items-center rounded-md bg-white pl-2 outline-1 outline-gray-300 gap-2">
                        <input
                            id="price"
                            name="price"
                            type="text"
                            placeholder="AB-1234"
                            className="focus:outline-none"
                        />
                    </div>
                </div>
            </div>

            <div>
                <label htmlFor="balance" className="block text-sm/6 text-xs">
                    Gift card balance
                </label>
                <div className="mt-2">
                    <div className="flex items-center rounded-md bg-white pl-2 outline-1 outline-gray-300 gap-2">
                        <input
                            id="price"
                            name="price"
                            type="text"
                            placeholder="0.00"
                            className="focus:outline-none"
                        />
                    </div>
                </div>
            </div>

            <div className="flex items-center rounded-md bg-gray-100 px-2 outline-1 outline-gray-300 gap-2">
                <div className="w-full">
                    <SummaryRow label="Total due" value={total} />
                    <SummaryRow label="Balance after change" value={0} />
                </div>
            </div>
        </div>
    );    
        
}