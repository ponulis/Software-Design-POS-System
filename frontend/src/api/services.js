import apiClient from './client';

export const servicesApi = {
  /**
   * Get all services
   * @param {Object} params - Query parameters
   */
  getAll: async (params = {}) => {
    const response = await apiClient.get('/services', { params });
    return response.data;
  },

  /**
   * Get service by ID
   * @param {number} serviceId - Service ID
   */
  getById: async (serviceId) => {
    const response = await apiClient.get(`/services/${serviceId}`);
    return response.data;
  },

  /**
   * Create a new service
   * @param {Object} serviceData - Service data
   */
  create: async (serviceData) => {
    const response = await apiClient.post('/services', serviceData);
    return response.data;
  },

  /**
   * Update a service
   * @param {number} serviceId - Service ID
   * @param {Object} serviceData - Updated service data
   */
  update: async (serviceId, serviceData) => {
    const response = await apiClient.patch(`/services/${serviceId}`, serviceData);
    return response.data;
  },

  /**
   * Delete a service
   * @param {number} serviceId - Service ID
   */
  delete: async (serviceId) => {
    const response = await apiClient.delete(`/services/${serviceId}`);
    return response.data;
  },
};
