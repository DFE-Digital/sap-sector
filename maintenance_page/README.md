## Maintenance Page

This directory contains a standalone static maintenance page that can be deployed
as a separate Kubernetes service and used as the ingress failover target during
data pipeline runs.

Expected service name:

- `get-school-improvement-insights-maintenance`

Build and push example:

```bash
docker build -f maintenance_page/Dockerfile -t ghcr.io/dfe-digital/sap-sector-maintenance:latest .
docker push ghcr.io/dfe-digital/sap-sector-maintenance:latest
```

Apply manifests example:

```bash
kubectl -n <namespace> apply -f maintenance_page/manifests/
```
