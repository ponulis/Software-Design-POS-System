import React, { useState } from "react";
import AppointmentsList from "../components/reservations/appointmentsList";
import AppointmentDetails from "../components/reservations/AppointmentDetails";
import AddAppointmentModal from "../components/reservations/AddAppointmentModal";
import RescheduleAppointmentModal from "../components/reservations/RescheduleAppointmentModal";
import { useAppointments } from "../components/reservations/useAppointments";

export default function Reservations() {
  const {
    appointments,
    selected,
    selectAppointment,
    addAppointment,
    cancelAppointment,
    rescheduleAppointment,
  } = useAppointments();

  const [showAddModal, setShowAddModal] = useState(false);
  const [showResModal, setShowResModal] = useState(false);

  const [newApt, setNewApt] = useState({
    date: "",
    time: "",
    customer: "",
    staff: "",
    service: "",
    duration: "",
    prepaid: "No",
  });

  const hours = Array.from({ length: 24 }, (_, i) =>
    i.toString().padStart(2, "0")
  );
  const minutes = Array.from({ length: 12 }, (_, i) =>
    (i * 5).toString().padStart(2, "0")
  );

  const calculateEndTime = (startTime, duration) => {
    if (!startTime || !duration) return "";
    const [h, m] = startTime.split(":").map(Number);
    const dur = parseInt(duration);
    const tot = h * 60 + m + dur;
    return `${String(Math.floor(tot / 60) % 24).padStart(2, "0")}:${String(
      tot % 60
    ).padStart(2, "0")}`;
  };

  const handleAdd = () => {
    addAppointment(newApt);
    setShowAddModal(false);
    setNewApt({
      date: "",
      time: "",
      customer: "",
      staff: "",
      service: "",
      duration: "",
      prepaid: "No",
    });
  };

  const handleReschedule = (apt) => {
    selectAppointment(apt);
    setShowResModal(true);
  };

  const submitReschedule = (updated) => {
    rescheduleAppointment(updated);
    setShowResModal(false);
  };

  return (
    <div className="min-h-screen bg-gray-100 p-6 flex gap-6">
      <div className="w-2/3 bg-white shadow rounded-xl p-6">
        <h2 className="text-sm font-semibold text-gray-500 uppercase mb-1">
          Schedule
        </h2>
        <h1 className="text-2xl font-bold mb-6">Daily Appointments</h1>

        <AppointmentsList
          appointments={appointments}
          selected={selected}
          onSelect={selectAppointment}
        />
      </div>

      <div className="w-1/3 bg-white shadow rounded-xl p-6">
        <AppointmentDetails
          appointment={selected}
          onCancel={cancelAppointment}
          onReschedule={handleReschedule}
        />
      </div>

      <div className="fixed bottom-6 right-6">
        <button
          onClick={() => setShowAddModal(true)}
          className="bg-purple-600 text-white px-4 py-2 rounded-xl shadow hover:bg-purple-700"
        >
          Add Appointment
        </button>
      </div>

      <AddAppointmentModal
        isOpen={showAddModal}
        onClose={() => setShowAddModal(false)}
        newApt={newApt}
        setNewApt={setNewApt}
        onAdd={handleAdd}
        hours={hours}
        minutes={minutes}
        calculateEndTime={calculateEndTime}
      />

      <RescheduleAppointmentModal
        isOpen={showResModal}
        appointment={selected}
        onClose={() => setShowResModal(false)}
        onSubmit={submitReschedule}
      />
    </div>
  );
}
