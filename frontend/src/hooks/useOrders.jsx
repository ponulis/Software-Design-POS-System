import { useState, useEffect, useCallback } from 'react';
import { ordersApi } from '../api/orders';

export function useOrders() {
  const [orders, setOrders] = useState([]);
  const [selectedOrder, setSelectedOrder] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [pagination, setPagination] = useState({
    page: 1,
    pageSize: 50,
    totalCount: 0,
    totalPages: 0,
  });

  // Fetch orders from API
  const fetchOrders = useCallback(async (params = {}) => {
    try {
      setLoading(true);
      setError(null);
      
      const response = await ordersApi.getAll({
        page: pagination.page,
        pageSize: pagination.pageSize,
        ...params,
      });

      if (response.data) {
        setOrders(response.data);
        setPagination({
          page: response.page,
          pageSize: response.pageSize,
          totalCount: response.totalCount,
          totalPages: response.totalPages,
        });
      } else {
        // Handle non-paginated response (fallback)
        setOrders(Array.isArray(response) ? response : []);
      }
    } catch (err) {
      console.error('Error fetching orders:', err);
      setError(err.response?.data?.message || 'Failed to fetch orders');
      setOrders([]);
    } finally {
      setLoading(false);
    }
  }, [pagination.page, pagination.pageSize]);

  // Initial load
  useEffect(() => {
    fetchOrders();
  }, [fetchOrders]);

  // Select an order
  const selectOrder = useCallback((order) => {
    setSelectedOrder(order);
  }, []);

  // Create a new order
  const createOrder = useCallback(async (orderData) => {
    try {
      setError(null);
      const newOrder = await ordersApi.create(orderData);
      await fetchOrders(); // Refresh list
      return { success: true, order: newOrder };
    } catch (err) {
      const errorMessage = err.response?.data?.message || 'Failed to create order';
      setError(errorMessage);
      return { success: false, error: errorMessage };
    }
  }, [fetchOrders]);

  // Update an order
  const updateOrder = useCallback(async (orderId, orderData) => {
    try {
      setError(null);
      const updatedOrder = await ordersApi.update(orderId, orderData);
      await fetchOrders(); // Refresh list
      if (selectedOrder?.id === orderId) {
        setSelectedOrder(updatedOrder);
      }
      return { success: true, order: updatedOrder };
    } catch (err) {
      const errorMessage = err.response?.data?.message || 'Failed to update order';
      setError(errorMessage);
      return { success: false, error: errorMessage };
    }
  }, [fetchOrders, selectedOrder]);

  // Cancel an order
  const cancelOrder = useCallback(async (orderId) => {
    try {
      setError(null);
      await ordersApi.cancel(orderId);
      await fetchOrders(); // Refresh list
      if (selectedOrder?.id === orderId) {
        setSelectedOrder(null);
      }
      return { success: true };
    } catch (err) {
      const errorMessage = err.response?.data?.message || 'Failed to cancel order';
      setError(errorMessage);
      return { success: false, error: errorMessage };
    }
  }, [fetchOrders, selectedOrder]);

  // Refresh orders
  const refreshOrders = useCallback(() => {
    fetchOrders();
  }, [fetchOrders]);

  return {
    orders,
    selectedOrder,
    loading,
    error,
    pagination,
    selectOrder,
    createOrder,
    updateOrder,
    cancelOrder,
    refreshOrders,
    fetchOrders,
  };
}
