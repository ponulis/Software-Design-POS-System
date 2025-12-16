import { useState, useEffect, useCallback } from 'react';
import { productModificationsApi } from '../api/productModifications';

export function useProductModifications() {
  const [modifications, setModifications] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  // Fetch modifications from API
  const fetchModifications = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      
      const response = await productModificationsApi.getAll();
      setModifications(Array.isArray(response) ? response : response.data || []);
    } catch (err) {
      console.error('Error fetching modifications:', err);
      setError(err.response?.data?.message || 'Failed to fetch modifications');
      setModifications([]);
    } finally {
      setLoading(false);
    }
  }, []);

  // Initial load
  useEffect(() => {
    fetchModifications();
  }, [fetchModifications]);

  // Create a new modification
  const createModification = useCallback(async (modificationData) => {
    try {
      setError(null);
      const newModification = await productModificationsApi.create(modificationData);
      await fetchModifications(); // Refresh list
      return { success: true, modification: newModification };
    } catch (err) {
      const errorMessage = err.response?.data?.message || 'Failed to create modification';
      setError(errorMessage);
      return { success: false, error: errorMessage };
    }
  }, [fetchModifications]);

  // Add value to modification
  const addValueToModification = useCallback(async (modificationId, value) => {
    try {
      setError(null);
      const newValue = await productModificationsApi.addValue(modificationId, value);
      await fetchModifications(); // Refresh list
      return { success: true, value: newValue };
    } catch (err) {
      const errorMessage = err.response?.data?.message || 'Failed to add value';
      setError(errorMessage);
      return { success: false, error: errorMessage };
    }
  }, [fetchModifications]);

  // Delete a modification
  const deleteModification = useCallback(async (modificationId) => {
    try {
      setError(null);
      await productModificationsApi.delete(modificationId);
      await fetchModifications(); // Refresh list
      return { success: true };
    } catch (err) {
      const errorMessage = err.response?.data?.message || 'Failed to delete modification';
      setError(errorMessage);
      return { success: false, error: errorMessage };
    }
  }, [fetchModifications]);

  return {
    modifications,
    loading,
    error,
    createModification,
    addValueToModification,
    deleteModification,
    refreshModifications: fetchModifications,
  };
}
