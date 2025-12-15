import { Navigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

export default function ProtectedRoute({ children, requiredRole = null }) {
  const { isAuthenticated, loading, user } = useAuth();

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gray-50">
        <div className="text-center">
          <div className="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600 mb-4"></div>
          <div className="text-gray-600">Loading...</div>
        </div>
      </div>
    );
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  // Check role-based access if requiredRole is specified
  if (requiredRole && user) {
    const roleHierarchy = {
      Employee: ['Employee', 'Manager', 'Admin'],
      Manager: ['Manager', 'Admin'],
      Admin: ['Admin'],
    };

    const allowedRoles = roleHierarchy[requiredRole] || [];
    const userRole = user.role || 'Employee';
    
    if (!allowedRoles.includes(userRole)) {
      return (
        <div className="min-h-screen flex items-center justify-center bg-gray-50">
          <div className="text-center max-w-md p-6 bg-white rounded-lg shadow-md">
            <h2 className="text-2xl font-bold text-gray-900 mb-2">Access Denied</h2>
            <p className="text-gray-600 mb-4">
              You don't have permission to access this page.
            </p>
            <p className="text-sm text-gray-500">
              Required role: <span className="font-semibold">{requiredRole}</span>
              <br />
              Your role: <span className="font-semibold">{userRole}</span>
            </p>
          </div>
        </div>
      );
    }
  }

  return children;
}
