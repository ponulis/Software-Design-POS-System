import apiClient from './client';

export const appointmentsApi = {
  /**
   * Get all appointments with optional filtering and pagination
   * @param {Object} params - Query parameters
   * @param {string} params.startDate - Filter by start date (ISO string)
   * @param {string} params.endDate - Filter by end date (ISO string)
   * @param {number} params.employeeId - Filter by employee ID
   * @param {string} params.status - Filter by appointment status
   * @param {number} params.page - Page number (default: 1)
   * @param {number} params.pageSize - Items per page (default: 50)
   */
  getAll: async (params = {}) => {
    const response = await apiClient.get('/appointments', { params });
    return response.data;
  },

  /**
   * Get appointment by ID
   * @param {number} appointmentId - Appointment ID
   */
  getById: async (appointmentId) => {
    const response = await apiClient.get(`/appointments/${appointmentId}`);
    return response.data;
  },

  /**
   * Create a new appointment
   * @param {Object} appointmentData - Appointment data
   */
  create: async (appointmentData) => {
    const response = await apiClient.post('/appointments', appointmentData);
    return response.data;
  },

  /**
   * Update/reschedule an appointment
   * @param {number} appointmentId - Appointment ID
   * @param {Object} appointmentData - Updated appointment data
   */
  update: async (appointmentId, appointmentData) => {
    const response = await apiClient.patch(`/appointments/${appointmentId}`, appointmentData);
    return response.data;
  },

  /**
   * Cancel an appointment
   * @param {number} appointmentId - Appointment ID
   */
  cancel: async (appointmentId) => {
    const response = await apiClient.delete(`/appointments/${appointmentId}`);
    return response.data;
  },

  /**
   * Get available time slots
   * @param {Object} params - Query parameters
   * @param {string} params.date - Date to check (ISO string)
   * @param {number} params.employeeId - Employee ID (optional)
   * @param {number} params.serviceId - Service ID (optional)
   */
  getAvailableSlots: async (params) => {
    const response = await apiClient.get('/appointments/available-slots', { params });
    return response.data;
  },
};
