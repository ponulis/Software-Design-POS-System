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

  const formatDate = (dateString) => {
    if (!dateString) return 'N/A';
    const date = new Date(dateString);
    return date.toLocaleDateString("en-US", {
      weekday: "long",
      year: "numeric",
      month: "long",
      day: "numeric",
    });
  };

  const formatDateTime = (dateString) => {
    if (!dateString) return 'N/A';
    const date = new Date(dateString);
    return date.toLocaleString("en-US", {
      month: "short",
      day: "numeric",
      hour: "2-digit",
      minute: "2-digit",
    });
  };

  const canModify = appointment.status !== 'Completed' && appointment.status !== 'Cancelled';

  return (
    <>
      <h2 className="text-sm font-semibold text-gray-500 uppercase mb-1">
        Appointment
      </h2>
      <h1 className="text-2xl font-bold mb-4">#{appointment.id}</h1>

      <div className="text-gray-700 space-y-4 mb-6">
        <div>
          <p className="text-xs font-semibold text-gray-500 uppercase">
            Customer
          </p>
          <p className="font-medium">{appointment.customerName}</p>
          <p className="text-sm text-gray-600">{appointment.customerPhone}</p>
        </div>
        <div>
          <p className="text-xs font-semibold text-gray-500 uppercase">
            Date & Time
          </p>
          <p>{formatDateTime(appointment.date)}</p>
        </div>
        <div>
          <p className="text-xs font-semibold text-gray-500 uppercase">
            Service
          </p>
          <p>{appointment.serviceName || 'N/A'}</p>
        </div>
        <div>
          <p className="text-xs font-semibold text-gray-500 uppercase">
            Employee
          </p>
          <p>{appointment.employeeName || 'N/A'}</p>
        </div>
        <div>
          <p className="text-xs font-semibold text-gray-500 uppercase">
            Status
          </p>
          <span className={`inline-block px-3 py-1 text-sm rounded-lg font-medium ${
            appointment.status === 'Scheduled' ? 'bg-green-100 text-green-700' :
            appointment.status === 'Completed' ? 'bg-blue-100 text-blue-700' :
            appointment.status === 'Cancelled' ? 'bg-red-100 text-red-700' :
            'bg-gray-100 text-gray-700'
          }`}>
            {appointment.status}
          </span>
        </div>
        {appointment.orderId && (
          <div>
            <p className="text-xs font-semibold text-gray-500 uppercase">
              Order
            </p>
            <p>Order #{appointment.orderId}</p>
            {appointment.orderStatus && (
              <p className="text-sm text-gray-600">Status: {appointment.orderStatus}</p>
            )}
            {appointment.orderTotal && (
              <p className="text-sm text-gray-600">Total: {appointment.orderTotal.toFixed(2)}€</p>
            )}
          </div>
        )}
        {appointment.notes && (
          <div>
            <p className="text-xs font-semibold text-gray-500 uppercase">
              Notes
            </p>
            <p className="text-sm">{appointment.notes}</p>
          </div>
        )}
      </div>

      {canModify && (
        <div className="flex gap-3 mb-6">
          <button
            className="flex-1 bg-gray-200 hover:bg-gray-300 transition p-2 rounded-lg font-medium"
            onClick={() => onReschedule(appointment)}
          >
            Reschedule
          </button>
          <button
            className="flex-1 bg-red-200 hover:bg-red-300 transition p-2 rounded-lg font-medium text-red-800"
            onClick={() => {
              if (window.confirm('Are you sure you want to cancel this appointment?')) {
                onCancel(appointment.id);
              }
            }}
          >
            Cancel
          </button>
        </div>
      )}

      <div>
        <h2 className="text-sm font-semibold text-gray-500 uppercase mb-1">
          Activity Log
        </h2>
        <ul className="list-disc pl-5 text-gray-700 text-sm space-y-1">
          <li>Created: {formatDateTime(appointment.createdAt)}</li>
          {appointment.updatedAt && (
            <li>Last updated: {formatDateTime(appointment.updatedAt)}</li>
          )}
          <li>Status: {appointment.status}</li>
        </ul>
      </div>
    </>
  );
}
