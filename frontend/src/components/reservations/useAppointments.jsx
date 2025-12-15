import { useState, useEffect, useCallback } from "react";
import { appointmentsApi } from "../../api/appointments";

export function useAppointments() {
  const [appointments, setAppointments] = useState([]);
  const [selected, setSelected] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  // Fetch appointments from API
  const fetchAppointments = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      
      const response = await appointmentsApi.getAll({
        page: 1,
        pageSize: 100,
      });

      if (response.data) {
        setAppointments(response.data);
      } else {
        setAppointments(Array.isArray(response) ? response : []);
      }
    } catch (err) {
      console.error('Error fetching appointments:', err);
      setError(err.response?.data?.message || 'Failed to fetch appointments');
      setAppointments([]);
    } finally {
      setLoading(false);
    }
  }, []);

  // Initial load
  useEffect(() => {
    fetchAppointments();
  }, [fetchAppointments]);

  const calculateEndTime = (startTime, durationMinutes) => {
    if (!startTime || !durationMinutes) return "";

    const [h, m] = startTime.split(":").map(Number);
    const dur = parseInt(durationMinutes);

    const total = h * 60 + m + dur;
    const endH = Math.floor(total / 60) % 24;
    const endM = total % 60;

    return `${String(endH).padStart(2, "0")}:${String(endM).padStart(2, "0")}`;
  };

  const formatAppointmentDate = (dateString, timeString) => {
    if (!dateString || !timeString) return "";
    const date = new Date(dateString);
    return `${date.toLocaleDateString("en-US", {
      weekday: "short",
      month: "short",
      day: "numeric",
    })}, ${timeString}`;
  };

  const addAppointment = async (appointmentData) => {
    try {
      setError(null);
      
      // Convert form data to API format
      const dateTime = new Date(`${appointmentData.date}T${appointmentData.time}`);
      
      const apiData = {
        date: dateTime.toISOString(),
        customerName: appointmentData.customer,
        customerPhone: appointmentData.phone || "",
        serviceId: appointmentData.serviceId ? parseInt(appointmentData.serviceId) : null,
        employeeId: appointmentData.employeeId ? parseInt(appointmentData.employeeId) : null,
        notes: appointmentData.notes || null,
        orderId: appointmentData.prepaid === "Yes" ? null : null, // TODO: Create order if prepaid
      };

      const newAppointment = await appointmentsApi.create(apiData);
      await fetchAppointments();
      return { success: true, appointment: newAppointment };
    } catch (err) {
      const errorMessage = err.response?.data?.message || 'Failed to create appointment';
      setError(errorMessage);
      return { success: false, error: errorMessage };
    }
  };

  const selectAppointment = (apt) => {
    setSelected(apt);
  };

  const cancelAppointment = async (appointmentId) => {
    try {
      setError(null);
      await appointmentsApi.cancel(appointmentId);
      await fetchAppointments();
      if (selected?.id === appointmentId) {
        setSelected(null);
      }
      return { success: true };
    } catch (err) {
      const errorMessage = err.response?.data?.message || 'Failed to cancel appointment';
      setError(errorMessage);
      return { success: false, error: errorMessage };
    }
  };

  const rescheduleAppointment = async (appointmentData) => {
    try {
      setError(null);
      
      const dateTime = new Date(`${appointmentData.sortDate}T${appointmentData.sortTime}`);
      
      const updateData = {
        date: dateTime.toISOString(),
      };

      const updatedAppointment = await appointmentsApi.update(appointmentData.id, updateData);
      await fetchAppointments();
      
      if (selected?.id === appointmentData.id) {
        setSelected(updatedAppointment);
      }
      
      return { success: true, appointment: updatedAppointment };
    } catch (err) {
      const errorMessage = err.response?.data?.message || 'Failed to reschedule appointment';
      setError(errorMessage);
      return { success: false, error: errorMessage };
    }
  };

  return {
    appointments,
    selected,
    loading,
    error,
    selectAppointment,
    addAppointment,
    cancelAppointment,
    rescheduleAppointment,
    refreshAppointments: fetchAppointments,
  };
}
