# Performance Testing

Performance testing ensures the service remains responsive as data and usage grow.

## Application performance

Metrics monitored include:
- p50 and p95 response times
- Requests per second
- Error and exception rates
- CPU and memory usage per pod

## Database performance

The application uses PostgreSQL and is read-heavy. Data is populated by a separate data-load engine.

We monitor:
- Query execution times for key pages (compare, find school, change school)
- Index usage for common filters and joins
- Connection pool saturation
- Growth in data volume over time

Expected characteristics:
- Database size in the low GB range (validated by monitoring)
- Periodic growth after data refreshes
- Some queries returning large result sets for comparison views


## Performance thresholds

- Key queries exceeding 2 seconds are investigated
- Regressions over 20% are reviewed
- Paging and indexing are applied where needed

## Production monitoring

Performance in production is monitored continuously.
Only smoke tests are run in production; no load testing is performed there.
