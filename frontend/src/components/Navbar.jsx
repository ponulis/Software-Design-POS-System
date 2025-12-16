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

  // Group navigation items by category
  const navGroups = [
    {
      label: "Main",
      items: [
        { to: "/", label: "Dashboard" },
        { to: "/payments", label: "Orders" },
        { to: "/reservations", label: "Appointments" },
      ],
    },
    {
      label: "Reports",
      items: [
        { to: "/payment-history", label: "Payments" },
        { to: "/order-history", label: "Orders" },
      ],
    },
    {
      label: "Management",
      items: [
        { to: "/catalog-products", label: "Products" },
        { to: "/taxes-and-service-charges", label: "Taxes" },
        { to: "/users-and-roles", label: "Users", adminOnly: true },
        { to: "/settings", label: "Settings" },
      ],
    },
  ];

  const allNavItems = navGroups.flatMap((group) => group.items);

  const NavLinkItem = ({ to, label, icon, onClick, isMobile = false }) => {
    const baseClasses = isMobile
      ? "flex items-center gap-3 px-4 py-3 rounded-lg transition-all font-medium text-white/90 hover:text-white hover:bg-white/10"
      : "flex items-center gap-2 px-3 py-2 rounded-lg transition-all font-medium text-sm whitespace-nowrap";
    
    const activeClasses = isMobile
      ? "text-white bg-white/20 shadow-sm"
      : "text-white bg-white/20 shadow-sm";
    
    const inactiveClasses = isMobile
      ? "text-white/80"
      : "text-white/80 hover:text-white hover:bg-white/10 hover:shadow-sm";

    return (
      <NavLink
        to={to}
        onClick={onClick}
        className={({ isActive }) =>
          `${baseClasses} ${isActive ? activeClasses : inactiveClasses}`
        }
      >
        {icon && <span className="text-base leading-none">{icon}</span>}
        <span>{label}</span>
      </NavLink>
    );
  };

  return (
    <div className="w-full bg-gradient-to-r from-blue-600 to-blue-700 shadow-lg sticky top-0 z-50 border-b border-blue-500/30">
      <nav className="max-w-7xl mx-auto px-4 py-3">
        <div className="flex items-center justify-between">
          {/* Mobile menu button */}
          <button
            onClick={() => setMobileMenuOpen(!mobileMenuOpen)}
            className="lg:hidden text-white p-2 rounded-lg hover:bg-white/10 transition-colors"
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

          {/* Desktop menu - Centered */}
          <div className="hidden lg:flex items-center justify-center flex-1 gap-1">
            {navGroups.map((group, groupIndex) => {
              const filteredItems = group.items.filter(
                (item) => !item.adminOnly || user?.role === "Admin"
              );
              
              if (filteredItems.length === 0) return null;

              return (
                <div key={group.label} className="flex items-center gap-1">
                  {groupIndex > 0 && (
                    <div className="h-6 w-px bg-white/20 mx-2" />
                  )}
                  <div className="flex items-center gap-1">
                    {filteredItems.map((item) => (
                      <NavLinkItem
                        key={item.to}
                        to={item.to}
                        label={item.label}
                        icon={item.icon}
                        isMobile={false}
                      />
                    ))}
                  </div>
                </div>
              );
            })}
          </div>

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

          {/* User info and logout - Right side */}
          <div className="flex items-center gap-2 md:gap-4">
            {user && (
              <div className="hidden sm:flex text-white/90 text-sm items-center gap-3">
                <div className="flex flex-col items-end">
                  <span className="font-semibold">{user.name || 'User'}</span>
                  <span className="text-xs text-white/70 capitalize">{user.role || 'Employee'}</span>
                </div>
                {user.businessId && (
                  <div className="hidden lg:block text-xs text-white/60 px-2.5 py-1 bg-white/10 rounded-md border border-white/10">
                    Business #{user.businessId}
                  </div>
                )}
              </div>
            )}
            <button
              onClick={handleLogout}
              className="px-3 md:px-4 py-2 rounded-lg transition-all font-medium text-white/90 hover:text-white hover:bg-white/15 border border-white/20 hover:border-white/30 text-sm shadow-sm hover:shadow-md"
              title="Logout"
            >
              <span className="hidden sm:inline">Logout</span>
              <span className="sm:hidden">Out</span>
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
