import { useState } from 'react';
import { useDiscounts } from '../hooks/useDiscounts';
import DiscountList from '../components/discounts/DiscountList';
import DiscountForm from '../components/discounts/DiscountForm';

export default function Discounts() {
  const { discounts, loading, error, createDiscount, updateDiscount, deleteDiscount } = useDiscounts();
  const [showForm, setShowForm] = useState(false);
  const [editingDiscount, setEditingDiscount] = useState(null);
  const [submitting, setSubmitting] = useState(false);

  const handleCreate = () => {
    setEditingDiscount(null);
    setShowForm(true);
  };

  const handleEdit = (discount) => {
    setEditingDiscount(discount);
    setShowForm(true);
  };

  const handleDelete = async (discountId) => {
    if (!window.confirm('Are you sure you want to delete this discount?')) {
      return;
    }

    const result = await deleteDiscount(discountId);
    if (!result.success) {
      alert(result.error || 'Failed to delete discount');
    }
  };

  const handleSubmit = async (discountData) => {
    setSubmitting(true);
    try {
      const result = editingDiscount
        ? await updateDiscount(editingDiscount.id, discountData)
        : await createDiscount(discountData);

      if (result.success) {
        setShowForm(false);
        setEditingDiscount(null);
      } else {
        alert(result.error || 'Failed to save discount');
      }
    } finally {
      setSubmitting(false);
    }
  };

  if (loading && discounts.length === 0) {
    return (
      <div className="min-h-screen bg-gray-100 p-6 flex items-center justify-center">
        <div className="text-center">
          <div className="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600 mb-4"></div>
          <p className="text-gray-600">Loading discounts...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-100 p-6">
      <div className="max-w-6xl mx-auto">
        <div className="mb-6 flex justify-between items-center">
          <div>
            <h2 className="text-sm font-semibold text-gray-500 uppercase mb-1">
              Discount Management
            </h2>
            <h1 className="text-2xl font-bold text-gray-900">Discounts</h1>
          </div>
          {!showForm && (
            <button
              onClick={handleCreate}
              className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition font-medium"
            >
              + Add Discount
            </button>
          )}
        </div>

        {error && (
          <div className="mb-4 p-4 bg-red-50 border border-red-200 rounded-lg">
            <p className="text-red-800 text-sm">{error}</p>
          </div>
        )}

        {showForm ? (
          <div className="bg-white rounded-lg shadow p-6">
            <h3 className="text-lg font-semibold mb-4">
              {editingDiscount ? 'Edit Discount' : 'Create Discount'}
            </h3>
            <DiscountForm
              discount={editingDiscount}
              onSubmit={handleSubmit}
              onCancel={() => {
                setShowForm(false);
                setEditingDiscount(null);
              }}
              submitting={submitting}
            />
          </div>
        ) : (
          <DiscountList discounts={discounts} onEdit={handleEdit} onDelete={handleDelete} />
        )}
      </div>
    </div>
  );
}
