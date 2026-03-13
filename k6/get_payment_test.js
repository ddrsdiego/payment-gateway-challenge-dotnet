import http from 'k6/http';
import {check} from 'k6';
import {BASE_URL, validPaymentData} from './config.js';

export const options = {
    insecureSkipTLSVerify: true,
    vus: 1,
    duration: '10s',
    thresholds: {
        http_req_duration: ['p(95)<500'],
        http_req_failed: ['rate<0.01'],
    },
};

/**
 * Helper: Create a payment and return the ID for testing
 */
function createPaymentAndGetId() {
    const payload = JSON.stringify(validPaymentData);

    const res = http.post(`${BASE_URL}/api/payments`, payload, {
        headers: {
            'Content-Type': 'application/json',
        },
    });

    if (res.status === 200) {
        const body = JSON.parse(res.body);
        return body.id;
    }

    return null;
}

/**
 * Test: Get payment with valid ID
 */
export function testGetPaymentSuccess() {
    // Create a payment first
    const paymentId = createPaymentAndGetId();

    if (!paymentId) {
        console.error('Failed to create payment for GET test');
        return;
    }

    const res = http.get(`${BASE_URL}/api/payments/${paymentId}`);

    check(res, {
        'GET success: status is 200': (r) => r.status === 200,
        'GET success: response has all fields': (r) => {
            const body = JSON.parse(r.body);
            return (
                body.id &&
                body.status &&
                body.cardNumberLastFour &&
                body.expiryMonth &&
                body.expiryYear &&
                body.currency &&
                body.amount
            );
        },
        'GET success: ID matches request': (r) => {
            const body = JSON.parse(r.body);
            return body.id === paymentId;
        },
        'GET success: status is Authorized or Declined': (r) => {
            const body = JSON.parse(r.body);
            return body.status === 'Authorized' || body.status === 'Declined';
        },
    });
}

/**
 * Test: Get payment with invalid ID (404)
 */
export function testGetPaymentNotFound() {
    const invalidId = '00000000-0000-0000-0000-000000000000';

    const res = http.get(`${BASE_URL}/api/payments/${invalidId}`);

    check(res, {
        'GET not found: status is 404': (r) => r.status === 404,
    });
}

/**
 * Main execution flow
 */
export default function () {
    // 80% test successful retrieval
    if (Math.random() < 0.8) {
        testGetPaymentSuccess();
    } else {
        // 20% test 404 scenarios
        testGetPaymentNotFound();
    }
}
