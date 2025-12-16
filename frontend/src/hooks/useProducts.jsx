import { useState, useEffect, useCallback } from 'react';
import { productsApi } from '../api/products';

export function useProducts() {
  const [products, setProducts] = useState([]);
  const [selectedProduct, setSelectedProduct] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  // Fetch products from API
  const fetchProducts = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      
      const response = await productsApi.getAll();
      setProducts(Array.isArray(response) ? response : response.data || []);
    } catch (err) {
      console.error('Error fetching products:', err);
      setError(err.response?.data?.message || 'Failed to fetch products');
      setProducts([]);
    } finally {
      setLoading(false);
    }
  }, []);

  // Initial load
  useEffect(() => {
    fetchProducts();
  }, [fetchProducts]);

  // Select a product
  const selectProduct = useCallback((product) => {
    setSelectedProduct(product);
  }, []);

  // Create a new product
  const createProduct = useCallback(async (productData) => {
    try {
      setError(null);
      console.log('Creating product with data:', productData);
      const newProduct = await productsApi.create(productData);
      console.log('Product created successfully:', newProduct);
      await fetchProducts(); // Refresh list
      return { success: true, product: newProduct };
    } catch (err) {
      console.error('Error creating product:', err);
      console.error('Error response:', err.response?.data);
      const errorMessage = err.response?.data?.message || err.message || 'Failed to create product';
      setError(errorMessage);
      return { success: false, error: errorMessage };
    }
  }, [fetchProducts]);

  // Update a product
  const updateProduct = useCallback(async (productId, productData) => {
    try {
      setError(null);
      const updatedProduct = await productsApi.update(productId, productData);
      await fetchProducts(); // Refresh list
      if (selectedProduct?.id === productId) {
        setSelectedProduct(updatedProduct);
      }
      return { success: true, product: updatedProduct };
    } catch (err) {
      const errorMessage = err.response?.data?.message || 'Failed to update product';
      setError(errorMessage);
      return { success: false, error: errorMessage };
    }
  }, [fetchProducts, selectedProduct]);

  // Delete a product
  const deleteProduct = useCallback(async (productId) => {
    try {
      setError(null);
      await productsApi.delete(productId);
      await fetchProducts(); // Refresh list
      if (selectedProduct?.id === productId) {
        setSelectedProduct(null);
      }
      return { success: true };
    } catch (err) {
      const errorMessage = err.response?.data?.message || 'Failed to delete product';
      setError(errorMessage);
      return { success: false, error: errorMessage };
    }
  }, [fetchProducts, selectedProduct]);

  // Refresh products
  const refreshProducts = useCallback(() => {
    fetchProducts();
  }, [fetchProducts]);

  return {
    products,
    selectedProduct,
    loading,
    error,
    selectProduct,
    createProduct,
    updateProduct,
    deleteProduct,
    refreshProducts,
  };
}
