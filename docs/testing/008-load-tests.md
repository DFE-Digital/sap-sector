# Load Testing

Load testing verifies that the service can handle expected and peak usage.

## Hosting model

The application runs in AKS:
- Behind a load balancer / ingress
- With health probes enabled
- Using horizontal pod autoscaling

## Expected usage

The service is not a public consumer application.
Typical usage is:
- Normal: 20–50 concurrent users
- Busy periods: 100–200 concurrent users
- Stress testing: up to 500 concurrent users

## Load scenarios

The following scenarios are tested:
1. Baseline load (50 users, 15 minutes)
2. Peak load (200 users, 30 minutes)
3. Stress load (up to 500 users)

User behaviour includes:
- Landing pages
- Compare school performance
- Charts and data-heavy pages
- School search and filtering
- Changing school context

## Success criteria

- p95 response time under 2 seconds
- Error rate below 1%
- No sustained resource exhaustion
- Autoscaling occurs without downtime
