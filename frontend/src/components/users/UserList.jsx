import React from 'react';

export default function UserList({ users = [], onEdit, onDelete }) {
  const getRoleColor = (role) => {
    switch (role) {
      case 'Admin':
        return 'bg-red-100 text-red-700';
      case 'Manager':
        return 'bg-blue-100 text-blue-700';
      case 'Employee':
        return 'bg-green-100 text-green-700';
      default:
        return 'bg-gray-100 text-gray-700';
    }
  };

  if (users.length === 0) {
    return (
      <div className="text-center py-12 text-gray-400">
        <p>No users found.</p>
        <p className="text-sm mt-2">Click "Add User" to create one.</p>
      </div>
    );
  }

  return (
    <div className="bg-white rounded-lg shadow">
      <div className="p-4 border-b border-gray-200">
        <h3 className="font-semibold text-gray-800">Users ({users.length})</h3>
      </div>
      <div className="divide-y divide-gray-200">
        {users.map((user) => (
          <div
            key={user.id}
            className="p-4 hover:bg-gray-50 transition"
          >
            <div className="flex justify-between items-start">
              <div className="flex-1">
                <div className="flex items-center gap-2 mb-1">
                  <h3 className="font-semibold text-gray-900">{user.name}</h3>
                  <span className={`px-2 py-0.5 text-xs rounded font-medium ${getRoleColor(user.role)}`}>
                    {user.role}
                  </span>
                  {user.isActive ? (
                    <span className="px-2 py-0.5 text-xs bg-green-100 text-green-700 rounded">
                      Active
                    </span>
                  ) : (
                    <span className="px-2 py-0.5 text-xs bg-gray-100 text-gray-700 rounded">
                      Inactive
                    </span>
                  )}
                </div>
                <div className="text-sm text-gray-600 space-y-1">
                  <p>Phone: {user.phone}</p>
                  <p className="text-xs text-gray-500">
                    Created: {new Date(user.createdAt).toLocaleDateString()}
                  </p>
                </div>
              </div>
              <div className="flex gap-2 ml-4">
                <button
                  onClick={() => onEdit(user)}
                  className="px-3 py-1 text-sm bg-blue-100 text-blue-700 rounded hover:bg-blue-200 transition"
                >
                  Edit
                </button>
                <button
                  onClick={() => {
                    if (window.confirm(`Are you sure you want to delete user "${user.name}"?`)) {
                      onDelete(user.id);
                    }
                  }}
                  className="px-3 py-1 text-sm bg-red-100 text-red-700 rounded hover:bg-red-200 transition"
                >
                  Delete
                </button>
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
