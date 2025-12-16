import apiClient from './client';

export const productModificationsApi = {
  /**
   * Get all product modifications (attributes)
   */
  getAll: async () => {
    const response = await apiClient.get('/product-modifications');
    return response.data;
  },

  /**
   * Get modification by ID
   * @param {number} modificationId - Modification ID
   */
  getById: async (modificationId) => {
    const response = await apiClient.get(`/product-modifications/${modificationId}`);
    return response.data;
  },

  /**
   * Create a new modification
   * @param {Object} modificationData - Modification data
   */
  create: async (modificationData) => {
    const response = await apiClient.post('/product-modifications', modificationData);
    return response.data;
  },

  /**
   * Add a value to a modification
   * @param {number} modificationId - Modification ID
   * @param {string} value - Value to add
   */
  addValue: async (modificationId, value) => {
    const response = await apiClient.post(`/product-modifications/${modificationId}/values`, { value });
    return response.data;
  },

  /**
   * Delete a modification
   * @param {number} modificationId - Modification ID
   */
  delete: async (modificationId) => {
    const response = await apiClient.delete(`/product-modifications/${modificationId}`);
    return response.data;
  },
};
