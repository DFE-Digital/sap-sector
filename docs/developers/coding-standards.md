# Coding Standards (Practical)

This document defines **how developers should write code** in this repository.

---

## Core principles

- Readability over cleverness
- Small, focused classes
- Single responsibility
- Explicit dependencies
- Async all the way

---

## Controllers

Controllers must:
- validate input
- call application services
- return views or redirects

Controllers must not:
- contain business logic
- access the database
- build Lucene queries

Refactor when controllers grow large.

---

## Services

Services represent **use-cases**.

Rules:
- one service per use-case
- no MVC dependencies
- return application-level DTOs
- no direct database access

---

## Repositories

Repositories:
- encapsulate data access
- are async-only
- expose domain-focused methods

Avoid “god repositories”.

---

## Models

Do not mix concerns:
- Domain models → Core
- DTOs → Core
- ViewModels → Web
- Persistence models → Infrastructure
