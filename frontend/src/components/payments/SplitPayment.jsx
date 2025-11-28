import { useState } from "react";
import OrderRow from "./OrderRow";

export default function SplitPayment({ total, items }) {

    const [numberOfGuests, setNumberOfGuests] = useState(1);
    const [guests, setGuests] = useState([{ id: 1 }]);

    const handleAddPerson = () => {
        setNumberOfGuests(prev => prev + 1);
        setGuests(prev => [...prev, { id: Date.now() }]);
    };

    const handleRemovePerson = (id) => {
        setNumberOfGuests(prev => Math.max(prev - 1, 0));
        setGuests(prev => prev.filter(guest => guest.id !== id));
    };

    return (
        <>
            <div className="flex  justify-center flex-col rounded-md bg-gray-100 px-2 outline-1 outline-gray-300 gap-2">

                <div className="flex w-full items-center my-2">
                    
                    <div className="text-sm">
                        Guests ({numberOfGuests})
                    </div>

                    <button
                        onClick={handleAddPerson}
                        className="ml-auto px-3 py-1 rounded-full font-bold text-gray-800 text-xs 
                                border border-gray-300 select-none cursor-pointer bg-gray-300
                                hover:bg-gray-400 hover:text-white transition"
                    >
                        Add person
                    </button>
                </div>
                {guests.map((guest, idx) => (
                    <div key={guest.id} className="flex w-full justify-center items-center gap-2 mb-2">
        
                            <div className=" flex w-full items-center bg-white p-2 outline-1 rounded-md  outline-gray-300">
                    
                                <div className="text-sm select-none">
                                    Guest {idx+1}
                                </div>
                
                            </div>  
                            <button
                                onClick={() => handleRemovePerson(guest.id)}
                                className="ml-auto h-6 w-6 pb-[1px] rounded-full font-bold text-center text-gray-800 text-xs 
                                        border border-gray-300 select-none cursor-pointer bg-gray-300
                                        hover:bg-gray-400 hover:text-white transition"
                            >
                                ×
                            </button>
                        </div>
                ))}

            </div>

            <div className="flex  justify-center flex-col rounded-md bg-gray-100 px-2 outline-1 outline-gray-300 gap-2 pb-2">

                <div className="flex w-full items-center my-2">
                    
                    <div className="text-sm">
                        Assign items
                    </div>

                    <div className="text-xs ml-auto text-gray-400">
                        Select each guest that ordered the item
                    </div>
                </div> 
                
                <div className="rounded-xl overflow-x-auto border border-gray-300 pr-2">
                    <table className="table-auto w-full bg-white">
                        <thead className="bg-gray-100">
                            <tr className="text-left">
                            <th className="py-2 pl-2 text-xs text-gray-600 h-12">ITEM</th>
                            {guests.map((guest, idx) => (
                                <th  key={guest.id} className="py-2 pl-2 text-xs text-center text-gray-600 h-12">GUEST {idx+1}</th> 
                            ))}
                            </tr>
                        </thead>
                        <tbody>
                            {items && items.length > 0 ? (
                            items.map((item) => (
                                <tr key={item.id} className="border-b border-gray-200 last:border-b-0">
                                <td className="pl-2 h-12 text-sm">
                                    <div className="flex flex-col">
                                    <span>{item.title}</span>
                                    <span className="text-gray-500 text-xs max-w-[180px] truncate">
                                        {item.subtitle}
                                    </span>
                                    </div>
                                </td>

                                {guests.map((guest) => (
                                    <td key={guest.id} className="text-center">
                                    <input
                                        type="checkbox"
                                        id={`checkbox-${item.id}-${guest.id}`}
                                        className="w-4 h-4 border border-gray-300 rounded-sm bg-white"
                                    />
                                    </td>
                                ))}
                                </tr>
                            ))
                            ) : (
                            <tr>
                                <td colSpan={1 + guests.length} className="text-center text-gray-400 py-4">
                                No items found
                                </td>
                            </tr>
                            )}
                        </tbody>
    
                    </table>
                </div>                
              
            </div> 

            <div className="flex  justify-center flex-col rounded-md bg-gray-100 px-2 outline-1 outline-gray-300 gap-2 pb-2">

                <div className="flex w-full items-center my-2">
                    <div className="text-sm">
                        Per person totals
                    </div>
                </div> 

                    <table className="table-auto w-full bg-inherit">
                        <thead className="bg-inherit">
                            <tr className="text-left">
                                <th className="py-2 pl-2 text-xs text-gray-400 h-12">PERSON</th>
                                <th className="py-2 pl-2 text-xs text-center text-gray-400 h-12">AMOUNT</th>
                            </tr>
                        </thead>
                        <tbody>
                            {guests && guests.length > 0 ? (
                            guests.map((guest, idx) => (
                                <tr key={guest.id} className="border-t border-gray-200 last:border-b-0">
                                <td className="pl-2 h-12 text-sm">
                                    <div className="flex flex-col">
                                        <span>Guest {idx + 1} </span>
                                    </div>
                                </td>

                                <td key={guest.id} className="text-center">
                                    {/* Temp hardcoded for mockup */}
                                    <p>0.00€</p>
                                </td>
                                </tr>
                            ))
                            ) : (
                            <tr>
                                <td colSpan="2" className="text-center text-gray-400 py-4">
                                No guests found
                                </td>
                            </tr>
                            )}
                            <tr className="border-t border-gray-200 last:border-b-0">
                                <td className="pl-2 h-12 text-sm">
                                    Assigned total
                                </td>
                                <td className="text-center">
                                    {/* Temp hardcoded for mockup */}
                                    0.00€
                                </td>
                            </tr>
                            <tr className="border-t border-gray-200 last:border-b-0">
                                <td className="pl-2 h-12 text-sm font-bold">
                                    Invoice total
                                </td>
                                <td className="text-center font-bold">
                                    {total}€
                                </td>
                            </tr>
                        </tbody>
    
                    </table>
            </div>
        </>
    );    
}
