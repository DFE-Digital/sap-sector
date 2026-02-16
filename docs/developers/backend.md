# Backend Development

---

## Application services

Services:
- represent user actions
- coordinate repositories and search
- enforce business rules

They must not:
- depend on MVC
- contain persistence logic

---

## PostgreSQL usage

Rules:
- async access only
- parameterised queries
- paging for lists and search
- small, explicit transactions
