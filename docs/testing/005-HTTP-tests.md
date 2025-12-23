# HTTP and Integration Testing

HTTP and integration tests validate the application without browser automation.

## Scope

These tests verify:
- Controller endpoints return expected status codes
- Authorisation rules are enforced (School vs LA)
- Responses contain correct data
- Errors are handled safely without exposing sensitive information

## Data access testing

The application uses Dapper with a PostgreSQL database.

Dapper queries are tested against a test PostgreSQL database to ensure:
- Queries execute successfully
- Parameters are applied correctly
- Paging and filtering behave as expected
- Performance remains within acceptable limits

These tests help identify issues that unit tests cannot detect (for example slow queries or incorrect joins).
