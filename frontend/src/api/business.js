import apiClient from './client';

export const businessApi = {
  /**
   * Get current business information
   */
  get: async () => {
    const response = await apiClient.get('/business');
    return response.data;
  },

  /**
   * Update business information
   * @param {Object} businessData - Updated business data
   */
  update: async (businessData) => {
    const response = await apiClient.patch('/business', businessData);
    return response.data;
  },
};
