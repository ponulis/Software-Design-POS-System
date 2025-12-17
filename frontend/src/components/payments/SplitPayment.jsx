import { useState, useEffect } from "react";
import OrderRow from "./OrderRow";
import PaymentButton from "./PaymentButton";
import CashCheckout from "./CashCheckout";
import GiftCardCheckout from "./GiftCardCheckout";
import CardCheckout from "./CardCheckout";
import StripeProvider from "../stripe/StripeProvider";
import { paymentsApi } from "../../api/payments";
import { useToast } from "../../context/ToastContext";
import { getErrorMessage } from "../../utils/errorHandler";

export default function SplitPayment({ order, items, subtotal, taxes, discounts, total, onPaymentComplete }) {
    // Validate required props
    if (!order || !order.id) {
        return (
            <div className="flex items-center justify-center h-full text-gray-400 min-h-[400px]">
                <div className="text-center">
                    <p className="text-lg mb-2">Invalid order</p>
                    <p className="text-sm">Order information is missing</p>
                </div>
            </div>
        );
    }
    
    // Normalize numeric values to ensure they're numbers
    const normalizedSubtotal = parseFloat(subtotal) || 0;
    const normalizedTaxes = parseFloat(taxes) || 0;
    const normalizedDiscounts = parseFloat(discounts) || 0;
    const normalizedTotal = parseFloat(total) || 0;
    
    // Helper function to safely format numbers
    const formatCurrency = (value) => {
        const num = parseFloat(value);
        return isNaN(num) ? '0.00' : num.toFixed(2);
    };
    
    const [numberOfGuests, setNumberOfGuests] = useState(1);
    const [guests, setGuests] = useState([{ id: 1, paymentMethod: null, paymentData: {} }]);
    const [itemAssignments, setItemAssignments] = useState({}); // { itemId: [guestIds] }
    const [guestTotals, setGuestTotals] = useState({}); // { guestId: total }
    const [processingPayments, setProcessingPayments] = useState({}); // { guestId: true/false }
    const [paymentStatuses, setPaymentStatuses] = useState({}); // { guestId: 'paid' | 'pending' | 'failed' }
    const { success: showSuccessToast, error: showErrorToast } = useToast();

    // Initialize item assignments - all items assigned to guest 1 by default
    useEffect(() => {
        if (items && items.length > 0) {
            const initialAssignments = {};
            items.forEach((item, idx) => {
                initialAssignments[item.id || idx] = [1]; // Assign to first guest by default
            });
            setItemAssignments(initialAssignments);
        }
    }, [items]);

    // Calculate totals per guest whenever assignments change
    useEffect(() => {
        if (!items || items.length === 0) {
            setGuestTotals({});
            return;
        }

        const totals = {};
        const taxRate = normalizedSubtotal > 0 ? normalizedTaxes / normalizedSubtotal : 0;
        const discountRate = normalizedSubtotal > 0 ? normalizedDiscounts / normalizedSubtotal : 0;

        guests.forEach(guest => {
            let guestSubtotal = 0;
            
            // Calculate subtotal for this guest based on assigned items
            items.forEach((item, idx) => {
                const itemId = item.id || idx;
                const assignedGuests = itemAssignments[itemId] || [];
                
                if (assignedGuests.includes(guest.id)) {
                    // Split item cost evenly among assigned guests
                    const itemTotal = (item.price || 0) * (item.qty || 1);
                    const sharePerGuest = itemTotal / assignedGuests.length;
                    guestSubtotal += sharePerGuest;
                }
            });

            // Calculate tax and discount proportionally
            const guestTax = guestSubtotal * taxRate;
            const guestDiscount = guestSubtotal * discountRate;
            const guestTotal = guestSubtotal - guestDiscount + guestTax;
            
            totals[guest.id] = {
                subtotal: guestSubtotal,
                tax: guestTax,
                discount: guestDiscount,
                total: guestTotal
            };
        });

        setGuestTotals(totals);
    }, [items, itemAssignments, guests, normalizedSubtotal, normalizedTaxes, normalizedDiscounts]);

    const handleAddPerson = () => {
        const newGuestId = Date.now();
        setNumberOfGuests(prev => prev + 1);
        setGuests(prev => [...prev, { 
            id: newGuestId, 
            paymentMethod: null, 
            paymentData: {} 
        }]);
    };

    const handleRemovePerson = (id) => {
        if (guests.length <= 1) {
            showErrorToast("At least one guest is required");
            return;
        }

        // Remove guest from all item assignments
        const newAssignments = { ...itemAssignments };
        Object.keys(newAssignments).forEach(itemId => {
            newAssignments[itemId] = newAssignments[itemId].filter(gId => gId !== id);
        });

        // Reassign items that were only assigned to removed guest
        Object.keys(newAssignments).forEach(itemId => {
            if (newAssignments[itemId].length === 0) {
                // Assign to first remaining guest
                const remainingGuests = guests.filter(g => g.id !== id);
                if (remainingGuests.length > 0) {
                    newAssignments[itemId] = [remainingGuests[0].id];
                }
            }
        });

        setItemAssignments(newAssignments);
        setNumberOfGuests(prev => Math.max(prev - 1, 1));
        setGuests(prev => prev.filter(guest => guest.id !== id));
        setPaymentStatuses(prev => {
            const newStatuses = { ...prev };
            delete newStatuses[id];
            return newStatuses;
        });
    };

    const handleItemAssignment = (itemId, guestId) => {
        setItemAssignments(prev => {
            const currentAssignments = prev[itemId] || [];
            const isAssigned = currentAssignments.includes(guestId);
            
            if (isAssigned) {
                // Remove assignment if it's the only one, prevent unassigning last guest
                if (currentAssignments.length > 1) {
                    return {
                        ...prev,
                        [itemId]: currentAssignments.filter(id => id !== guestId)
                    };
                }
                return prev; // Keep at least one assignment
            } else {
                // Add assignment
                return {
                    ...prev,
                    [itemId]: [...currentAssignments, guestId]
                };
            }
        });
    };

    const handlePaymentMethodChange = (guestId, method) => {
        setGuests(prev => prev.map(guest => 
            guest.id === guestId 
                ? { ...guest, paymentMethod: method, paymentData: {} }
                : guest
        ));
        setPaymentStatuses(prev => {
            const newStatuses = { ...prev };
            delete newStatuses[guestId]; // Reset payment status when method changes
            return newStatuses;
        });
    };

    const handlePaymentDataChange = (guestId, data) => {
        setGuests(prev => prev.map(guest => 
            guest.id === guestId 
                ? { ...guest, paymentData: { ...guest.paymentData, ...data } }
                : guest
        ));
    };

    const calculateAssignedTotal = () => {
        return Object.values(guestTotals).reduce((sum, guestTotal) => sum + (guestTotal.total || 0), 0);
    };

    const handleProcessPayment = async (guestId) => {
        const guest = guests.find(g => g.id === guestId);
        if (!guest) return;

        const guestTotal = guestTotals[guestId];
        if (!guestTotal || guestTotal.total <= 0) {
            showErrorToast("Guest total must be greater than zero");
            return;
        }

        if (!guest.paymentMethod) {
            showErrorToast("Please select a payment method for this guest");
            return;
        }

        setProcessingPayments(prev => ({ ...prev, [guestId]: true }));

        try {
            // For split payments, we need to process all payments at once
            // So we'll collect all guest payments and send them together
            // But for now, let's process individually and then combine
            
            // This is a simplified version - in a real implementation,
            // you'd want to collect all payments first, then send them together
            showErrorToast("Please use 'Process All Payments' to complete split payment");
            setProcessingPayments(prev => ({ ...prev, [guestId]: false }));
        } catch (err) {
            const errorMessage = getErrorMessage(err);
            showErrorToast(errorMessage);
            setPaymentStatuses(prev => ({ ...prev, [guestId]: 'failed' }));
            setProcessingPayments(prev => ({ ...prev, [guestId]: false }));
        }
    };

    const handleProcessAllPayments = async () => {
        // Business Flow Step 1: Validate order (already validated in component mount)
        if (!order || !order.id) {
            showErrorToast("Order validation failed: Order information is missing");
            return;
        }

        // Business Flow Step 2: Validate all guests have payment methods
        const guestsWithoutMethod = guests.filter(g => !g.paymentMethod);
        if (guestsWithoutMethod.length > 0) {
            showErrorToast("Please select payment methods for all guests");
            return;
        }

        // Business Flow Step 2: Validate all items are assigned
        const unassignedItems = items.filter((item, idx) => {
            const itemId = item.id || idx;
            const assignments = itemAssignments[itemId] || [];
            return assignments.length === 0;
        });

        if (unassignedItems.length > 0) {
            showErrorToast("Please assign all items to at least one guest");
            return;
        }

        // Business Flow Step 2: Validate totals match (Calculate totals step)
        const assignedTotal = calculateAssignedTotal();
        const totalDifference = Math.abs(assignedTotal - normalizedTotal);
        if (totalDifference > 0.01) {
            showErrorToast(`Assigned total (${formatCurrency(assignedTotal)}€) does not match order total (${formatCurrency(normalizedTotal)}€)`);
            return;
        }

        // Business Flow Step 3: Validate card payments are confirmed (Card Payment Processing)
        for (const guest of guests) {
            if (guest.paymentMethod === 'Card') {
                const paymentData = guest.paymentData || {};
                if (!paymentData.isConfirmed) {
                    showErrorToast(`Card payment for Guest ${guests.indexOf(guest) + 1} must be confirmed before processing`);
                    return;
                }
            }
        }

        setProcessingPayments(prev => {
            const newState = {};
            guests.forEach(g => { newState[g.id] = true; });
            return newState;
        });

        try {
            // Prepare split payment request
            const splitPayments = [];

            // Business Flow: Card Payment Processing
            // Step 1: Validate card details (already done via confirmation)
            // Step 2: Check available funds (handled by Stripe)
            // Step 3: Authorize transaction (handled by Stripe confirmation)
            // Step 4: Create payment records
            for (const guest of guests) {
                const guestTotal = guestTotals[guest.id];
                const paymentData = guest.paymentData || {};

                if (guest.paymentMethod === 'Card') {
                    if (!paymentData.clientSecret || !paymentData.paymentIntentId) {
                        throw new Error(`Card payment not ready for Guest ${guests.indexOf(guest) + 1}. Please ensure card details are entered and payment intent is created.`);
                    }

                    if (!paymentData.isConfirmed) {
                        throw new Error(`Card payment for Guest ${guests.indexOf(guest) + 1} must be confirmed before processing`);
                    }

                    splitPayments.push({
                        amount: guestTotal.total,
                        method: 'Card',
                        paymentIntentId: paymentData.paymentIntentId
                    });
                } else {
                    splitPayments.push({
                        amount: guestTotal.total,
                        method: guest.paymentMethod === 'Cash' ? 'Cash' : 'GiftCard',
                        cashReceived: guest.paymentMethod === 'Cash' ? paymentData.cashReceived : null,
                        giftCardCode: guest.paymentMethod === 'Gift Card' ? paymentData.giftCardCode : null
                    });
                }
            }

            // Business Flow: Create split payments (backend validates and creates payment records)
            const response = await paymentsApi.createSplitPayments({
                orderId: order?.id,
                payments: splitPayments
            });

            // Business Flow: Payment Authorization Success
            showSuccessToast(`Split payment processed successfully! All ${response.payments.length} payments completed.`);
            
            // Update payment statuses
            const newStatuses = {};
            guests.forEach(g => { newStatuses[g.id] = 'paid'; });
            setPaymentStatuses(newStatuses);

            // Notify parent component
            if (onPaymentComplete) {
                onPaymentComplete(response.order);
            }
        } catch (err) {
            // Business Flow: Payment Authorization Failure
            // Distinguish between validation errors and payment failures
            const errorMessage = getErrorMessage(err);
            const isValidationError = errorMessage.includes('validation') || 
                                    errorMessage.includes('invalid') ||
                                    errorMessage.includes('not found') ||
                                    errorMessage.includes('status') ||
                                    errorMessage.includes('not ready') ||
                                    errorMessage.includes('must be confirmed');
            
            if (isValidationError) {
                showErrorToast(`Order validation failed: ${errorMessage}`);
            } else {
                showErrorToast(`Payment failed: ${errorMessage}`);
            }
            console.error('Split payment error:', err);
        } finally {
            setProcessingPayments(prev => {
                const newState = {};
                guests.forEach(g => { newState[g.id] = false; });
                return newState;
            });
        }
    };

    const assignedTotal = calculateAssignedTotal();
    const allPaymentsReady = guests.every(g => {
        if (!g.paymentMethod) return false;
        const guestTotal = guestTotals[g.id];
        if (!guestTotal || guestTotal.total <= 0) return false;
        
        if (g.paymentMethod === 'Card') {
            return g.paymentData?.clientSecret && g.paymentData?.isConfirmed;
        } else if (g.paymentMethod === 'Cash') {
            return g.paymentData?.cashReceived >= guestTotal.total;
        } else if (g.paymentMethod === 'Gift Card') {
            return g.paymentData?.giftCardCode && g.paymentData?.giftCardBalance >= guestTotal.total;
        }
        return false;
    });

    return (
        <>
            <div className="flex justify-center flex-col rounded-md bg-gray-100 px-2 outline-1 outline-gray-300 gap-2">
                <div className="flex w-full items-center my-2">
                    <div className="text-sm">
                        Guests ({numberOfGuests})
                    </div>
                    <button
                        onClick={handleAddPerson}
                        className="ml-auto px-3 py-1 rounded-full font-bold text-gray-800 text-xs 
                                border border-gray-300 select-none cursor-pointer bg-gray-300
                                hover:bg-gray-400 hover:text-white transition"
                    >
                        Add person
                    </button>
                </div>
                {guests.map((guest, idx) => (
                    <div key={guest.id} className="flex w-full justify-center items-center gap-2 mb-2">
                        <div className="flex w-full items-center bg-white p-2 outline-1 rounded-md outline-gray-300">
                            <div className="text-sm select-none">
                                Guest {idx + 1}
                            </div>
                        </div>
                        {guests.length > 1 && (
                            <button
                                onClick={() => handleRemovePerson(guest.id)}
                                className="ml-auto h-6 w-6 pb-[1px] rounded-full font-bold text-center text-gray-800 text-xs 
                                        border border-gray-300 select-none cursor-pointer bg-gray-300
                                        hover:bg-gray-400 hover:text-white transition"
                            >
                                ×
                            </button>
                        )}
                    </div>
                ))}
            </div>

            <div className="flex justify-center flex-col rounded-md bg-gray-100 px-2 outline-1 outline-gray-300 gap-2 pb-2">
                <div className="flex w-full items-center my-2">
                    <div className="text-sm">
                        Assign items
                    </div>
                    <div className="text-xs ml-auto text-gray-400">
                        Select each guest that ordered the item
                    </div>
                </div>
                
                <div className="rounded-xl overflow-x-auto border border-gray-300 pr-2">
                    <table className="table-auto w-full bg-white">
                        <thead className="bg-gray-100">
                            <tr className="text-left">
                                <th className="py-2 pl-2 text-xs text-gray-600 h-12">ITEM</th>
                                {guests.map((guest, idx) => (
                                    <th key={guest.id} className="py-2 pl-2 text-xs text-center text-gray-600 h-12">
                                        GUEST {idx + 1}
                                    </th>
                                ))}
                            </tr>
                        </thead>
                        <tbody>
                            {items && items.length > 0 ? (
                                items.map((item, itemIdx) => {
                                    const itemId = item.id || itemIdx;
                                    const assignedGuests = itemAssignments[itemId] || [];
                                    return (
                                        <tr key={itemId} className="border-b border-gray-200 last:border-b-0">
                                            <td className="pl-2 h-12 text-sm">
                                                <div className="flex flex-col">
                                                    <span>{item.title}</span>
                                                    <span className="text-gray-500 text-xs max-w-[180px] truncate">
                                                        {item.subtitle}
                                                    </span>
                                                </div>
                                            </td>
                                            {guests.map((guest) => {
                                                const isAssigned = assignedGuests.includes(guest.id);
                                                const isOnlyAssignment = assignedGuests.length === 1 && isAssigned;
                                                return (
                                                    <td key={guest.id} className="text-center">
                                                        <input
                                                            type="checkbox"
                                                            checked={isAssigned}
                                                            onChange={() => handleItemAssignment(itemId, guest.id)}
                                                            disabled={isOnlyAssignment}
                                                            className="w-4 h-4 border border-gray-300 rounded-sm bg-white disabled:opacity-50 disabled:cursor-not-allowed"
                                                        />
                                                    </td>
                                                );
                                            })}
                                        </tr>
                                    );
                                })
                            ) : (
                                <tr>
                                    <td colSpan={1 + guests.length} className="text-center text-gray-400 py-4">
                                        No items found
                                    </td>
                                </tr>
                            )}
                        </tbody>
                    </table>
                </div>
            </div>

            {/* Payment method selection and processing per guest */}
            {guests.map((guest, idx) => {
                const guestTotal = guestTotals[guest.id] || { total: 0, subtotal: 0, tax: 0, discount: 0 };
                const isProcessing = processingPayments[guest.id] || false;
                const paymentStatus = paymentStatuses[guest.id];

                return (
                    <div key={guest.id} className="flex justify-center flex-col rounded-md bg-gray-100 px-2 outline-1 outline-gray-300 gap-2 pb-2">
                        <div className="flex w-full items-center my-2">
                            <div className="text-sm font-semibold">
                                Guest {idx + 1} Payment
                            </div>
                            {paymentStatus === 'paid' && (
                                <span className="ml-auto px-2 py-1 text-xs bg-green-100 text-green-700 rounded">
                                    ✓ Paid
                                </span>
                            )}
                        </div>

                        <div className="bg-white p-4 rounded-md">
                            <div className="mb-3">
                                <div className="text-xs text-gray-600 mb-2">Total: {formatCurrency(guestTotal.total)}€</div>
                                <div className="flex gap-2">
                                    <PaymentButton
                                        onClick={() => handlePaymentMethodChange(guest.id, 'Cash')}
                                        selected={guest.paymentMethod === 'Cash'}
                                        disabled={paymentStatus === 'paid'}
                                    >
                                        Cash
                                    </PaymentButton>
                                    <PaymentButton
                                        onClick={() => handlePaymentMethodChange(guest.id, 'Card')}
                                        selected={guest.paymentMethod === 'Card'}
                                        disabled={paymentStatus === 'paid'}
                                    >
                                        Card
                                    </PaymentButton>
                                    <PaymentButton
                                        onClick={() => handlePaymentMethodChange(guest.id, 'Gift Card')}
                                        selected={guest.paymentMethod === 'Gift Card'}
                                        disabled={paymentStatus === 'paid'}
                                    >
                                        Gift Card
                                    </PaymentButton>
                                </div>
                            </div>

                            {guest.paymentMethod && paymentStatus !== 'paid' && (
                                <StripeProvider>
                                    {guest.paymentMethod === 'Cash' && (
                                        <CashCheckout
                                            total={guestTotal.total}
                                            onCashReceivedChange={(cashReceived) => 
                                                handlePaymentDataChange(guest.id, { cashReceived })
                                            }
                                        />
                                    )}
                                    {guest.paymentMethod === 'Card' && (
                                        <CardCheckout
                                            total={guestTotal.total}
                                            items={items.filter((item, itemIdx) => {
                                                const itemId = item.id || itemIdx;
                                                return (itemAssignments[itemId] || []).includes(guest.id);
                                            })}
                                            orderId={order?.id}
                                            isSplitPaymentMode={true}
                                            onPaymentDataChange={(data) => handlePaymentDataChange(guest.id, data)}
                                        />
                                    )}
                                    {guest.paymentMethod === 'Gift Card' && (
                                        <GiftCardCheckout
                                            total={guestTotal.total}
                                            onGiftCardChange={(code, balance) => 
                                                handlePaymentDataChange(guest.id, { giftCardCode: code, giftCardBalance: balance })
                                            }
                                        />
                                    )}
                                </StripeProvider>
                            )}
                        </div>
                    </div>
                );
            })}

            <div className="flex justify-center flex-col rounded-md bg-gray-100 px-2 outline-1 outline-gray-300 gap-2 pb-2">
                <div className="flex w-full items-center my-2">
                    <div className="text-sm">
                        Per person totals
                    </div>
                </div>

                <table className="table-auto w-full bg-inherit">
                    <thead className="bg-inherit">
                        <tr className="text-left">
                            <th className="py-2 pl-2 text-xs text-gray-400 h-12">PERSON</th>
                            <th className="py-2 pl-2 text-xs text-center text-gray-400 h-12">AMOUNT</th>
                        </tr>
                    </thead>
                    <tbody>
                        {guests && guests.length > 0 ? (
                            guests.map((guest, idx) => {
                                const guestTotal = guestTotals[guest.id] || { total: 0 };
                                return (
                                    <tr key={guest.id} className="border-t border-gray-200 last:border-b-0">
                                        <td className="pl-2 h-12 text-sm">
                                            <div className="flex flex-col">
                                                <span>Guest {idx + 1}</span>
                                            </div>
                                        </td>
                                        <td className="text-center">
                                            <p>{formatCurrency(guestTotal.total)}€</p>
                                        </td>
                                    </tr>
                                );
                            })
                        ) : (
                            <tr>
                                <td colSpan="2" className="text-center text-gray-400 py-4">
                                    No guests found
                                </td>
                            </tr>
                        )}
                        <tr className="border-t border-gray-200 last:border-b-0">
                            <td className="pl-2 h-12 text-sm">
                                Assigned total
                            </td>
                            <td className="text-center">
                                {formatCurrency(assignedTotal)}€
                            </td>
                        </tr>
                        <tr className="border-t border-gray-200 last:border-b-0">
                            <td className="pl-2 h-12 text-sm font-bold">
                                Invoice total
                            </td>
                            <td className="text-center font-bold">
                                {formatCurrency(normalizedTotal)}€
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>

            {/* Process All Payments Button */}
            {!allPaymentsReady && (
                <div className="text-center text-sm text-gray-500 py-2">
                    Complete payment details for all guests to proceed
                </div>
            )}
            <div className="flex flex-row gap-8 rounded-full justify-end w-full">
                <PaymentButton
                    isImportant={true}
                    onClick={handleProcessAllPayments}
                    disabled={!allPaymentsReady || Object.values(processingPayments).some(p => p)}
                >
                    {Object.values(processingPayments).some(p => p) ? 'PROCESSING...' : 'PROCESS ALL PAYMENTS'}
                </PaymentButton>
            </div>
        </>
    );
}
