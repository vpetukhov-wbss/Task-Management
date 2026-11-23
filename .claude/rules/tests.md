---
paths:
  - "tests/**"
  - "**/*Tests.cs"
  - "**/*Test.cs"
---
# Testing rules
- Test business behavior and invariants, including edge, failure, concurrency, cancellation, tenancy and authorization.
- Real external dependencies with disposable containers when valuable; never point test suites at production resources.
- Unit tests quick and deterministic; integration tests isolate mutable state and data; avoid global mutable environment variables when parallelizing.
- No brittle sleeps; use synchronization/Eventually patterns with bounded timeouts for async processing.
- Test fixtures are explicit and readable; don't mock EF LINQ queries where actual relational translation is the risk.
