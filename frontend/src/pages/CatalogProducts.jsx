import { useState } from 'react';
import { useProducts } from '../hooks/useProducts';
import ProductGrid from '../components/products/ProductGrid';
import ProductModal from '../components/products/ProductModal';

export default function CatalogProducts() {
  const {
    products,
    selectedProduct,
    loading,
    error,
    selectProduct,
    createProduct,
    updateProduct,
    deleteProduct,
    refreshProducts,
  } = useProducts();

  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingProduct, setEditingProduct] = useState(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [filterAvailable, setFilterAvailable] = useState('all'); // 'all', 'available', 'unavailable'

  // Filter products
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

  const handleCreate = () => {
    console.log('Create button clicked, opening modal');
    setEditingProduct(null);
    setIsModalOpen(true);
    console.log('Modal state set to true');
  };

  const handleEdit = (product) => {
    setEditingProduct(product);
    setIsModalOpen(true);
  };

  const handleDelete = async (productId) => {
    const result = await deleteProduct(productId);
    if (result.success) {
      // Product deleted successfully
    }
  };

  const handleSubmit = async (formData) => {
    let result;
    if (editingProduct) {
      result = await updateProduct(editingProduct.id, formData);
    } else {
      result = await createProduct(formData);
    }
    
    if (result.success) {
      setIsModalOpen(false);
      setEditingProduct(null);
    }
    
    // Return result so modal can handle errors
    return result;
  };

  if (loading && products.length === 0) {
    return (
      <div className="min-h-screen bg-gray-100 p-6 flex items-center justify-center">
        <div className="text-center">
          <div className="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600 mb-4"></div>
          <p className="text-gray-600">Loading products...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-100 p-6">
      <div className="max-w-7xl mx-auto">
        {/* Header */}
        <div className="mb-6 flex justify-between items-center">
          <div>
            <h2 className="text-sm font-semibold text-gray-500 uppercase mb-1">
              Product Catalog
            </h2>
            <h1 className="text-2xl font-bold text-gray-900">Products</h1>
          </div>
          <button
            onClick={handleCreate}
            className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition font-medium"
          >
            + Add Product
          </button>
        </div>

        {/* Error message */}
        {error && (
          <div className="mb-4 p-4 bg-red-50 border border-red-200 rounded-lg">
            <p className="text-red-800 text-sm">{error}</p>
          </div>
        )}

        {/* Search and Filter */}
        <div className="mb-6 bg-white p-4 rounded-lg shadow">
          <div className="flex gap-4">
            <div className="flex-1">
              <input
                type="text"
                placeholder="Search products by name, description, or tags..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>
            <select
              value={filterAvailable}
              onChange={(e) => setFilterAvailable(e.target.value)}
              className="px-4 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              <option value="all">All Products</option>
              <option value="available">Available Only</option>
              <option value="unavailable">Unavailable Only</option>
            </select>
            <button
              onClick={refreshProducts}
              className="px-4 py-2 bg-gray-200 text-gray-800 rounded-lg hover:bg-gray-300 transition"
            >
              Refresh
            </button>
          </div>
        </div>

        {/* Product count */}
        <div className="mb-4 text-sm text-gray-600">
          Showing {filteredProducts.length} of {products.length} products
        </div>

        {/* Product Grid */}
        <ProductGrid
          products={filteredProducts}
          onSelect={selectProduct}
          onEdit={handleEdit}
          onDelete={handleDelete}
        />
      </div>

      {/* Product Modal */}
      <ProductModal
        isOpen={isModalOpen}
        onClose={() => {
          setIsModalOpen(false);
          setEditingProduct(null);
        }}
        product={editingProduct}
        onSubmit={handleSubmit}
      />
    </div>
  );
}
