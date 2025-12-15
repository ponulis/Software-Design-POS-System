import apiClient from './client';

export const taxesApi = {
  /**
   * Get all tax rules
   * @param {Object} params - Query parameters
   */
  getAll: async (params = {}) => {
    const response = await apiClient.get('/taxes', { params });
    return response.data;
  },

  /**
   * Get tax rule by ID
   * @param {number} taxId - Tax ID
   */
  getById: async (taxId) => {
    const response = await apiClient.get(`/taxes/${taxId}`);
    return response.data;
  },

  /**
   * Create a new tax rule
   * @param {Object} taxData - Tax data
   */
  create: async (taxData) => {
    const response = await apiClient.post('/taxes', taxData);
    return response.data;
  },

  /**
   * Update a tax rule
   * @param {number} taxId - Tax ID
   * @param {Object} taxData - Updated tax data
   */
  update: async (taxId, taxData) => {
    const response = await apiClient.patch(`/taxes/${taxId}`, taxData);
    return response.data;
  },

  /**
   * Delete a tax rule
   * @param {number} taxId - Tax ID
   */
  delete: async (taxId) => {
    const response = await apiClient.delete(`/taxes/${taxId}`);
    return response.data;
  },
};
