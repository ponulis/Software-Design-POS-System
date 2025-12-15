/**
 * Utility functions for handling errors consistently across the application
 */

/**
 * Extracts a user-friendly error message from an error object
 * @param {Error} error - The error object
 * @returns {string} - User-friendly error message
 */
export function getErrorMessage(error) {
  if (!error) {
    return 'An unexpected error occurred';
  }

  // Network errors
  if (error.isNetworkError || !error.response) {
    return 'Network error. Please check your connection and try again.';
  }

  // API errors
  if (error.response?.data) {
    return (
      error.response.data.message ||
      error.response.data.error ||
      error.response.data.title ||
      'An error occurred while processing your request'
    );
  }

  // Generic error message
  return error.message || 'An unexpected error occurred';
}

/**
 * Determines if an error is a network error
 * @param {Error} error - The error object
 * @returns {boolean}
 */
export function isNetworkError(error) {
  return error?.isNetworkError || !error?.response;
}

/**
 * Determines if an error is a server error (5xx)
 * @param {Error} error - The error object
 * @returns {boolean}
 */
export function isServerError(error) {
  return error?.response?.status >= 500 && error?.response?.status < 600;
}

/**
 * Determines if an error is a client error (4xx)
 * @param {Error} error - The error object
 * @returns {boolean}
 */
export function isClientError(error) {
  return error?.response?.status >= 400 && error?.response?.status < 500;
}
