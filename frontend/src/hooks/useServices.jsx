import { useState, useEffect, useCallback } from 'react';
import { servicesApi } from '../api/services';

export function useServices() {
  const [services, setServices] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  const fetchServices = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      
      const response = await servicesApi.getAll();
      setServices(Array.isArray(response) ? response : response.data || []);
    } catch (err) {
      console.error('Error fetching services:', err);
      setError(err.response?.data?.message || 'Failed to fetch services');
      setServices([]);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchServices();
  }, [fetchServices]);

  return {
    services,
    loading,
    error,
    refreshServices: fetchServices,
  };
}
