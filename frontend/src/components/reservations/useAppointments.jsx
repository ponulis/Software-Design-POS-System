import { useState } from "react";

export function useAppointments() {
  const [appointments, setAppointments] = useState([]);
  const [selected, setSelected] = useState(null);

  const calculateEndTime = (startTime, duration) => {
    if (!startTime || !duration) return "";

    const [h, m] = startTime.split(":").map(Number);
    const dur = parseInt(duration);

    const total = h * 60 + m + dur;
    const endH = Math.floor(total / 60) % 24;
    const endM = total % 60;

    return `${String(endH).padStart(2, "0")}:${String(endM).padStart(2, "0")}`;
  };


  const addAppointment = (newApt) => {
    const id = `APT-${Math.floor(1000 + Math.random() * 9000)}`;
    const order = `ORD-${Math.floor(1000 + Math.random() * 9000)}`;
    const endTime = calculateEndTime(newApt.time, newApt.duration);

    const formattedDate =
      newApt.date && newApt.time
        ? `${new Date(newApt.date).toLocaleDateString("en-US", {
            weekday: "short",
            month: "short",
            day: "numeric",
          })}, ${newApt.time}`
        : "";

    const appointment = {
      ...newApt,
      id,
      order,
      status: "Scheduled",
      end: endTime,
      date: formattedDate,
      duration: `${newApt.duration} minutes`,
      sortDate: newApt.date,
      sortTime: newApt.time,
    };

    setAppointments((prev) =>
      [...prev, appointment].sort((a, b) => {
        const dateA = new Date(`${a.sortDate}T${a.sortTime}`);
        const dateB = new Date(`${b.sortDate}T${b.sortTime}`);
        return dateA - dateB;
      })
    );
  };

  const selectAppointment = (apt) => {
    setSelected(apt);
  };

  const cancelAppointment = (id) => {
    setAppointments((prev) =>
      prev.map((apt) =>
        apt.id === id ? { ...apt, status: "Cancelled" } : apt
      )
    );

    if (selected?.id === id) {
      setSelected((prev) => ({ ...prev, status: "Cancelled" }));
    }
  };

  const rescheduleAppointment = (updated) => {
    const startTime = updated.time || updated.sortTime || "";

    const dur = updated.duration;

    const updatedEnd = updated.end || calculateEndTime(startTime, dur);

    const formattedDate =
      (updated.sortDate || updated.date) && (updated.sortTime || startTime)
        ? `${new Date(updated.sortDate || updated.date).toLocaleDateString("en-US", {
            weekday: "short",
            month: "short",
            day: "numeric",
          })}, ${updated.sortTime || startTime}`
        : "";

    let durationDisplay = dur || "";
    if (durationDisplay && !String(durationDisplay).includes("minute")) {
      durationDisplay = `${durationDisplay} minutes`;
    }

    const finalApt = {
      ...updated,
      sortDate: updated.sortDate || updated.date || "",
      sortTime: updated.sortTime || startTime,
      end: updatedEnd,
      date: formattedDate,
      duration: durationDisplay,
    };

    setAppointments((prev) =>
      [...prev.map((a) => (a.id === updated.id ? finalApt : a))].sort(
        (a, b) => {
          const dateA = new Date(`${a.sortDate}T${a.sortTime}`);
          const dateB = new Date(`${b.sortDate}T${b.sortTime}`);
          return dateA - dateB;
        }
      )
    );

    setSelected(finalApt);
  };

  return {
    appointments,
    selected,
    selectAppointment,
    addAppointment,
    cancelAppointment,
    rescheduleAppointment,
  };
}
