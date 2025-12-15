import { useState, useEffect, useCallback } from 'react';
import { employeesApi } from '../api/employees';

export function useUsers() {
  const [users, setUsers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  const fetchUsers = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      
      const response = await employeesApi.getAll();
      setUsers(Array.isArray(response) ? response : response.data || []);
    } catch (err) {
      console.error('Error fetching users:', err);
      setError(err.response?.data?.message || 'Failed to fetch users');
      setUsers([]);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchUsers();
  }, [fetchUsers]);

  const createUser = useCallback(async (userData) => {
    try {
      setError(null);
      const newUser = await employeesApi.create(userData);
      await fetchUsers();
      return { success: true, user: newUser };
    } catch (err) {
      const errorMessage = err.response?.data?.message || 'Failed to create user';
      setError(errorMessage);
      return { success: false, error: errorMessage };
    }
  }, [fetchUsers]);

  const updateUser = useCallback(async (userId, userData) => {
    try {
      setError(null);
      const updatedUser = await employeesApi.update(userId, userData);
      await fetchUsers();
      return { success: true, user: updatedUser };
    } catch (err) {
      const errorMessage = err.response?.data?.message || 'Failed to update user';
      setError(errorMessage);
      return { success: false, error: errorMessage };
    }
  }, [fetchUsers]);

  const deleteUser = useCallback(async (userId) => {
    try {
      setError(null);
      await employeesApi.delete(userId);
      await fetchUsers();
      return { success: true };
    } catch (err) {
      const errorMessage = err.response?.data?.message || 'Failed to delete user';
      setError(errorMessage);
      return { success: false, error: errorMessage };
    }
  }, [fetchUsers]);

  return {
    users,
    loading,
    error,
    createUser,
    updateUser,
    deleteUser,
    refreshUsers: fetchUsers,
  };
}
