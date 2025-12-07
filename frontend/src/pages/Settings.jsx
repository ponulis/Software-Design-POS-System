"use client";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";

const businessSchema = z.object({
  id: z.number().optional(),
  owner_id: z.coerce.number().min(1, "Owner ID is required"),
  name: z.string().min(1, "Business name is required"),
  description: z.string().optional(),
  address: z.string().min(1, "Address is required"),
  contact_email: z.string().email("Invalid email address"),
  phone_number: z
    .string()
    .regex(/^[0-9+\-() ]+$/, "Invalid phone number")
    .min(5, "Phone number too short"),
});

const businessFields = [
  { name: "id", label: "Business ID", type: "number", disabled: true },
  { name: "owner_id", label: "Owner ID", type: "number" },
  { name: "name", label: "Business Name", type: "text" },
  { name: "description", label: "Description", type: "textarea" },
  { name: "address", label: "Address", type: "text" },
  { name: "contact_email", label: "Contact Email", type: "email" },
  { name: "phone_number", label: "Phone Number", type: "tel" },
];

const sampleBusinessData = {
  id: 1,
  owner_id: 12345,
  name: "Test Corporation",
  description: "A leading provider of test solutions",
  address: "Vilnius, Lithuania, Test Street 123",
  contact_email: "contact@testcorp.com",
  phone_number: "+370 600 00000",
};

export default function Settings() {
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm({
    resolver: zodResolver(businessSchema),
    defaultValues: sampleBusinessData,
  });

  const onSubmit = (data) => {
    console.log("Submitted:", data);
  };

  return (
    <div className="min-h-screen bg-gray-50 p-8">
      <div className="max-w-2xl mx-auto bg-white rounded-lg shadow-md p-6">
        <h1 className="text-2xl font-bold mb-6">Business Settings</h1>
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          {businessFields.map((field) => (
            <div key={field.name}>
              <label className="block text-sm font-medium text-gray-700">
                {field.label}
              </label>
              {field.type === "textarea" ? (
                <textarea
                  {...register(field.name)}
                  className="mt-1 w-full rounded-lg border border-gray-300 p-2"
                  rows={3}
                />
              ) : (
                <input
                  {...register(field.name)}
                  type={field.type}
                  disabled={field.disabled}
                  className="mt-1 w-full rounded-lg border border-gray-300 p-2"
                />
              )}
              {errors[field.name] && (
                <p className="text-red-500 text-sm mt-1">
                  {errors[field.name].message}
                </p>
              )}
            </div>
          ))}
          <button
            type="submit"
            className="w-full bg-blue-600 text-white py-2 rounded-lg font-semibold hover:bg-blue-700 transition"
          >
            Save Changes
          </button>
        </form>
      </div>
    </div>
  );
}