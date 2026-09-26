# SiloAI Financial Ledger (امور مالی)

> **Audience:** an external agent or developer who has never seen this codebase.
> After reading this file you should understand what the financial feature does, why it is
> built the way it is, where every piece lives, and the invariants you must not break.
>
> **Source task:** [Issue #11 — SiloAI financial ledger](https://github.com/Avizhegroup/siloai-sync/issues/11)
> **Related team change:** PR #10 (`9-AddCoefficientToCost`) — per-customer `PriceMultiplier`.

---

## 1. The business problem

SiloAI sells AI usage (RAG chat, OCR, reports, page-agent) to customers. Every AI call costs
real money (the upstream model provider, e.g. Liara/OpenAI, bills per token). The business needs
to:

1. **Charge customers** for each AI call, in **Toman** (IRR), using a configurable formula.
2. **Never lose or double-count money.** A retried HTTP request, a timeout, or a concurrent
   duplicate must not charge a customer twice.
3. **Be auditable.** Every rial that moves must be traceable to a record. Historical charges
   must not change retroactively when the exchange rate or pricing changes.
4. **Support per-customer pricing.** Some customers pay a higher coefficient (markup) than others.

The core architectural rule is an **append-only ledger**:

> No single field like `RemainingCredit` is ever the source of truth by itself. Every movement
> of money inserts a new, immutable row in the transactions table. The account balance is a
> *cached aggregate* of those rows — it can always be recomputed as `SUM(transactions)`.

---

## 2. Currency & units (important — read first)

There are **two currencies** in play and mixing them up was a real bug:

| Concept | Currency | Where |
|---|---|---|
| `Customer.RemainingCredit` | **USD** | Legacy column; the Customers admin UI enters/displays it as dollars (`$`, label "اعتبار باقیمانده(دلار)") |
| `LedgerAccount.BalanceToman` | **Toman (IRR)** | The real, authoritative balance |
| `UsageRecord.CostUsd` / `FxRateUsed` / `ChargeToman` | mixed (snapshot) | Per-call audit record |

**Rule:** anything the admin types into the Customers page is **USD**. Anything the ledger stores
or the billing engine computes is **Toman**. Conversion between them always uses the active FX
rate (`FxRateSetting.TomanPerUsd`) at the moment of the operation — never a hard-coded number.

---

## 3. Domain model (SiloAI.Domains/Entities)

All entities follow the repo convention `[Table("tbl_X")]` + `[Column("fld_Y")]`.

### `LedgerAccount` — one per customer
| Column | Type | Notes |
|---|---|---|
| `fld_Id` | `Guid` | PK |
| `fld_CustomerId` | `int` | **unique index** → exactly 1:1 with `Customer` |
| `fld_BalanceToman` | `decimal(18,2)` | cached aggregate of transactions |
| `fld_RowVersion` | `rowversion` | optimistic concurrency token |
| `fld_CreatedAt` / `fld_UpdatedAt` | `datetime2` | |

An account is created **at the same moment the customer is created**, starting at `0`.

### `LedgerTransaction` — the append-only book of record
| Column | Type | Notes |
|---|---|---|
| `fld_Id` | `Guid` | PK |
| `fld_AccountId` | `Guid` | FK → `LedgerAccount` (Cascade) |
| `fld_Type` | `int` | `TopUp=1, Usage=2, Refund=3, Adjustment=4` |
| `fld_Amount` | `decimal(18,2)` | **positive = credit, negative = debit** |
| `fld_BalanceAfter` | `decimal(18,2)` | snapshot of balance right after this row |
| `fld_IdempotencyKey` | `nvarchar(200)` | **UNIQUE index** — the real guard against double-charging |
| `fld_UsageRecordId` | `Guid?` | FK → `UsageRecord` (**ON DELETE NO ACTION**, see §9) |
| `fld_Description` | `nvarchar(500)` | |
| `fld_CreatedAt` | `datetime2` | |

**No UPDATE or DELETE is ever allowed on this table.** Corrections are new rows (e.g. a
`Refund` or `Adjustment`). This is what makes the ledger auditable.

### `UsageRecord` — technical detail of one AI call
Captures the token counts and **snapshots** every pricing input at charge time so history is
immutable:
`InputTokens`, `CachedTokens`, `OutputTokens`, `CostUsd`, `FxRateUsed`, `MultiplierUsed`,
`FloorTomanUsed`, `ChargeToman`, plus `Feature`, `Model`, `ConversationId`, `CreatedAt`.

> **Why snapshots, not foreign keys:** if pricing or FX rates change later, past usage rows must
> not change. Snapshotting the inputs keeps old reports correct forever.

### `PricingSetting` / `FxRateSetting` — historical configuration
Both are **insert-only, time-versioned**. To change a price or rate you insert a *new* row with a
new `EffectiveFrom`; you never update an existing row. Reads always take:

```
WHERE EffectiveFrom <= now ORDER BY EffectiveFrom DESC LIMIT 1
```

- `PricingSetting`: `Feature`, `Multiplier` (M), `FloorToman`, `FloorUsd`, `EffectiveFrom`.
- `FxRateSetting`: `TomanPerUsd`, `EffectiveFrom`.

### Enums
```csharp
public enum LedgerTransactionType { TopUp = 1, Usage = 2, Refund = 3, Adjustment = 4 }
public enum UsageFeature { SupportChat = 1, Report = 2, PageAgent = 3, Ocr = 4 }
```

---

## 4. The pricing formula

Implemented by `IPricingEngine.CalculateAsync` → `PricingEngine`
(`SiloAI.Application.Api/Services/PricingEngine.cs`).

```
costUsd    = (inputTokens  - cachedTokens) × inputPricePerToken
           +  cachedTokens × cachedPricePerToken
           +  outputTokens × outputPricePerToken
costToman  = costUsd × fxRate
chargeToman = max( costToman × M ,  floorToman ,  fxRate × floorUsd )
```

- `M` = `PricingSetting.Multiplier` (the feature-level markup).
- The `max(...)` applies the business **floor**: a call is never cheaper than `floorToman` or
  `fxRate × floorUsd`, whichever is higher.
- Model token prices come from config `AiPricing:Models:{model}:InputPerMillionTokens` etc.
- **Fail-fast:** if no active FX rate or pricing row exists, the engine throws
  `InvalidOperationException` (a 500). There is deliberately **no silent fallback number** — a
  missing configuration is a deployment bug, not a runtime condition to paper over.

The result is a `ChargeResult` record carrying every value used, which is then snapshotted onto
the `UsageRecord`.

### Per-customer coefficient (`PriceMultiplier`) — team change, PR #10
On top of the feature-level `M`, each customer has a `PriceMultiplier` (default `1.0`, must be
≥ 1). It multiplies the raw USD cost of a call for *that* customer:

- `AiCostCalculator.Calculate(tokenUsage, priceMultiplier)` returns `priceUsage * priceMultiplier`.
- `ChatAgentService.SendWithAgentSessionAsync(sessionJson, query, priceMultiplier)` takes the
  multiplier and passes it to the calculator.
- `RagChatSendHandler` / `SendChatCommandHandler` resolve `customer?.PriceMultiplier ?? 1.0m` and
  pass it through.

> **⚠ Interaction you must know:** `PriceMultiplier` scales the **legacy USD `priceUsage`** path
> (the value still shown as `PriceUsage` on chat responses and used to decrement the legacy
> `RemainingCredit` column). The **ledger** path (`PricingEngine` → `ChargeToman`) computes its
> charge from the token counts and the *feature-level* `PricingSetting` and does **not**
> currently apply `PriceMultiplier`. See §10 "Known gap / follow-up".

---

## 5. The heart: atomic, idempotent charging

`ICreditLedgerService` → `CreditLedgerService`
(`SiloAI.Application.Api/Services/CreditLedgerService.cs`) is the **only** place balances change.
No handler may write `BalanceToman` directly.

```csharp
Task<ChargeOutcome> ChargeAsync(int customerId, ChargeResult charge, UsageRecord usageRecord,
                                string idempotencyKey, CancellationToken ct);
Task<ChargeOutcome> TopUpAsync(int customerId, decimal amountToman, string reference,
                                string? description, CancellationToken ct);
Task<decimal>       GetBalanceAsync(int customerId, CancellationToken ct);

public enum ChargeOutcome { Success, InsufficientBalance, DuplicateIgnored }
```

### `ChargeAsync` flow
1. **Fast idempotency check** — if a row already exists with this `IdempotencyKey`, return
   `DuplicateIgnored` before doing any work.
2. **Atomic conditional debit** (EF Core `ExecuteUpdateAsync`, one SQL statement, no
   read-then-write):
   ```sql
   UPDATE LedgerAccounts
   SET BalanceToman = BalanceToman - @amount, UpdatedAt = @now
   WHERE CustomerId = @id AND BalanceToman >= @amount
   ```
   If `rowsAffected == 0` → roll back → `InsufficientBalance`. The database row lock guarantees
   two concurrent requests can never both succeed when funds only cover one.
3. Insert the `UsageRecord` and the `LedgerTransaction` (negative `Amount`, `BalanceAfter`
   snapshot) **in the same DB transaction**.
4. On commit → `Success`. On a unique-constraint violation of `IdempotencyKey` (a race where a
   twin request committed first) → roll back → `DuplicateIgnored`.

The service runs in its **own `DbContext` scope** so the ledger transaction never accidentally
sweeps up unrelated entities tracked by a caller's context.

### `TopUpAsync`
Same pattern but credits the account. `reference` is the **payment-gateway transaction id** and
doubles as the idempotency key, so a top-up can never be recorded twice. Throws if the customer
has no ledger account.

### Invariant (acceptance criterion)
```
SUM(LedgerTransaction.Amount) for an account  ==  LedgerAccount.BalanceToman   (always)
```

---

## 6. Handler integration

All three AI handlers follow the same template: **pre-check → call AI → charge**.

| Handler | Feature | Idempotency key |
|---|---|---|
| `RagChatSendHandler` | `SupportChat` | `ragchat:{sessionId}:{turnIndex}` |
| `SendChatCommandHandler` (legacy chat) | `SupportChat` | `chat:{sessionId}:{turnIndex}` |
| `OcrCommandHandler` | `Ocr` | `ocr:{guid}` |

### The pre-flight check (`HasCreditAsync`)
**Every** handler now gates on the **ledger** balance via `ICreditLedgerService.GetBalanceAsync`
before calling the (paid) model:

```csharp
var balance = await ledgerService.GetBalanceAsync(customerId.Value, ct);
return balance > 0;
```

> **Why this matters (a real bug that was fixed):** the pre-check originally read the legacy
> `Customer.RemainingCredit` while charging read the ledger. A backfilled customer could have
> `RemainingCredit > 0` but `BalanceToman = 0` → pass the pre-check, pay OpenAI, then fail at
> charge time. Unifying both on the ledger removes that wasted-cost hole.

### Turn index
`AiChatSession.TurnIndex` counts user turns in a conversation. The idempotency key is built from
the *current* `TurnIndex + 1`, and `TurnIndex` is only advanced after a successful charge — so a
retried request for the same turn reproduces the same key and is dropped as `DuplicateIgnored`.

### Legacy credit cache
The handlers still decrement the legacy USD `Customer.RemainingCredit` for display continuity,
**converted back from the actual Toman charge** with the same snapshot rate
(`charge.ChargeToman / charge.FxRateUsed`) so the two columns stay consistent until the legacy
column is removed.

---

## 7. Customer create/update — currency conversion

`CreateCustomerCommandHandler` and `UpdateCustomerCommandHandler` treat the incoming
`RemainingCredit` as **USD** (that is what the UI collects) and mirror it into the ledger in
**Toman**:

- **Create:** converts `RemainingCredit(USD) × activeFxRate` and records it through
  `ICreditLedgerService.TopUpAsync` (key `customer-create:{id}`), so the very first balance is a
  real ledger row and `SUM(transactions) == balance` holds from the start. Fails fast if no FX
  rate is seeded.
- **Update:** computes the USD delta and appends an **`Adjustment`** ledger transaction for
  `delta(USD) × fxRate`, guarded so the balance can never go negative.

---

## 8. Admin API (SiloAI.Api/Controllers/FinancialController.cs)

Route prefix `admin/financial`, **JWT-only** (same as other admin controllers —
`[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]`).

| Method | Route | Purpose |
|---|---|---|
| GET | `accounts` | all ledger accounts + customer names |
| GET | `transactions?customerId=&take=` | recent ledger rows (default 100, max 500) |
| GET | `usage?customerId=&take=` | recent usage records |
| GET | `pricing` | all pricing-setting versions |
| POST | `pricing` | append a new `PricingSetting` (new `EffectiveFrom`) |
| GET | `fx-rates` | all FX-rate versions |
| POST | `fx-rates` | append a new `FxRateSetting` |
| POST | `topup` | credit an account; body = `TopUpLedgerCommand`. `409` if the `Reference` was already used. |

Customer management (with `PriceMultiplier` and USD credit) stays on `CustomersController`
(`admin/customers`).

---

## 9. EF Core migrations

Scaffolded per repo convention:
`dotnet ef migrations add <Name> --project SiloAI.Domains --startup-project SiloAI.Api`

1. **`20260923162712_AddFinancialLedger`** — creates the 5 tables, adds `AiChatSession.TurnIndex`,
   backfills one empty `LedgerAccount` per existing customer, and seeds **1 FX rate + 4 pricing
   rows** (one per feature) so the first AI call never fails on missing configuration.
   - The `LedgerTransaction → UsageRecord` FK is **`ON DELETE NO ACTION`** (`DeleteBehavior.Restrict`).
     SQL Server rejected `SetNull` here because it created a *second cascade path* from
     `tbl_AiCustomers` (`Customer→LedgerAccounts→LedgerTransactions` and
     `Customer→UsageRecords→LedgerTransactions`). `NO ACTION` is also semantically right: the
     ledger is immutable and a transaction row must never be silently rewritten when a usage
     record is deleted.
2. **`20260925180245_FixLedgerAccountCurrencyUnits`** — data-fix migration. The first migration
   backfilled accounts at `0` while the legacy USD column still held the real credit. This one
   rebases every account that has **no transactions yet** to `RemainingCredit(USD) × oldest FX
   rate`, recording each as a `TopUp` ledger row. Accounts that already have real activity are
   left untouched.
3. **`20260919110039_AddPriceMultiplierToCustomer`** *(team, PR #10)* — adds
   `Customer.PriceMultiplier decimal(18,2)` with default `1.0`.

---

## 10. Admin UI (SiloAI.UI)

### `/financial` page (`Pages/Financial.razor[.cs]`)
- `[Authorize]`, RTL/Farsi, uses the existing `card-ai` / `stat-card` design tokens.
- Telerik components: `TelerikTabStrip` with tabs **حساب‌ها** (accounts), **تراکنش‌ها**
  (transactions), **سوابق مصرف** (usage), **قیمت‌گذاری** (pricing), **نرخ ارز** (FX rates);
  `TelerikDropDownList` / `TelerikNumericTextBox` / `TelerikTextBox` / `TelerikButton` for the
  top-up and settings forms.
- Sidebar link **«امور مالی»** (`MainLayout.razor`) inside the `AuthorizeView`, so only logged-in
  admins see it.
- The top-up form requires a **reference** (gateway id) — enforced client-side and server-side.

### `/customers` page *(team, PR #10)*
Added a **«ضریب عددی»** (`PriceMultiplier`) column and input (create + inline edit), validated
`≥ 1.0`. Credit is still entered in USD.

---

## 11. Known gap / follow-up (be honest about this)

The two pricing paths are **not yet unified**:

- **Ledger charge** (`PricingEngine.ChargeToman`) uses the feature-level `PricingSetting.Multiplier`.
- **Legacy `priceUsage`** (`AiCostCalculator`) applies the per-customer `PriceMultiplier`.

So today a customer's `PriceMultiplier` affects the legacy `RemainingCredit` deduction and the
`PriceUsage` shown on chat responses, but **not** the authoritative `LedgerTransaction` amount.
If the business intends the per-customer coefficient to drive the *actual* ledger charge, the
`PriceMultiplier` must be folded into `IPricingEngine.CalculateAsync` (or `ChargeAsync`) for that
customer. This should be reconciled deliberately in a follow-up rather than by accident.

---

## 12. Quick reference — where everything lives

| Concern | File |
|---|---|
| Entities | `SiloAI.Domains/Entities/{LedgerAccount,LedgerTransaction,UsageRecord,PricingSetting,FxRateSetting,LedgerEnums}.cs`, `Customer.cs` (PriceMultiplier), `AiChatSession.cs` (TurnIndex) |
| EF config | `SiloAI.Domains/AiApiContext.cs` |
| Contracts / DTOs | `SiloAI.Application.Shared/Contracts/Financial/{IPricingEngine,ICreditLedgerService}.cs`, `Features/Financial/...` |
| Services | `SiloAI.Application.Api/Services/{PricingEngine,CreditLedgerService}.cs` |
| Handlers | `SiloAI.Application.Api/Features/{RagChat/Commands/Send,Chat/Commands/Send,Agent/Commands/Ocr,Customer/...}` |
| Cost calculator (PriceMultiplier) | `SiloAI.Agent/Billing/AiCostCalculator.cs`, `SiloAI.Agent/Chat/ChatAgentService.cs` |
| Admin API | `SiloAI.Api/Controllers/FinancialController.cs`, `CustomersController.cs` |
| UI | `SiloAI.UI/Pages/Financial.razor[.cs]`, `Pages/Customers.razor[.cs]`, `Shared/MainLayout.razor` |
| Migrations | `SiloAI.Domains/Migrations/2026*.cs` |

## 13. Golden invariants (do not break)

1. `LedgerTransaction` is **append-only** — never UPDATE/DELETE; correct via new rows.
2. `BalanceToman` changes **only** inside `CreditLedgerService` via the atomic conditional UPDATE.
3. Every balance change has a matching ledger row → `SUM(Amount) == BalanceToman`.
4. `IdempotencyKey` is a **DB-level UNIQUE** constraint; the in-code check is just a fast path.
5. Customers UI ↔ ledger is **USD ↔ Toman**; always convert with the active FX rate.
6. `PricingSetting`/`FxRateSetting` are **insert-only**, read as "latest `EffectiveFrom` ≤ now".
7. Usage records **snapshot** all pricing inputs; never FK to live settings.
8. Every paid AI handler **pre-checks the ledger balance** before calling the model.
