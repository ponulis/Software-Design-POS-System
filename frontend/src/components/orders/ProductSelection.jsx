import React, { useState } from 'react';
import { useProducts } from '../../hooks/useProducts';

export default function ProductSelection({ onAddToCart }) {
  const { products, loading } = useProducts();
  const [searchTerm, setSearchTerm] = useState('');
  const [filterAvailable, setFilterAvailable] = useState('all');

  const filteredProducts = products.filter((product) => {
    const matchesSearch =
      product.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
      product.description?.toLowerCase().includes(searchTerm.toLowerCase()) ||
      product.tags?.some((tag) =>
        tag.toLowerCase().includes(searchTerm.toLowerCase())
      );

    const matchesFilter =
      filterAvailable === 'all' ||
      (filterAvailable === 'available' && product.available) ||
      (filterAvailable === 'unavailable' && !product.available);

    return matchesSearch && matchesFilter;
  });

  if (loading) {
    return (
      <div className="text-center py-8">
        <div className="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600 mb-4"></div>
        <p className="text-gray-600">Loading products...</p>
      </div>
    );
  }

  return (
    <div className="bg-white rounded-lg border border-gray-200">
      <div className="p-4 border-b border-gray-200">
        <h3 className="font-semibold text-gray-800 mb-3">Select Products</h3>
        <div className="flex gap-2">
          <input
            type="text"
            placeholder="Search products..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="flex-1 px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 text-sm"
          />
          <select
            value={filterAvailable}
            onChange={(e) => setFilterAvailable(e.target.value)}
            className="px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 text-sm"
          >
            <option value="all">All</option>
            <option value="available">Available</option>
            <option value="unavailable">Unavailable</option>
          </select>
        </div>
      </div>
      <div className="p-4 max-h-96 overflow-y-auto">
        {filteredProducts.length === 0 ? (
          <p className="text-center text-gray-400 py-8">No products found</p>
        ) : (
          <div className="grid grid-cols-2 gap-3">
            {filteredProducts.map((product) => (
              <button
                key={product.id}
                onClick={() => onAddToCart(product)}
                disabled={!product.available}
                className={`p-3 rounded-lg border text-left transition ${
                  product.available
                    ? 'border-gray-200 hover:border-blue-500 hover:bg-blue-50 cursor-pointer'
                    : 'border-gray-100 bg-gray-50 opacity-50 cursor-not-allowed'
                }`}
              >
                <div className="flex justify-between items-start mb-1">
                  <h4 className="font-medium text-sm text-gray-900">{product.name}</h4>
                  <span className="font-semibold text-sm text-blue-600">
                    {product.price.toFixed(2)}€
                  </span>
                </div>
                {product.description && (
                  <p className="text-xs text-gray-500 line-clamp-2">{product.description}</p>
                )}
                {!product.available && (
                  <p className="text-xs text-red-600 mt-1">Unavailable</p>
                )}
              </button>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
