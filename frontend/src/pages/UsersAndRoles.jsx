import { useState } from 'react';
import { useUsers } from '../hooks/useUsers';
import UserList from '../components/users/UserList';
import UserForm from '../components/users/UserForm';

export default function UsersAndRoles() {
  const { users, loading, error, createUser, updateUser, deleteUser } = useUsers();
  const [showForm, setShowForm] = useState(false);
  const [editingUser, setEditingUser] = useState(null);
  const [submitting, setSubmitting] = useState(false);

  const handleCreate = () => {
    setEditingUser(null);
    setShowForm(true);
  };

  const handleEdit = (user) => {
    setEditingUser(user);
    setShowForm(true);
  };

  const handleDelete = async (userId) => {
    if (!window.confirm('Are you sure you want to delete this user?')) {
      return;
    }

    const result = await deleteUser(userId);
    if (!result.success) {
      alert(result.error || 'Failed to delete user');
    }
  };

  const handleSubmit = async (userData) => {
    setSubmitting(true);
    try {
      const result = editingUser
        ? await updateUser(editingUser.id, userData)
        : await createUser(userData);

      if (result.success) {
        setShowForm(false);
        setEditingUser(null);
      } else {
        alert(result.error || 'Failed to save user');
      }
    } finally {
      setSubmitting(false);
    }
  };

  if (loading && users.length === 0) {
    return (
      <div className="min-h-screen bg-gray-100 p-6 flex items-center justify-center">
        <div className="text-center">
          <div className="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600 mb-4"></div>
          <p className="text-gray-600">Loading users...</p>
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
              User Management
            </h2>
            <h1 className="text-2xl font-bold text-gray-900">Users & Roles</h1>
          </div>
          {!showForm && (
            <button
              onClick={handleCreate}
              className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition font-medium"
            >
              + Add User
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
              {editingUser ? 'Edit User' : 'Create User'}
            </h3>
            <UserForm
              user={editingUser}
              onSubmit={handleSubmit}
              onCancel={() => {
                setShowForm(false);
                setEditingUser(null);
              }}
              submitting={submitting}
            />
          </div>
        ) : (
          <UserList users={users} onEdit={handleEdit} onDelete={handleDelete} />
        )}
      </div>
    </div>
  );
}
