import Home from "./pages/Home";
import Settings from "./pages/Settings";
import CatalogProducts from "./pages/CatalogProducts";
import TaxesAndServiceCharges from "./pages/TaxesAndServiceCharges";
import UsersAndRoles from "./pages/UsersAndRoles";
import OrdersAndPayments from "./pages/OrdersAndPayments";
import Reservations from "./pages/Reservations";
import Navbar from "./components/Navbar";
import { Routes, Route } from "react-router-dom";

export default function App() {
  return (
    <div className="min-h-screen bg-gray-100">
      <Navbar />

    <div className="p-6">
      <Routes>
        <Route path="/" element={<Home />} />
        <Route path="/settings" element={<Settings />} />
        <Route path="/catalog-products" element={<CatalogProducts />} />
        <Route path="/catalog-products" element={<CatalogProducts />} />
        <Route path="/taxes-and-service-charges" element={<TaxesAndServiceCharges />} />
        <Route path="/users-and-roles" element={<UsersAndRoles />} />
        <Route path="/orders-and-payments" element={<OrdersAndPayments />} />
        <Route path="/reservations" element={<Reservations />} />
      </Routes>
      </div>
    </div>
  );
}
