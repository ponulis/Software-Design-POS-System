import apiClient from './client';

export const dashboardApi = {
  /**
   * Get dashboard statistics and data
   * @param {Object} params - Query parameters (date range, etc.)
   */
  getStats: async (params = {}) => {
    const response = await apiClient.get('/dashboard', { params });
    return response.data;
  },
};
