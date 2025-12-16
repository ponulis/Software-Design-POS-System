import { useState } from "react";
import { NavLink, useNavigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";

export default function Navbar() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);

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
        { to: "/payments", label: "Orders", icon: "💰" },
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
    <div className="w-full bg-blue-600 shadow-lg sticky top-0 z-50">
      <nav className="max-w-7xl mx-auto">
        {/* Main navigation bar */}
        <div className="flex items-center justify-between px-4 py-3">
          {/* Logo/Brand */}
          <div className="flex items-center gap-3">
            <button
              onClick={() => setMobileMenuOpen(!mobileMenuOpen)}
              className="lg:hidden text-white p-2 rounded-lg hover:bg-white/10 transition"
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
            <h1 className="text-xl font-bold text-white">POS System</h1>
          </div>

          {/* Desktop Navigation */}
          <div className="hidden lg:flex items-center gap-1 flex-1 justify-center">
            {navGroups.map((group, groupIdx) => (
              <div key={group.label} className="flex items-center">
                {group.items
                  .filter((item) => !item.adminOnly || user?.role === "Admin")
                  .map((item) => (
                    <NavLinkItem key={item.to} to={item.to} label={item.label} icon={item.icon} />
                  ))}
                {groupIdx < navGroups.length - 1 && (
                  <div className="h-6 w-px bg-white/20 mx-2" />
                )}
              </div>
            ))}
          </div>

          {/* User Profile & Logout */}
          <div className="flex items-center gap-3">
            {user && (
              <div className="hidden md:flex items-center gap-3 text-white/90">
                <div className="flex flex-col items-end text-sm">
                  <span className="font-semibold text-white">{user.name || "User"}</span>
                  <span className="text-xs text-white/70 capitalize">{user.role || "Employee"}</span>
                </div>
                {user.businessId && (
                  <div className="hidden lg:block text-xs text-white/60 px-2 py-1 bg-white/10 rounded">
                    Business #{user.businessId}
                  </div>
                )}
              </div>
            )}
            <button
              onClick={handleLogout}
              className="flex items-center gap-2 px-4 py-2 rounded-lg transition-colors font-medium text-white/90 hover:text-white hover:bg-white/10 border border-white/20 text-sm"
              title="Logout"
            >
              <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1" />
              </svg>
              <span className="hidden sm:inline">Logout</span>
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
