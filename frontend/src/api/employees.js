import apiClient from './client';

export const employeesApi = {
  /**
   * Get all employees/users
   * @param {Object} params - Query parameters
   */
  getAll: async (params = {}) => {
    const response = await apiClient.get('/employees', { params });
    return response.data;
  },

  /**
   * Get employee by ID
   * @param {number} employeeId - Employee ID
   */
  getById: async (employeeId) => {
    const response = await apiClient.get(`/employees/${employeeId}`);
    return response.data;
  },

  /**
   * Create a new employee/user
   * @param {Object} employeeData - Employee data
   */
  create: async (employeeData) => {
    const response = await apiClient.post('/employees', employeeData);
    return response.data;
  },

  /**
   * Update an employee/user
   * @param {number} employeeId - Employee ID
   * @param {Object} employeeData - Updated employee data
   */
  update: async (employeeId, employeeData) => {
    const response = await apiClient.patch(`/employees/${employeeId}`, employeeData);
    return response.data;
  },

  /**
   * Delete an employee/user
   * @param {number} employeeId - Employee ID
   */
  delete: async (employeeId) => {
    const response = await apiClient.delete(`/employees/${employeeId}`);
    return response.data;
  },
};
