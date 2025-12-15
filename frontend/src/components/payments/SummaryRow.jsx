export default function SummaryRow({ label, value, isTotal }) {
  const baseClasses = "flex justify-between text-xs py-1";
  const totalClasses = "flex justify-between text-xs border-t border-gray-300 pt-1 font-bold";
  
  // Handle both string and number values
  const numericValue = typeof value === 'string' ? parseFloat(value) : value;
  
  const formattedValue = isNaN(numericValue) || numericValue === null || numericValue === undefined
    ? '0.00'
    : numericValue.toFixed(2);

  return (
    <div className={isTotal ? totalClasses : baseClasses}>
      <span className="text-gray-800">{label}</span>
      <span className="text-gray-800">{formattedValue}€</span>
    </div>
  );
}
