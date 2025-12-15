import { NavLink, useNavigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";

export default function Navbar() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const base = "px-4 py-2 rounded-lg transition-colors font-medium";
  const inactive = "text-white/80 hover:text-white hover:bg-white/10";
  const active = "text-white bg-white/20";

  const handleLogout = () => {
    logout();
    navigate("/login");
  };

  return (
    <div className="w-full bg-blue-600 shadow-lg sticky top-0 z-50">
      <nav className="max-w-7xl mx-auto p-4">
        <div className="flex items-center justify-between">
          <ul className="flex gap-3">
            <li>
              <NavLink to="/" className={({ isActive }) => `${base} ${isActive ? active : inactive}`}>
                Home
              </NavLink>
            </li>
            <li>
              <NavLink to="/settings" className={({ isActive }) => `${base} ${isActive ? active : inactive}`}>
                Settings
              </NavLink>
            </li>
            <li>
              <NavLink to="/catalog-products" className={({ isActive }) => `${base} ${isActive ? active : inactive}`}>
                Catalog Products
              </NavLink>
            </li>
            <li>
              <NavLink to="/taxes-and-service-charges" className={({ isActive }) => `${base} ${isActive ? active : inactive}`}>
                Taxes & Service Charges
              </NavLink>
            </li>
            <li>
              <NavLink to="/users-and-roles" className={({ isActive }) => `${base} ${isActive ? active : inactive}`}>
                Users & Roles
              </NavLink>
            </li>
            <li>
              <NavLink to="/payments" className={({ isActive }) => `${base} ${isActive ? active : inactive}`}>
                Payments
              </NavLink>
            </li>
            <li>
              <NavLink to="/payment-history" className={({ isActive }) => `${base} ${isActive ? active : inactive}`}>
                Payment History
              </NavLink>
            </li>
            <li>
              <NavLink to="/order-history" className={({ isActive }) => `${base} ${isActive ? active : inactive}`}>
                Order History
              </NavLink>
            </li>
            <li>
              <NavLink to="/reservations" className={({ isActive }) => `${base} ${isActive ? active : inactive}`}>
                Reservations
              </NavLink>
            </li>
          </ul>
          <div className="flex items-center gap-4">
            {user && (
              <div className="text-white/90 text-sm flex items-center gap-2">
                <div className="flex flex-col items-end">
                  <span className="font-medium">{user.name || 'User'}</span>
                  <span className="text-xs text-white/70 capitalize">{user.role || 'Employee'}</span>
                </div>
                {user.businessId && (
                  <div className="text-xs text-white/60">
                    Business #{user.businessId}
                  </div>
                )}
              </div>
            )}
            <button
              onClick={handleLogout}
              className="px-4 py-2 rounded-lg transition-colors font-medium text-white/80 hover:text-white hover:bg-white/10 border border-white/20"
              title="Logout"
            >
              Logout
            </button>
          </div>
        </div>
      </nav>
    </div>
  );
}
