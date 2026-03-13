import http from 'k6/http';
import {check} from 'k6';
import {sleep} from 'k6';
import {BASE_URL, validPaymentData, stages} from './config.js';

const stage = __ENV.STAGE || 'smoke';

export const options = {
    insecureSkipTLSVerify: true,
    stages: stages[stage],
    thresholds: {
        http_req_duration: ['p(95)<500'],
        http_req_failed: ['rate<0.01'],
    },
};

/**
 * Main test: Full flow - Create payment → Retrieve payment
 */
export default function () {
    // Step 1: Create a payment
    const createPayload = JSON.stringify(validPaymentData);

    const createRes = http.post(`${BASE_URL}/api/payments`, createPayload, {
        headers: {
            'Content-Type': 'application/json',
        },
        tags: {
            name: 'CreatePayment',
        },
    });

    // Check create response
    const createSuccess = check(createRes, {
        'POST: status is 201': (r) => r.status === 201,
        'POST: status is Authorized': (r) => {
            try {
                const body = JSON.parse(r.body);
                return body.status === 'Authorized';
            } catch {
                return false;
            }
        },
        'POST: has valid UUID': (r) => {
            try {
                const body = JSON.parse(r.body);
                return body.id && /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(body.id);
            } catch {
                return false;
            }
        },
    });

    if (!createSuccess) {
        console.error('Failed to create payment in full flow test');
        return;
    }

    // Extract payment ID from response
    let paymentId;
    try {
        const body = JSON.parse(createRes.body);
        paymentId = body.id;
    } catch {
        console.error('Failed to parse create payment response');
        return;
    }

    // Step 2: Small delay to simulate real user behavior
    sleep(0.5);

    // Step 3: Retrieve the created payment
    const getRes = http.get(`${BASE_URL}/api/payments/${paymentId}`, {
        tags: {
            name: 'GetPayment',
        },
    });

    // Check get response
    check(getRes, {
        'GET: status is 200': (r) => r.status === 200,
        'GET: response has all fields': (r) => {
            try {
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
            } catch {
                return false;
            }
        },
        'GET: retrieved ID matches created ID': (r) => {
            try {
                const body = JSON.parse(r.body);
                return body.id === paymentId;
            } catch {
                return false;
            }
        },
        'GET: status matches created status': (r) => {
            try {
                const createBody = JSON.parse(createRes.body);
                const getBody = JSON.parse(r.body);
                return getBody.status === createBody.status;
            } catch {
                return false;
            }
        },
    });
}
