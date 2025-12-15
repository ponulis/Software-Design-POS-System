import React, { useEffect, useRef } from 'react';
import ReceiptView from './ReceiptView';

export default function ReceiptPrint({ orderId, onClose }) {
  const printRef = useRef(null);

  useEffect(() => {
    const handlePrint = () => {
      window.print();
      if (onClose) {
        setTimeout(() => {
          onClose();
        }, 100);
      }
    };

    // Trigger print after a short delay to ensure content is loaded
    const timer = setTimeout(() => {
      handlePrint();
    }, 500);

    return () => clearTimeout(timer);
  }, [onClose]);

  return (
    <div className="hidden print:block">
      <style>{`
        @media print {
          body * {
            visibility: hidden;
          }
          .print\\:block,
          .print\\:block * {
            visibility: visible;
          }
          .print\\:block {
            position: absolute;
            left: 0;
            top: 0;
            width: 100%;
          }
          @page {
            margin: 0.5cm;
          }
          button {
            display: none !important;
          }
        }
      `}</style>
      <div ref={printRef} className="print:block">
        <ReceiptView orderId={orderId} />
      </div>
    </div>
  );
}
