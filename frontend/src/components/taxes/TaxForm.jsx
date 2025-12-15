import React, { useState, useEffect } from 'react';

export default function TaxForm({ tax, onSubmit, onCancel }) {
  const [formData, setFormData] = useState({
    name: '',
    rate: '',
    isActive: true,
    effectiveFrom: '',
    effectiveTo: '',
  });
  const [errors, setErrors] = useState({});
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    if (tax) {
      setFormData({
        name: tax.name || '',
        rate: tax.rate?.toString() || '',
        isActive: tax.isActive ?? true,
        effectiveFrom: tax.effectiveFrom
          ? new Date(tax.effectiveFrom).toISOString().split('T')[0]
          : '',
        effectiveTo: tax.effectiveTo
          ? new Date(tax.effectiveTo).toISOString().split('T')[0]
          : '',
      });
    }
  }, [tax]);

  const validate = () => {
    const newErrors = {};

    if (!formData.name.trim()) {
      newErrors.name = 'Tax name is required';
    }

    const rate = parseFloat(formData.rate);
    if (isNaN(rate) || rate < 0 || rate > 100) {
      newErrors.rate = 'Rate must be between 0 and 100';
    }

    if (formData.effectiveFrom) {
      const fromDate = new Date(formData.effectiveFrom);
      if (formData.effectiveTo) {
        const toDate = new Date(formData.effectiveTo);
        if (toDate < fromDate) {
          newErrors.effectiveTo = 'End date must be after start date';
        }
      }
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    
    if (!validate()) {
      return;
    }

    setSubmitting(true);
    try {
      const submitData = {
        name: formData.name.trim(),
        rate: parseFloat(formData.rate),
        isActive: formData.isActive,
        effectiveFrom: formData.effectiveFrom
          ? new Date(formData.effectiveFrom).toISOString()
          : null,
        effectiveTo: formData.effectiveTo
          ? new Date(formData.effectiveTo).toISOString()
          : null,
      };

      const result = await onSubmit(submitData);
      if (result?.success) {
        // Form will be closed by parent
      } else if (result?.error) {
        alert(result.error);
      }
    } catch (error) {
      console.error('Error submitting tax form:', error);
      alert('Failed to save tax rule. Please try again.');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Tax Name <span className="text-red-500">*</span>
        </label>
        <input
          type="text"
          value={formData.name}
          onChange={(e) => setFormData({ ...formData, name: e.target.value })}
          className={`w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 ${
            errors.name ? 'border-red-500' : 'border-gray-300'
          }`}
          placeholder="e.g., VAT, Sales Tax"
          maxLength={200}
        />
        {errors.name && (
          <p className="mt-1 text-sm text-red-600">{errors.name}</p>
        )}
      </div>

      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Tax Rate (%) <span className="text-red-500">*</span>
        </label>
        <input
          type="number"
          step="0.01"
          min="0"
          max="100"
          value={formData.rate}
          onChange={(e) => setFormData({ ...formData, rate: e.target.value })}
          className={`w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 ${
            errors.rate ? 'border-red-500' : 'border-gray-300'
          }`}
          placeholder="e.g., 21.0"
        />
        {errors.rate && (
          <p className="mt-1 text-sm text-red-600">{errors.rate}</p>
        )}
        <p className="mt-1 text-xs text-gray-500">
          Enter the tax rate as a percentage (e.g., 21 for 21%)
        </p>
      </div>

      <div>
        <label className="flex items-center gap-2">
          <input
            type="checkbox"
            checked={formData.isActive}
            onChange={(e) => setFormData({ ...formData, isActive: e.target.checked })}
            className="w-4 h-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500"
          />
          <span className="text-sm font-medium text-gray-700">Active</span>
        </label>
        <p className="mt-1 text-xs text-gray-500">
          Only active taxes will be applied to orders
        </p>
      </div>

      <div className="grid grid-cols-2 gap-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Effective From
          </label>
          <input
            type="date"
            value={formData.effectiveFrom}
            onChange={(e) => setFormData({ ...formData, effectiveFrom: e.target.value })}
            className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Effective To (Optional)
          </label>
          <input
            type="date"
            value={formData.effectiveTo}
            onChange={(e) => setFormData({ ...formData, effectiveTo: e.target.value })}
            className={`w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 ${
              errors.effectiveTo ? 'border-red-500' : 'border-gray-300'
            }`}
          />
          {errors.effectiveTo && (
            <p className="mt-1 text-sm text-red-600">{errors.effectiveTo}</p>
          )}
        </div>
      </div>

      <div className="flex gap-3 pt-4">
        <button
          type="button"
          onClick={onCancel}
          className="flex-1 px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50 transition"
        >
          Cancel
        </button>
        <button
          type="submit"
          disabled={submitting}
          className="flex-1 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition disabled:opacity-50 disabled:cursor-not-allowed"
        >
          {submitting ? 'Saving...' : tax ? 'Update' : 'Create'}
        </button>
      </div>
    </form>
  );
}
