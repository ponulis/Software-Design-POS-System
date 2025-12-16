import { useState, useEffect } from 'react';
import { useProductModifications } from '../../hooks/useProductModifications';

export default function ProductForm({ product, onSubmit, onCancel, isSubmitting = false }) {
  const { modifications, loading: modificationsLoading, createModification } = useProductModifications();
  
  const [formData, setFormData] = useState({
    name: '',
    description: '',
    price: '',
    tags: '',
    available: true,
    modificationIds: [],
    inventoryItems: [],
  });

  const [selectedModifications, setSelectedModifications] = useState([]);
  const [newModificationName, setNewModificationName] = useState('');
  const [newModificationValues, setNewModificationValues] = useState('');
  const [newModificationPriceType, setNewModificationPriceType] = useState('None');
  const [newModificationFixedPrice, setNewModificationFixedPrice] = useState('');
  const [newModificationPercentagePrice, setNewModificationPercentagePrice] = useState('');
  const [showCreateModification, setShowCreateModification] = useState(false);
  const [errors, setErrors] = useState({});

  useEffect(() => {
    if (product) {
      setFormData({
        name: product.name || '',
        description: product.description || '',
        price: product.price?.toString() || '',
        tags: product.tags?.join(', ') || '',
        available: product.available !== undefined ? product.available : true,
        modificationIds: product.modifications?.map(m => m.id) || [],
        inventoryItems: product.inventoryItems || [],
      });
      setSelectedModifications(product.modifications || []);
    }
  }, [product]);

  // Helper function to regenerate combinations with current selected modifications
  const regenerateCombinations = () => {
    if (selectedModifications.length === 0) {
      setFormData(prev => ({ ...prev, inventoryItems: [] }));
      return;
    }

    const combinations = [];
    const generateCombos = (mods, currentCombo = {}) => {
      if (mods.length === 0) {
        combinations.push({ ...currentCombo });
        return;
      }

      const [currentMod, ...restMods] = mods;
      currentMod.values.forEach(value => {
        generateCombos(restMods, {
          ...currentCombo,
          [currentMod.name]: value.id,
        });
      });
    };

    generateCombos(selectedModifications);

    const inventoryItems = combinations.map(combo => ({
      modificationValueIds: combo,
      quantity: 0,
    }));

    setFormData(prev => ({ ...prev, inventoryItems }));
  };

  // Generate inventory combinations when modifications change (only for new products)
  useEffect(() => {
    if (!product) {
      if (selectedModifications.length > 0) {
        regenerateCombinations();
      } else {
        setFormData(prev => ({ ...prev, inventoryItems: [] }));
      }
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [selectedModifications.length, product]); // Only depend on length to avoid infinite loops

  const handleInventoryQuantityChange = (index, quantity) => {
    setFormData(prev => {
      const newItems = [...prev.inventoryItems];
      newItems[index] = { ...newItems[index], quantity: parseInt(quantity) || 0 };
      return { ...prev, inventoryItems: newItems };
    });
  };

  const handleModificationToggle = (modification) => {
    setSelectedModifications(prev => {
      const isSelected = prev.some(m => m.id === modification.id);
      if (isSelected) {
        // Remove modification
        const updated = prev.filter(m => m.id !== modification.id);
        setFormData(formPrev => ({
          ...formPrev,
          modificationIds: updated.map(m => m.id),
        }));
        return updated;
      } else {
        // Add modification
        const updated = [...prev, modification];
        setFormData(formPrev => ({
          ...formPrev,
          modificationIds: updated.map(m => m.id),
        }));
        return updated;
      }
    });
  };

  const handleCreateModification = async () => {
    if (!newModificationName.trim()) {
      setErrors(prev => ({ ...prev, newModification: 'Modification name is required' }));
      return;
    }

    const values = newModificationValues
      .split(',')
      .map(v => v.trim())
      .filter(v => v.length > 0);

    // Validate pricing based on type
    if (newModificationPriceType === 'Fixed' && (!newModificationFixedPrice || parseFloat(newModificationFixedPrice) < 0)) {
      setErrors(prev => ({ ...prev, newModification: 'Fixed price must be specified and non-negative' }));
      return;
    }

    if (newModificationPriceType === 'Percentage' && (!newModificationPercentagePrice || parseFloat(newModificationPercentagePrice) < 0 || parseFloat(newModificationPercentagePrice) > 100)) {
      setErrors(prev => ({ ...prev, newModification: 'Percentage must be between 0 and 100' }));
      return;
    }

    const modificationData = {
      name: newModificationName.trim(),
      values: values,
      priceType: newModificationPriceType,
      fixedPriceAddition: newModificationPriceType === 'Fixed' ? parseFloat(newModificationFixedPrice) : null,
      percentagePriceIncrease: newModificationPriceType === 'Percentage' ? parseFloat(newModificationPercentagePrice) : null,
    };

    const result = await createModification(modificationData);

    if (result.success) {
      setNewModificationName('');
      setNewModificationValues('');
      setNewModificationPriceType('None');
      setNewModificationFixedPrice('');
      setNewModificationPercentagePrice('');
      setShowCreateModification(false);
      setErrors(prev => ({ ...prev, newModification: '' }));
    } else {
      setErrors(prev => ({ ...prev, newModification: result.error }));
    }
  };

  const handleChange = (e) => {
    const { name, value, type, checked } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : value,
    }));
    // Clear error for this field
    if (errors[name]) {
      setErrors((prev) => ({ ...prev, [name]: '' }));
    }
  };

  const validate = () => {
    const newErrors = {};

    if (!formData.name.trim()) {
      newErrors.name = 'Product name is required';
    }

    if (!formData.price || parseFloat(formData.price) < 0) {
      newErrors.price = 'Valid price is required';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = (e) => {
    e.preventDefault();
    if (!validate()) {
      return;
    }

    const tagsArray = formData.tags
      .split(',')
      .map((tag) => tag.trim())
      .filter((tag) => tag.length > 0);

    // Build modification value IDs map for inventory items
    // Only include inventory items that have modification values (if modifications are selected)
    // or include all items if no modifications are selected
    const inventoryItems = formData.inventoryItems
      .filter(item => {
        // If modifications are selected, only include items with modification values
        if (formData.modificationIds.length > 0) {
          return item.modificationValueIds && Object.keys(item.modificationValueIds).length > 0;
        }
        // If no modifications, include all items (for simple products)
        return true;
      })
      .map(item => {
        // item.modificationValueIds is already in the format { "Color": 1, "Size": 3 }
        // where keys are modification names and values are modification value IDs
        return {
          modificationValueIds: item.modificationValueIds || {},
          quantity: item.quantity || 0,
        };
      });

    onSubmit({
      name: formData.name.trim(),
      description: formData.description.trim() || null,
      price: parseFloat(formData.price),
      tags: tagsArray,
      available: formData.available,
      modificationIds: formData.modificationIds || [],
      inventoryItems: inventoryItems || [],
    });
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      <div>
        <label htmlFor="name" className="block text-sm font-medium text-gray-700 mb-1">
          Product Name <span className="text-red-500">*</span>
        </label>
        <input
          type="text"
          id="name"
          name="name"
          value={formData.name}
          onChange={handleChange}
          className={`w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 ${
            errors.name ? 'border-red-500' : 'border-gray-300'
          }`}
          placeholder="Enter product name"
        />
        {errors.name && <p className="text-red-500 text-xs mt-1">{errors.name}</p>}
      </div>

      <div>
        <label htmlFor="description" className="block text-sm font-medium text-gray-700 mb-1">
          Description
        </label>
        <textarea
          id="description"
          name="description"
          value={formData.description}
          onChange={handleChange}
          rows={3}
          className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2"
          placeholder="Enter product description"
        />
      </div>

      <div>
        <label htmlFor="price" className="block text-sm font-medium text-gray-700 mb-1">
          Price (€) <span className="text-red-500">*</span>
        </label>
        <input
          type="number"
          id="price"
          name="price"
          value={formData.price}
          onChange={handleChange}
          step="0.01"
          min="0"
          className={`w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 ${
            errors.price ? 'border-red-500' : 'border-gray-300'
          }`}
          placeholder="0.00"
        />
        {errors.price && <p className="text-red-500 text-xs mt-1">{errors.price}</p>}
      </div>

      <div>
        <label htmlFor="tags" className="block text-sm font-medium text-gray-700 mb-1">
          Tags (comma-separated)
        </label>
        <input
          type="text"
          id="tags"
          name="tags"
          value={formData.tags}
          onChange={handleChange}
          className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2"
          placeholder="tag1, tag2, tag3"
        />
        <p className="text-xs text-gray-500 mt-1">Separate tags with commas</p>
      </div>

      <div className="flex items-center">
        <input
          type="checkbox"
          id="available"
          name="available"
          checked={formData.available}
          onChange={handleChange}
          className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
        />
        <label htmlFor="available" className="ml-2 block text-sm text-gray-700">
          Product is available
        </label>
      </div>

      {/* Modification Attributes Section */}
      <div className="border-t pt-4 mt-4">
        <div className="flex justify-between items-center mb-3">
          <label className="block text-sm font-medium text-gray-700">
            Modification Attributes (e.g., Color, Size)
          </label>
          <button
            type="button"
            onClick={() => setShowCreateModification(!showCreateModification)}
            className="text-sm text-blue-600 hover:text-blue-700"
          >
            + Create New
          </button>
        </div>

        {/* Create New Modification Form */}
        {showCreateModification && (
          <div className="mb-4 p-3 bg-gray-50 rounded-lg border border-gray-200">
            <div className="space-y-3">
              <input
                type="text"
                value={newModificationName}
                onChange={(e) => setNewModificationName(e.target.value)}
                placeholder="Attribute name (e.g., Color, Size)"
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 text-sm"
              />
              <input
                type="text"
                value={newModificationValues}
                onChange={(e) => setNewModificationValues(e.target.value)}
                placeholder="Values (comma-separated, e.g., Red, Blue, Green)"
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 text-sm"
              />
              
              {/* Pricing Options */}
              <div className="space-y-2">
                <label className="block text-xs font-medium text-gray-700">
                  Price Modification (Optional)
                </label>
                <select
                  value={newModificationPriceType}
                  onChange={(e) => {
                    setNewModificationPriceType(e.target.value);
                    setNewModificationFixedPrice('');
                    setNewModificationPercentagePrice('');
                  }}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 text-sm"
                >
                  <option value="None">No price modification</option>
                  <option value="Fixed">Fixed price addition</option>
                  <option value="Percentage">Percentage increase</option>
                </select>
                
                {newModificationPriceType === 'Fixed' && (
                  <div>
                    <input
                      type="number"
                      step="0.01"
                      min="0"
                      value={newModificationFixedPrice}
                      onChange={(e) => setNewModificationFixedPrice(e.target.value)}
                      placeholder="Fixed amount (e.g., 5.00)"
                      className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 text-sm"
                    />
                    <p className="text-xs text-gray-500 mt-1">This amount will be added to the product price</p>
                  </div>
                )}
                
                {newModificationPriceType === 'Percentage' && (
                  <div>
                    <input
                      type="number"
                      step="0.01"
                      min="0"
                      max="100"
                      value={newModificationPercentagePrice}
                      onChange={(e) => setNewModificationPercentagePrice(e.target.value)}
                      placeholder="Percentage (e.g., 10 for 10%)"
                      className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 text-sm"
                    />
                    <p className="text-xs text-gray-500 mt-1">Percentage increase (0-100)</p>
                  </div>
                )}
              </div>
              
              <div className="flex gap-2">
                <button
                  type="button"
                  onClick={handleCreateModification}
                  className="px-3 py-1 bg-blue-600 text-white rounded text-sm hover:bg-blue-700"
                >
                  Create
                </button>
                <button
                  type="button"
                  onClick={() => {
                    setShowCreateModification(false);
                    setNewModificationName('');
                    setNewModificationValues('');
                    setNewModificationPriceType('None');
                    setNewModificationFixedPrice('');
                    setNewModificationPercentagePrice('');
                    setErrors(prev => ({ ...prev, newModification: '' }));
                  }}
                  className="px-3 py-1 bg-gray-200 text-gray-800 rounded text-sm hover:bg-gray-300"
                >
                  Cancel
                </button>
              </div>
              {errors.newModification && (
                <p className="text-red-500 text-xs">{errors.newModification}</p>
              )}
            </div>
          </div>
        )}

        {/* Available Modifications */}
        {modificationsLoading ? (
          <p className="text-sm text-gray-500">Loading modifications...</p>
        ) : modifications.length === 0 ? (
          <p className="text-sm text-gray-500">No modifications available. Create one to get started.</p>
        ) : (
          <div className="space-y-2 max-h-40 overflow-y-auto">
            {modifications.map((modification) => {
              const isSelected = selectedModifications.some(m => m.id === modification.id);
              return (
                <label
                  key={modification.id}
                  className="flex items-start p-2 border rounded-lg cursor-pointer hover:bg-gray-50"
                >
                  <input
                    type="checkbox"
                    id={`modification-${modification.id}`}
                    checked={isSelected}
                    onChange={() => handleModificationToggle(modification)}
                    className="mt-1 h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
                  />
                  <div className="ml-2 flex-1">
                    <div className="text-sm font-medium text-gray-700">{modification.name}</div>
                    <div className="text-xs text-gray-500">
                      Values: {modification.values.map(v => v.value).join(', ')}
                    </div>
                    {modification.priceType && modification.priceType !== 'None' && (
                      <div className="text-xs text-blue-600 mt-1">
                        {modification.priceType === 'Fixed' && modification.fixedPriceAddition && (
                          <>+€{parseFloat(modification.fixedPriceAddition).toFixed(2)}</>
                        )}
                        {modification.priceType === 'Percentage' && modification.percentagePriceIncrease && (
                          <>+{parseFloat(modification.percentagePriceIncrease).toFixed(1)}%</>
                        )}
                      </div>
                    )}
                  </div>
                </label>
              );
            })}
          </div>
        )}
      </div>

      {/* Inventory Section */}
      {selectedModifications.length > 0 && (
        <div className="border-t pt-4 mt-4">
          <label className="block text-sm font-medium text-gray-700 mb-3">
            Inventory Quantities
          </label>
          <div className="space-y-2 max-h-60 overflow-y-auto">
            {formData.inventoryItems.map((item, index) => {
              // Build display label from modification values
              const labelParts = selectedModifications.map(mod => {
                const valueId = item.modificationValueIds[mod.name];
                const value = mod.values.find(v => v.id === valueId);
                return `${mod.name}: ${value?.value || 'N/A'}`;
              });
              const label = labelParts.join(', ');

              return (
                <div key={index} className="flex items-center gap-3 p-2 border rounded-lg">
                  <div className="flex-1 text-sm text-gray-700">{label}</div>
                  <input
                    type="number"
                    min="0"
                    value={item.quantity || 0}
                    onChange={(e) => handleInventoryQuantityChange(index, e.target.value)}
                    className="w-24 px-2 py-1 border border-gray-300 rounded text-sm"
                    placeholder="Qty"
                  />
                </div>
              );
            })}
          </div>
          {formData.inventoryItems.length === 0 && (
            <p className="text-sm text-gray-500 mt-2">
              Select modification attributes above to set inventory quantities.
            </p>
          )}
        </div>
      )}

      <div className="flex gap-3 pt-4">
        <button
          type="submit"
          disabled={isSubmitting}
          className="flex-1 bg-blue-600 text-white py-2 px-4 rounded-lg hover:bg-blue-700 transition font-medium disabled:opacity-50 disabled:cursor-not-allowed"
        >
          {isSubmitting ? 'Saving...' : (product ? 'Update Product' : 'Create Product')}
        </button>
        <button
          type="button"
          onClick={onCancel}
          disabled={isSubmitting}
          className="flex-1 bg-gray-200 text-gray-800 py-2 px-4 rounded-lg hover:bg-gray-300 transition font-medium disabled:opacity-50 disabled:cursor-not-allowed"
        >
          Cancel
        </button>
      </div>
    </form>
  );
}
