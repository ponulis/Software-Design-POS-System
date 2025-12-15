# Features To-Do List
## Remaining Features to Implement (Based on DEVELOPMENT_PLAN.md)

**Last Updated:** Based on DEVELOPMENT_PLAN.md comparison  
**Backend Progress:** ~82% complete (32 features implemented)  
**Frontend Progress:** ~30% complete (UI structure exists, needs API integration)

**Note:** This list only includes features explicitly mentioned in DEVELOPMENT_PLAN.md

---

## 🔴 Critical Priority (Must Have for MVP)

### Backend Features

1. **API Integration & Error Handling** (Day 5 - BE1)
   - [ ] Review all endpoints for consistency with API contract
   - [ ] Add comprehensive error handling:
     - [ ] Validation errors (400)
     - [ ] Not found errors (404)
     - [ ] Authorization errors (401, 403)
     - [ ] Server errors (500)
   - [ ] Implement proper HTTP status codes
   - [ ] Add request validation middleware
   - [ ] Create error response schema per API spec

2. **Database Optimization & Testing** (Day 5 - BE1)
   - [ ] Add database indexes for performance:
     - [ ] Orders by BusinessId, Status ✅ (already done)
     - [ ] Appointments by Date, EmployeeId (User.Id), BusinessId ✅ (already done)
     - [ ] Payments by OrderId ✅ (already done)
     - [ ] Products by BusinessId, Available ✅ (already done)
     - [ ] Users by BusinessId, Role ✅ (already done)
   - [ ] Write integration tests for critical flows:
     - [ ] Order creation and payment
     - [ ] Appointment rescheduling
     - [ ] Split payments
   - [ ] Test concurrency scenarios:
     - [ ] Multiple simultaneous payments
     - [ ] Concurrent appointment modifications
   - [ ] Performance testing and optimization

3. **API Documentation & Deployment Prep** (Day 5 - BE2)
   - [ ] Ensure OpenAPI/Swagger documentation is complete
   - [ ] Add API endpoint descriptions
   - [ ] Document request/response examples
   - [ ] Set up CORS properly for production
   - [ ] Configure environment variables
   - [ ] Prepare deployment configuration

### Frontend Features

4. **Project Setup & Authentication UI** (Day 1 - FE1)
   - [ ] Set up API client/axios configuration with base URL
   - [ ] Create authentication context/hooks (`useAuth.jsx`)
   - [ ] Build login page component
   - [ ] Implement token storage (localStorage/sessionStorage)
   - [ ] Add protected route wrapper component
   - [ ] Update App.jsx with authentication routing

5. **Core Layout & Navigation** (Day 1 - FE2)
   - [ ] Review and enhance Navbar component
   - [ ] Create main layout wrapper with sidebar/navigation
   - [ ] Set up routing structure for all pages:
     - [ ] `/payments` - Payment processing
     - [ ] `/reservations` - Appointment management
     - [ ] `/catalog-products` - Product catalog
     - [ ] `/taxes-and-service-charges` - Tax configuration
     - [ ] `/users-and-roles` - User management
     - [ ] `/settings` - Business settings
   - [ ] Add loading states and error boundaries

6. **Order Creation UI** (Day 1 - FE1)
   - [ ] Create order creation page/component
   - [ ] Build product selection interface (grid/list view)
   - [ ] Implement cart functionality:
     - [ ] Add/remove items
     - [ ] Quantity adjustment
     - [ ] Item notes
   - [ ] Display order summary (subtotal calculation)
   - [ ] Connect to backend `/orders` API

7. **Product Catalog UI** (Day 1 - FE2)
   - [ ] Build product catalog page (`CatalogProducts.jsx`)
   - [ ] Create product card/list components
   - [ ] Implement product search and filtering
   - [ ] Add product CRUD forms (create/edit modal)
   - [ ] Connect to `/menu-items` API endpoints
   - [ ] Display product availability status

8. **Payment UI - Order Details Display** (Day 2 - FE1)
   - [ ] Enhance `OrderDetails.jsx` component:
     - [ ] Display item list with quantities, unit prices, total prices
     - [ ] Show subtotal (Order.SubTotal), taxes (Order.Tax), discounts (Order.Discount)
     - [ ] Display final total (Order.Total)
     - [ ] **Note:** Service charges not in Order model - remove if not needed
   - [ ] Create `SummaryRow.jsx` component for totals
   - [ ] Create `OrderRow.jsx` component for individual items
   - [ ] Implement real-time calculation updates

9. **Payment Selection UI** (Day 2 - FE2)
   - [ ] Create payment method selection component
   - [ ] Build payment button component (`PaymentButton.jsx`)
   - [ ] Create `CheckoutDetails.jsx` wrapper component
   - [ ] Implement payment method switching logic
   - [ ] Add cancel payment functionality

10. **Cash Payment UI** (Day 2 - FE1)
    - [ ] Build `CashCheckout.jsx` component per mockup:
      - [ ] Cash received input field
      - [ ] Change calculation display
      - [ ] Handle exact payment, overpayment, insufficient cash scenarios
      - [ ] Display change amount prominently
    - [ ] Integrate with `POST /payments` endpoint (include OrderId in request body)
    - [ ] Add validation and error handling

11. **Card Payment UI** (Day 2 - FE2)
    - [ ] Build `CardCheckout.jsx` component per mockup:
      - [ ] Card details form (using Stripe Elements or similar)
      - [ ] Payment processing UI
      - [ ] Loading states during authorization
    - [ ] Integrate Stripe client SDK (or payment gateway SDK)
    - [ ] Implement card tokenization (card details never reach backend)
    - [ ] Connect to payment endpoint
    - [ ] Handle payment success/failure scenarios

12. **Gift Card Payment UI** (Day 3 - FE1)
    - [ ] Build `GiftCardCheckout.jsx` component per mockup:
      - [ ] Gift card code input field
      - [ ] Display gift card balance
      - [ ] Show gift card information
      - [ ] Handle insufficient balance scenario
      - [ ] Display remaining balance after payment
    - [ ] Integrate with gift card validation API (`GET /gift-cards/{code}`)
    - [ ] Connect to `POST /payments` endpoint with method="GiftCard"
    - [ ] Add validation and error messages

13. **Split Payment UI Foundation** (Day 3 - FE2)
    - [ ] Create `SplitPayment.jsx` component per mockup:
      - [ ] Add multiple clients functionality
      - [ ] Item assignment interface (assign items to clients)
      - [ ] Individual amount calculation per client
      - [ ] Display each client's total
    - [ ] Implement split calculation logic:
      - [ ] Distribute items across clients
      - [ ] Calculate individual totals (with taxes/discounts)
      - [ ] Handle partial payments

14. **Split Payment UI - Payment Processing** (Day 3 - FE1)
    - [ ] Enhance split payment component:
      - [ ] Process payments for each client separately
      - [ ] Support different payment methods per client (card, cash, gift card)
      - [ ] Show payment status per client
      - [ ] Display overall order completion status
    - [ ] Integrate with split payment API
    - [ ] Add progress indicator for multi-step payment

15. **Payment Flow Integration & Testing** (Day 3 - FE2)
    - [ ] Integrate all payment methods into unified flow:
      - [ ] Update `Payments.jsx` page
      - [ ] Connect `CheckoutDetails.jsx` to all payment components
      - [ ] Implement payment method switching
    - [ ] Add payment success/failure handling:
      - [ ] Success confirmation modal
      - [ ] Receipt display/printing
      - [ ] Error messages and retry logic
    - [ ] Test end-to-end payment flows

16. **Appointments Console UI** (Day 4 - FE1)
    - [ ] Enhance `AppointmentsList.jsx` per mockup:
      - [ ] Display daily schedule view
      - [ ] Show appointment time slots
      - [ ] Color-code by status
      - [ ] Click to view details
    - [ ] Build `AppointmentDetails.jsx` component:
      - [ ] Display customer information (Appointment.CustomerName, CustomerPhone)
      - [ ] Show assigned employee (Appointment.EmployeeId → User.Name)
      - [ ] Display service details (Appointment.ServiceId → Service.Name, DurationMinutes)
      - [ ] Show associated order (Appointment.OrderId → Order details)
      - [ ] Display payment status (via Order.Status if OrderId exists)
      - [ ] Show appointment status (Appointment.Status)
      - [ ] Display activity log (Appointment.CreatedAt, UpdatedAt)
    - [ ] Add reschedule and cancel buttons

17. **Reschedule Appointment UI** (Day 4 - FE2)
    - [ ] Enhance `RescheduleAppointmentModal.jsx` per mockup:
      - [ ] Display current appointment details
      - [ ] Show available time slots
      - [ ] Calendar/date picker
      - [ ] Time slot selection
      - [ ] Service duration display
      - [ ] Confirm/cancel buttons
    - [ ] Integrate with available slots API
    - [ ] Implement time slot selection logic
    - [ ] Connect to reschedule endpoint

18. **Cancel Appointment UI** (Day 4 - FE1)
    - [ ] Build cancel appointment modal per mockup:
      - [ ] Confirmation dialog
      - [ ] Display customer notification info
      - [ ] Show refund information (if prepaid)
      - [ ] Cancel/confirm buttons
    - [ ] Integrate with cancellation endpoint
    - [ ] Add success/error handling
    - [ ] Update appointments list after cancellation

19. **Add Appointment UI Enhancement** (Day 4 - FE2)
    - [ ] Enhance `AddAppointmentModal.jsx`:
      - [ ] Customer information form (Appointment.CustomerName, CustomerPhone)
      - [ ] Employee selection dropdown (fetch from `/employees` endpoint, map to Appointment.EmployeeId)
      - [ ] Service selection (fetch services, map to Appointment.ServiceId)
      - [ ] Date and time picker (Appointment.Date)
      - [ ] Duration display (from Service.DurationMinutes)
      - [ ] Prepaid option toggle (if selected, create Order and set Appointment.OrderId)
      - [ ] Notes field (Appointment.Notes)
    - [ ] Integrate with `POST /appointments` API
    - [ ] Add validation (required fields, time conflicts)
    - [ ] Implement time slot availability checking (use available-slots endpoint)

20. **Error Handling & User Feedback** (Day 5 - FE1)
    - [ ] Add error handling throughout payment flows:
      - [ ] Network errors
      - [ ] Validation errors
      - [ ] Payment failures
    - [ ] Implement loading states:
      - [ ] Payment processing indicators
      - [ ] API call loading spinners
    - [ ] Add success notifications:
      - [ ] Payment confirmation
      - [ ] Order creation success
      - [ ] Appointment updates
    - [ ] Improve form validation and error messages

21. **UI Polish & Responsiveness** (Day 5 - FE2)
    - [ ] Review all UI components against mockups
    - [ ] Ensure consistent styling:
      - [ ] Color scheme
      - [ ] Typography
      - [ ] Spacing and layout
    - [ ] Add responsive design:
      - [ ] Mobile-friendly layouts
      - [ ] Tablet optimization
    - [ ] Improve accessibility:
      - [ ] Keyboard navigation
      - [ ] Screen reader support
      - [ ] Focus indicators

22. **End-to-End Testing** (Day 5 - FE1)
    - [ ] Test complete payment flows:
      - [ ] Card payment end-to-end
      - [ ] Cash payment end-to-end
      - [ ] Gift card payment end-to-end
      - [ ] Split payment end-to-end
    - [ ] Test appointment flows:
      - [ ] Create appointment
      - [ ] Reschedule appointment
      - [ ] Cancel appointment
    - [ ] Fix any discovered bugs
    - [ ] Test error scenarios

23. **Final Integration & Bug Fixes** (Day 5 - FE2)
    - [ ] Integrate all components:
      - [ ] Ensure navigation works
      - [ ] Test routing between pages
      - [ ] Verify data flow between components
    - [ ] Fix cross-browser compatibility issues
    - [ ] Test on different screen sizes
    - [ ] Address any remaining UI/UX issues
    - [ ] Prepare for demo/presentation

---

## 🟡 High Priority (Nice to Have - From Development Plan)

### Backend Features

24. **Business Logic Refinement** (Day 5 - BE2)
    - [ ] Implement discount system:
      - [ ] Discount application logic (Discount.Type: Percentage or FixedAmount)
      - [ ] Discount validation (ValidFrom, ValidTo, IsActive)
    - [ ] Enhance tax calculation:
      - [ ] Support multiple tax rates (Tax model supports multiple Tax records per Business)
      - [ ] Tax application per product/service (Tax.Rate percentage)
    - [ ] **Note:** Service charge calculation not in Order model - remove or add ServiceCharge field to Order model

### Frontend Features

25. **Receipt Generation/Printing** (Day 3 - Mentioned in deliverables)
    - [ ] Implement receipt display/printing functionality
    - [ ] Connect to `/orders/{orderId}/receipt` endpoint
    - [ ] Add print preview
    - [ ] Support receipt formatting

---

## 📋 Testing & Quality Assurance (From Development Plan)

### Backend Testing (Day 5)

26. **Unit Tests**
    - [ ] Unit tests for business logic (PricingService, PaymentService)
    - [ ] Unit tests for API endpoints
    - [ ] Database transaction tests

### Frontend Testing (Day 5)

27. **Component Tests**
    - [ ] Component unit tests (critical components)
    - [ ] Integration tests for payment flows
    - [ ] Manual testing of all user flows

---

## Summary

**Total Features from Development Plan:** 27 features identified  
**Critical Priority:** 23 features (must have for MVP)  
**High Priority:** 2 features (nice to have)  
**Testing & QA:** 2 areas  

**Backend Status:**
- ✅ Database Setup & Entity Framework Configuration
- ✅ Authentication & Authorization Infrastructure
- ✅ Order Service - Core CRUD
- ✅ Product & Service Catalog APIs
- ✅ Pricing & Calculation Service
- ✅ Payment Service - Core Infrastructure
- ✅ Payment Endpoint - Order Payment Integration
- ✅ Cash Payment Flow
- ✅ Gift Card Service
- ✅ Card Payment Integration (Stripe)
- ✅ Split Payment Backend Logic
- ✅ Payment Processing Refinement
- ✅ Appointment Service - Core CRUD
- ✅ Appointment Reschedule Logic
- ✅ Appointment Cancellation & Refunds
- ✅ Appointment-Order Integration
- ✅ Business Logic Refinement (Discounts & Taxes)
- ✅ Database Optimization (Indexes)
- ⚠️ API Integration & Error Handling (partially done - GlobalExceptionHandlerMiddleware exists)
- ⚠️ API Documentation (Swagger exists but needs completion)
- ⚠️ Integration Tests (not started)
- ⚠️ Unit Tests (not started)

**Frontend Status:**
- ⚠️ Project Setup & Authentication UI (not started)
- ⚠️ Core Layout & Navigation (partial - Navbar exists)
- ⚠️ Order Creation UI (not started)
- ⚠️ Product Catalog UI (not started)
- ⚠️ Payment UI components (exist but need API integration)
- ⚠️ Appointment UI components (exist but need API integration)
- ⚠️ Error Handling & User Feedback (not started)
- ⚠️ UI Polish & Responsiveness (not started)
- ⚠️ End-to-End Testing (not started)
- ⚠️ Final Integration & Bug Fixes (not started)

**Estimated Completion:**
- Backend MVP: ~90% complete (mostly testing and documentation remaining)
- Frontend MVP: ~30% complete (UI structure exists, needs API integration)
- Full MVP: 1-2 weeks (focus on frontend integration)

---

## Notes

- This list is strictly based on DEVELOPMENT_PLAN.md
- Backend is mostly complete - focus should be on frontend integration
- Frontend components exist but need to be connected to backend APIs
- Testing is critical before production deployment
- All features align with the 1-week sprint plan structure
