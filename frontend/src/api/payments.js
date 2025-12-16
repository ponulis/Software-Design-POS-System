import apiClient from './client';

export const paymentsApi = {
  /**
   * Get all payments with optional filtering and pagination
   * @param {Object} params - Query parameters
   * @param {string} params.method - Filter by payment method
   * @param {string} params.startDate - Filter by start date (ISO string)
   * @param {string} params.endDate - Filter by end date (ISO string)
   * @param {number} params.page - Page number (default: 1)
   * @param {number} params.pageSize - Items per page (default: 50)
   */
  getAll: async (params = {}) => {
    const response = await apiClient.get('/payments', { params });
    return response.data;
  },

  /**
   * Get payment by ID
   * @param {number} paymentId - Payment ID
   */
  getById: async (paymentId) => {
    const response = await apiClient.get(`/payments/${paymentId}`);
    return response.data;
  },

  /**
   * Create a new payment
   * @param {Object} paymentData - Payment data
   */
  create: async (paymentData) => {
    const response = await apiClient.post('/payments', paymentData);
    return response.data;
  },

  /**
   * Get payment history with optional filtering
   * @param {Object} params - Query parameters
   * @param {number} params.orderId - Filter by order ID
   * @param {string} params.startDate - Filter by start date (ISO string)
   * @param {string} params.endDate - Filter by end date (ISO string)
   */
  getHistory: async (params = {}) => {
    const response = await apiClient.get('/payments/history', { params });
    return response.data;
  },

  /**
   * Create split payments (multiple payments for one order)
   * @param {Object} splitPaymentData - Split payment data
   * @param {number} splitPaymentData.orderId - Order ID
   * @param {Array} splitPaymentData.payments - Array of payment objects
   */
  createSplitPayments: async (splitPaymentData) => {
    const response = await apiClient.post('/payments/split', splitPaymentData);
    return response.data;
  },
};
