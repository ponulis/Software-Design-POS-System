export default function PaymentButton({ children, isImportant, selected, onClick}) {


  const baseClasses = "rounded-full font-semibold text-gray-800 flex-1 p-3 text-xs text-center border border-gray-300 select-none hover:text-white hover:text-white";
  
  let colorClasses = "";

  if (isImportant) {
    colorClasses = "bg-blue-200 border-gray-300 hover:bg-blue-300 hover:border-blue-400";
  } else if (selected) {      
    colorClasses = "bg-gray-300 border-gray-300 hover:bg-gray-300 text-white";
  } else
  {
    colorClasses = "bg-gray-100 border-gray-300 hover:bg-gray-300";
  }

  return (
    <div>
      <div className={`${baseClasses} ${colorClasses}`}
            onClick={onClick}
      >
        {children}
      </div>
    </div>

  );
}
