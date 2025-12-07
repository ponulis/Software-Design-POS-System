import React from "react";

export default function PaymentsList({ payments=[], selected, onSelect }) {
  return (
    <div className="space-y-4">
      {payments.length === 0 ? (
        <div className="text-center py-12 text-gray-400">
          <p>No ready payments yet.</p>
        </div>
      ) : (
        payments.map((pay) => (
          <div
            key={pay.id}
            onClick={() => onSelect(pay)}
            className={`grid grid-cols-5 items-center p-4 rounded-lg border cursor-pointer transition hover:bg-purple-50 
              ${selected?.id === pay.id ? "ring-2 ring-purple-400 bg-purple-100" : ""} 
              ${pay.status === "Prepaid" ? "bg-purple-50 border-purple-100" : "bg-white border-purple-100"}`}
          >
            <div>
              <p className="font-semibold text-gray-800">{pay.date}</p>
            </div>
            <p>{pay.price}€</p>
            <p>{pay.customer}</p>
            <p>{pay.service}</p>
            <span
              className={`px-3 py-1 text-sm rounded-lg 
              ${pay.status === "Prepaid" && "bg-green-100 text-green-700"} 
              ${pay.status === "Unpaid" && "bg-red-100 text-red-700"}
              ${pay.status === "Paid" && "bg-blue-100 text-blue-700"}`}
            >
              {pay.status}
            </span>
          </div>
        ))
      )}
    </div>
  );
}
