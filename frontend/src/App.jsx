import Home from "./pages/Home";
import Login from "./pages/Login";
import Settings from "./pages/Settings";
import CatalogProducts from "./pages/CatalogProducts";
import TaxesAndServiceCharges from "./pages/TaxesAndServiceCharges";
import UsersAndRoles from "./pages/UsersAndRoles";
import Payments from "./pages/Payments";
import PaymentHistory from "./pages/PaymentHistory";
import OrderHistory from "./pages/OrderHistory";
import Reservations from "./pages/Reservations";
import Navbar from "./components/Navbar";
import ProtectedRoute from "./components/ProtectedRoute";
import ErrorBoundary from "./components/ErrorBoundary";
import ToastContainer from "./components/ToastContainer";
import { AuthProvider } from "./context/AuthContext";
import { ToastProvider } from "./context/ToastContext";
import { Routes, Route, Navigate } from "react-router-dom";
import Discounts from "./pages/Discounts";

export default function App() {
  return (
    <ErrorBoundary>
      <ToastProvider>
        <AuthProvider>
          <ToastContainer />
          <Routes>
            <Route path="/login" element={<Login />} />
            <Route
              path="/*"
              element={
                <>
                  <Navbar />
                  <div className="p-6">
                    <Routes>
                      <Route
                        path="/"
                        element={
                          <ProtectedRoute>
                            <Home />
                          </ProtectedRoute>
                        }
                      />
                      <Route
                        path="/settings"
                        element={
                          <ProtectedRoute>
                            <Settings />
                          </ProtectedRoute>
                        }
                      />
                      <Route
                        path="/catalog-products"
                        element={
                          <ProtectedRoute>
                            <CatalogProducts />
                          </ProtectedRoute>
                        }
                      />
                      <Route
                        path="/taxes-and-service-charges"
                        element={
                          <ProtectedRoute>
                            <TaxesAndServiceCharges />
                          </ProtectedRoute>
                        }
                      />
                      <Route
                        path="/users-and-roles"
                        element={
                          <ProtectedRoute requiredRole="Admin">
                            <UsersAndRoles />
                          </ProtectedRoute>
                        }
                      />
                  <Route
                    path="/payments"
                    element={
                      <ProtectedRoute>
                        <Payments />
                      </ProtectedRoute>
                    }
                  />
                  <Route
                    path="/payment-history"
                    element={
                      <ProtectedRoute>
                        <PaymentHistory />
                      </ProtectedRoute>
                    }
                  />
                  <Route
                    path="/order-history"
                    element={
                      <ProtectedRoute>
                        <OrderHistory />
                      </ProtectedRoute>
                    }
                  />
                  <Route
                    path="/reservations"
                    element={
                      <ProtectedRoute>
                        <Reservations />
                      </ProtectedRoute>
                    }
                  />
                  <Route
                    path="/discounts"
                    element={
                      <ProtectedRoute>
                        <Discounts />
                      </ProtectedRoute>
                    }
                  />
                  
                      <Route path="*" element={<Navigate to="/" replace />} />
                    </Routes>
                  </div>
                </>
              }
            />
          </Routes>
        </AuthProvider>
      </ToastProvider>
    </ErrorBoundary>
  );
}
