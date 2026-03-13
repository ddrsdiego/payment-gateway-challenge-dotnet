export const BASE_URL = __ENV.BASE_URL || 'http://localhost:5067';
export const options = {
  thresholds: {
    http_req_duration: ['p(95)<500'], // 95% of requests must complete below 500ms
    http_req_failed: ['rate<0.01'],   // Error rate must be below 1%
  },
};

export const stages = {
  smoke: [
    { duration: '30s', target: 1 },
  ],
  load: [
    { duration: '30s', target: 1 },
    { duration: '1m', target: 10 },
    { duration: '30s', target: 0 },
  ],
  stress: [
    { duration: '30s', target: 1 },
    { duration: '30s', target: 10 },
    { duration: '30s', target: 50 },
    { duration: '30s', target: 0 },
  ],
};

/**
 * Valid test data for payment processing
 */
export const validPaymentData = {
  cardNumber: '2222405343248877',
  expiryMonth: 4,
  expiryYear: 2027,
  currency: 'GBP',
  amount: 100,
  cvv: '123',
};

/**
 * Invalid test data for rejection scenarios
 */
export const invalidPaymentData = {
  cardNumber: '2222405343248877',
  expiryMonth: 12,
  expiryYear: 2020, // Expired card
  currency: 'GBP',
  amount: 100,
  cvv: '123',
};
