import React from "react";

export default function AppointmentDetails({
  appointment,
  onCancel,
  onReschedule,
}) {
  if (!appointment) {
    return (
      <div className="flex items-center justify-center h-full text-gray-400">
        <p>Select an appointment to view details</p>
      </div>
    );
  }

  return (
    <>
      <h2 className="text-sm font-semibold text-gray-500 uppercase mb-1">
        Appointment
      </h2>
      <h1 className="text-2xl font-bold mb-4">{appointment.id}</h1>

      <div className="text-gray-700 space-y-4 mb-6">
        <div>
          <p className="text-xs font-semibold text-gray-500 uppercase">
            Service
          </p>
          <p>{appointment.service}</p>
        </div>
        <div>
          <p className="text-xs font-semibold text-gray-500 uppercase">
            Duration
          </p>
          <p>{appointment.duration}</p>
        </div>
        <div>
          <p className="text-xs font-semibold text-gray-500 uppercase">
            Status
          </p>
          <p>{appointment.status}</p>
        </div>
        <div>
          <p className="text-xs font-semibold text-gray-500 uppercase">
            Prepaid
          </p>
          <p>{appointment.prepaid}</p>
        </div>
        <div>
          <p className="text-xs font-semibold text-gray-500 uppercase">
            Order
          </p>
          <p>{appointment.order}</p>
        </div>
      </div>

      <div className="flex gap-3 mb-6">
        <button
          className="flex-1 bg-gray-200 hover:bg-gray-300 transition p-2 rounded-lg font-medium"
          onClick={() => onReschedule(appointment)}
        >
          Reschedule
        </button>
        <button
          className="flex-1 bg-gray-200 hover:bg-gray-300 transition p-2 rounded-lg font-medium"
          onClick={() => onCancel(appointment.id)}
        >
          Cancel appointment
        </button>
      </div>

      <div>
        <h2 className="text-sm font-semibold text-gray-500 uppercase mb-1">
          Activity Log
        </h2>
        <ul className="list-disc pl-5 text-gray-700 text-sm">
          <li>Appointment created.</li>
          <li>Status: {appointment.status}</li>
        </ul>
      </div>
    </>
  );
}
