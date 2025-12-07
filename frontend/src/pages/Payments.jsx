import PaymentsList from "../components/payments/PaymentsList";
import { usePayments } from "../components/payments/usePayments";
import PaymentDetails from "../components/payments/PaymentDetails";

export default function Payments() {
  const {
    payments,
    selected,
    selectPayment,
  } = usePayments();

  return (
    <div className="min-h-screen bg-gray-100 p-6 flex gap-6">
      <div className="w-2/3 bg-white shadow rounded-xl p-6">
        <h2 className="text-sm font-semibold text-gray-500 uppercase mb-1">
          List of payments for appointments
        </h2>
        <h1 className="text-2xl font-bold mb-6">Payments</h1>

        <PaymentsList
          payments={payments}
          selected={selected}
          onSelect={selectPayment}
        />
      </div>

      <div className="w-1/3 bg-white shadow rounded-xl p-6">
        <PaymentDetails
          //Commented our for testing
          //payment={selected}
        />
      </div>
    </div>
  );
}
