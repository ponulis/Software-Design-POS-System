import React, { useState } from 'react';
import { useAuth } from '../../context/AuthContext';
import ProductSelection from './ProductSelection';
import Cart from './Cart';
import OrderSummary from './OrderSummary';

export default function OrderCreation({ onCreateOrder, onCancel }) {
  const { user } = useAuth();
  const [cartItems, setCartItems] = useState([]);
  const [spotId, setSpotId] = useState(1); // Default spot ID
  const [submitting, setSubmitting] = useState(false);

  const addToCart = (product) => {
    if (!product.available) return;

    const existingItemIndex = cartItems.findIndex(item => item.menuId === product.id);
    
    if (existingItemIndex >= 0) {
      // Update quantity
      const updatedItems = [...cartItems];
      updatedItems[existingItemIndex].quantity += 1;
      setCartItems(updatedItems);
    } else {
      // Add new item
      setCartItems([
        ...cartItems,
        {
          menuId: product.id,
          name: product.name,
          description: product.description,
          price: product.price,
          quantity: 1,
          notes: '',
        },
      ]);
    }
  };

  const updateQuantity = (index, quantity) => {
    const updatedItems = [...cartItems];
    updatedItems[index].quantity = quantity;
    setCartItems(updatedItems);
  };

  const removeItem = (index) => {
    setCartItems(cartItems.filter((_, i) => i !== index));
  };

  const updateNotes = (index, notes) => {
    const updatedItems = [...cartItems];
    updatedItems[index].notes = notes;
    setCartItems(updatedItems);
  };

  const calculateTotals = () => {
    const subtotal = cartItems.reduce((sum, item) => sum + item.price * item.quantity, 0);
    const tax = subtotal * 0.1; // 10% tax (should come from backend)
    const discount = 0; // No discount by default
    const total = subtotal + tax - discount;
    return { subtotal, tax, discount, total };
  };

  const handleSubmit = async () => {
    if (cartItems.length === 0) {
      alert('Please add at least one product to the order');
      return;
    }

    if (!user?.userId) {
      alert('User information not available');
      return;
    }

    setSubmitting(true);
    try {
      const { subtotal, tax, discount, total } = calculateTotals();
      
      const orderData = {
        spotId: spotId,
        createdBy: user.userId,
        items: cartItems.map(item => ({
          menuId: item.menuId,
          quantity: item.quantity,
          price: item.price,
          notes: item.notes || null,
        })),
        // Status will default to 'Pending' on the backend
      };

      await onCreateOrder(orderData);
    } catch (error) {
      console.error('Error creating order:', error);
      alert('Failed to create order. Please try again.');
    } finally {
      setSubmitting(false);
    }
  };

  const { subtotal, tax, discount, total } = calculateTotals();

  return (
    <div className="flex flex-col gap-4 h-full">
      <div className="flex justify-between items-center mb-2">
        <h2 className="text-xl font-bold">Create New Order</h2>
        <button
          onClick={onCancel}
          className="text-gray-600 hover:text-gray-800 text-sm"
        >
          Cancel
        </button>
      </div>

      <div className="flex-1 overflow-y-auto space-y-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Spot ID
          </label>
          <input
            type="number"
            min="1"
            value={spotId}
            onChange={(e) => setSpotId(parseInt(e.target.value) || 1)}
            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>

        <ProductSelection onAddToCart={addToCart} />

        {cartItems.length > 0 && (
          <>
            <Cart
              items={cartItems}
              onUpdateQuantity={updateQuantity}
              onRemoveItem={removeItem}
              onUpdateNotes={updateNotes}
            />
            <OrderSummary
              subtotal={subtotal}
              tax={tax}
              discount={discount}
              total={total}
            />
          </>
        )}
      </div>

      <div className="flex gap-2 pt-4 border-t border-gray-200">
        <button
          onClick={onCancel}
          className="flex-1 px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50 transition"
        >
          Cancel
        </button>
        <button
          onClick={handleSubmit}
          disabled={cartItems.length === 0 || submitting}
          className="flex-1 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition disabled:opacity-50 disabled:cursor-not-allowed"
        >
          {submitting ? 'Creating...' : 'Create Order'}
        </button>
      </div>
    </div>
  );
}
