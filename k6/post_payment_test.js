import http from 'k6/http';
import {check} from 'k6';
import {BASE_URL, validPaymentData, invalidPaymentData} from './config.js';

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
 * Test: Create payment with valid data (Authorized)
 */
export function testCreatePaymentSuccess() {
    const payload = JSON.stringify(validPaymentData);

    const res = http.post(`${BASE_URL}/api/payments`, payload, {
        headers: {
            'Content-Type': 'application/json',
        },
    });

    check(res, {
        'POST success: status is 201': (r) => r.status === 201,
        'POST success: status is Authorized': (r) => {
            const body = JSON.parse(r.body);
            return body.status === 'Authorized';
        },
        'POST success: has valid ID': (r) => {
            const body = JSON.parse(r.body);
            return body.id && /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(body.id);
        },
        'POST success: response has all fields': (r) => {
            const body = JSON.parse(r.body);
            return (
                body.id &&
                body.status &&
                body.cardNumberLastFour === 8877 &&
                body.expiryMonth === 4 &&
                body.expiryYear === 2027 &&
                body.currency === 'GBP' &&
                body.amount === 100
            );
        },
    });
}

/**
 * Test: Create payment with invalid data (Rejected)
 */
export function testCreatePaymentRejected() {
    const payload = JSON.stringify(invalidPaymentData);

    const res = http.post(`${BASE_URL}/api/payments`, payload, {
        headers: {
            'Content-Type': 'application/json',
        },
    });

    check(res, {
        'POST rejected: status is 400': (r) => r.status === 400,
    });
}

/**
 * Main execution flow
 */
export default function () {
    // 50% test authorized payments
    if (Math.random() < 0.5) {
        testCreatePaymentSuccess();
    } else {
        // 50% test rejected payments
        testCreatePaymentRejected();
    }
}
