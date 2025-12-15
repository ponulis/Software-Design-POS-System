import { useStripePayment } from "./useStripePayment";

export default function CardPaymentHandler({ children, onPaymentReady }) {
  const { confirmCardPayment, stripe, elements } = useStripePayment();

  // Expose the confirm function to parent via callback
  React.useEffect(() => {
    if (onPaymentReady && stripe && elements) {
      onPaymentReady({
        confirmPayment: async (clientSecret, orderId, paymentIntentId) => {
          return await confirmCardPayment(clientSecret, orderId, paymentIntentId);
        },
      });
    }
  }, [stripe, elements, confirmCardPayment, onPaymentReady]);

  return children;
}
