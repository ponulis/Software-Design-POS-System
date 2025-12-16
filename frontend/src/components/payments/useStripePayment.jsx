import { useStripe, useElements } from '@stripe/react-stripe-js';
import { useCallback } from 'react';
import { stripeApi } from '../../api/stripe';
import { useToast } from '../../context/ToastContext';
import { getErrorMessage } from '../../utils/errorHandler';

export function useStripePayment() {
  const stripe = useStripe();
  const elements = useElements();
  const { error: showErrorToast, success: showSuccessToast } = useToast();

  const confirmCardPayment = useCallback(async (clientSecret, orderId, paymentIntentId) => {
    if (!stripe || !elements) {
      throw new Error('Stripe not initialized');
    }

    const cardElement = elements.getElement('card');
    if (!cardElement) {
      throw new Error('Card element not found');
    }

    // Confirm payment with Stripe
    const { error: stripeError, paymentIntent } = await stripe.confirmCardPayment(
      clientSecret,
      {
        payment_method: {
          card: cardElement,
        },
      }
    );

    if (stripeError) {
      throw new Error(stripeError.message || 'Payment failed');
    }

    if (paymentIntent.status !== 'succeeded') {
      throw new Error(`Payment status: ${paymentIntent.status}`);
    }

    // Call backend to create payment record
    const confirmResponse = await stripeApi.confirmPayment({
      orderId,
      paymentIntentId,
    });

    return confirmResponse.payment;
  }, [stripe, elements]);

  return {
    confirmCardPayment,
    stripe,
    elements,
  };
}
