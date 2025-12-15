import React, { useState, useEffect } from "react"; 
import SummaryRow from "./SummaryRow";
import { giftCardsApi } from "../../api/giftCards";

export default function GiftCardCheckout({ total, onGiftCardChange }) {
    const [giftCardCode, setGiftCardCode] = useState('');
    const [giftCardBalance, setGiftCardBalance] = useState(null);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState(null);
    const [validated, setValidated] = useState(false);

    useEffect(() => {
        // Reset when code changes
        if (!giftCardCode) {
            setGiftCardBalance(null);
            setError(null);
            setValidated(false);
            if (onGiftCardChange) {
                onGiftCardChange(null, null);
            }
        }
    }, [giftCardCode, onGiftCardChange]);

    const validateGiftCard = async () => {
        if (!giftCardCode.trim()) {
            setError('Please enter a gift card code');
            return;
        }

        setLoading(true);
        setError(null);

        try {
            const giftCard = await giftCardsApi.getByCode(giftCardCode.trim());
            
            if (!giftCard.isActive) {
                setError('This gift card is not active');
                setGiftCardBalance(null);
                setValidated(false);
                return;
            }

            if (giftCard.balance <= 0) {
                setError('This gift card has no balance');
                setGiftCardBalance(null);
                setValidated(false);
                return;
            }

            const numericTotal = parseFloat(total) || 0;
            if (giftCard.balance < numericTotal) {
                setError(`Insufficient balance. Available: ${giftCard.balance.toFixed(2)}€`);
                setGiftCardBalance(giftCard.balance);
                setValidated(false);
            } else {
                setGiftCardBalance(giftCard.balance);
                setValidated(true);
                setError(null);
            }

            if (onGiftCardChange) {
                onGiftCardChange(giftCardCode.trim(), giftCard.balance);
            }
        } catch (err) {
            setError(err.response?.data?.message || 'Invalid gift card code');
            setGiftCardBalance(null);
            setValidated(false);
            if (onGiftCardChange) {
                onGiftCardChange(null, null);
            }
        } finally {
            setLoading(false);
        }
    };

    const numericTotal = parseFloat(total) || 0;
    const balance = giftCardBalance || 0;
    const remainingBalance = Math.max(0, balance - numericTotal);

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
                <label htmlFor="gift-card-code" className="block text-sm/6 text-xs">
                    Gift card code
                </label>
                <div className="mt-2 flex gap-2">
                    <div className="flex-1 flex items-center rounded-md bg-white pl-2 outline-1 outline-gray-300 gap-2">
                        <input
                            id="gift-card-code"
                            name="gift-card-code"
                            type="text"
                            placeholder="AB-1234"
                            className="focus:outline-none w-full"
                            value={giftCardCode}
                            onChange={(e) => setGiftCardCode(e.target.value.toUpperCase())}
                            onBlur={validateGiftCard}
                            disabled={loading}
                        />
                    </div>
                    <button
                        onClick={validateGiftCard}
                        disabled={loading || !giftCardCode.trim()}
                        className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed text-sm"
                    >
                        {loading ? '...' : 'Validate'}
                    </button>
                </div>
                {error && (
                    <p className="mt-1 text-xs text-red-600">{error}</p>
                )}
                {validated && (
                    <p className="mt-1 text-xs text-green-600">✓ Gift card validated</p>
                )}
            </div>

            {giftCardBalance !== null && (
                <div>
                    <label htmlFor="balance" className="block text-sm/6 text-xs">
                        Gift card balance
                    </label>
                    <div className="mt-2">
                        <div className="flex items-center rounded-md bg-gray-100 pl-2 outline-1 outline-gray-300 gap-2">
                            <div className="shrink-0 text-base text-gray-400 select-none">€</div>
                            <input
                                id="balance"
                                name="balance"
                                type="text"
                                value={giftCardBalance.toFixed(2)}
                                className="focus:outline-none w-full bg-gray-100"
                                readOnly
                            />
                        </div>
                    </div>
                </div>
            )}

            <div className="flex items-center rounded-md bg-gray-100 px-2 outline-1 outline-gray-300 gap-2">
                <div className="w-full">
                    <SummaryRow label="Total due" value={total} />
                    {giftCardBalance !== null && (
                        <SummaryRow label="Balance after payment" value={remainingBalance} />
                    )}
                </div>
            </div>
        </div>
    );    
}