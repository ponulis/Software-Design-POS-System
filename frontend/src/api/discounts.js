import apiClient from './client';

export const discountsApi = {
  /**
   * Get all discounts
   * @param {Object} params - Query parameters
   */
  getAll: async (params = {}) => {
    const response = await apiClient.get('/discounts', { params });
    return response.data;
  },

  /**
   * Get discount by ID
   * @param {number} discountId - Discount ID
   */
  getById: async (discountId) => {
    const response = await apiClient.get(`/discounts/${discountId}`);
    return response.data;
  },

  /**
   * Create a new discount
   * @param {Object} discountData - Discount data
   */
  create: async (discountData) => {
    const response = await apiClient.post('/discounts', discountData);
    return response.data;
  },

  /**
   * Update a discount
   * @param {number} discountId - Discount ID
   * @param {Object} discountData - Updated discount data
   */
  update: async (discountId, discountData) => {
    const response = await apiClient.patch(`/discounts/${discountId}`, discountData);
    return response.data;
  },

  /**
   * Delete a discount
   * @param {number} discountId - Discount ID
   */
  delete: async (discountId) => {
    const response = await apiClient.delete(`/discounts/${discountId}`);
    return response.data;
  },
};
