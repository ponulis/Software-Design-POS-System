import React, { useState, useEffect } from "react";

export default function CardDetailsInput({ onCardDetailsChange }) {
    const [cardNumber, setCardNumber] = useState('');
    const [expiryMonth, setExpiryMonth] = useState('');
    const [expiryYear, setExpiryYear] = useState('');
    const [cvv, setCvv] = useState('');
    const [cardholderName, setCardholderName] = useState('');

    // Format card number with spaces (e.g., 1234 5678 9012 3456)
    const handleCardNumberChange = (e) => {
        let value = e.target.value.replace(/\s/g, '').replace(/[^0-9]/g, '');
        if (value.length > 16) value = value.substring(0, 16);
        
        // Add spaces every 4 digits
        const formatted = value.match(/.{1,4}/g)?.join(' ') || value;
        setCardNumber(formatted);
    };

    // Format expiry month (MM)
    const handleExpiryMonthChange = (e) => {
        let value = e.target.value.replace(/[^0-9]/g, '');
        if (value.length > 2) value = value.substring(0, 2);
        if (value.length === 2 && parseInt(value) > 12) value = '12';
        setExpiryMonth(value);
    };

    // Format expiry year (YY or YYYY)
    const handleExpiryYearChange = (e) => {
        let value = e.target.value.replace(/[^0-9]/g, '');
        if (value.length > 4) value = value.substring(0, 4);
        // If 2 digits, assume 20XX
        if (value.length === 2 && parseInt(value) < 30) {
            value = '20' + value;
        }
        setExpiryYear(value);
    };

    // Format CVV (3-4 digits)
    const handleCvvChange = (e) => {
        let value = e.target.value.replace(/[^0-9]/g, '');
        if (value.length > 4) value = value.substring(0, 4);
        setCvv(value);
    };

    // Notify parent component when card details change
    useEffect(() => {
        if (onCardDetailsChange) {
            const cardNumberClean = cardNumber.replace(/\s/g, '');
            const isValid = cardNumberClean.length >= 13 && 
                          expiryMonth.length === 2 && 
                          expiryYear.length === 4 && 
                          cvv.length >= 3 && 
                          cardholderName.trim().length > 0;

            if (isValid) {
                onCardDetailsChange({
                    cardNumber: cardNumberClean,
                    expiryMonth,
                    expiryYear,
                    cvv,
                    cardholderName: cardholderName.trim(),
                    isValid: true
                });
            } else {
                onCardDetailsChange({
                    cardNumber: cardNumberClean,
                    expiryMonth,
                    expiryYear,
                    cvv,
                    cardholderName: cardholderName.trim(),
                    isValid: false
                });
            }
        }
    }, [cardNumber, expiryMonth, expiryYear, cvv, cardholderName, onCardDetailsChange]);

    const cardNumberClean = cardNumber.replace(/\s/g, '');
    const isCardNumberValid = cardNumberClean.length >= 13 && cardNumberClean.length <= 16;
    const isExpiryValid = expiryMonth.length === 2 && expiryYear.length === 4;
    const isCvvValid = cvv.length >= 3 && cvv.length <= 4;
    const isNameValid = cardholderName.trim().length > 0;

    return (
        <div className="space-y-4">
            <div>
                <label htmlFor="card-number" className="block text-sm/6 text-xs mb-2">
                    Card Number
                </label>
                <div className={`flex items-center rounded-md bg-white pl-2 outline-1 outline-gray-300 gap-2 ${
                    cardNumber && !isCardNumberValid ? 'outline-red-500' : ''
                }`}>
                    <input
                        id="card-number"
                        name="card-number"
                        type="text"
                        placeholder="1234 5678 9012 3456"
                        className="focus:outline-none w-full"
                        value={cardNumber}
                        onChange={handleCardNumberChange}
                        maxLength={19} // 16 digits + 3 spaces
                    />
                </div>
                {cardNumber && !isCardNumberValid && (
                    <p className="mt-1 text-xs text-red-600">
                        Please enter a valid card number (13-16 digits)
                    </p>
                )}
            </div>

            <div className="grid grid-cols-2 gap-4">
                <div>
                    <label htmlFor="expiry-month" className="block text-sm/6 text-xs mb-2">
                        Expiry Month
                    </label>
                    <div className={`flex items-center rounded-md bg-white pl-2 outline-1 outline-gray-300 gap-2 ${
                        expiryMonth && !isExpiryValid ? 'outline-red-500' : ''
                    }`}>
                        <input
                            id="expiry-month"
                            name="expiry-month"
                            type="text"
                            placeholder="MM"
                            className="focus:outline-none w-full"
                            value={expiryMonth}
                            onChange={handleExpiryMonthChange}
                            maxLength={2}
                        />
                    </div>
                </div>

                <div>
                    <label htmlFor="expiry-year" className="block text-sm/6 text-xs mb-2">
                        Expiry Year
                    </label>
                    <div className={`flex items-center rounded-md bg-white pl-2 outline-1 outline-gray-300 gap-2 ${
                        expiryYear && !isExpiryValid ? 'outline-red-500' : ''
                    }`}>
                        <input
                            id="expiry-year"
                            name="expiry-year"
                            type="text"
                            placeholder="YYYY"
                            className="focus:outline-none w-full"
                            value={expiryYear}
                            onChange={handleExpiryYearChange}
                            maxLength={4}
                        />
                    </div>
                </div>
            </div>

            <div className="grid grid-cols-2 gap-4">
                <div>
                    <label htmlFor="cvv" className="block text-sm/6 text-xs mb-2">
                        CVV
                    </label>
                    <div className={`flex items-center rounded-md bg-white pl-2 outline-1 outline-gray-300 gap-2 ${
                        cvv && !isCvvValid ? 'outline-red-500' : ''
                    }`}>
                        <input
                            id="cvv"
                            name="cvv"
                            type="text"
                            placeholder="123"
                            className="focus:outline-none w-full"
                            value={cvv}
                            onChange={handleCvvChange}
                            maxLength={4}
                        />
                    </div>
                    {cvv && !isCvvValid && (
                        <p className="mt-1 text-xs text-red-600">
                            CVV must be 3-4 digits
                        </p>
                    )}
                </div>

                <div>
                    <label htmlFor="cardholder-name" className="block text-sm/6 text-xs mb-2">
                        Cardholder Name
                    </label>
                    <div className={`flex items-center rounded-md bg-white pl-2 outline-1 outline-gray-300 gap-2 ${
                        cardholderName && !isNameValid ? 'outline-red-500' : ''
                    }`}>
                        <input
                            id="cardholder-name"
                            name="cardholder-name"
                            type="text"
                            placeholder="John Doe"
                            className="focus:outline-none w-full"
                            value={cardholderName}
                            onChange={(e) => setCardholderName(e.target.value)}
                        />
                    </div>
                    {cardholderName && !isNameValid && (
                        <p className="mt-1 text-xs text-red-600">
                            Please enter cardholder name
                        </p>
                    )}
                </div>
            </div>
        </div>
    );
}
