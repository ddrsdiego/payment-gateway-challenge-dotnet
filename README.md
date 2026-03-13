# Instructions for candidates

This is the .NET version of the Payment Gateway challenge. If you haven't already read this [README.md](https://github.com/cko-recruitment/) on the details of this exercise, please do so now. 

## Template structure
```
src/
    PaymentGateway.Api - a skeleton ASP.NET Core Web API
test/
    PaymentGateway.Api.Tests - an empty xUnit test project
imposters/ - contains the bank simulator configuration. Don't change this

.editorconfig - don't change this. It ensures a consistent set of rules for submissions when reformatting code
docker-compose.yml - configures the bank simulator
PaymentGateway.sln
```

Feel free to change the structure of the solution, use a different test library etc.

## Resiliency with Polly

The Payment Gateway API includes automatic retry mechanisms for external Bank Simulator API calls using **Polly**.

### Configuration

Retry settings are externalized via `appsettings.json`:

```json
{
  "BankSimulator": {
    "BaseUrl": "http://localhost:8080",
    "Retry": {
      "RetryCount": 3,
      "DelayInSeconds": 1
    }
  }
}
```

**Parameters:**
- `RetryCount`: Number of retry attempts (default: 3)
- `DelayInSeconds`: Initial delay between retries in seconds (default: 1)
- Delays use exponential backoff: `DelayInSeconds × 2^(retryAttempt-1)`
  - 1st retry: 1s
  - 2nd retry: 2s
  - 3rd retry: 4s

### How It Works

1. **Retry Triggers:** HttpRequestExceptions and HTTP 5xx/408 status codes
2. **Exponential Backoff:** Delays increase exponentially to avoid overwhelming the service
3. **Transparent:** No changes needed in `BankSimulatorClient`—Polly handles retries at the HttpClient level
4. **Configuration-Driven:** Adjust retry behavior without redeploying

### Implementation Details

- **File:** `src/PaymentGateway.Api/Settings/BankSimulatorOptions.cs` — Configuration POCOs
- **File:** `src/PaymentGateway.Api/Program.cs` — Polly policy registration
- **Package:** `Microsoft.Extensions.Http.Polly` v8.0.0

### Performance

The `BankSimulatorClient` uses cached `JsonSerializerOptions` to minimize allocations and comply with code analyzer CA1869 recommendations.

---

## 🧪 Integration Tests with K6

The project includes integration and load testing scripts using **K6** for end-to-end API validation and performance measurement.

### Prerequisites

1. **K6 installed:** https://k6.io/docs/get-started/installation/
2. **API running:** `dotnet run --project src/PaymentGateway.Api`
3. **Bank Simulator running:** `docker-compose up` (port 8080)

### Test Scripts

```
k6/
├── config.js              # Shared configuration (base URL, thresholds, stages)
├── post_payment_test.js    # Functional tests for POST /api/payments
├── get_payment_test.js     # Functional tests for GET /api/payments/{id}
└── full_flow_test.js       # End-to-end flow: POST → GET with load progression
```

### Running Tests

#### 1️⃣ POST Endpoint Tests (Functional)

```bash
k6 run k6/post_payment_test.js
```

**Coverage:**
- Authorized payments (status 200, `status = "Authorized"`)
- Rejected payments (status 400, `status = "Rejected"`)
- Response field validation

---

#### 2️⃣ GET Endpoint Tests (Functional)

```bash
k6 run k6/get_payment_test.js
```

**Coverage:**
- Successful retrieval (status 200)
- Not found scenario (status 404)
- Data consistency validation

---

#### 3️⃣ Full Flow Tests (Integration + Performance)

```bash
# Smoke test (default - 1 VU for 30s)
k6 run k6/full_flow_test.js

# Load test (10 VUs progressive for 1m)
k6 run -e STAGE=load k6/full_flow_test.js

# Stress test (up to 50 VUs progressive)
k6 run -e STAGE=stress k6/full_flow_test.js
```

**Coverage:**
- Complete flow: POST → GET chained
- Progressive load stages
- Performance under various volumes

---

### Environment Variables

| Variable | Default | Usage |
|----------|---------|-------|
| `BASE_URL` | `http://localhost:5067` | API base URL |
| `STAGE` | `smoke` | Test stage: `smoke`, `load`, `stress` |

**Example:**

```bash
k6 run -e BASE_URL=http://localhost:5067 -e STAGE=load k6/full_flow_test.js
```

---

### Performance Thresholds

All tests validate:

```javascript
http_req_duration: ['p(95)<500']   // 95% of requests complete within 500ms
http_req_failed: ['rate<0.01']     // Error rate below 1%
```

Tests fail if thresholds are violated, providing performance insights.

---

### Test Scenarios

| Script | Scenario | Expected Status | Assertions |
|--------|----------|-----------------|-----------|
| `post_payment_test.js` | Valid payment | 200 | Authorized, all fields present |
| `post_payment_test.js` | Invalid data | 400 | Rejected status |
| `get_payment_test.js` | Existing ID | 200 | All fields present and valid |
| `get_payment_test.js` | Missing ID | 404 | Error response |
| `full_flow_test.js` | POST → GET | 200/200 | ID consistency across requests |

---

## Design Decisions & Assumptions

### Architecture
- **CQRS with MediatR** — Separates read (`GetPayment`) and write (`ProcessPayment`) concerns
- **In-memory storage** — `ConcurrentDictionary<Guid, Payment>` keyed by payment ID guarantees O(1) lookups on `GET /payments/{id}` and thread-safe concurrent writes. Chosen for simplicity given the exercise scope; data does not persist across restarts. In production, this would be replaced by a database with an index on the payment ID.
- **Value Objects** — `Money` and `CardLastFourDigits` encapsulate validation and domain rules
- **Railway-oriented programming** — `CSharpFunctionalExtensions` (`Result`, `Maybe`) for explicit error handling without exceptions as control flow
- **Polly retry** — Exponential backoff (3 retries) for transient bank simulator failures
- **Error handling strategy** — Errors are categorised by intent:
  - `400 Bad Request` — invalid client input (card number, expiry, CVV, amount, currency)
  - `404 Not Found` — payment ID does not exist
  - `500 Internal Server Error` — infrastructure failures (bank simulator unavailable, unexpected exceptions). Note: a `4xx` response from the bank simulator is also surfaced as `500` since it indicates a contract mismatch between gateway and bank, not a client error.

### Assumptions
- Amount is in **minor currency units** (e.g., `100` = £1.00)
- Supported currencies: **USD, EUR, GBP** only
- Card number must be **14–19 numeric digits**; only the last 4 digits are stored
- CVV must be **3–4 numeric digits**
- **Declined payments are stored** — retrievable via GET for audit purposes
- Payment ID is **server-generated** (UUID); clients cannot supply their own
- Current expiry month is **valid**; past months of the current year are rejected
