import React, { useState, useEffect } from "react";
import { hours, minutes, calculateEndTime } from "./timeUtils";

export default function RescheduleAppointmentModal({
  isOpen,
  onClose,
  appointment,
  onSubmit,
}) {
  const [date, setDate] = useState("");
  const [time, setTime] = useState(""); 
  const [duration, setDuration] = useState(""); 
  const durations = [15, 30, 45, 60, 90, 120];

  useEffect(() => {
    if (appointment) {
      setDate(appointment.sortDate || "");
      const t = appointment.sortTime || "";
      const [hh, mm] = t.split(":");
      setTime(`${hh || ""}:${mm || ""}`);
      setDuration(appointment.duration || "");
    }
  }, [appointment]);

  if (!isOpen || !appointment) return null;

  const handleSubmit = (e) => {
    e.preventDefault();
    const end = calculateEndTime(time, duration);
    onSubmit({
      ...appointment,
      sortDate: date,
      sortTime: time,
      duration,
      end, 
    });
  };

  const hour = (time || "").split(":")[0];
  const minute = (time || "").split(":")[1];
  const endTime = calculateEndTime(time, duration);

  return (
    <div className="fixed inset-0 flex justify-center items-center p-4 bg-black/20">
      <div className="bg-white p-6 rounded-lg w-full max-w-md shadow">
        <h2 className="text-xl font-bold mb-4">Reschedule Appointment</h2>

        <form className="space-y-4" onSubmit={handleSubmit}>
          {/* Date picker */}
          <div>
            <p className="text-xs font-semibold uppercase text-gray-500">New Date</p>
            <input
              type="date"
              className="w-full border p-2 rounded"
              required
              value={date}
              onChange={(e) => setDate(e.target.value)}
            />
          </div>

          {/* Time picker */}
          <div>
            <p className="text-xs font-semibold uppercase text-gray-500">New Time</p>
            <div className="flex gap-2">
              <select
                className="flex-1 border p-2 rounded text-sm"
                value={hour || ""}
                onChange={(e) => setTime(`${e.target.value}:${minute || ""}`)}
                required
              >
                <option value="">Hour</option>
                {hours.map((h) => (
                  <option key={h} value={h}>{h}</option>
                ))}
              </select>

              <select
                className="flex-1 border p-2 rounded text-sm"
                value={minute || ""}
                onChange={(e) => setTime(`${hour || ""}:${e.target.value}`)}
                required
              >
                <option value="">Min</option>
                {minutes.map((m) => (
                  <option key={m} value={m}>{m}</option>
                ))}
              </select>
            </div>
          </div>

          <div>
            <p className="text-xs font-semibold uppercase text-gray-500">Duration (minutes)</p>
            <select
              className="w-full border p-2 rounded text-sm"
              value={duration || ""}
              onChange={(e) => setDuration(e.target.value)}
              required
            >
              <option value="">Select duration</option>
              {durations.map((d) => (
                <option key={d} value={d}>{d} min</option>
              ))}
            </select>
          </div>

          {endTime && (
            <div className="bg-gray-50 p-3 rounded border border-gray-200">
              <p className="text-xs font-semibold uppercase text-gray-500 mb-1">
                Ends
              </p>
              <p className="text-sm font-medium">{endTime}</p>
            </div>
          )}

          <div className="flex justify-end gap-2">
            <button
              type="button"
              className="px-4 py-2 bg-gray-200 rounded"
              onClick={onClose}
            >
              Cancel
            </button>
            <button className="px-4 py-2 bg-purple-600 text-white rounded">Save</button>
          </div>
        </form>
      </div>
    </div>
  );
}
