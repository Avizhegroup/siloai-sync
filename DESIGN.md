# SiloAI — Design System

**Version:** 1.0
**Scope:** `SiloAI.UI` (Blazor Server admin panel)
**Direction:** Light, clean, RTL/Farsi, Telerik-compatible

---

## 1. Principles

1. **Structural, not decorative.** SiloAI is an operations console for AI infrastructure (customers, API keys, RAG knowledge, chat). Surfaces stay flat and quiet; color is spent on meaning, not decoration.
2. **Two accents, two jobs.** Blue (`--silo-accent`) means *action / navigation / system*. Amber (`--silo-amber`) means *value* — specifically credit balances, since a silo's job is storing reserves. Nothing else uses amber. This keeps the palette legible instead of "colorful for its own sake."
3. **Data looks like data.** Anything machine-generated or precise — API key labels, IDs, dates, credit amounts, similarity scores — renders in monospace. Anything written in Farsi prose renders in the UI sans-serif. This distinction is the single biggest visual upgrade over the previous default-Bootstrap look.
4. **No card-kit sameness.** Stat blocks are one flat strip with hairline dividers, not three identical shadowed boxes. Tables are flat bordered containers, not cards-with-shadow-wrapping-a-table.
5. **RTL is the default, not a patch.** Sidebar on the right, active-state rail on the inline-start edge (`border-inline-start`), icons and spacing use logical properties so the system doesn't silently break if an LTR page is ever needed.

---

## 2. Design tokens

All tokens are CSS custom properties on `:root`, defined in `abstracts/_tokens.scss`. SCSS variables are not used for the new system — this makes the tokens visible/overridable in DevTools and avoids a second parallel color system.

### 2.1 Color

| Token | Value | Usage |
|---|---|---|
| `--silo-bg` | `#F6F7F9` | App background |
| `--silo-surface` | `#FFFFFF` | Cards, panels, sidebar, inputs |
| `--silo-border` | `#E3E6EB` | Default hairline border |
| `--silo-border-strong` | `#D3D8E0` | Input borders, emphasis dividers |
| `--silo-text` | `#12151C` | Primary text |
| `--silo-text-muted` | `#626B79` | Secondary text, labels |
| `--silo-text-faint` | `#98A1AF` | Placeholder, meta, disabled |
| `--silo-ink` | `#1E2A4A` | Brand mark, login background, headings accent |
| `--silo-accent` | `#2F5AA8` | Primary actions, links, active nav, focus ring |
| `--silo-accent-hover` | `#284C8F` | Hover state of the above |
| `--silo-accent-tint` | `#EAF0FA` | Active nav background, selected-row tint, user chat bubble |
| `--silo-amber` | `#C98A2C` | Credit balances, value figures **only** |
| `--silo-amber-tint` | `#FBF1DF` | "Expiring soon" pill background |
| `--silo-green` | `#1E8A5B` | Success, active status |
| `--silo-green-tint` | `#E6F5EE` | Active-status pill background |
| `--silo-red` | `#C23B3B` | Danger, revoked, zero/negative credit |
| `--silo-red-tint` | `#FBEAEA` | Revoked-status pill background |

**Rule:** never introduce a new hex value in a page or component file. If a shade is missing, add it here first.

### 2.2 Typography

| Token | Value |
|---|---|
| `--font-sans` | `'IRANSans', Tahoma, 'Segoe UI', sans-serif` |
| `--font-mono` | `ui-monospace, 'JetBrains Mono', 'Cascadia Code', Consolas, 'Courier New', monospace` |

IRANSans is already vendored (`abstracts/font.scss`) — no new font files required. `--font-mono` uses a system-stack fallback chain so it renders correctly with zero new assets; teams that want pixel-perfect JetBrains Mono can self-host it later and the token is the only place that needs to change.

Available IRANSans weights: `400` (regular), `500` (medium), `700` (bold), `900` (black). There is no `600` — don't reference it.

| Token | Size | Use |
|---|---|---|
| `--fs-xs` | `.76rem` | Meta text, pill labels, table meta |
| `--fs-sm` | `.82rem` | Secondary buttons, small labels |
| `--fs-base` | `.875rem` | Body text, table cells |
| `--fs-md` | `.95rem` | Panel titles, form labels |
| `--fs-lg` | `1.1rem` | Page subheads |
| `--fs-xl` | `1.3rem` | Page titles (`<h2>` topbar) |
| `--fs-2xl` | `1.7rem` | Stat-strip numbers |

### 2.3 Spacing

4px base unit: `--space-1: 4px` … `--space-8: 40px` (see token file for the full scale: 4/8/12/16/20/24/32/40).

### 2.4 Radius & shadow

| Token | Value | Use |
|---|---|---|
| `--radius-sm` | `6px` | Nav items, pills-adjacent controls |
| `--radius-md` | `10px` | Buttons, inputs, panels, stat strip |
| `--radius-lg` | `16px` | Login card |
| `--radius-pill` | `999px` | Status badges |
| `--shadow-sm` | `0 1px 2px rgba(18,21,28,.04)` | Rarely used — flat design avoids card shadows |
| `--shadow-md` | `0 8px 24px rgba(18,21,28,.08)` | Dropdowns, popovers, Telerik notification |
| `--shadow-login` | `0 20px 60px rgba(18,21,28,.25)` | Login card only |

---

## 3. Components

### 3.1 Sidebar (`.sidebar`)
Light surface, 1px `border-inline-start`, fixed, `240px`. Brand mark is a 3-bar "silo cross-section" glyph, not an icon font. Nav items (`.nav-item-ai`): muted text, `border-inline-start: 3px solid transparent`; `.active` gets `--silo-accent-tint` background, `--silo-accent` text/border, `font-weight:500`.

### 3.2 Topbar (`.topbar`)
No border, generous top padding, page `<h2>` + one-line muted description, primary action button trailing edge.

### 3.3 Stat strip (`.stat-card` container → new `.stat-strip` wrapper)
One `.stat-strip` panel, children `.stat-card` divided by `border-inline-start: 1px solid var(--silo-border)` (first child none). Numbers in `--font-mono`, `--fs-2xl`, `font-weight:700`. Default number color is `--silo-text`; use `.stat-card.accent`, `.stat-card.amber`, `.stat-card.red` modifiers to color specific figures (e.g., active keys = accent, revoked = red).

### 3.4 Panel / card (`.card-ai`)
Flat surface, 1px border, `--radius-md`, **no shadow**. Header (`.card-ai-header`): bottom border, `--fs-md` semibold-weight(700) title, optional trailing meta text in `--silo-text-faint`.

### 3.5 Table (Bootstrap `.table` inside `.table-responsive`)
Header row: `--silo-bg` background, `--fs-xs` `--silo-text-faint` text, bottom border. Body rows: hairline bottom border, no zebra striping, hover = `--silo-bg`. Any cell holding an ID/date/key/amount gets `--font-mono` via a `.mono` helper class (used inline where the razor markup already isolates that cell — e.g. wrap key labels).

### 3.6 Status badges (`.badge-active` / `.badge-expired` / `.badge-revoked`)
Pill (`--radius-pill`), tint background + solid text using the green/amber/red tint pairs above, small leading dot via `::before`. Replaces the previous bright flat `#dcfce7`/`#fee2e2` Tailwind-style colors with the token set so they match the rest of the palette exactly.

### 3.7 Buttons
- `.btn-primary` → solid `--silo-accent`, hover `--silo-accent-hover`, white text, `--radius-md`.
- `.btn-outline-primary/secondary/danger/info` → bordered, `--silo-border-strong`, colored text matching intent, fills on hover.
- `.btn-ai-primary` (login) → same primary treatment, full width.
- All buttons: `--radius-md`, no uppercase, no letter-spacing tricks.

### 3.8 Forms
`.form-control` / `.form-select`: `--silo-border-strong` border, `--radius-md`, focus = 2px `--silo-accent-tint` ring + `--silo-accent` border. Labels: `--fs-sm`, `font-weight:500`, `--silo-text-muted`.

### 3.9 Alerts
Re-skinned to tint/solid-text pairs matching the badge system (`alert-success` → green pair, `alert-danger` → red pair, `alert-warning` → amber pair) instead of Bootstrap defaults, so a success alert and an active-status badge read as the same "success" visually.

### 3.10 Chat (`.chat-row`, `.chat-bubble`, `.chat-citation`)
Bot bubble: surface + border. User bubble: `--silo-accent-tint` background. Citation badge: solid `--silo-ink` chip with the citation index in `--font-mono`. Similarity score and chunk index render in `--font-mono`, `--silo-text-faint`.

### 3.11 Login (`.login-bg`, `.login-card`)
Background gradient uses `--silo-ink` (was a near-identical dark gray before — now explicitly tied to the brand token instead of a one-off value). Card: white, `--radius-lg`, `--shadow-login`.

### 3.12 Telerik (`TelerikNotification`, future grids)
`.k-notification` re-skinned to the token palette (success/error/warning map to the green/red/amber pairs) so Telerik popups don't look like a foreign component. `.k-button` inherits `--font-sans`; a primary Telerik button maps to `--silo-accent`. This is a light touch — the app currently only uses `TelerikNotification` in production; extend `pages/components/_overrides.scss` if grids/dialogs are added later.

---

## 4. File architecture

```
wwwroot/styles/
├─ Site.scss                     entry point — import order matters
├─ abstracts/
│  ├─ _tokens.scss               ← NEW. Single source of truth for all tokens.
│  ├─ colors.scss                unchanged (legacy $variables, still used by legacy CSS in general.scss)
│  ├─ font.scss                  unchanged (IRANSans @font-face + inherited legacy utility/glyphicon CSS)
│  └─ measures.scss               unchanged
├─ libraries/
│  └─ bootstrap.rtl.css           unchanged
└─ pages/
   ├─ pages.scss                 unchanged (chat → layout → general, same as before)
   ├─ layout/
   │  └─ layout.scss             REWRITTEN — sidebar/topbar/stat-strip/card-ai/badges/login/buttons on tokens
   ├─ chat/
   │  └─ chat.scss               REWRITTEN — same tokens applied to chat bubbles/citations
   └─ general.scss               EDITED — see note below
```

### Import order (`Site.scss` → `pages/pages.scss`)

```
abstracts/font.scss
abstracts/_tokens.scss   ← new, loads before colors so tokens are available everywhere after
abstracts/colors.scss
abstracts/measures.scss
libraries/bootstrap.rtl.css
pages/chat/chat.scss
pages/layout/layout.scss
pages/general.scss       ← re-skin section appended at the end of this file (see below)
```

### How `general.scss` was edited

`general.scss` (978 lines) is largely dead CSS carried over from a different, older product in this codebase — classes like `.plaque-part`, `.rfid-conf`, `.truck-cross-config`, `.TagHistoryModalSummary` have nothing to do with SiloAI and don't appear in any `SiloAI.UI/Pages/*.razor` file. That legacy content is left alone — rewriting or deleting it is out of scope for a visual redesign and risks breaking something outside this audit's view (it may be shared with other apps in the same solution).

Two changes were made directly inside the file:

1. The existing `.btn-primary` rule (previously hardcoded to `#003e81`) was edited in place to use `var(--silo-accent)`, since it directly conflicted with the new palette and is actively used across SiloAI pages.
2. A new section, clearly marked `SiloAI Design System — re-skin section`, was appended to the **end** of the file. It re-skins the Bootstrap primitives SiloAI actually uses (`.btn-outline-*`, `.table`, `.form-control`, `.alert-*`, links, `.text-*`) and the Telerik notification component, using only the new tokens. Being appended last means it wins the cascade over any earlier same-specificity legacy rule, without needing to touch or audit that legacy rule itself.

Recommend a separate cleanup pass to delete the dead legacy code once it's confirmed unused elsewhere — not part of this change.

---

## 5. Do / Don't

- **Do** use `.mono` (new utility, defined in `_overrides.scss`) on any cell/value that is an ID, key label, date, timestamp, or currency amount.
- **Do** use the `accent` / `amber` / `red` modifier classes on `.stat-card` — don't invent new stat colors.
- **Don't** use amber for anything except credit/value figures — it will stop meaning "money" if it's reused as a generic warning color (use the existing `--silo-amber-tint` pill for "expiring" status, which is still value-adjacent).
- **Don't** add box-shadow to panels/cards; the system is intentionally flat. Shadow tokens exist only for floating elements (notifications, dropdowns) and the login card.
- **Don't** hardcode hex colors in page-level `.razor` `style="..."` attributes — every color need should be satisfiable by an existing class or token.
