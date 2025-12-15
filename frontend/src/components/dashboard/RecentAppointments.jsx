import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { appointmentsApi } from '../../api/appointments';

export default function RecentAppointments({ appointments: providedAppointments, upcomingCount = 0 }) {
  const navigate = useNavigate();
  const [appointments, setAppointments] = useState(providedAppointments || []);
  const [loading, setLoading] = useState(!providedAppointments);

  useEffect(() => {
    // If appointments are provided, use them directly
    if (providedAppointments) {
      setAppointments(providedAppointments);
      setLoading(false);
      return;
    }

    // Otherwise, fetch appointments if upcomingCount > 0
    const fetchAppointments = async () => {
      try {
        setLoading(true);
        const response = await appointmentsApi.getAll({
          page: 1,
          pageSize: 5,
          status: 'Scheduled',
        });
        setAppointments(response.data || []);
      } catch (err) {
        console.error('Error fetching appointments:', err);
        setAppointments([]);
      } finally {
        setLoading(false);
      }
    };

    if (upcomingCount > 0) {
      fetchAppointments();
    } else {
      setLoading(false);
    }
  }, [providedAppointments, upcomingCount]);

  const formatDateTime = (dateString) => {
    if (!dateString) return 'N/A';
    const date = new Date(dateString);
    return date.toLocaleString('en-US', {
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
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
      default:
        return 'bg-gray-100 text-gray-700';
    }
  };

  return (
    <div className="bg-white rounded-lg shadow border border-gray-200">
      <div className="p-4 border-b border-gray-200 flex justify-between items-center">
        <h3 className="text-lg font-semibold text-gray-900">Upcoming Appointments</h3>
        <button
          onClick={() => navigate('/reservations')}
          className="text-sm text-blue-600 hover:text-blue-800 font-medium"
        >
          View All
        </button>
      </div>
      {loading ? (
        <div className="p-8 text-center">
          <div className="inline-block animate-spin rounded-full h-6 w-6 border-b-2 border-blue-600 mb-2"></div>
          <p className="text-sm text-gray-500">Loading...</p>
        </div>
      ) : appointments.length === 0 ? (
        <div className="p-8 text-center text-gray-400">
          <p>No upcoming appointments</p>
        </div>
      ) : (
        <div className="divide-y divide-gray-200">
          {appointments.map((apt) => (
            <div
              key={apt.id}
              onClick={() => navigate('/reservations')}
              className="p-4 hover:bg-gray-50 cursor-pointer transition"
            >
              <div className="flex justify-between items-start">
                <div>
                  <p className="font-medium text-gray-900">{apt.customerName}</p>
                  <p className="text-sm text-gray-600 mt-1">
                    {formatDateTime(apt.date)}
                  </p>
                  {apt.serviceName && (
                    <p className="text-sm text-gray-500 mt-1">{apt.serviceName}</p>
                  )}
                  {apt.employeeName && (
                    <p className="text-xs text-gray-500 mt-1">With {apt.employeeName}</p>
                  )}
                </div>
                <span className={`px-2 py-1 text-xs rounded font-medium ${getStatusColor(apt.status)}`}>
                  {apt.status}
                </span>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
