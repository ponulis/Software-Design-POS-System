import ProductCard from './ProductCard';

export default function ProductGrid({ products, onSelect, onEdit, onDelete }) {
  if (products.length === 0) {
    return (
      <div className="text-center py-12 text-gray-400">
        <p>No products found.</p>
      </div>
    );
  }

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
      {products.map((product) => (
        <ProductCard
          key={product.id}
          product={product}
          onSelect={onSelect}
          onEdit={onEdit}
          onDelete={onDelete}
        />
      ))}
    </div>
  );
}
