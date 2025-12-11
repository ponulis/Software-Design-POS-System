export default function OrderRow({ title, subtitle, qty, price }) {
  const lineTotal = (qty * price).toFixed(2);

  return (
    <tr className="border-b border-gray-200 last:border-b-0">
      <td className="pl-2 h-12 text-sm">
        <div className="flex flex-col">
          <span>{title}</span>
          <span className="text-gray-500 text-xs max-w-[180px] truncate">{subtitle}</span>
        </div>
      </td>
      <td>{qty}</td>
      <td>{price.toFixed(2)}€</td>
      <td>{lineTotal}€</td>
    </tr>
  );
}
