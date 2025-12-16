import React, { useEffect } from 'react';
import { useForm } from 'react-hook-form';

export default function TaxForm({ tax, onSubmit, onCancel, submitting }) {
  const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
  } = useForm({
    defaultValues: tax || {
      name: '',
      rate: 0,
      isActive: true,
      effectiveFrom: new Date().toISOString().split('T')[0],
      effectiveTo: null,
    },
  });

  useEffect(() => {
    if (tax) {
      reset({
        name: tax.name || '',
        rate: tax.rate || 0,
        isActive: tax.isActive !== undefined ? tax.isActive : true,
        effectiveFrom: tax.effectiveFrom 
          ? new Date(tax.effectiveFrom).toISOString().split('T')[0]
          : new Date().toISOString().split('T')[0],
        effectiveTo: tax.effectiveTo 
          ? new Date(tax.effectiveTo).toISOString().split('T')[0]
          : null,
      });
    }
  }, [tax, reset]);

  const onFormSubmit = (data) => {
    onSubmit({
      ...data,
      rate: parseFloat(data.rate),
      effectiveFrom: new Date(data.effectiveFrom).toISOString(),
      effectiveTo: data.effectiveTo ? new Date(data.effectiveTo).toISOString() : null,
    });
  };

  return (
    <form onSubmit={handleSubmit(onFormSubmit)} className="space-y-4">
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Tax Name <span className="text-red-500">*</span>
        </label>
        <input
          {...register('name', { required: 'Tax name is required' })}
          type="text"
          className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
          disabled={submitting}
        />
        {errors.name && (
          <p className="text-red-500 text-xs mt-1">{errors.name.message}</p>
        )}
      </div>

      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Tax Rate (%) <span className="text-red-500">*</span>
        </label>
        <input
          {...register('rate', { 
            required: 'Tax rate is required',
            min: { value: 0, message: 'Rate must be 0 or greater' },
            max: { value: 100, message: 'Rate cannot exceed 100%' }
          })}
          type="number"
          step="0.01"
          min="0"
          max="100"
          className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
          disabled={submitting}
        />
        {errors.rate && (
          <p className="text-red-500 text-xs mt-1">{errors.rate.message}</p>
        )}
        <p className="mt-1 text-xs text-gray-500">
          Enter the tax rate as a percentage (e.g., 21 for 21%)
        </p>
      </div>

      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Effective From <span className="text-red-500">*</span>
        </label>
        <input
          {...register('effectiveFrom', { required: 'Effective from date is required' })}
          type="date"
          className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
          disabled={submitting}
        />
        {errors.effectiveFrom && (
          <p className="text-red-500 text-xs mt-1">{errors.effectiveFrom.message}</p>
        )}
      </div>

      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Effective To (optional)
        </label>
        <input
          {...register('effectiveTo')}
          type="date"
          className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
          disabled={submitting}
        />
      </div>

      <div className="flex items-center">
        <input
          {...register('isActive')}
          type="checkbox"
          id="isActive"
          className="w-4 h-4 text-blue-600 border-gray-300 rounded focus:ring-blue-500"
          disabled={submitting}
        />
        <label htmlFor="isActive" className="ml-2 text-sm text-gray-700">
          Active
        </label>
      </div>

      <div className="flex gap-3 pt-4">
        <button
          type="button"
          onClick={onCancel}
          className="flex-1 px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50 transition disabled:opacity-50"
          disabled={submitting}
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
