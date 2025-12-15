import apiClient from './client';

export const productsApi = {
  /**
   * Get all products/menu items
   * @param {Object} params - Query parameters
   */
  getAll: async (params = {}) => {
    const response = await apiClient.get('/menu-items', { params });
    return response.data;
  },

  /**
   * Get product by ID
   * @param {number} productId - Product ID
   */
  getById: async (productId) => {
    const response = await apiClient.get(`/menu-items/${productId}`);
    return response.data;
  },

  /**
   * Create a new product
   * @param {Object} productData - Product data
   */
  create: async (productData) => {
    const response = await apiClient.post('/menu-items', productData);
    return response.data;
  },

  /**
   * Update a product
   * @param {number} productId - Product ID
   * @param {Object} productData - Updated product data
   */
  update: async (productId, productData) => {
    const response = await apiClient.patch(`/menu-items/${productId}`, productData);
    return response.data;
  },

  /**
   * Delete a product
   * @param {number} productId - Product ID
   */
  delete: async (productId) => {
    const response = await apiClient.delete(`/menu-items/${productId}`);
    return response.data;
  },
};
