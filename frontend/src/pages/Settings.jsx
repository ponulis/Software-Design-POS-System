"use client";
import { useState, useEffect } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import { businessApi } from "../api/business";
import { useToast } from "../context/ToastContext";
import LoadingSpinner from "../components/LoadingSpinner";

const businessSchema = z.object({
  name: z.string().min(1, "Business name is required").max(200, "Name cannot exceed 200 characters"),
  description: z.string().max(1000, "Description cannot exceed 1000 characters").optional(),
  address: z.string().min(1, "Address is required").max(500, "Address cannot exceed 500 characters"),
  contactEmail: z.string().email("Invalid email address").max(200, "Email cannot exceed 200 characters"),
  phoneNumber: z.string().min(5, "Phone number too short").max(50, "Phone number cannot exceed 50 characters"),
});

const businessFields = [
  { name: "name", label: "Business Name", type: "text", required: true },
  { name: "description", label: "Description", type: "textarea", required: false },
  { name: "address", label: "Address", type: "text", required: true },
  { name: "contactEmail", label: "Contact Email", type: "email", required: true },
  { name: "phoneNumber", label: "Phone Number", type: "tel", required: true },
];

export default function Settings() {
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [successMessage, setSuccessMessage] = useState("");
  const [errorMessage, setErrorMessage] = useState("");

  const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
  } = useForm({
    resolver: zodResolver(businessSchema),
  });

  useEffect(() => {
    fetchBusinessData();
  }, []);

  const fetchBusinessData = async () => {
    try {
      setLoading(true);
      setErrorMessage("");
      const business = await businessApi.get();
      
      reset({
        name: business.name || "",
        description: business.description || "",
        address: business.address || "",
        contactEmail: business.contactEmail || "",
        phoneNumber: business.phoneNumber || "",
      });
    } catch (err) {
      console.error("Error fetching business data:", err);
      setErrorMessage(err.response?.data?.message || "Failed to load business information");
    } finally {
      setLoading(false);
    }
  };

  const onSubmit = async (data) => {
    try {
      setSubmitting(true);
      setErrorMessage("");
      setSuccessMessage("");
      
      await businessApi.update(data);
      const successMsg = "Business settings updated successfully!";
      setSuccessMessage(successMsg);
      showSuccessToast(successMsg);
      
      // Clear success message after 3 seconds
      setTimeout(() => setSuccessMessage(""), 3000);
    } catch (err) {
      console.error("Error updating business:", err);
      const errorMsg = err.response?.data?.message || "Failed to update business settings";
      setErrorMessage(errorMsg);
      showErrorToast(errorMsg);
    } finally {
      setSubmitting(false);
    }
  };

  if (loading) {
    return <LoadingSpinner fullScreen text="Loading business settings..." />;
  }

  return (
    <div className="min-h-screen bg-gray-100 p-8">
      <div className="max-w-2xl mx-auto bg-white rounded-lg shadow-md p-6">
        <h1 className="text-2xl font-bold mb-6">Business Settings</h1>
        
        {successMessage && (
          <div className="mb-4 p-4 bg-green-50 border border-green-200 rounded-lg">
            <p className="text-green-800 text-sm">{successMessage}</p>
          </div>
        )}
        
        {errorMessage && (
          <div className="mb-4 p-4 bg-red-50 border border-red-200 rounded-lg">
            <p className="text-red-800 text-sm">{errorMessage}</p>
          </div>
        )}

        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          {businessFields.map((field) => (
            <div key={field.name}>
              <label className="block text-sm font-medium text-gray-700">
                {field.label}
                {field.required && <span className="text-red-500 ml-1">*</span>}
              </label>
              {field.type === "textarea" ? (
                <textarea
                  {...register(field.name)}
                  className="mt-1 w-full rounded-lg border border-gray-300 p-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                  rows={3}
                  disabled={submitting}
                />
              ) : (
                <input
                  {...register(field.name)}
                  type={field.type}
                  className="mt-1 w-full rounded-lg border border-gray-300 p-2 focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:bg-gray-100"
                  disabled={submitting}
                />
              )}
              {errors[field.name] && (
                <p className="text-red-500 text-sm mt-1">
                  {errors[field.name].message}
                </p>
              )}
            </div>
          ))}
          <div className="flex gap-3 pt-4">
            <button
              type="button"
              onClick={fetchBusinessData}
              disabled={submitting}
              className="flex-1 bg-gray-200 text-gray-800 py-2 rounded-lg font-semibold hover:bg-gray-300 transition disabled:opacity-50 disabled:cursor-not-allowed"
            >
              Reset
            </button>
            <button
              type="submit"
              disabled={submitting}
              className="flex-1 bg-blue-600 text-white py-2 rounded-lg font-semibold hover:bg-blue-700 transition disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {submitting ? "Saving..." : "Save Changes"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}