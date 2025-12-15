import apiClient from './client';

export const giftCardsApi = {
  /**
   * Get gift card by code
   * @param {string} code - Gift card code
   */
  getByCode: async (code) => {
    const response = await apiClient.get(`/gift-cards/${code}`);
    return response.data;
  },

  /**
   * Create/issue a new gift card
   * @param {Object} giftCardData - Gift card data
   */
  create: async (giftCardData) => {
    const response = await apiClient.post('/gift-cards', giftCardData);
    return response.data;
  },

  /**
   * Update gift card balance
   * @param {string} code - Gift card code
   * @param {Object} updateData - Update data (e.g., balance adjustment)
   */
  update: async (code, updateData) => {
    const response = await apiClient.patch(`/gift-cards/${code}`, updateData);
    return response.data;
  },
};
