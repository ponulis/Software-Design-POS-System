export default function ProductCard({ product, onSelect, onEdit, onDelete }) {
  const handleSelect = () => {
    onSelect(product);
  };

  const handleEdit = (e) => {
    e.stopPropagation();
    onEdit(product);
  };

  const handleDelete = (e) => {
    e.stopPropagation();
    if (window.confirm(`Are you sure you want to delete "${product.name}"?`)) {
      onDelete(product.id);
    }
  };

  return (
    <div
      onClick={handleSelect}
      className={`bg-white rounded-lg shadow-md p-4 cursor-pointer transition hover:shadow-lg border-2 ${
        product.available ? 'border-gray-200' : 'border-red-200 opacity-60'
      }`}
    >
      <div className="flex justify-between items-start mb-2">
        <h3 className="text-lg font-semibold text-gray-800">{product.name}</h3>
        <div className="flex gap-2">
          <button
            onClick={handleEdit}
            className="text-blue-600 hover:text-blue-800 text-sm px-2 py-1 rounded hover:bg-blue-50"
          >
            Edit
          </button>
          <button
            onClick={handleDelete}
            className="text-red-600 hover:text-red-800 text-sm px-2 py-1 rounded hover:bg-red-50"
          >
            Delete
          </button>
        </div>
      </div>

      {product.description && (
        <p className="text-sm text-gray-600 mb-2 line-clamp-2">{product.description}</p>
      )}

      <div className="flex items-center justify-between mt-3">
        <span className="text-xl font-bold text-gray-900">
          {parseFloat(product.price).toFixed(2)}€
        </span>
        <span
          className={`px-2 py-1 text-xs rounded ${
            product.available
              ? 'bg-green-100 text-green-700'
              : 'bg-red-100 text-red-700'
          }`}
        >
          {product.available ? 'Available' : 'Unavailable'}
        </span>
      </div>

      {product.tags && product.tags.length > 0 && (
        <div className="mt-2 flex flex-wrap gap-1">
          {product.tags.map((tag, idx) => (
            <span
              key={idx}
              className="px-2 py-1 text-xs bg-gray-100 text-gray-600 rounded"
            >
              {tag}
            </span>
          ))}
        </div>
      )}
    </div>
  );
}
