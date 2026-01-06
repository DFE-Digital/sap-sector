# Health Check Testing

Health checks ensure the application and its dependencies are running correctly.

## Health endpoints

The application exposes health endpoints used by the platform and pipelines:
- /health
- /health/live
- /health/ready

These endpoints confirm:
- The application has started successfully
- Required dependencies are reachable
- The service is ready to receive traffic

## Deployment checks

Health endpoints are used by:
- AKS liveness and readiness probes
- Deployment validation
- Autoscaling decisions

A deployment is considered successful only once the readiness check passes.
