import { Fragment, useState } from 'react';
import ProductForm from './ProductForm';

export default function ProductModal({ isOpen, onClose, product, onSubmit }) {
  const [submitError, setSubmitError] = useState(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  // Debug logging
  console.log('ProductModal render - isOpen:', isOpen, 'product:', product);

  if (!isOpen) {
    console.log('ProductModal: isOpen is false, returning null');
    return null;
  }

  const handleSubmit = async (formData) => {
    setIsSubmitting(true);
    setSubmitError(null);
    try {
      const result = await onSubmit(formData);
      // Modal closing is handled by parent component based on success
      if (!result?.success) {
        setSubmitError(result?.error || 'Failed to save product');
      }
    } catch (error) {
      console.error('Error submitting product:', error);
      setSubmitError(error.message || 'An unexpected error occurred');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="fixed inset-0 flex justify-center items-center p-4 bg-black/30 z-50">
      <div className="bg-white rounded-lg shadow-lg w-full max-w-2xl max-h-[90vh] overflow-y-auto">
        <div className="sticky top-0 bg-white border-b px-6 py-4 flex justify-between items-center z-10">
          <h2 className="text-xl font-bold">
            {product ? 'Edit Product' : 'Create New Product'}
          </h2>
          <button
            onClick={onClose}
            className="text-gray-400 hover:text-gray-600 text-2xl leading-none"
            aria-label="Close"
          >
            ×
          </button>
        </div>

        <div className="p-6">
          {submitError && (
            <div className="mb-4 p-3 bg-red-50 border border-red-200 rounded-lg">
              <p className="text-red-800 text-sm">{submitError}</p>
            </div>
          )}
          <ProductForm
            product={product}
            onSubmit={handleSubmit}
            onCancel={onClose}
            isSubmitting={isSubmitting}
          />
        </div>
      </div>
    </div>
  );
}
