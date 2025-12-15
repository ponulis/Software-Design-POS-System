export default function PaymentButton({ children, isImportant, selected, onClick, disabled }) {
  const baseClasses = "rounded-full font-semibold text-gray-800 flex-1 p-3 text-xs text-center border border-gray-300 select-none transition";
  
  let colorClasses = "";

  if (disabled) {
    colorClasses = "bg-gray-200 border-gray-300 text-gray-400 cursor-not-allowed";
  } else if (isImportant) {
    colorClasses = "bg-blue-200 border-gray-300 hover:bg-blue-300 hover:border-blue-400 hover:text-white cursor-pointer";
  } else if (selected) {      
    colorClasses = "bg-gray-300 border-gray-300 hover:bg-gray-300 text-white cursor-pointer";
  } else {
    colorClasses = "bg-gray-100 border-gray-300 hover:bg-gray-300 cursor-pointer";
  }

  return (
    <div>
      <div 
        className={`${baseClasses} ${colorClasses}`}
        onClick={disabled ? undefined : onClick}
      >
        {children}
      </div>
    </div>
  );
}
