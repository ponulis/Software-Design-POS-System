import { useState } from "react";
import { NavLink, useNavigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";

export default function Navbar() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);
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
          {/* Mobile menu button */}
          <button
            onClick={() => setMobileMenuOpen(!mobileMenuOpen)}
            className="lg:hidden text-white p-2 rounded-lg hover:bg-white/10"
            aria-label="Toggle menu"
          >
            <svg className="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              {mobileMenuOpen ? (
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
              ) : (
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 6h16M4 12h16M4 18h16" />
              )}
            </svg>
          </button>

          {/* Desktop menu */}
          <ul className="hidden lg:flex gap-3">
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

          {/* Mobile menu */}
          {mobileMenuOpen && (
            <div className="absolute top-full left-0 right-0 bg-blue-600 lg:hidden border-t border-blue-500">
              <ul className="flex flex-col p-4 gap-2">
                <li>
                  <NavLink to="/" className={({ isActive }) => `${base} ${isActive ? active : inactive}`} onClick={() => setMobileMenuOpen(false)}>
                    Home
                  </NavLink>
                </li>
                <li>
                  <NavLink to="/settings" className={({ isActive }) => `${base} ${isActive ? active : inactive}`} onClick={() => setMobileMenuOpen(false)}>
                    Settings
                  </NavLink>
                </li>
                <li>
                  <NavLink to="/catalog-products" className={({ isActive }) => `${base} ${isActive ? active : inactive}`} onClick={() => setMobileMenuOpen(false)}>
                    Catalog Products
                  </NavLink>
                </li>
                <li>
                  <NavLink to="/taxes-and-service-charges" className={({ isActive }) => `${base} ${isActive ? active : inactive}`} onClick={() => setMobileMenuOpen(false)}>
                    Taxes & Service Charges
                  </NavLink>
                </li>
                <li>
                  <NavLink to="/users-and-roles" className={({ isActive }) => `${base} ${isActive ? active : inactive}`} onClick={() => setMobileMenuOpen(false)}>
                    Users & Roles
                  </NavLink>
                </li>
                <li>
                  <NavLink to="/payments" className={({ isActive }) => `${base} ${isActive ? active : inactive}`} onClick={() => setMobileMenuOpen(false)}>
                    Payments
                  </NavLink>
                </li>
                <li>
                  <NavLink to="/payment-history" className={({ isActive }) => `${base} ${isActive ? active : inactive}`} onClick={() => setMobileMenuOpen(false)}>
                    Payment History
                  </NavLink>
                </li>
                <li>
                  <NavLink to="/order-history" className={({ isActive }) => `${base} ${isActive ? active : inactive}`} onClick={() => setMobileMenuOpen(false)}>
                    Order History
                  </NavLink>
                </li>
                <li>
                  <NavLink to="/reservations" className={({ isActive }) => `${base} ${isActive ? active : inactive}`} onClick={() => setMobileMenuOpen(false)}>
                    Reservations
                  </NavLink>
                </li>
              </ul>
            </div>
          )}

          <div className="flex items-center gap-2 md:gap-4">
            {user && (
              <div className="hidden sm:flex text-white/90 text-sm items-center gap-2">
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
              className="px-3 md:px-4 py-2 rounded-lg transition-colors font-medium text-white/80 hover:text-white hover:bg-white/10 border border-white/20 text-sm"
              title="Logout"
            >
              <span className="hidden sm:inline">Logout</span>
              <span className="sm:hidden">Out</span>
            </button>
          </div>
        </div>
      </nav>
    </div>
  );
}
