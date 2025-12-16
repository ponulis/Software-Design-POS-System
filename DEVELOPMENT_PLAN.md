# Software Development Plan - DAB POS System
## 1-Week Sprint Plan for 4-Person Team (2 Frontend, 2 Backend)

**Project Duration:** 1 Week (5 working days)  
**Team Composition:** 
- Frontend Developer 1 (FE1)
- Frontend Developer 2 (FE2)
- Backend Developer 1 (BE1)
- Backend Developer 2 (BE2)

**Reference Document:** PSD_by_DAB.pdf

---

## Day 1: Foundation & Setup (Monday)

### Morning (4 hours)

#### Backend Developer 1 (BE1)
- [ ] **Database Setup & Entity Framework Configuration**
  - Set up Entity Framework Core with SQL Server/PostgreSQL
  - Create DbContext with all entities from Data Model:
    - Business, User/Employee, Product, Service, Order, OrderItem
    - Appointment, Tax, Discount, Payment, GiftCard
  - Configure relationships and constraints per ER diagram
  - Create initial migration
  - Seed database with sample data (at least 1 business, 2 employees, 5 products, 3 services)

#### Backend Developer 2 (BE2)
- [ ] **Authentication & Authorization Infrastructure**
  - Implement JWT authentication middleware
  - Create User service with password hashing (using PasswordHash field)
  - Set up role-based authorization (Employee, Manager, Admin per User.Role)
  - Create login endpoint (to be added to API contract: `/auth/login`) returning JWT token
  - Implement user context extraction from JWT (user_id, business_id)
  - Add authentication middleware to Program.cs
  - **Note:** Authentication endpoints need to be added to `api_contracts.yaml`

#### Frontend Developer 1 (FE1)
- [ ] **Project Setup & Authentication UI**
  - Set up API client/axios configuration with base URL
  - Create authentication context/hooks (`useAuth.jsx`)
  - Build login page component
  - Implement token storage (localStorage/sessionStorage)
  - Add protected route wrapper component
  - Update App.jsx with authentication routing

#### Frontend Developer 2 (FE2)
- [ ] **Core Layout & Navigation**
  - Review and enhance Navbar component
  - Create main layout wrapper with sidebar/navigation
  - Set up routing structure for all pages:
    - `/payments` - Payment processing
    - `/reservations` - Appointment management
    - `/catalog-products` - Product catalog
    - `/taxes-and-service-charges` - Tax configuration
    - `/users-and-roles` - User management
    - `/settings` - Business settings
  - Add loading states and error boundaries

### Afternoon (4 hours)

#### Backend Developer 1 (BE1)
- [ ] **Order Service - Core CRUD**
  - Implement Order controller per `api_contracts.yaml`:
    - `POST /orders` - Create order (includes spotId, createdBy, items array)
    - `GET /orders` - Get all orders (filtered by BusinessId)
    - `GET /orders/{orderId}` - Get order by ID
    - `PATCH /orders/{orderId}` - Update order
    - `DELETE /orders/{orderId}` - Cancel order
  - Create OrderService with business logic:
    - Order validation (product availability via Product.Available)
    - Order status management per OrderStatus enum (Draft, Placed, Paid, Cancelled)
    - Calculate Order.SubTotal, Order.Discount, Order.Tax, Order.Total
  - Implement OrderItem CRUD operations per `api_contracts.yaml`:
    - `POST /order-items` - Create order item
    - `GET /order-items` - Get order items
    - `GET /order-items/{orderItemId}` - Get order item
    - `PATCH /order-items/{orderItemId}` - Update order item
    - `DELETE /order-items/{orderItemId}` - Delete order item

#### Backend Developer 2 (BE2)
- [ ] **Product & Service Catalog APIs**
  - Implement Product/MenuItem controller per `api_contracts.yaml`:
    - `GET /menu-items` - List all products (filtered by BusinessId)
    - `GET /menu-items/{menuItemId}` - Get product details
    - `POST /menu-items` - Create product (Product model: Name, Description, Price, Tags, Available)
    - `PATCH /menu-items/{menuItemId}` - Update product
    - `DELETE /menu-items/{menuItemId}` - Delete product
  - **Note:** Service entity exists (Service model) but Service endpoints need to be added to `api_contracts.yaml`:
    - `GET /services` - List services
    - `POST /services` - Create service
    - `PATCH /services/{serviceId}` - Update service
  - Implement availability checking logic (Product.Available, Service.Available)

#### Frontend Developer 1 (FE1)
- [ ] **Order Creation UI**
  - Create order creation page/component
  - Build product selection interface (grid/list view)
  - Implement cart functionality:
    - Add/remove items
    - Quantity adjustment
    - Item notes
  - Display order summary (subtotal calculation)
  - Connect to backend `/orders` API

#### Frontend Developer 2 (FE2)
- [ ] **Product Catalog UI**
  - Build product catalog page (`CatalogProducts.jsx`)
  - Create product card/list components
  - Implement product search and filtering
  - Add product CRUD forms (create/edit modal)
  - Connect to `/menu-items` API endpoints
  - Display product availability status

### End of Day 1 Deliverables
- ✅ Database schema created and seeded
- ✅ Authentication working (backend + frontend)
- ✅ Basic order creation flow (UI + API)
- ✅ Product catalog UI functional

---

## Day 2: Payment Processing Core (Tuesday)

### Morning (4 hours)

#### Backend Developer 1 (BE1)
- [ ] **Pricing & Calculation Service**
  - Create PricingService for:
    - Subtotal calculation from order items (Order.SubTotal)
    - Tax calculation (retrieve tax rates from Tax model, stored in Order.Tax)
    - Discount application (retrieve from Discount model, stored in Order.Discount)
    - Final total computation (Order.Total = SubTotal - Discount + Tax)
  - Implement discount retrieval logic (query Discount model by BusinessId)
  - **Note:** Tax and Discount endpoints need to be added to `api_contracts.yaml`:
    - `GET /taxes` - Get tax rates
    - `POST /taxes` - Create tax rule
    - `GET /discounts` - Get discounts
    - `POST /discounts` - Create discount
  - Ensure calculations match Order schema structure

#### Backend Developer 2 (BE2)
- [ ] **Payment Service - Core Infrastructure**
  - Implement Payment controller per `api_contracts.yaml`:
    - `POST /payments` - Create payment record (includes OrderId, Amount, Method, CreatedBy)
    - `GET /payments` - List payments
    - `GET /payments/{paymentId}` - Get payment details
    - `DELETE /payments/{paymentId}` - Delete payment
  - Create PaymentService with:
    - Payment method validation (Cash, Card, GiftCard per PaymentMethod enum)
    - Payment status tracking via Order.Status
    - Transaction logging (metadata: method, amount, timestamp, CreatedBy)
  - Implement order-payment relationship (Payment.OrderId → Order.Id)
  - **Note:** Payment model supports GiftCard method, but API contract enum only shows [Cash, Card] - update API contract to include GiftCard

#### Frontend Developer 1 (FE1)
- [ ] **Payment UI - Order Details Display**
  - Enhance `OrderDetails.jsx` component:
    - Display item list with quantities, unit prices, total prices
    - Show subtotal (Order.SubTotal), taxes (Order.Tax), discounts (Order.Discount)
    - Display final total (Order.Total)
    - **Note:** Service charges not in Order model - remove if not needed
  - Create `SummaryRow.jsx` component for totals
  - Create `OrderRow.jsx` component for individual items
  - Implement real-time calculation updates

#### Frontend Developer 2 (FE2)
- [ ] **Payment Selection UI**
  - Create payment method selection component
  - Build payment button component (`PaymentButton.jsx`)
  - Create `CheckoutDetails.jsx` wrapper component
  - Implement payment method switching logic
  - Add cancel payment functionality

### Afternoon (4 hours)

#### Backend Developer 1 (BE1)
- [ ] **Payment Endpoint - Order Payment Integration**
  - Implement payment processing via `POST /payments` endpoint per `api_contracts.yaml`:
    - Validate order status (Order.Status must be Draft or Placed)
    - Calculate totals using PricingService before creating payment
    - Accept payment request body: { orderId, amount, method, createdBy }
    - Update order status to "Paid" on successful payment (Order.Status = Paid)
    - Create payment record via Payment controller
    - Return payment confirmation
  - **Note:** Receipt generation endpoint (`/orders/{orderId}/receipt`) needs to be added to `api_contracts.yaml` if required
  - **Note:** The plan originally mentioned `/orders/{orderId}/pay` but API contract uses `POST /payments` with OrderId reference - align with API contract

#### Backend Developer 2 (BE2)
- [ ] **Cash Payment Flow**
  - Implement cash payment handling in PaymentService:
    - Record cash amount received
    - Calculate change (cash_amount - total)
    - Handle insufficient cash scenario
    - Log transaction with method="cash"
  - Add validation for cash payment (amount >= total)
  - Implement change calculation logic

#### Frontend Developer 1 (FE1)
- [ ] **Cash Payment UI**
  - Build `CashCheckout.jsx` component per mockup:
    - Cash received input field
    - Change calculation display
    - Handle exact payment, overpayment, insufficient cash scenarios
    - Display change amount prominently
  - Integrate with `POST /payments` endpoint (include OrderId in request body)
  - Add validation and error handling

#### Frontend Developer 2 (FE2)
- [ ] **Card Payment UI**
  - Build `CardCheckout.jsx` component per mockup:
    - Card details form (using Stripe Elements or similar)
    - Payment processing UI
    - Loading states during authorization
  - Integrate Stripe client SDK (or payment gateway SDK)
  - Implement card tokenization (card details never reach backend)
  - Connect to payment endpoint
  - Handle payment success/failure scenarios

### End of Day 2 Deliverables
- ✅ Payment calculation logic working
- ✅ Cash payment flow complete (backend + frontend)
- ✅ Card payment UI ready (Stripe integration started)
- ✅ Order payment endpoint functional

---

## Day 3: Advanced Payments & Gift Cards (Wednesday)

### Morning (4 hours)

#### Backend Developer 1 (BE1)
- [ ] **Gift Card Service**
  - GiftCard entity already exists in models:
    - Code (unique identifier)
    - Balance, OriginalAmount
    - IssuedDate, ExpiryDate
    - IsActive status
  - **Note:** GiftCard endpoints need to be added to `api_contracts.yaml`:
    - `GET /gift-cards/{code}` - Validate and get balance
    - `POST /gift-cards` - Create/issue gift card
    - `PATCH /gift-cards/{code}` - Update balance (on redemption)
  - Create GiftCardService with:
    - Balance validation
    - Balance deduction logic
    - Expiry checking (ExpiryDate)
    - IsActive status checking

#### Backend Developer 2 (BE2)
- [ ] **Card Payment Integration (Stripe)**
  - Integrate Stripe payment gateway:
    - Set up Stripe API client
    - Create payment intent creation logic
    - Handle Stripe webhook (optional for MVP)
  - Implement card payment in PaymentService:
    - Create Stripe payment intent
    - Store transaction metadata (transaction_id, authorization_code, timestamp)
    - Handle authorization success/failure
    - Update order status on success
  - Add error handling for declined cards

#### Frontend Developer 1 (FE1)
- [ ] **Gift Card Payment UI**
  - Build `GiftCardCheckout.jsx` component per mockup:
    - Gift card code input field
    - Display gift card balance
    - Show gift card information
    - Handle insufficient balance scenario
    - Display remaining balance after payment
  - Integrate with gift card validation API (to be added: `GET /gift-cards/{code}`)
  - Connect to `POST /payments` endpoint with method="GiftCard" (update API contract enum to include GiftCard)
  - Add validation and error messages

#### Frontend Developer 2 (FE2)
- [ ] **Split Payment UI Foundation**
  - Create `SplitPayment.jsx` component per mockup:
    - Add multiple clients functionality
    - Item assignment interface (assign items to clients)
    - Individual amount calculation per client
    - Display each client's total
  - Implement split calculation logic:
    - Distribute items across clients
    - Calculate individual totals (with taxes/discounts)
    - Handle partial payments

### Afternoon (4 hours)

#### Backend Developer 1 (BE1)
- [ ] **Split Payment Backend Logic**
  - Extend Payment entity to support split payments:
    - Add relationship to multiple payments
    - Track partial payment amounts
    - Link payments to order items
  - Implement split payment validation:
    - Ensure sum of split payments equals order total
    - Validate all payments completed before closing order
  - Create split payment endpoint logic:
    - Accept array of payments with assigned items
    - Process each payment individually
    - Update order status only when all splits complete

#### Backend Developer 2 (BE2)
- [ ] **Payment Processing Refinement**
  - Enhance `POST /payments` endpoint to support split payments:
    - Accept multiple payment records for same OrderId
    - Support split payment mode (multiple payments for one order)
    - Handle multiple payment methods in single order
    - Track payment status per split (sum of payments must equal Order.Total)
    - Update Order.Status to "Paid" only when sum of payments equals total
  - Add payment validation:
    - Ensure sum of payment amounts matches Order.Total
    - Prevent duplicate payments (validate payment uniqueness)
    - Handle payment failures gracefully
  - Create payment history/audit logging (use Payment.PaidAt, Payment.CreatedBy)

#### Frontend Developer 1 (FE1)
- [ ] **Split Payment UI - Payment Processing**
  - Enhance split payment component:
    - Process payments for each client separately
    - Support different payment methods per client (card, cash, gift card)
    - Show payment status per client
    - Display overall order completion status
  - Integrate with split payment API
  - Add progress indicator for multi-step payment

#### Frontend Developer 2 (FE2)
- [ ] **Payment Flow Integration & Testing**
  - Integrate all payment methods into unified flow:
    - Update `Payments.jsx` page
    - Connect `CheckoutDetails.jsx` to all payment components
    - Implement payment method switching
  - Add payment success/failure handling:
    - Success confirmation modal
    - Receipt display/printing
    - Error messages and retry logic
  - Test end-to-end payment flows

### End of Day 3 Deliverables
- ✅ Gift card payment flow complete
- ✅ Card payment fully integrated with Stripe
- ✅ Split payment functionality working
- ✅ All payment methods integrated in UI

---

## Day 4: Appointment Management (Thursday)

### Morning (4 hours)

#### Backend Developer 1 (BE1)
- [ ] **Appointment Service - Core CRUD**
  - Implement Appointment controller per `api_contracts.yaml`:
    - `POST /appointments` - Create appointment (includes date, customerName, customerPhone, notes)
    - `GET /appointments` - Get all appointments (with filtering by BusinessId)
    - `GET /appointments/{appointmentId}` - Get appointment details
    - `PATCH /appointments/{appointmentId}` - Modify/reschedule appointment (per API contract, not PUT)
    - `DELETE /appointments/{appointmentId}` - Cancel appointment
  - Create AppointmentService with:
    - Time slot validation (Appointment.Date)
    - Conflict checking (no overlapping appointments for same EmployeeId/ServiceId)
    - Business hours validation
    - Employee availability checking (Appointment.EmployeeId)
  - **Note:** API contract uses PATCH (not PUT) for appointment modification

#### Backend Developer 2 (BE2)
- [ ] **Appointment Reschedule Logic**
  - Implement reschedule validation via `PATCH /appointments/{appointmentId}`:
    - Check new time slot availability (Appointment.Date)
    - Verify no conflicts with other appointments (same EmployeeId, overlapping time)
    - Validate within business hours
    - Check employee schedule (Appointment.EmployeeId)
  - **Note:** Available slots endpoint needs to be added to `api_contracts.yaml`:
    - `GET /appointments/available-slots` - Get available time slots
    - Filter by date, employee, service duration (Service.DurationMinutes)
  - Implement appointment status management per AppointmentStatus enum:
    - Scheduled, Completed, Cancelled, Rescheduled
  - Add activity log for appointment changes (use Appointment.UpdatedAt)

#### Frontend Developer 1 (FE1)
- [ ] **Appointments Console UI**
  - Enhance `AppointmentsList.jsx` per mockup:
    - Display daily schedule view
    - Show appointment time slots
    - Color-code by status
    - Click to view details
  - Build `AppointmentDetails.jsx` component:
    - Display customer information (Appointment.CustomerName, CustomerPhone)
    - Show assigned employee (Appointment.EmployeeId → User.Name)
    - Display service details (Appointment.ServiceId → Service.Name, DurationMinutes)
    - Show associated order (Appointment.OrderId → Order details)
    - Display payment status (via Order.Status if OrderId exists)
    - Show appointment status (Appointment.Status)
    - Display activity log (Appointment.CreatedAt, UpdatedAt)
  - Add reschedule and cancel buttons

#### Frontend Developer 2 (FE2)
- [ ] **Reschedule Appointment UI**
  - Enhance `RescheduleAppointmentModal.jsx` per mockup:
    - Display current appointment details
    - Show available time slots
    - Calendar/date picker
    - Time slot selection
    - Service duration display
    - Confirm/cancel buttons
  - Integrate with available slots API
  - Implement time slot selection logic
  - Connect to reschedule endpoint

### Afternoon (4 hours)

#### Backend Developer 1 (BE1)
- [ ] **Appointment Cancellation & Refunds**
  - Implement cancellation logic via `DELETE /appointments/{appointmentId}`:
    - Verify service not completed (Appointment.Status != Completed)
    - Update status to Cancelled (Appointment.Status = Cancelled)
    - Handle prepaid appointments (check Appointment.OrderId)
  - Integrate with Order Service for refunds:
    - Check if appointment has associated paid order (Appointment.OrderId, Order.Status = Paid)
    - **Note:** Refund endpoint needs to be added to `api_contracts.yaml` if required:
      - `POST /orders/{orderId}/refund` - Process refund for cancelled appointment
    - Update order status (Order.Status = Cancelled or create refund payment)
  - Add cancellation reason/notes (use Appointment.Notes field)
  - Implement audit logging for cancellations (use Appointment.UpdatedAt)

#### Backend Developer 2 (BE2)
- [ ] **Appointment-Order Integration**
  - Link appointments to orders:
    - Create order when appointment is prepaid (set Appointment.OrderId)
    - Associate order with appointment (Order.Appointment navigation property)
    - Track payment status (via Order.Status)
  - Enhance appointment queries:
    - Include order information (Appointment.OrderId → Order)
    - Include payment status (via Order.Status if OrderId exists)
    - Filter by payment status
  - Implement appointment search/filtering:
    - By date range (Appointment.Date)
    - By employee (Appointment.EmployeeId → User)
    - By customer (Appointment.CustomerName, CustomerPhone)
    - By status (Appointment.Status)
  - **Note:** Employee endpoints exist in API contract (`/employees`) but map to User model internally

#### Frontend Developer 1 (FE1)
- [ ] **Cancel Appointment UI**
  - Build cancel appointment modal per mockup:
    - Confirmation dialog
    - Display customer notification info
    - Show refund information (if prepaid)
    - Cancel/confirm buttons
  - Integrate with cancellation endpoint
  - Add success/error handling
  - Update appointments list after cancellation

#### Frontend Developer 2 (FE2)
- [ ] **Add Appointment UI Enhancement**
  - Enhance `AddAppointmentModal.jsx`:
    - Customer information form (Appointment.CustomerName, CustomerPhone)
    - Employee selection dropdown (fetch from `/employees` endpoint, map to Appointment.EmployeeId)
    - Service selection (fetch services, map to Appointment.ServiceId)
    - Date and time picker (Appointment.Date)
    - Duration display (from Service.DurationMinutes)
    - Prepaid option toggle (if selected, create Order and set Appointment.OrderId)
    - Notes field (Appointment.Notes)
  - Integrate with `POST /appointments` API
  - Add validation (required fields, time conflicts)
  - Implement time slot availability checking (use available-slots endpoint when added)

### End of Day 4 Deliverables
- ✅ Appointment CRUD operations complete
- ✅ Reschedule functionality working
- ✅ Cancel appointment with refund handling
- ✅ Appointments console UI functional

---

## Day 5: Integration, Testing & Polish (Friday)

### Morning (4 hours)

#### Backend Developer 1 (BE1)
- [ ] **API Integration & Error Handling**
  - Review all endpoints for consistency with API contract
  - Add comprehensive error handling:
    - Validation errors (400)
    - Not found errors (404)
    - Authorization errors (401, 403)
    - Server errors (500)
  - Implement proper HTTP status codes
  - Add request validation middleware
  - Create error response schema per API spec

#### Backend Developer 2 (BE2)
- [ ] **Business Logic Refinement**
  - Implement discount system:
    - **Note:** Discount endpoints need to be added to `api_contracts.yaml`:
      - `GET /discounts` - Get discounts (filtered by BusinessId)
      - `POST /discounts` - Create discount
      - `PATCH /discounts/{discountId}` - Update discount
    - Discount application logic (Discount.Type: Percentage or FixedAmount)
    - Discount validation (ValidFrom, ValidTo, IsActive)
  - Enhance tax calculation:
    - Support multiple tax rates (Tax model supports multiple Tax records per Business)
    - Tax application per product/service (Tax.Rate percentage)
    - **Note:** Tax endpoints need to be added to `api_contracts.yaml`:
      - `GET /taxes` - Get tax rates
      - `POST /taxes` - Create tax rule
      - `PATCH /taxes/{taxId}` - Update tax rule
  - **Note:** Service charge calculation not in Order model - remove or add ServiceCharge field to Order model
  - **Note:** Business settings endpoints need to be added to `api_contracts.yaml` if required

#### Frontend Developer 1 (FE1)
- [ ] **Error Handling & User Feedback**
  - Add error handling throughout payment flows:
    - Network errors
    - Validation errors
    - Payment failures
  - Implement loading states:
    - Payment processing indicators
    - API call loading spinners
  - Add success notifications:
    - Payment confirmation
    - Order creation success
    - Appointment updates
  - Improve form validation and error messages

#### Frontend Developer 2 (FE2)
- [ ] **UI Polish & Responsiveness**
  - Review all UI components against mockups
  - Ensure consistent styling:
    - Color scheme
    - Typography
    - Spacing and layout
  - Add responsive design:
    - Mobile-friendly layouts
    - Tablet optimization
  - Improve accessibility:
    - Keyboard navigation
    - Screen reader support
    - Focus indicators

### Afternoon (4 hours)

#### Backend Developer 1 (BE1)
- [ ] **Database Optimization & Testing**
  - Add database indexes for performance:
    - Orders by BusinessId, Status
    - Appointments by Date, EmployeeId (User.Id), BusinessId
    - Payments by OrderId
    - Products by BusinessId, Available
    - Users by BusinessId, Role
  - Write integration tests for critical flows:
    - Order creation and payment
    - Appointment rescheduling
    - Split payments
  - Test concurrency scenarios:
    - Multiple simultaneous payments
    - Concurrent appointment modifications
  - Performance testing and optimization

#### Backend Developer 2 (BE2)
- [ ] **API Documentation & Deployment Prep**
  - Ensure OpenAPI/Swagger documentation is complete
  - Add API endpoint descriptions
  - Document request/response examples
  - Set up CORS properly for production
  - Configure environment variables
  - Prepare deployment configuration

#### Frontend Developer 1 (FE1)
- [ ] **End-to-End Testing**
  - Test complete payment flows:
    - Card payment end-to-end
    - Cash payment end-to-end
    - Gift card payment end-to-end
    - Split payment end-to-end
  - Test appointment flows:
    - Create appointment
    - Reschedule appointment
    - Cancel appointment
  - Fix any discovered bugs
  - Test error scenarios

#### Frontend Developer 2 (FE2)
- [ ] **Final Integration & Bug Fixes**
  - Integrate all components:
    - Ensure navigation works
    - Test routing between pages
    - Verify data flow between components
  - Fix cross-browser compatibility issues
  - Test on different screen sizes
  - Address any remaining UI/UX issues
  - Prepare for demo/presentation

### End of Day 5 Deliverables
- ✅ All features integrated and tested
- ✅ Error handling comprehensive
- ✅ UI polished and responsive
- ✅ API documentation complete
- ✅ System ready for demo

---

## Critical Path & Dependencies

### Day 1 Dependencies
- Database setup must complete before any API development
- Authentication must be ready before protected routes

### Day 2 Dependencies
- Order creation must be complete before payment processing
- Pricing service must be ready before payment calculations

### Day 3 Dependencies
- Core payment flow must work before split payments
- Gift card entity must exist before gift card UI

### Day 4 Dependencies
- Appointment CRUD must be ready before reschedule/cancel
- Order integration needed for prepaid appointments

### Day 5 Dependencies
- All previous days' work must be complete for integration

---

## Daily Standup Checklist

Each day, team should discuss:
1. **What was completed yesterday?**
2. **What will be worked on today?**
3. **Any blockers or dependencies?**
4. **API contract changes needed?**
5. **Integration points to coordinate?**

---

## Testing Strategy

### Backend Testing
- Unit tests for business logic (PricingService, PaymentService)
- Integration tests for API endpoints
- Database transaction tests

### Frontend Testing
- Component unit tests (critical components)
- Integration tests for payment flows
- Manual testing of all user flows

### End-to-End Testing
- Complete order → payment flow
- Appointment creation → reschedule → cancel flow
- Split payment scenarios

---

## Risk Mitigation

### High Priority Risks
1. **Stripe Integration Complexity**
   - Mitigation: Start early (Day 2), use Stripe test mode
   - Fallback: Mock payment gateway for MVP

2. **Split Payment Complexity**
   - Mitigation: Break into smaller tasks, test incrementally
   - Simplify: Support max 2-3 splits for MVP

3. **Time Slot Conflict Detection**
   - Mitigation: Implement early, add comprehensive tests
   - Use database constraints for data integrity

4. **Integration Issues**
   - Mitigation: Daily integration checks, API contract reviews
   - Maintain clear communication between frontend/backend

---

## Success Criteria

### Must Have (MVP)
- ✅ User authentication and authorization
- ✅ Order creation with items
- ✅ Payment by cash
- ✅ Payment by card (Stripe integration)
- ✅ Payment by gift card
- ✅ Split payment (basic)
- ✅ Appointment creation
- ✅ Appointment rescheduling
- ✅ Appointment cancellation
- ✅ Basic UI matching mockups

### Nice to Have (If Time Permits)
- Receipt generation/printing
- Advanced discount rules
- Email/SMS notifications
- Appointment reminders
- Advanced reporting
- Multi-business support (if not already implemented)

---

## Notes

- **API Contract:** All implementations must follow `api_contracts.yaml`
- **Design Reference:** UI should match mockups in PSD document sections 5.1 and 5.2
- **Business Flows:** Implementation should follow sequence diagrams in sections 2.1-2.5
- **Data Model:** Database schema must match ER diagram in section 4 (models exist: Business, User, Product, Service, Order, OrderItem, Appointment, Tax, Discount, Payment, GiftCard)
- **Architecture:** Follow package diagram structure in section 3

## API Contract Alignment Issues

The following endpoints/features are mentioned in this plan but need to be added to `api_contracts.yaml`:
1. **Authentication:** `/auth/login` endpoint
2. **Tax Management:** `GET /taxes`, `POST /taxes`, `PATCH /taxes/{taxId}`
3. **Discount Management:** `GET /discounts`, `POST /discounts`, `PATCH /discounts/{discountId}`
4. **Gift Card Management:** `GET /gift-cards/{code}`, `POST /gift-cards`, `PATCH /gift-cards/{code}`
5. **Service Management:** `GET /services`, `POST /services`, `PATCH /services/{serviceId}`
6. **Appointment Available Slots:** `GET /appointments/available-slots`
7. **Receipt Generation:** `GET /orders/{orderId}/receipt` (optional)
8. **Refund Processing:** `POST /orders/{orderId}/refund` (optional)
9. **Business Settings:** Business management endpoints (if required)

**Payment Method Enum:** API contract shows `[Cash, Card]` but Payment model supports `GiftCard` - update API contract enum to include `GiftCard`.

**Terminology:** API contract uses "Employee" but models use "User" - ensure consistency in API responses.

---

## Team Communication

### Recommended Tools
- **Daily Standups:** 15 minutes each morning
- **Slack/Discord:** For real-time communication
- **Git:** Feature branches, daily commits
- **API Contract:** Shared YAML file, update as needed

### Code Review
- All PRs require at least one reviewer
- Backend changes reviewed by other backend dev
- Frontend changes reviewed by other frontend dev
- Integration points reviewed by both teams

---

**End of Development Plan**

