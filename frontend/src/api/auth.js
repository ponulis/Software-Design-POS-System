import apiClient from './client';

export const authApi = {
  login: async (phone, password) => {
    const response = await apiClient.post('/auth/login', {
      phone,
      password,
    });
    return response.data;
  },
};
