import React from 'react';
import TaxForm from './TaxForm';

export default function TaxModal({ isOpen, onClose, tax, onSubmit }) {
  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 flex justify-center items-center p-4 bg-black/30 z-50">
      <div className="bg-white rounded-lg shadow-lg p-6 w-full max-w-2xl max-h-[90vh] overflow-y-auto">
        <div className="flex justify-between items-center mb-4">
          <h2 className="text-xl font-bold">
            {tax ? 'Edit Tax Rule' : 'Create Tax Rule'}
          </h2>
          <button
            onClick={onClose}
            className="text-gray-400 hover:text-gray-600 text-2xl"
          >
            ×
          </button>
        </div>

        <TaxForm
          tax={tax}
          onSubmit={async (formData) => {
            const result = await onSubmit(formData);
            if (result?.success) {
              onClose();
            }
            return result;
          }}
          onCancel={onClose}
        />
      </div>
    </div>
  );
}
