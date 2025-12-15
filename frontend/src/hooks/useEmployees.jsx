import { useState, useEffect, useCallback } from 'react';
import { employeesApi } from '../api/employees';

export function useEmployees() {
  const [employees, setEmployees] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  const fetchEmployees = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      
      const response = await employeesApi.getAll();
      setEmployees(Array.isArray(response) ? response : response.data || []);
    } catch (err) {
      console.error('Error fetching employees:', err);
      setError(err.response?.data?.message || 'Failed to fetch employees');
      setEmployees([]);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchEmployees();
  }, [fetchEmployees]);

  return {
    employees,
    loading,
    error,
    refreshEmployees: fetchEmployees,
  };
}
