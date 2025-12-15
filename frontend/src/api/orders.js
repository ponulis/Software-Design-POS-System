import apiClient from './client';

export const ordersApi = {
  /**
   * Get all orders with optional filtering and pagination
   * @param {Object} params - Query parameters
   * @param {string} params.status - Filter by order status
   * @param {string} params.startDate - Filter by start date (ISO string)
   * @param {string} params.endDate - Filter by end date (ISO string)
   * @param {number} params.spotId - Filter by spot ID
   * @param {number} params.page - Page number (default: 1)
   * @param {number} params.pageSize - Items per page (default: 50)
   */
  getAll: async (params = {}) => {
    const response = await apiClient.get('/orders', { params });
    return response.data;
  },

  /**
   * Get order by ID
   * @param {number} orderId - Order ID
   */
  getById: async (orderId) => {
    const response = await apiClient.get(`/orders/${orderId}`);
    return response.data;
  },

  /**
   * Create a new order
   * @param {Object} orderData - Order data
   */
  create: async (orderData) => {
    const response = await apiClient.post('/orders', orderData);
    return response.data;
  },

  /**
   * Update an order
   * @param {number} orderId - Order ID
   * @param {Object} orderData - Updated order data
   */
  update: async (orderId, orderData) => {
    const response = await apiClient.patch(`/orders/${orderId}`, orderData);
    return response.data;
  },

  /**
   * Cancel/delete an order
   * @param {number} orderId - Order ID
   */
  cancel: async (orderId) => {
    const response = await apiClient.delete(`/orders/${orderId}`);
    return response.data;
  },

  /**
   * Get order receipt
   * @param {number} orderId - Order ID
   */
  getReceipt: async (orderId) => {
    const response = await apiClient.get(`/orders/${orderId}/receipt`);
    return response.data;
  },

  /**
   * Process refund for an order
   * @param {number} orderId - Order ID
   * @param {Object} refundData - Refund data
   */
  refund: async (orderId, refundData) => {
    const response = await apiClient.post(`/orders/${orderId}/refund`, refundData);
    return response.data;
  },
};
