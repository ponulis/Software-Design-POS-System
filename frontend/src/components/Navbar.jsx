import { useState } from "react";
import { NavLink, useNavigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";

export default function Navbar() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);
  const base = "rounded-lg text-center p-2 transition-colors font-medium";
  const inactive = "text-white/80 hover:text-white hover:bg-white/10";
  const active = "text-white bg-white/20";

  const handleLogout = () => {
    logout();
    navigate("/login");
  };

  // Group navigation items by category
  const navGroups = [
    {
      label: "Main",
      items: [
        { to: "/", label: "Dashboard", icon: "📊" },
        { to: "/payments", label: "Transactions", icon: "💰" },
        { to: "/reservations", label: "Appointments", icon: "📅" },
      ],
    },
    {
      label: "Reports",
      items: [
        { to: "/payment-history", label: "Payments", icon: "💳" },
        { to: "/order-history", label: "Orders", icon: "📋" },
      ],
    },
    {
      label: "Management",
      items: [
        { to: "/catalog-products", label: "Products", icon: "🛍️" },
        { to: "/taxes-and-service-charges", label: "Taxes", icon: "📊" },
        { to: "/users-and-roles", label: "Users", icon: "👥", adminOnly: true },
        { to: "/discounts", label: "Discounts", icon: "🎁" },
        { to: "/settings", label: "Settings", icon: "⚙️" },
      ],
    },
  ];

  const allNavItems = navGroups.flatMap((group) => group.items);

  const NavLinkItem = ({ to, label, icon, onClick, isMobile = false }) => {
    const baseClasses = isMobile
      ? "flex items-center gap-3 px-4 py-3 rounded-lg transition-colors font-medium text-white/90 hover:text-white hover:bg-white/10"
      : "flex items-center gap-2 px-4 py-2 rounded-lg transition-colors font-medium text-sm";
    
    const activeClasses = isMobile
      ? "text-white bg-white/20"
      : "text-white bg-white/20";
    
    const inactiveClasses = isMobile
      ? "text-white/80"
      : "text-white/80 hover:text-white hover:bg-white/10";

    return (
      <NavLink
        to={to}
        onClick={onClick}
        className={({ isActive }) =>
          `${baseClasses} ${isActive ? activeClasses : inactiveClasses}`
        }
      >
        {icon && <span className="text-base">{icon}</span>}
        <span>{label}</span>
      </NavLink>
    );
  };

  return (
    <div className="w-full bg-blue-600 shadow-lg sticky top-0 z-50 flex">
      <nav className="max-w-7xl mx-auto p-4">
        <div className="flex items-center justify-between gap-4">
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
          <ul className="hidden lg:flex gap-6">
            <li className="flex-grow flex items-center justify-center">
              <NavLink to="/" className={({ isActive }) => `${base} ${isActive ? active : inactive}`}>
                Dashboard
              </NavLink>
            </li>
            <li className="flex-grow flex items-center justify-center">
              <NavLink to="/settings" className={({ isActive }) => `${base} ${isActive ? active : inactive}`}>
                Business Settings
              </NavLink>
            </li>
            <li className="flex-grow flex items-center justify-center">
              <NavLink to="/catalog-products" className={({ isActive }) => `${base} ${isActive ? active : inactive}`}>
                Product Catalog
              </NavLink>
            </li>
            <li className="flex-grow flex items-center justify-center">
              <NavLink to="/taxes-and-service-charges" className={({ isActive }) => `${base} ${isActive ? active : inactive}`}>
                Tax Management
              </NavLink>
            </li>
            <li className="flex-grow flex items-center justify-center">
              <NavLink to="/users-and-roles" className={({ isActive }) => `${base} ${isActive ? active : inactive}`}>
                User Management
              </NavLink>
            </li>
            <li className="flex-grow flex items-center justify-center">
              <NavLink to="/payments" className={({ isActive }) => `${base} ${isActive ? active : inactive}`}>
                Transactions
              </NavLink>
            </li>
            <li className="flex-grow flex items-center justify-center">
              <NavLink to="/payment-history" className={({ isActive }) => `${base} ${isActive ? active : inactive}`}>
                Payment History
              </NavLink>
            </li>
            <li className="flex-grow flex items-center justify-center">
              <NavLink to="/order-history" className={({ isActive }) => `${base} ${isActive ? active : inactive}`}>
                Order History
              </NavLink>
            </li>
            <li className="flex-grow flex items-center justify-center">
              <NavLink to="/reservations" className={({ isActive }) => `${base} ${isActive ? active : inactive}`}>
                Reservations
              </NavLink>
            </li>
            <li className="flex-grow flex items-center justify-center">
              <NavLink to="/discounts" className={({ isActive }) => `${base} ${isActive ? active : inactive}`}>
                Discount Management
              </NavLink>
            </li>
          </ul>

          {/* Mobile menu */}
          {mobileMenuOpen && (
            <div className="absolute top-full left-0 right-0 bg-blue-600 lg:hidden border-t border-blue-500">
              <ul className="flex flex-col p-4 gap-2">
                <li>
                  <NavLink to="/" className={({ isActive }) => `${base} ${isActive ? active : inactive}`} onClick={() => setMobileMenuOpen(false)}>
                    Dashboard
                  </NavLink>
                </li>
                <li>
                  <NavLink to="/settings" className={({ isActive }) => `${base} ${isActive ? active : inactive}`} onClick={() => setMobileMenuOpen(false)}>
                    Business Settings
                  </NavLink>
                </li>
                <li>
                  <NavLink to="/catalog-products" className={({ isActive }) => `${base} ${isActive ? active : inactive}`} onClick={() => setMobileMenuOpen(false)}>
                    Product Catalog
                  </NavLink>
                </li>
                <li>
                  <NavLink to="/taxes-and-service-charges" className={({ isActive }) => `${base} ${isActive ? active : inactive}`} onClick={() => setMobileMenuOpen(false)}>
                    Tax Management
                  </NavLink>
                </li>
                <li>
                  <NavLink to="/users-and-roles" className={({ isActive }) => `${base} ${isActive ? active : inactive}`} onClick={() => setMobileMenuOpen(false)}>
                    User Management
                  </NavLink>
                </li>
                <li>
                  <NavLink to="/payments" className={({ isActive }) => `${base} ${isActive ? active : inactive}`} onClick={() => setMobileMenuOpen(false)}>
                    Transactions
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
                <li className="flex-grow flex items-center justify-center">
                  <NavLink to="/discounts" className={({ isActive }) => `${base} ${isActive ? active : inactive}`}>
                    Discount Management
                  </NavLink>
                </li>
              </ul>
            </div>
          )}


          <div className="flex items-center p-2 rounded ml-auto">
            {user && (
              <div className="hidden sm:flex text-white/90 text-sm items-center p-2 ">
                <div className="flex flex-col items-start">
                  <span className="font-medium">{user.name || 'User'}</span>
                  <span className="text-xs text-white/70">{user.role || 'Employee'}</span>
                </div>
                {user.businessId && (
                  <div className="hidden lg:block text-xs text-white/60 px-2">
                    Business #{user.businessId}
                  </div>
                )}
              </div>
            )}
            <button
              onClick={handleLogout}
              className="rounded-lg p-2 transition-colors font-medium text-white/80 hover:text-white hover:bg-white/10  text-sm"
              title="Logout"
            >
              <span className="hidden sm:inline">Logout</span>
              <span className="sm:hidden">➜]</span>
            </button>
          </div>

        </div>

        {/* Mobile Menu */}
        {mobileMenuOpen && (
          <div className="lg:hidden border-t border-blue-500 bg-blue-600">
            <div className="px-4 py-3 space-y-4">
              {navGroups.map((group) => {
                const filteredItems = group.items.filter(
                  (item) => !item.adminOnly || user?.role === "Admin"
                );
                
                if (filteredItems.length === 0) return null;

                return (
                  <div key={group.label}>
                    <h3 className="text-xs font-semibold text-white/60 uppercase tracking-wider mb-2 px-2">
                      {group.label}
                    </h3>
                    <div className="space-y-1">
                      {filteredItems.map((item) => (
                        <NavLinkItem
                          key={item.to}
                          to={item.to}
                          label={item.label}
                          icon={item.icon}
                          isMobile={true}
                          onClick={() => setMobileMenuOpen(false)}
                        />
                      ))}
                    </div>
                  </div>
                );
              })}
              
              {/* User info in mobile menu */}
              {user && (
                <div className="pt-4 border-t border-blue-500">
                  <div className="px-2 py-2 text-sm text-white/80">
                    <div className="font-semibold text-white">{user.name || "User"}</div>
                    <div className="text-xs text-white/70 capitalize">{user.role || "Employee"}</div>
                    {user.businessId && (
                      <div className="text-xs text-white/60 mt-1">Business #{user.businessId}</div>
                    )}
                  </div>
                </div>
              )}
            </div>
          </div>
        )}
      </nav>
    </div>
  );
}
