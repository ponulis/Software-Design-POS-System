import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    headers: {
      // More permissive CSP for development - allows backend API
      'Content-Security-Policy': "script-src 'self' 'unsafe-eval' 'unsafe-inline'; connect-src 'self' http://localhost:5168 http://localhost:* ws://localhost:*;"
    }
  }
});
