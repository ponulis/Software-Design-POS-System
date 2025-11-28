import React from 'react';
import ReactDOM from 'react-dom/client';
import './index.css';
import App from './App';

// Optional: remove CRA's reportWebVitals if you don't need it
// import reportWebVitals from './reportWebVitals';

const root = ReactDOM.createRoot(document.getElementById('root'));
root.render(
  <React.StrictMode>
    <App />
  </React.StrictMode>
);

