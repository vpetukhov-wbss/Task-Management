---
paths:
  - "**/*.cs"
  - "**/*.razor"
  - "**/*.razor.cs"
  - "**/*.xaml.cs"
---
# C# async/await and bounded concurrency
- Read `knowledge/ASYNC_AWAIT_CONCURRENCY.md` for examples, exceptions, and official references before non-trivial async changes.
- Avoid sync-over-async (`.Result`, `.Wait`, blocking `GetResult`) on request/UI paths; use `Task`-returning APIs and `await` end-to-end.
- `async void` only for event signatures that demand it; no unobserved/unowned Tasks or request-scoped fire-and-forget jobs.
- Propagate cancellation to I/O; distinguish caller cancellation from timeout/transient fault and preserve committed side effects.
- Use `Task.WhenAll` only for independent, bounded work. Never parallelize access to a shared EF Core `DbContext`; add backpressure to bulk fan-out.
- Avoid `Task.Run` for already-async I/O or as an ASP.NET Core throughput workaround; choose background workers for owned long work.
- Don't await under lock/monitor; use async coordination with `try/finally` when needed.
- Use `ConfigureAwait(false)` purposefully in general-purpose libraries, **not** as a blanket app rule; MAUI/Blazor UI dispatch matters.
- `Task.Delay` is valid for cancellable waits, but isn't a durable scheduler; use background services/queues where warranted.
- Review cancellation, exceptions, resource lifetimes, concurrent updates and rollback behavior with tests; do not claim faster without measurement.
