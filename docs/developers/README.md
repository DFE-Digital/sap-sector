# Developer Handbook

This folder is the **practical handbook for developers working on this service**.

It explains **how to work on the codebase day-to-day**:
- how the code is structured
- how to set up a local environment
- how to write, test, and review code
- how to follow agreed standards and workflows

This folder does **not** duplicate architectural decisions or strategy.
Those are documented separately.

---

## How to use this handbook

If you are new to the project:

1. Read **Overview**
2. Follow **Development setup**
3. Read **Coding standards**
4. Refer to other sections as needed

---

## Important distinction

- **This folder (`developers/`)**  
  → *How to work on the code*

- **Architecture & decisions**  
  → `/docs/architecture` and `/docs/adrs`

- **Security, testing, and operations (deep detail)**  
  → `/docs/security`, `/docs/testing`, `/docs/operational`

If something feels duplicated, the authoritative version lives **outside** this folder.

---

## Contents

### Getting started
- [Overview](001-overview.md)
- [Development setup](002-dev-setup.md)
- [Git workflow](003-git-flow.md)

### Writing code
- [Coding standards](coding-standards.md)
- [Project structure](project-structure.md)
- [Backend development](backend.md)
- [Frontend development](frontend.md)
- [Authentication](authentication.md)
- [Search (Lucene)](search-lucene.md)

### Quality & safety
- [Testing](testing.md)
- [Security for developers](security.md)

### Running locally
- [Local development](local-development.md)
