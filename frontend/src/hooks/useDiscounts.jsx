import { useState, useEffect, useCallback } from 'react';
import { discountsApi } from '../api/discounts';

export function useDiscounts() {
  const [discounts, setDiscounts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  const fetchDiscounts = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      
      const response = await discountsApi.getAll();
      setDiscounts(Array.isArray(response) ? response : response.data || []);
    } catch (err) {
      console.error('Error fetching discounts:', err);
      setError(err.response?.data?.message || 'Failed to fetch discounts');
      setDiscounts([]);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchDiscounts();
  }, [fetchDiscounts]);

  const createDiscount = useCallback(async (discountData) => {
    try {
      setError(null);
      const newDiscount = await discountsApi.create(discountData);
      await fetchDiscounts();
      return { success: true, discount: newDiscount };
    } catch (err) {
      const errorMessage = err.response?.data?.message || 'Failed to create discount';
      setError(errorMessage);
      return { success: false, error: errorMessage };
    }
  }, [fetchDiscounts]);

  const updateDiscount = useCallback(async (discountId, discountData) => {
    try {
      setError(null);
      const updatedDiscount = await discountsApi.update(discountId, discountData);
      await fetchDiscounts();
      return { success: true, discount: updatedDiscount };
    } catch (err) {
      const errorMessage = err.response?.data?.message || 'Failed to update discount';
      setError(errorMessage);
      return { success: false, error: errorMessage };
    }
  }, [fetchDiscounts]);

  const deleteDiscount = useCallback(async (discountId) => {
    try {
      setError(null);
      await discountsApi.delete(discountId);
      await fetchDiscounts();
      return { success: true };
    } catch (err) {
      const errorMessage = err.response?.data?.message || 'Failed to delete discount';
      setError(errorMessage);
      return { success: false, error: errorMessage };
    }
  }, [fetchDiscounts]);

  return {
    discounts,
    loading,
    error,
    createDiscount,
    updateDiscount,
    deleteDiscount,
    refreshDiscounts: fetchDiscounts,
  };
}
