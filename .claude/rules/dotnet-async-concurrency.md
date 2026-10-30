---
paths:
  - "**/*.cs"
  - "**/*.csproj"
  - "**/*.razor"
  - "**/*.razor.cs"
  - "**/*.xaml.cs"
---
# Async/await & concurrency — .NET code review guardrails (2026-10-30)

This is a compact, scenario-sensitive project rule. The full reusable guide and review skill live in the public **AI Engineering Setups**:
- https://github.com/vpetukhov-wbss/ai-engineering-setups/blob/main/setups/dotnet/knowledge/ASYNC_AWAIT_CONCURRENCY.md (proposed in PR #2 until merged)
- https://github.com/vpetukhov-wbss/ai-engineering-setups/pull/2

## Required checks for changed async code
1. Avoid sync-over-async (`.Result`, `.Wait()`, blocking `GetAwaiter().GetResult()`) on HTTP/UI/worker hot paths. Prefer `await` through layers; avoid `async void` except required event signatures with explicit exception handling.
2. Every `Task` must have an owner: await it or use an explicitly managed lifecycle. Never start request-scoped fire-and-forget work that can outlive its DbContext/DI scope.
3. Propagate `CancellationToken` to cancellable I/O including EF Core, HTTP, streams, AI/model inference and background operations. Distinguish caller cancellation from timeout and transient faults; consider consistency if an irreversible operation has already committed.
4. Do not wrap asynchronous I/O in `Task.Run`. CPU-heavy work may justify offloading for a responsive desktop/mobile UI; a busy ASP.NET Core request normally needs a bounded background worker instead.
5. `Task.WhenAll` only for **independent and bounded** work. Avoid unbounded fan-out and never run concurrent EF Core operations on the same `DbContext`. Use explicit concurrency limits/backpressure according to measured DB/API/model capacity.
6. Avoid blocking on tasks while holding `lock`/monitor; use `SemaphoreSlim.WaitAsync` with `try/finally` or a queue/channel for truly async coordination. `await` in a C# `lock` is not permitted.
7. Observe failures and cancellation of all tasks, including when multiple `WhenAll` branches fault; don't silently swallow errors. Use `await using` for async-disposable owned resources.
8. `Task.Delay`/`PeriodicTimer` are valid for lightweight cancellable waits. For durable scheduled/business work use a hosted service with correct lifetime, or a persistent job/queue/scheduler when restart/retry guarantees are needed.
9. Do not mandate `ConfigureAwait(false)` in app code. Consider it for context-independent reusable libraries; preserve UI/renderer contexts in MAUI and Blazor.
10. Do not introduce `ValueTask`, additional concurrency frameworks, arbitrary timeouts or fixed fan-out numbers without evidence. Preserve existing architecture and quality gates.

## Review / proof
- Inspect actual call sites and identify I/O vs CPU, dependencies, tenant boundaries, shared mutable resources and operation idempotency.
- Add focused tests for cancellation, faults, resource disposal, concurrent operations, shutdown, rate limiting/backpressure and UI-context behavior when affected.
- Report measurements for performance claims. Do not mark build/tests as passed unless executed.

## Evidence
A related Aram Tchekrekjian public post lists async pitfalls; the user's specific new seven-point post was not fully available. These original rules are also grounded in Microsoft ASP.NET Core performance guidance and Stephen Toub's ConfigureAwait FAQ:
https://learn.microsoft.com/aspnet/core/performance/performance-best-practices
https://devblogs.microsoft.com/dotnet/configureawait-faq/
