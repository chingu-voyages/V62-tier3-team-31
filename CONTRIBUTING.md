# Contributing to V62 Tier 3 Team 31

Thanks for contributing to this ecommerce project. This repo contains a React storefront client and a .NET ecommerce API, so changes should stay focused, easy to review, and aligned with the current product/API contracts.

## Project Structure

- `client/` - React, Vite, TypeScript frontend.
- `server/` - .NET API, application contracts, core entities, infrastructure, and EF Core migrations.
- `docs/` - team planning and meeting documents.

## Development Workflow

Before starting work:

- Pick or create a GitHub issue for the task.
- Confirm the expected scope with the team when requirements are unclear.
- Create a branch from the latest `main`.
- Keep each branch focused on one feature, fix, refactor, or documentation update.
- Avoid mixing unrelated frontend, backend, and documentation changes in the same PR.

## Branch Naming

Use short, descriptive branch names:

```bash
feature/register-page
feature/product-listing
fix/auth-cookie-path
fix/product-stock-contract
refactor/storefront-sections
docs/update-contributing
chore/update-dependencies
```

## Commit Convention

Use clear conventional-style commits:

```bash
feat: add product listing page
fix: return exact product stock quantity
refactor: split storefront page sections
docs: update contribution guide
chore: update client dependencies
```

Avoid vague commits:

```bash
update stuff
fix things
final changes
```

## Pull Request Guidelines

Each PR should:

- Solve one clear problem.
- Keep the diff reasonably small.
- Explain what changed and why.
- Link related issues or tasks.
- Include screenshots for visible frontend changes.
- Mention any API contract changes.
- Avoid committing secrets, local config, build output, or unrelated files.

Suggested PR format:

```md
## Summary

Short explanation of the change.

## Changes

- Added...
- Updated...
- Refactored...

## Verification

- [ ] client: npm run lint
- [ ] client: npm run build
- [ ] server: dotnet build

## Notes

Anything reviewers should know.

## Linked Issues

Closes #issue-number
```

## Code Principles

### General

- Prefer readable code over clever code.
- Keep changes small and easy to review.
- Follow DRY, KISS, and YAGNI.
- Separate UI, business logic, API contracts, and persistence concerns.
- Do not introduce abstractions until they remove real duplication or clarify ownership.
- Preserve existing user or teammate work unless the task explicitly requires changing it.

### Frontend

- Keep React components focused.
- Move reusable behavior into hooks or helpers when it is shared or makes a component too large.
- Keep mock data shaped like the backend API contract.
- Derive display labels from data instead of storing duplicate state.
- Keep category, product, auth, and cart behavior separated where practical.
- Verify responsive layouts after visible UI changes.

Useful client commands:

```bash
cd client
npm run lint
npm run build
```

### Backend

- Keep controllers thin.
- Move reusable validation, mapping, token, and query behavior out of controllers.
- Keep API DTOs in the application layer.
- Keep EF Core persistence details in infrastructure.
- Keep core entities free from API-specific namespaces.
- Return API data honestly; avoid hiding or transforming domain values unless that is an explicit contract.

Useful server command:

```bash
cd server
dotnet build
```

## Before Submitting

Check that:

- The branch is up to date with `main`.
- The app builds successfully.
- Linting passes where applicable.
- No generated `dist/`, `bin/`, `obj/`, secrets, or local-only files are included.
- The PR description clearly explains the change.
- Any visual changes include screenshots or reviewer notes.

## Project Philosophy

This project should grow through small, well-scoped improvements. Favor clear boundaries, practical architecture, and honest API contracts over large rewrites or premature abstractions.
