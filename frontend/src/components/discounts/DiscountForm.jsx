import React, { useEffect } from 'react';
import { useForm } from 'react-hook-form';

export default function DiscountForm({ discount, onSubmit, onCancel, submitting }) {
  const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
    watch,
  } = useForm({
    defaultValues: discount || {
      name: '',
      description: '',
      type: 'Percentage',
      value: 0,
      isActive: true,
      validFrom: null,
      validTo: null,
    },
  });

  const discountType = watch('type');

  useEffect(() => {
    if (discount) {
      reset({
        name: discount.name || '',
        description: discount.description || '',
        type: discount.type || 'Percentage',
        value: discount.value || 0,
        isActive: discount.isActive !== undefined ? discount.isActive : true,
        validFrom: discount.validFrom 
          ? new Date(discount.validFrom).toISOString().split('T')[0]
          : null,
        validTo: discount.validTo 
          ? new Date(discount.validTo).toISOString().split('T')[0]
          : null,
      });
    }
  }, [discount, reset]);

  const onFormSubmit = (data) => {
    onSubmit({
      ...data,
      value: parseFloat(data.value),
      validFrom: data.validFrom ? new Date(data.validFrom).toISOString() : null,
      validTo: data.validTo ? new Date(data.validTo).toISOString() : null,
    });
  };

  return (
    <form onSubmit={handleSubmit(onFormSubmit)} className="space-y-4">
      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Discount Name <span className="text-red-500">*</span>
        </label>
        <input
          {...register('name', { required: 'Discount name is required' })}
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
          Description
        </label>
        <textarea
          {...register('description')}
          rows={3}
          className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
          disabled={submitting}
        />
      </div>

      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Discount Type <span className="text-red-500">*</span>
        </label>
        <select
          {...register('type', { required: 'Discount type is required' })}
          className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
          disabled={submitting}
        >
          <option value="Percentage">Percentage</option>
          <option value="FixedAmount">Fixed Amount</option>
        </select>
      </div>

      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Value <span className="text-red-500">*</span>
        </label>
        <div className="flex items-center gap-2">
          <input
            {...register('value', { 
              required: 'Value is required',
              min: { value: 0, message: 'Value must be 0 or greater' }
            })}
            type="number"
            step={discountType === 'Percentage' ? '0.01' : '0.01'}
            min="0"
            max={discountType === 'Percentage' ? '100' : undefined}
            className="flex-1 px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
            disabled={submitting}
          />
          <span className="text-gray-600">
            {discountType === 'Percentage' ? '%' : '€'}
          </span>
        </div>
        {errors.value && (
          <p className="text-red-500 text-xs mt-1">{errors.value.message}</p>
        )}
      </div>

      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Valid From (optional)
        </label>
        <input
          {...register('validFrom')}
          type="date"
          className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
          disabled={submitting}
        />
      </div>

      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1">
          Valid To (optional)
        </label>
        <input
          {...register('validTo')}
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
          disabled={submitting}
          className="flex-1 px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50 transition disabled:opacity-50"
        >
          Cancel
        </button>
        <button
          type="submit"
          disabled={submitting}
          className="flex-1 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition disabled:opacity-50 disabled:cursor-not-allowed"
        >
          {submitting ? 'Saving...' : discount ? 'Update' : 'Create'}
        </button>
      </div>
    </form>
  );
}
