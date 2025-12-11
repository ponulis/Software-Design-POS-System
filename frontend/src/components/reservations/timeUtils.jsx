export const hours = Array.from({ length: 24 }, (_, i) =>
  i.toString().padStart(2, "0")
);

export const minutes = Array.from({ length: 12 }, (_, i) =>
  (i * 5).toString().padStart(2, "0")
);

export const calculateEndTime = (startTime, duration) => {
  if (!startTime || !duration) return "";
  const [hours, minutes] = startTime.split(":").map(Number);
  const durationMinutes = parseInt(duration);
  const totalMinutes = hours * 60 + minutes + durationMinutes;
  const endHours = Math.floor(totalMinutes / 60) % 24;
  const endMinutes = totalMinutes % 60;

  return `${endHours.toString().padStart(2, "0")}:${endMinutes
    .toString()
    .padStart(2, "0")}`;
};
