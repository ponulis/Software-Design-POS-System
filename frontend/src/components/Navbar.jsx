import { NavLink } from "react-router-dom";

export default function Navbar() {
  const base = "px-4 py-2 rounded-lg transition-colors font-medium";
  const inactive = "text-white/80 hover:text-white hover:bg-white/10";
  const active = "text-white bg-white/20";

  return (
    <div className="w-full bg-blue-600 shadow-lg sticky top-0 z-50">
      <nav className="max-w-7xl mx-auto p-4">
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
            <NavLink to="/reservations" className={({ isActive }) => `${base} ${isActive ? active : inactive}`}>
              Reservations
            </NavLink>
          </li>
        </ul>
      </nav>
    </div>
  );
}
