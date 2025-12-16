# Frontend Implementation Plan
## Feature-by-Feature Implementation Guide

**Based on:** `DEVELOPMENT_PLAN.md`  
**Current State:** Frontend structure exists (~30% complete), backend APIs are implemented (~90% complete)  
**Goal:** Complete frontend integration with backend APIs

---

## Phase 1: Foundation & Authentication (Priority: Critical)

### Feature 1.1: Authentication Integration
**Status:** ⚠️ Partial - AuthContext exists but not fully integrated  
**Backend API:** `POST /api/auth/login`, `POST /api/auth/register`  
**Estimated Time:** 2-3 hours

**Tasks:**
- [ ] Integrate `AuthContext` with backend login API (`POST /api/auth/login`)
- [ ] Implement token storage and refresh logic
- [ ] Update `Login.jsx` to call real API instead of mock
- [ ] Add error handling for authentication failures
- [ ] Implement logout functionality
- [ ] Add user profile display (name, role) in Navbar
- [ ] Test protected routes with real authentication

**Files to Modify:**
- `frontend/src/context/AuthContext.jsx` - Add API integration
- `frontend/src/pages/Login.jsx` - Connect to backend
- `frontend/src/components/Navbar.jsx` - Display user info
- `frontend/src/components/ProtectedRoute.jsx` - Verify token validity

**Dependencies:** None  
**Blocks:** All other features

---

### Feature 1.2: API Client Enhancement
**Status:** ✅ Basic setup exists  
**Backend API:** All endpoints  
**Estimated Time:** 1 hour

**Tasks:**
- [ ] Verify API base URL configuration
- [ ] Add request/response interceptors for error handling
- [ ] Implement retry logic for failed requests
- [ ] Add loading state management
- [ ] Create API service modules (auth, orders, payments, etc.)

**Files to Modify:**
- `frontend/src/api/client.js` - Enhance interceptors
- `frontend/src/api/auth.js` - Add auth API methods
- Create: `frontend/src/api/orders.js`
- Create: `frontend/src/api/payments.js`
- Create: `frontend/src/api/appointments.js`
- Create: `frontend/src/api/products.js`
- Create: `frontend/src/api/services.js`
- Create: `frontend/src/api/taxes.js`
- Create: `frontend/src/api/discounts.js`
- Create: `frontend/src/api/giftCards.js`
- Create: `frontend/src/api/business.js`
- Create: `frontend/src/api/employees.js`

**Dependencies:** Feature 1.1  
**Blocks:** All API integrations

---

## Phase 2: Order Management (Priority: Critical)

### Feature 2.1: Order Creation UI
**Status:** ❌ Not implemented  
**Backend API:** `POST /api/orders`, `GET /api/orders`, `GET /api/menu-items`  
**Estimated Time:** 4-5 hours

**Tasks:**
- [ ] Create order creation page/component (`OrderCreation.jsx`)
- [ ] Build product selection interface (grid/list view)
- [ ] Implement cart functionality:
  - Add/remove items
  - Quantity adjustment
  - Item notes
- [ ] Display order summary (subtotal calculation)
- [ ] Connect to `POST /api/orders` API
- [ ] Add spot selection (if applicable)
- [ ] Implement draft order saving
- [ ] Add order validation before submission

**Files to Create:**
- `frontend/src/pages/OrderCreation.jsx` (or integrate into Payments page)
- `frontend/src/components/orders/ProductGrid.jsx`
- `frontend/src/components/orders/Cart.jsx`
- `frontend/src/components/orders/CartItem.jsx`
- `frontend/src/components/orders/OrderSummary.jsx`
- `frontend/src/hooks/useOrders.jsx`

**Files to Modify:**
- `frontend/src/pages/Payments.jsx` - Add order creation flow
- `frontend/src/api/orders.js` - Add order API methods

**Dependencies:** Feature 1.2, Feature 3.1 (Product Catalog)  
**Blocks:** Feature 2.2 (Payment Processing)

---

### Feature 2.2: Order List & Details
**Status:** ⚠️ Partial - Mock data exists  
**Backend API:** `GET /api/orders`, `GET /api/orders/{orderId}`, `PATCH /api/orders/{orderId}`  
**Estimated Time:** 3-4 hours

**Tasks:**
- [ ] Replace mock data in `Payments.jsx` with real API calls
- [ ] Implement order list with pagination (`PaginatedResponse`)
- [ ] Add filtering by status, date range, spot
- [ ] Create order details view component
- [ ] Add order status badges (Draft, Placed, Paid, Cancelled)
- [ ] Implement order update functionality
- [ ] Add order cancellation (for Draft/Placed orders)
- [ ] Display order receipt link

**Files to Modify:**
- `frontend/src/pages/Payments.jsx` - Replace mock data
- `frontend/src/components/payments/PaymentsList.jsx` - Connect to API
- `frontend/src/components/payments/PaymentDetails.jsx` - Show order details
- `frontend/src/hooks/useOrders.jsx` - Add order management hooks

**Dependencies:** Feature 1.2, Feature 2.1  
**Blocks:** None

---

## Phase 3: Product & Service Catalog (Priority: High)

### Feature 3.1: Product Catalog UI
**Status:** ❌ Placeholder only  
**Backend API:** `GET /api/menu-items`, `POST /api/menu-items`, `PATCH /api/menu-items/{id}`, `DELETE /api/menu-items/{id}`  
**Estimated Time:** 4-5 hours

**Tasks:**
- [ ] Replace placeholder in `CatalogProducts.jsx`
- [ ] Build product grid/list view
- [ ] Implement product CRUD operations:
  - Create product modal/form
  - Edit product modal/form
  - Delete product confirmation
- [ ] Add product search functionality
- [ ] Display product availability status
- [ ] Show product tags/categories
- [ ] Add product image upload (if supported)
- [ ] Implement pagination for product list

**Files to Modify:**
- `frontend/src/pages/CatalogProducts.jsx` - Full implementation
- `frontend/src/api/products.js` - Add product API methods

**Files to Create:**
- `frontend/src/components/products/ProductGrid.jsx`
- `frontend/src/components/products/ProductCard.jsx`
- `frontend/src/components/products/ProductForm.jsx`
- `frontend/src/components/products/ProductModal.jsx`
- `frontend/src/hooks/useProducts.jsx`

**Dependencies:** Feature 1.2  
**Blocks:** Feature 2.1 (Order Creation)

---

### Feature 3.2: Service Catalog UI
**Status:** ❌ Not implemented  
**Backend API:** `GET /api/services`, `POST /api/services`, `PATCH /api/services/{id}`  
**Estimated Time:** 3-4 hours

**Tasks:**
- [ ] Create services page or add to catalog page
- [ ] Build service list/grid view
- [ ] Implement service CRUD operations
- [ ] Display service duration, price, availability
- [ ] Add service selection for appointments
- [ ] Link services to appointment creation

**Files to Create:**
- `frontend/src/pages/Services.jsx` (or add to CatalogProducts)
- `frontend/src/components/services/ServiceList.jsx`
- `frontend/src/components/services/ServiceForm.jsx`
- `frontend/src/api/services.js` - Add service API methods
- `frontend/src/hooks/useServices.jsx`

**Dependencies:** Feature 1.2  
**Blocks:** Feature 4.2 (Appointment Creation)

---

## Phase 4: Payment Processing (Priority: Critical)

### Feature 4.1: Payment Details Display
**Status:** ⚠️ Partial - Mock data  
**Backend API:** `GET /api/orders/{orderId}`, Pricing calculation  
**Estimated Time:** 2-3 hours

**Tasks:**
- [ ] Enhance `OrderDetails.jsx` component:
  - Display item list with quantities, unit prices, totals
  - Show subtotal (`Order.SubTotal`)
  - Display taxes (`Order.Tax`)
  - Show discounts (`Order.Discount`)
  - Display final total (`Order.Total`)
- [ ] Create `SummaryRow.jsx` component for totals
- [ ] Enhance `OrderRow.jsx` component for individual items
- [ ] Implement real-time calculation updates
- [ ] Connect to order API for live data

**Files to Modify:**
- `frontend/src/components/payments/OrderDetails.jsx` - Connect to API
- `frontend/src/components/payments/SummaryRow.jsx` - Enhance display
- `frontend/src/components/payments/OrderRow.jsx` - Add real data

**Dependencies:** Feature 2.1, Feature 1.2  
**Blocks:** Feature 4.2, 4.3, 4.4

---

### Feature 4.2: Cash Payment UI
**Status:** ⚠️ Partial - Component exists  
**Backend API:** `POST /api/payments` (method: Cash)  
**Estimated Time:** 2-3 hours

**Tasks:**
- [ ] Enhance `CashCheckout.jsx` component:
  - Cash received input field
  - Change calculation display
  - Handle exact payment, overpayment, insufficient cash scenarios
  - Display change amount prominently
- [ ] Integrate with `POST /api/payments` endpoint
- [ ] Add validation and error handling
- [ ] Show payment confirmation
- [ ] Handle payment success/failure scenarios

**Files to Modify:**
- `frontend/src/components/payments/CashCheckout.jsx` - Connect to API
- `frontend/src/api/payments.js` - Add payment API methods

**Dependencies:** Feature 4.1, Feature 1.2  
**Blocks:** None

---

### Feature 4.3: Card Payment UI (Stripe)
**Status:** ⚠️ Partial - Component exists  
**Backend API:** `POST /api/stripe/create-payment-intent`, `POST /api/stripe/confirm-payment`, `POST /api/payments`  
**Estimated Time:** 4-5 hours

**Tasks:**
- [ ] Enhance `CardCheckout.jsx` component:
  - Integrate Stripe Elements (or payment gateway SDK)
  - Card details form
  - Payment processing UI
  - Loading states during authorization
- [ ] Implement card tokenization
- [ ] Connect to Stripe backend endpoints
- [ ] Handle payment success/failure scenarios
- [ ] Add error handling for declined cards
- [ ] Display payment confirmation

**Files to Modify:**
- `frontend/src/components/payments/CardCheckout.jsx` - Full Stripe integration
- `frontend/src/api/stripe.js` - Create Stripe API client

**Dependencies:** Feature 4.1, Feature 1.2  
**Blocks:** None

**Note:** Requires Stripe.js library installation

---

### Feature 4.4: Gift Card Payment UI
**Status:** ⚠️ Partial - Component exists  
**Backend API:** `GET /api/gift-cards/{code}`, `POST /api/payments` (method: GiftCard)  
**Estimated Time:** 2-3 hours

**Tasks:**
- [ ] Enhance `GiftCardCheckout.jsx` component:
  - Gift card code input field
  - Display gift card balance (validate via API)
  - Show gift card information
  - Handle insufficient balance scenario
  - Display remaining balance after payment
- [ ] Integrate with gift card validation API
- [ ] Connect to payment endpoint with method="GiftCard"
- [ ] Add validation and error messages
- [ ] Handle payment success/failure

**Files to Modify:**
- `frontend/src/components/payments/GiftCardCheckout.jsx` - Connect to API
- `frontend/src/api/giftCards.js` - Add gift card API methods

**Dependencies:** Feature 4.1, Feature 1.2  
**Blocks:** None

---

### Feature 4.5: Split Payment UI
**Status:** ⚠️ Partial - Component exists  
**Backend API:** `POST /api/payments` (multiple payments for same order)  
**Estimated Time:** 4-5 hours

**Tasks:**
- [ ] Enhance `SplitPayment.jsx` component:
  - Add multiple clients functionality
  - Item assignment interface (assign items to clients)
  - Individual amount calculation per client
  - Display each client's total (with taxes/discounts)
- [ ] Implement split calculation logic:
  - Distribute items across clients
  - Calculate individual totals
  - Handle partial payments
- [ ] Process payments for each client separately
- [ ] Support different payment methods per client
- [ ] Show payment status per client
- [ ] Display overall order completion status
- [ ] Integrate with split payment API

**Files to Modify:**
- `frontend/src/components/payments/SplitPayment.jsx` - Full implementation
- `frontend/src/api/payments.js` - Add split payment support

**Dependencies:** Feature 4.1, Feature 1.2  
**Blocks:** None

---

### Feature 4.6: Payment Flow Integration
**Status:** ⚠️ Partial - Components exist but not integrated  
**Backend API:** All payment endpoints  
**Estimated Time:** 3-4 hours

**Tasks:**
- [ ] Integrate all payment methods into unified flow:
  - Update `Payments.jsx` page
  - Connect `CheckoutDetails.jsx` to all payment components
  - Implement payment method switching
- [ ] Add payment success/failure handling:
  - Success confirmation modal
  - Receipt display/printing
  - Error messages and retry logic
- [ ] Test end-to-end payment flows
- [ ] Add loading states throughout payment process
- [ ] Implement payment history display

**Files to Modify:**
- `frontend/src/pages/Payments.jsx` - Integrate all flows
- `frontend/src/components/payments/CheckoutDetails.jsx` - Connect components
- `frontend/src/components/payments/PaymentButton.jsx` - Enhance functionality

**Dependencies:** Feature 4.2, 4.3, 4.4, 4.5  
**Blocks:** None

---

## Phase 5: Appointment Management (Priority: High)

### Feature 5.1: Appointment List & Details
**Status:** ⚠️ Partial - Mock data exists  
**Backend API:** `GET /api/appointments`, `GET /api/appointments/{id}`  
**Estimated Time:** 3-4 hours

**Tasks:**
- [ ] Replace mock data in `Reservations.jsx` with real API calls
- [ ] Enhance `AppointmentsList.jsx`:
  - Display daily schedule view
  - Show appointment time slots
  - Color-code by status (Scheduled, Completed, Cancelled, Rescheduled)
  - Click to view details
- [ ] Enhance `AppointmentDetails.jsx`:
  - Display customer information (`CustomerName`, `CustomerPhone`)
  - Show assigned employee (`EmployeeId` → User name)
  - Display service details (`ServiceId` → Service name, duration)
  - Show associated order (`OrderId` → Order details)
  - Display payment status (via Order.Status if OrderId exists)
  - Show appointment status
  - Display activity log (`CreatedAt`, `UpdatedAt`)
- [ ] Add reschedule and cancel buttons
- [ ] Implement pagination for appointments list
- [ ] Add filtering by date, employee, customer, status

**Files to Modify:**
- `frontend/src/pages/Reservations.jsx` - Replace mock data
- `frontend/src/components/reservations/AppointmentsList.jsx` - Connect to API
- `frontend/src/components/reservations/AppointmentDetails.jsx` - Enhance display
- `frontend/src/components/reservations/useAppointments.jsx` - Add API integration
- `frontend/src/api/appointments.js` - Add appointment API methods

**Dependencies:** Feature 1.2, Feature 3.2 (Services)  
**Blocks:** Feature 5.2, 5.3

---

### Feature 5.2: Create Appointment UI
**Status:** ⚠️ Partial - Component exists with mock data  
**Backend API:** `POST /api/appointments`, `GET /api/appointments/available-slots` (if exists), `GET /api/employees`, `GET /api/services`  
**Estimated Time:** 4-5 hours

**Tasks:**
- [ ] Enhance `AddAppointmentModal.jsx`:
  - Customer information form (`CustomerName`, `CustomerPhone`)
  - Employee selection dropdown (fetch from `/api/employees`, map to `EmployeeId`)
  - Service selection (fetch services, map to `ServiceId`)
  - Date and time picker (`Date`)
  - Duration display (from `Service.DurationMinutes`)
  - Prepaid option toggle (if selected, create Order and set `OrderId`)
  - Notes field (`Notes`)
- [ ] Integrate with `POST /api/appointments` API
- [ ] Add validation (required fields, time conflicts)
- [ ] Implement time slot availability checking
- [ ] Handle appointment creation success/failure
- [ ] Refresh appointments list after creation

**Files to Modify:**
- `frontend/src/components/reservations/AddAppointmentModal.jsx` - Connect to API
- `frontend/src/api/appointments.js` - Add create appointment method
- `frontend/src/api/employees.js` - Add employee API methods

**Dependencies:** Feature 5.1, Feature 3.2  
**Blocks:** None

---

### Feature 5.3: Reschedule Appointment UI
**Status:** ⚠️ Partial - Component exists  
**Backend API:** `PATCH /api/appointments/{id}`, `GET /api/appointments/available-slots`  
**Estimated Time:** 3-4 hours

**Tasks:**
- [ ] Enhance `RescheduleAppointmentModal.jsx`:
  - Display current appointment details
  - Show available time slots (if endpoint exists)
  - Calendar/date picker
  - Time slot selection
  - Service duration display
  - Confirm/cancel buttons
- [ ] Integrate with available slots API (if available)
- [ ] Implement time slot selection logic
- [ ] Connect to reschedule endpoint (`PATCH /api/appointments/{id}`)
- [ ] Handle reschedule success/failure
- [ ] Update appointments list after reschedule

**Files to Modify:**
- `frontend/src/components/reservations/RescheduleAppointmentModal.jsx` - Connect to API
- `frontend/src/api/appointments.js` - Add reschedule method

**Dependencies:** Feature 5.1  
**Blocks:** None

---

### Feature 5.4: Cancel Appointment UI
**Status:** ❌ Not implemented  
**Backend API:** `DELETE /api/appointments/{id}`, `POST /api/orders/{orderId}/refund` (if prepaid)  
**Estimated Time:** 2-3 hours

**Tasks:**
- [ ] Build cancel appointment modal:
  - Confirmation dialog
  - Display customer notification info
  - Show refund information (if prepaid)
  - Cancel/confirm buttons
- [ ] Integrate with cancellation endpoint (`DELETE /api/appointments/{id}`)
- [ ] Handle refund processing if order exists
- [ ] Add success/error handling
- [ ] Update appointments list after cancellation

**Files to Create:**
- `frontend/src/components/reservations/CancelAppointmentModal.jsx`

**Files to Modify:**
- `frontend/src/components/reservations/AppointmentDetails.jsx` - Add cancel button handler
- `frontend/src/api/appointments.js` - Add cancel method
- `frontend/src/api/orders.js` - Add refund method (if needed)

**Dependencies:** Feature 5.1  
**Blocks:** None

---

## Phase 6: Business Configuration (Priority: Medium)

### Feature 6.1: Tax Management UI
**Status:** ❌ Placeholder only  
**Backend API:** `GET /api/taxes`, `POST /api/taxes`, `PATCH /api/taxes/{id}`  
**Estimated Time:** 3-4 hours

**Tasks:**
- [ ] Replace placeholder in `TaxesAndServiceCharges.jsx`
- [ ] Build tax list view
- [ ] Implement tax CRUD operations:
  - Create tax rule form
  - Edit tax rule form
  - Delete tax rule confirmation
- [ ] Display tax rate, effective dates, active status
- [ ] Add validation for tax rates
- [ ] Show tax application in orders

**Files to Modify:**
- `frontend/src/pages/TaxesAndServiceCharges.jsx` - Full implementation
- `frontend/src/api/taxes.js` - Add tax API methods

**Files to Create:**
- `frontend/src/components/taxes/TaxList.jsx`
- `frontend/src/components/taxes/TaxForm.jsx`
- `frontend/src/hooks/useTaxes.jsx`

**Dependencies:** Feature 1.2  
**Blocks:** None

**Note:** Service charges not in Order model - may need to remove or add to backend

---

### Feature 6.2: Discount Management UI
**Status:** ❌ Not implemented  
**Backend API:** `GET /api/discounts`, `POST /api/discounts`, `PATCH /api/discounts/{id}`  
**Estimated Time:** 3-4 hours

**Tasks:**
- [ ] Create discount management page or add to settings
- [ ] Build discount list view
- [ ] Implement discount CRUD operations:
  - Create discount form (Percentage/FixedAmount)
  - Edit discount form
  - Delete discount confirmation
- [ ] Display discount type, value, valid dates, active status
- [ ] Add validation for discount values
- [ ] Show discount application in orders

**Files to Create:**
- `frontend/src/pages/Discounts.jsx` (or add to Settings)
- `frontend/src/components/discounts/DiscountList.jsx`
- `frontend/src/components/discounts/DiscountForm.jsx`
- `frontend/src/api/discounts.js` - Add discount API methods
- `frontend/src/hooks/useDiscounts.jsx`

**Dependencies:** Feature 1.2  
**Blocks:** None

---

### Feature 6.3: Business Settings UI
**Status:** ⚠️ Partial - Form exists but not connected  
**Backend API:** `GET /api/business`, `PATCH /api/business`  
**Estimated Time:** 2-3 hours

**Tasks:**
- [ ] Connect `Settings.jsx` form to backend API
- [ ] Fetch current business data on load
- [ ] Implement business update functionality
- [ ] Add validation for business fields
- [ ] Display success/error messages
- [ ] Show business information (name, address, contact)

**Files to Modify:**
- `frontend/src/pages/Settings.jsx` - Connect to API
- `frontend/src/api/business.js` - Add business API methods

**Dependencies:** Feature 1.2  
**Blocks:** None

---

### Feature 6.4: User & Role Management UI
**Status:** ❌ Placeholder only  
**Backend API:** `GET /api/employees`, `POST /api/employees`, `PATCH /api/employees/{id}`  
**Estimated Time:** 4-5 hours

**Tasks:**
- [ ] Replace placeholder in `UsersAndRoles.jsx`
- [ ] Build user/employee list view
- [ ] Implement user CRUD operations:
  - Create user form (name, email, role, password)
  - Edit user form
  - Delete user confirmation
- [ ] Display user roles (Employee, Manager, Admin)
- [ ] Add role-based access control in UI
- [ ] Show user activity/status
- [ ] Implement password reset functionality (if supported)

**Files to Modify:**
- `frontend/src/pages/UsersAndRoles.jsx` - Full implementation
- `frontend/src/api/employees.js` - Add employee API methods

**Files to Create:**
- `frontend/src/components/users/UserList.jsx`
- `frontend/src/components/users/UserForm.jsx`
- `frontend/src/components/users/RoleBadge.jsx`
- `frontend/src/hooks/useUsers.jsx`

**Dependencies:** Feature 1.2  
**Blocks:** None

**Note:** Requires Admin role for access

---

## Phase 7: Dashboard & Analytics (Priority: Medium)

### Feature 7.1: Dashboard UI
**Status:** ❌ Placeholder only  
**Backend API:** `GET /api/dashboard`  
**Estimated Time:** 4-5 hours

**Tasks:**
- [ ] Replace placeholder in `Home.jsx`
- [ ] Build dashboard with key metrics:
  - Today's revenue
  - Today's orders count
  - Today's appointments count
  - Pending payments
  - Recent orders
  - Recent appointments
- [ ] Create dashboard cards/components
- [ ] Add charts/graphs (if data available)
- [ ] Implement real-time updates (optional)
- [ ] Add date range filters

**Files to Modify:**
- `frontend/src/pages/Home.jsx` - Full implementation
- `frontend/src/api/dashboard.js` - Add dashboard API methods

**Files to Create:**
- `frontend/src/components/dashboard/MetricCard.jsx`
- `frontend/src/components/dashboard/RecentOrders.jsx`
- `frontend/src/components/dashboard/RecentAppointments.jsx`
- `frontend/src/hooks/useDashboard.jsx`

**Dependencies:** Feature 1.2, Feature 2.2, Feature 5.1  
**Blocks:** None

---

## Phase 8: Error Handling & Polish (Priority: High)

### Feature 8.1: Global Error Handling
**Status:** ⚠️ Partial - Basic interceptors exist  
**Estimated Time:** 2-3 hours

**Tasks:**
- [ ] Enhance error handling throughout application:
  - Network errors
  - Validation errors (400)
  - Authorization errors (401, 403)
  - Not found errors (404)
  - Server errors (500)
- [ ] Create error boundary component
- [ ] Add error toast notifications
- [ ] Implement retry logic for failed requests
- [ ] Add error logging (optional)

**Files to Create:**
- `frontend/src/components/ErrorBoundary.jsx`
- `frontend/src/components/ErrorToast.jsx`
- `frontend/src/utils/errorHandler.js`

**Files to Modify:**
- `frontend/src/api/client.js` - Enhance error handling
- `frontend/src/App.jsx` - Add ErrorBoundary

**Dependencies:** Feature 1.2  
**Blocks:** None

---

### Feature 8.2: Loading States & User Feedback
**Status:** ⚠️ Partial  
**Estimated Time:** 3-4 hours

**Tasks:**
- [ ] Add loading states throughout application:
  - Payment processing indicators
  - API call loading spinners
  - Form submission loading states
- [ ] Add success notifications:
  - Payment confirmation
  - Order creation success
  - Appointment updates
  - CRUD operation success
- [ ] Improve form validation and error messages
- [ ] Add skeleton loaders for data fetching

**Files to Create:**
- `frontend/src/components/LoadingSpinner.jsx`
- `frontend/src/components/SkeletonLoader.jsx`
- `frontend/src/components/SuccessToast.jsx`
- `frontend/src/hooks/useToast.jsx`

**Dependencies:** Feature 1.2  
**Blocks:** None

---

### Feature 8.3: UI Polish & Responsiveness
**Status:** ⚠️ Partial - Basic styling exists  
**Estimated Time:** 4-5 hours

**Tasks:**
- [ ] Review all UI components against mockups
- [ ] Ensure consistent styling:
  - Color scheme
  - Typography
  - Spacing and layout
- [ ] Add responsive design:
  - Mobile-friendly layouts
  - Tablet optimization
  - Desktop enhancements
- [ ] Improve accessibility:
  - Keyboard navigation
  - Screen reader support
  - Focus indicators
  - ARIA labels
- [ ] Add animations and transitions
- [ ] Optimize images and assets

**Dependencies:** All previous features  
**Blocks:** None

---

## Phase 9: Advanced Features (Priority: Low)

### Feature 9.1: Receipt Generation & Printing
**Status:** ❌ Not implemented  
**Backend API:** `GET /api/orders/{orderId}/receipt`  
**Estimated Time:** 2-3 hours

**Tasks:**
- [ ] Create receipt display component
- [ ] Integrate with receipt API endpoint
- [ ] Add print functionality
- [ ] Format receipt for printing
- [ ] Add receipt download (PDF) if supported

**Files to Create:**
- `frontend/src/components/receipts/ReceiptView.jsx`
- `frontend/src/components/receipts/ReceiptPrint.jsx`

**Dependencies:** Feature 2.2  
**Blocks:** None

---

### Feature 9.2: Payment History & Reports
**Status:** ⚠️ Partial - List exists but needs enhancement  
**Backend API:** `GET /api/payments` (with filtering)  
**Estimated Time:** 3-4 hours

**Tasks:**
- [ ] Enhance payment history display
- [ ] Add filtering by date range, method, status
- [ ] Implement payment export (CSV/PDF)
- [ ] Add payment statistics
- [ ] Create payment reports

**Files to Modify:**
- `frontend/src/components/payments/PaymentsList.jsx` - Add filtering
- `frontend/src/pages/Payments.jsx` - Add export functionality

**Dependencies:** Feature 4.6  
**Blocks:** None

---

### Feature 9.3: Order History & Reports
**Status:** ❌ Not implemented  
**Backend API:** `GET /api/orders` (with filtering)  
**Estimated Time:** 3-4 hours

**Tasks:**
- [ ] Create order history page
- [ ] Add filtering and search
- [ ] Implement order export
- [ ] Add order statistics
- [ ] Create order reports

**Files to Create:**
- `frontend/src/pages/OrderHistory.jsx`
- `frontend/src/components/orders/OrderFilters.jsx`
- `frontend/src/components/orders/OrderExport.jsx`

**Dependencies:** Feature 2.2  
**Blocks:** None

---

## Implementation Priority Summary

### Critical (Must Have for MVP)
1. ✅ Feature 1.1: Authentication Integration
2. ✅ Feature 1.2: API Client Enhancement
3. ✅ Feature 2.1: Order Creation UI
4. ✅ Feature 2.2: Order List & Details
5. ✅ Feature 3.1: Product Catalog UI
6. ✅ Feature 4.1: Payment Details Display
7. ✅ Feature 4.2: Cash Payment UI
8. ✅ Feature 4.3: Card Payment UI (Stripe)
9. ✅ Feature 4.4: Gift Card Payment UI
10. ✅ Feature 4.6: Payment Flow Integration
11. ✅ Feature 5.1: Appointment List & Details
12. ✅ Feature 5.2: Create Appointment UI
13. ✅ Feature 5.3: Reschedule Appointment UI
14. ✅ Feature 5.4: Cancel Appointment UI

### High Priority
15. ✅ Feature 3.2: Service Catalog UI
16. ✅ Feature 4.5: Split Payment UI
17. ✅ Feature 6.1: Tax Management UI
18. ✅ Feature 6.3: Business Settings UI
19. ✅ Feature 8.1: Global Error Handling
20. ✅ Feature 8.2: Loading States & User Feedback

### Medium Priority
21. ✅ Feature 6.2: Discount Management UI
22. ✅ Feature 6.4: User & Role Management UI
23. ✅ Feature 7.1: Dashboard UI
24. ✅ Feature 8.3: UI Polish & Responsiveness

### Low Priority (Nice to Have)
25. ✅ Feature 9.1: Receipt Generation & Printing
26. ✅ Feature 9.2: Payment History & Reports
27. ✅ Feature 9.3: Order History & Reports

---

## Estimated Timeline

**Critical Features:** 40-50 hours (5-6 days for 2 developers)  
**High Priority Features:** 15-20 hours (2-3 days)  
**Medium Priority Features:** 15-20 hours (2-3 days)  
**Low Priority Features:** 8-10 hours (1-2 days)

**Total Estimated Time:** 78-100 hours (10-13 days for 2 developers)

---

## Dependencies Map

```
Feature 1.1 (Auth) → Feature 1.2 (API Client) → All Features
Feature 1.2 → Feature 2.1, 2.2, 3.1, 3.2, 4.x, 5.x, 6.x, 7.1
Feature 3.1 → Feature 2.1 (Order Creation)
Feature 3.2 → Feature 5.1, 5.2 (Appointments)
Feature 2.1 → Feature 2.2 → Feature 4.1 → Feature 4.2, 4.3, 4.4, 4.5 → Feature 4.6
Feature 5.1 → Feature 5.2, 5.3, 5.4
Feature 2.2, 5.1 → Feature 7.1 (Dashboard)
All Features → Feature 8.3 (UI Polish)
```

---

## Notes

1. **Backend APIs:** Most backend APIs are implemented (~90%). Verify endpoint availability before starting each feature.

2. **Mock Data:** Many components currently use mock data. Replace systematically with API calls.

3. **Pagination:** Backend supports `PaginatedResponse<T>`. Implement pagination in all list views.

4. **Error Handling:** Implement consistent error handling patterns early to avoid rework.

5. **Testing:** Test each feature after implementation, especially payment flows and appointment management.

6. **Stripe Integration:** Requires Stripe.js library. Set up Stripe account and test keys.

7. **Role-Based Access:** Some features require specific roles (Admin, Manager). Implement role checks in UI.

8. **Environment Variables:** Ensure `.env` file is configured with API base URL and Stripe keys.

---

## Next Steps

1. Start with Phase 1 (Foundation & Authentication)
2. Move to Phase 2 (Order Management)
3. Continue with Phase 4 (Payment Processing)
4. Implement Phase 5 (Appointment Management)
5. Complete remaining phases based on priority

**Recommended Approach:** Work in parallel where possible (e.g., FE1 on Orders, FE2 on Products), but coordinate on shared components and API integration.
