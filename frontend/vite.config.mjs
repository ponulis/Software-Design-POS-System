import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    headers: {
      // More permissive CSP for development - allows backend API and Stripe
      'Content-Security-Policy': "script-src 'self' 'unsafe-eval' 'unsafe-inline' https://js.stripe.com https://r.stripe.com; connect-src 'self' http://localhost:5168 http://localhost:* ws://localhost:* https://api.stripe.com https://r.stripe.com; frame-src https://js.stripe.com https://hooks.stripe.com;"
    }
  }
});
