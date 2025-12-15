import SummaryRow from "./SummaryRow";
import OrderRow from "./OrderRow";

export default function OrderDetails({ paymentType, items, subtotal, taxes, discounts, total, orderStatus }) {
    return (
        <div className="bg-gray-50 rounded-xl p-6 w-full">
            {paymentType && (
                <h1 className="text-2xl font-bold mb-4 text-left">
                    Payment by {paymentType}
                </h1>
            )}
            <h2 className="mb-4 text-left">
                Order details
            </h2>

            {orderStatus && (
                <div className="mb-4">
                    <span className={`px-3 py-1 text-sm rounded-lg font-medium ${
                        orderStatus === 'Paid' ? 'bg-green-100 text-green-700' :
                        orderStatus === 'Cancelled' ? 'bg-red-100 text-red-700' :
                        orderStatus === 'Placed' ? 'bg-blue-100 text-blue-700' :
                        'bg-gray-100 text-gray-700'
                    }`}>
                        Status: {orderStatus}
                    </span>
                </div>
            )}

            <div className="rounded-xl overflow-hidden border border-gray-300">
                <table className="table-auto w-full bg-white">
                <thead className="bg-gray-100">
                    <tr className="text-left">
                    <th className="py-2 pl-2 text-xs text-gray-600 h-12">LIST OF ITEMS</th>
                    <th className="py-2 text-xs text-gray-600 h-12">QUANTITIES</th>
                    <th className="py-2 text-xs text-gray-600 h-12">UNIT PRICE</th>
                    <th className="py-2 text-xs text-gray-600 h-12">LINE TOTAL</th>
                    </tr>
                </thead>
                <tbody>
                    {items && items.length > 0 ? (
                    items.map((item, idx) => (
                        <OrderRow
                        key={idx}
                        title={item.title}
                        subtitle={item.subtitle}
                        qty={item.qty}
                        price={item.price}
                        />
                    ))
                    ) : (
                    <tr>
                        <td colSpan="4" className="text-center text-gray-400 py-4">
                        No items found
                        </td>
                    </tr>
                    )}
                </tbody>

                </table>
            </div>

            <div className="flex">
                <div className="ml-auto w-[180px] h-[100px]">
                <SummaryRow label="Subtotal:" value={subtotal} />
                <SummaryRow label="Taxes:" value={taxes} />
                <SummaryRow label="Discounts:" value={discounts} />
                <SummaryRow label="Total amount:" value={total} isTotal />
                </div>
            </div>
        </div>
    );
}