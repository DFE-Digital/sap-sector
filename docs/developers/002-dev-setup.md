# Development Setup

This document explains how to get the service running locally.

---

## Prerequisites

- .NET SDK (see repository configuration)
- Node.js (for frontend assets)
- Docker (if required for database/search)

---

## Initial setup

1. Clone the repository
2. Restore .NET dependencies
3. Install npm dependencies
4. Build frontend assets
5. Run the web application

---

## Environment configuration

- Configuration is environment-based
- Secrets must not be committed
- Use approved secret stores

---

## Common setup issues

- Frontend assets not built
- Database connection errors
- Search index not initialised

Check logs first, then this handbook, then ask the team.
