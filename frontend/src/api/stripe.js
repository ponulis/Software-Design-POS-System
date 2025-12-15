import apiClient from './client';

export const stripeApi = {
  /**
   * Create a Stripe payment intent
   * @param {Object} paymentData - Payment data (orderId, amount)
   */
  createPaymentIntent: async (paymentData) => {
    const response = await apiClient.post('/stripe/create-payment-intent', paymentData);
    return response.data;
  },

  /**
   * Confirm a Stripe payment
   * @param {Object} confirmData - Payment confirmation data
   */
  confirmPayment: async (confirmData) => {
    const response = await apiClient.post('/stripe/confirm-payment', confirmData);
    return response.data;
  },
};
