import { useState } from "react";

export function usePayments() {
  const [selected, setSelected] = useState(null);

  // Mock payments for testing
  const [payments, _] = useState([
    {
      id: 1,
      date: "2025-12-01",
      price: "100",
      customer: "John Paul",
      service: "Feet massage",
      status: "Unpaid",
    },
    {
      id: 2,
      date: "2025-11-15",
      price: "50",
      service: "Feet massage",
      customer: "Peter John",
      status: "Prepaid",
    },
    {
      id: 3,
      date: "2025-11-20",
      price: "75",
      customer: "Chris Tady",
      service: "Feet massage",
      status: "Paid",
    },
  ]);


  const selectPayment = (apt) => {
    setSelected(apt);
  };

  return {
    selected,
    payments,
    selectPayment
  };
}
