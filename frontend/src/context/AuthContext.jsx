import { createContext, useContext, useState, useEffect } from 'react';
import { authApi } from '../api/auth';

const AuthContext = createContext(null);

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};

export const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);
  const [isAuthenticated, setIsAuthenticated] = useState(false);

  useEffect(() => {
    // Check if user is already logged in
    const token = localStorage.getItem('authToken');
    const storedUser = localStorage.getItem('user');

    if (token && storedUser) {
      try {
        const userData = JSON.parse(storedUser);
        setUser(userData);
        setIsAuthenticated(true);
      } catch (error) {
        console.error('Error parsing stored user data:', error);
        localStorage.removeItem('authToken');
        localStorage.removeItem('user');
      }
    }
    setLoading(false);
  }, []);

  const login = async (phone, password) => {
    try {
      const response = await authApi.login(phone, password);
      
      if (!response || !response.token) {
        return {
          success: false,
          error: 'Invalid response from server. Please try again.',
        };
      }

      // Store token and user data
      localStorage.setItem('authToken', response.token);
      const userData = {
        userId: response.userId,
        businessId: response.businessId,
        name: response.name || 'User',
        role: response.role || 'Employee',
      };
      localStorage.setItem('user', JSON.stringify(userData));

      setUser(userData);
      setIsAuthenticated(true);

      return { success: true };
    } catch (error) {
      console.error('Login error:', error);
      
      // Handle network errors
      if (error.isNetworkError) {
        return {
          success: false,
          error: 'Network error. Please check your connection and try again.',
        };
      }

      // Handle API errors
      const errorMessage = error.response?.data?.message 
        || error.response?.data?.error
        || error.message
        || 'Login failed. Please check your credentials.';

      return {
        success: false,
        error: errorMessage,
      };
    }
  };

  const logout = () => {
    localStorage.removeItem('authToken');
    localStorage.removeItem('user');
    setUser(null);
    setIsAuthenticated(false);
  };

  const value = {
    user,
    isAuthenticated,
    loading,
    login,
    logout,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};
