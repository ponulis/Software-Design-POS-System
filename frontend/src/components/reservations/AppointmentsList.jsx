import React from "react";

export default function AppointmentsList({ appointments, selected, onSelect }) {
  return (
    <div className="space-y-4">
      {appointments.length === 0 ? (
        <div className="text-center py-12 text-gray-400">
          <p>No appointments yet. Click "Add Appointment" to create one.</p>
        </div>
      ) : (
        appointments.map((apt) => (
          <div
            key={apt.id}
            onClick={() => onSelect(apt)}
            className={`grid grid-cols-5 items-center p-4 rounded-lg border cursor-pointer transition hover:bg-purple-50 
              ${selected?.id === apt.id ? "ring-2 ring-purple-400 bg-purple-100" : ""} 
              ${apt.status === "Scheduled" ? "bg-purple-50 border-purple-100" : "bg-white"}`}
          >
            <div>
              <p className="font-semibold text-gray-800">{apt.date}</p>
              <p className="text-xs text-gray-500">Ends {apt.end}</p>
            </div>
            <p>{apt.customer}</p>
            <p>{apt.staff}</p>
            <p>{apt.service}</p>
            <span
              className={`px-3 py-1 text-sm rounded-lg ${
                apt.status === "Scheduled" && "bg-green-100 text-green-700"
              } ${apt.status === "Cancelled" && "bg-red-100 text-red-700"}
              ${apt.status === "Completed" && "bg-blue-100 text-blue-700"}`}
            >
              {apt.status}
            </span>
          </div>
        ))
      )}
    </div>
  );
}
