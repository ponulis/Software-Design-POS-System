import React, { useState, useEffect } from "react";
import { useServices } from "../../hooks/useServices";
import { useEmployees } from "../../hooks/useEmployees";

export default function AddAppointmentModal({
  isOpen,
  onClose,
  newApt,
  setNewApt,
  onAdd,
  hours = [],
  minutes = [],
  calculateEndTime = () => "",
}) {
  const { services, loading: servicesLoading } = useServices();
  const { employees, loading: employeesLoading } = useEmployees();
  const [submitting, setSubmitting] = useState(false);

  if (!isOpen) return null;

  const handleSubmit = async () => {
    if (!newApt.date || !newApt.time || !newApt.customer) {
      alert('Please fill in all required fields');
      return;
    }

    setSubmitting(true);
    const result = await onAdd(newApt);
    setSubmitting(false);
    
    if (result?.success) {
      onClose();
    } else if (result?.error) {
      alert(result.error);
    }
  }; 

  return (
    <div className="fixed inset-0 flex justify-center items-center p-4 bg-black/30 z-50">
      <div className="bg-white rounded-lg shadow-lg p-6 w-full max-w-3xl">
        <h2 className="text-xl font-bold mb-4">New Appointment</h2>

        <div className="grid grid-cols-3 gap-4 mb-4">
          {Object.keys(newApt).map((key) => (
            <div key={key}>
              <p className="text-xs font-semibold uppercase text-gray-500 mb-1">
                {key}
              </p>

              {key === "date" ? (
                <input
                  type="date"
                  className="w-full border p-2 rounded text-sm"
                  value={newApt[key] || ""}
                  onChange={(e) =>
                    setNewApt({ ...newApt, [key]: e.target.value })
                  }
                />
              ) : key === "time" ? (
                <div className="flex gap-2">
                  <select
                    className="flex-1 border p-2 rounded text-sm"
                    value={(newApt[key] || "").split(":")[0] || ""}
                    onChange={(e) => {
                      const mins = (newApt[key] || "").split(":")[1] || "00";
                      setNewApt({ ...newApt, time: `${e.target.value}:${mins}` });
                    }}
                  >
                    <option value="">Hour</option>
                    {hours.map((h) => (
                      <option key={h} value={h}>
                        {h}
                      </option>
                    ))}
                  </select>

                  <select
                    className="flex-1 border p-2 rounded text-sm"
                    value={(newApt[key] || "").split(":")[1] || ""}
                    onChange={(e) => {
                      const hour = (newApt[key] || "").split(":")[0] || "00";
                      setNewApt({ ...newApt, time: `${hour}:${e.target.value}` });
                    }}
                  >
                    <option value="">Min</option>
                    {minutes.map((m) => (
                      <option key={m} value={m}>
                        {m}
                      </option>
                    ))}
                  </select>
                </div>
              ) : key === "duration" ? (
                <select
                  className="w-full border p-2 rounded text-sm"
                  value={newApt[key] || ""}
                  onChange={(e) =>
                    setNewApt({ ...newApt, [key]: e.target.value })
                  }
                >
                  <option value="">Select duration</option>
                  <option value="15">15 minutes</option>
                  <option value="30">30 minutes</option>
                  <option value="45">45 minutes</option>
                  <option value="60">60 minutes</option>
                  <option value="90">90 minutes</option>
                  <option value="120">120 minutes</option>
                </select>
              ) : key === "prepaid" ? (
                <select
                  className="w-full border p-2 rounded text-sm"
                  value={newApt[key] || "No"}
                  onChange={(e) =>
                    setNewApt({ ...newApt, [key]: e.target.value })
                  }
                >
                  <option value="Yes">Yes</option>
                  <option value="No">No</option>
                </select>
              ) : (
                <input
                  className="w-full border p-2 rounded text-sm"
                  value={newApt[key] || ""}
                  onChange={(e) =>
                    setNewApt({ ...newApt, [key]: e.target.value })
                  }
                />
              )}
            </div>
          ))}

          {newApt.time && newApt.duration && (
            <div className="col-span-3 bg-gray-50 p-3 rounded border border-gray-200">
              <p className="text-xs font-semibold uppercase text-gray-500 mb-1">
                Calculated End Time
              </p>
              <p className="text-sm font-medium">
                {calculateEndTime(newApt.time, newApt.duration)}
              </p>
            </div>
          )}
        </div>

        <div className="flex gap-3 justify-end">
          <button
            className="bg-gray-200 px-4 py-2 rounded hover:bg-gray-300"
            onClick={onClose}
            type="button"
          >
            Cancel
          </button>
          <button
            className="bg-purple-600 text-white px-4 py-2 rounded hover:bg-purple-700"
            onClick={onAdd}
            type="button"
          >
            Add
          </button>
        </div>
      </div>
    </div>
  );
}
