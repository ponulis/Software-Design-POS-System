import React from "react";

export default function AppointmentsList({ appointments, selected, onSelect }) {
  const formatDate = (dateString) => {
    if (!dateString) return 'N/A';
    const date = new Date(dateString);
    return date.toLocaleDateString("en-US", {
      weekday: "short",
      month: "short",
      day: "numeric",
    });
  };

  const formatTime = (dateString) => {
    if (!dateString) return '';
    const date = new Date(dateString);
    return date.toLocaleTimeString("en-US", {
      hour: "2-digit",
      minute: "2-digit",
    });
  };

  const getStatusColor = (status) => {
    switch (status) {
      case 'Scheduled':
        return 'bg-green-100 text-green-700';
      case 'Completed':
        return 'bg-blue-100 text-blue-700';
      case 'Cancelled':
        return 'bg-red-100 text-red-700';
      case 'Rescheduled':
        return 'bg-yellow-100 text-yellow-700';
      default:
        return 'bg-gray-100 text-gray-700';
    }
  };

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
              bg-white border-purple-100`}
          >
            <div>
              <p className="font-semibold text-gray-800">{formatDate(apt.date)}</p>
              <p className="text-xs text-gray-500">{formatTime(apt.date)}</p>
            </div>
            <p className="text-sm">{apt.customerName || 'N/A'}</p>
            <p className="text-sm">{apt.employeeName || 'N/A'}</p>
            <p className="text-sm">{apt.serviceName || 'N/A'}</p>
            <span
              className={`px-3 py-1 text-sm rounded-lg font-medium ${getStatusColor(apt.status)}`}
            >
              {apt.status || 'Scheduled'}
            </span>
          </div>
        ))
      )}
    </div>
  );
}
