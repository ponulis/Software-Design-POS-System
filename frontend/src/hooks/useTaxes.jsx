import { useState, useEffect, useCallback } from 'react';
import { taxesApi } from '../api/taxes';

export function useTaxes() {
  const [taxes, setTaxes] = useState([]);
  const [selectedTax, setSelectedTax] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  const fetchTaxes = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      
      const response = await taxesApi.getAll();
      setTaxes(Array.isArray(response) ? response : response.data || []);
    } catch (err) {
      console.error('Error fetching taxes:', err);
      setError(err.response?.data?.message || 'Failed to fetch taxes');
      setTaxes([]);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchTaxes();
  }, [fetchTaxes]);

  const selectTax = useCallback((tax) => {
    setSelectedTax(tax);
  }, []);

  const createTax = useCallback(async (taxData) => {
    try {
      setError(null);
      const newTax = await taxesApi.create(taxData);
      await fetchTaxes();
      return { success: true, tax: newTax };
    } catch (err) {
      const errorMessage = err.response?.data?.message || 'Failed to create tax';
      setError(errorMessage);
      return { success: false, error: errorMessage };
    }
  }, [fetchTaxes]);

  const updateTax = useCallback(async (taxId, taxData) => {
    try {
      setError(null);
      const updatedTax = await taxesApi.update(taxId, taxData);
      await fetchTaxes();
      if (selectedTax?.id === taxId) {
        setSelectedTax(updatedTax);
      }
      return { success: true, tax: updatedTax };
    } catch (err) {
      const errorMessage = err.response?.data?.message || 'Failed to update tax';
      setError(errorMessage);
      return { success: false, error: errorMessage };
    }
  }, [fetchTaxes, selectedTax]);

  const deleteTax = useCallback(async (taxId) => {
    try {
      setError(null);
      await taxesApi.delete(taxId);
      await fetchTaxes();
      if (selectedTax?.id === taxId) {
        setSelectedTax(null);
      }
      return { success: true };
    } catch (err) {
      const errorMessage = err.response?.data?.message || 'Failed to delete tax';
      setError(errorMessage);
      return { success: false, error: errorMessage };
    }
  }, [fetchTaxes, selectedTax]);

  const refreshTaxes = useCallback(() => {
    fetchTaxes();
  }, [fetchTaxes]);

  return {
    taxes,
    selectedTax,
    loading,
    error,
    selectTax,
    createTax,
    updateTax,
    deleteTax,
    refreshTaxes,
  };
}
