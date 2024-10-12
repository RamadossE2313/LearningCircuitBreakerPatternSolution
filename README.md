## Circuit Breaker Pattern

The **Circuit Breaker** design pattern is used to prevent repeated failures when connecting to external services. It helps improve system resilience by stopping the flow of requests when failures are detected.

### States:
1. **Closed**: Requests flow normally until the failure threshold is reached.
2. **Open**: Requests are blocked temporarily after multiple failures to allow the external service time to recover.
3. **Half-Open**: A limited number of requests are allowed to check if the external service has recovered.

This pattern helps avoid overwhelming a failing service and improves overall system stability.


![Circuit Breaker Diagram](https://github.com/user-attachments/assets/33a011ea-4503-48c5-b7dd-0d5e59d32e12)

### States

![Circuit Breaker States](https://github.com/user-attachments/assets/f7f28c6b-547e-4ee6-a0a9-29fb849d8025)

**Circuit Breaker Explanation**:
- **Closed State**: All requests are allowed. The circuit is healthy.
- **Open State**: No requests are allowed. The circuit is broken due to multiple failures.
- **Half-Open State**: A few requests are allowed to test if the circuit can recover.

## Health Checks
https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
This application includes built-in health checks to monitor the status of various services, ensuring the application is running smoothly. Health checks are vital for production environments as they help identify issues early and maintain application reliability.

### Key Features

- **Custom Health Checks**: Monitor specific endpoints and services, such as the Weather Forecast API.
- **Azure Cosmos DB Health Check**: Verify the availability and responsiveness of the Azure Cosmos DB instance.
- **Health Check Endpoint**: Expose a `/healthz` endpoint that returns the status of all health checks in a standardized format.

### Usage

To access the health check information, simply send a request to the `/healthz` endpoint:

<img width="646" alt="image" src="https://github.com/user-attachments/assets/20ce1707-1ff0-4e9a-ab64-b8c42d39425d">



