# SiloAI — Design System

**Version:** 1.1
**Scope:** `SiloAI.UI` (Blazor Server admin panel)
**Direction:** Light, clean, RTL/Farsi, Telerik-compatible

---

## 1. Principles

1. **Structural, not decorative.** SiloAI is an operations console for AI infrastructure (customers, API keys, RAG knowledge, chat). Surfaces stay flat and quiet; color is spent on meaning, not decoration.
2. **Two accents, two jobs.** Blue (`--silo-accent`) means *action / navigation / system*. Amber (`--silo-amber`) means *value* — specifically credit balances, since a silo's job is storing reserves. Nothing else uses amber. This keeps the palette legible instead of "colorful for its own sake." Prices, rates, multipliers and floors (USD per million tokens, FX rate, M) are *data*, not credit: they render in mono, neutral color.
3. **Data looks like data.** Anything machine-generated or precise — API key labels, IDs, dates, credit amounts, similarity scores — renders in monospace. Anything written in Farsi prose renders in the UI sans-serif. This distinction is the single biggest visual upgrade over the previous default-Bootstrap look. Inline `<code>` (e.g. `X-Api-Key`) uses `--font-mono` too.
4. **No card-kit sameness.** Stat blocks are one flat strip with hairline dividers, not three identical shadowed boxes. Tables are flat bordered containers, not cards-with-shadow-wrapping-a-table.
5. **RTL is the default, not a patch.** Drawer on the right, active-state rail on the inline-start edge (`border-inline-start`), icons and spacing use logical properties so the system doesn't silently break if an LTR page is ever needed.

---

## 2. Design tokens

All tokens are CSS custom properties on `:root`, defined in `abstracts/_tokens.scss`. SCSS variables are not used for the new system — this makes the tokens visible/overridable in DevTools and avoids a second parallel color system.

### 2.1 Color

| Token | Value | Usage |
|---|---|---|
| `--silo-bg` | `#F6F7F9` | App background |
| `--silo-surface` | `#FFFFFF` | Cards, panels, drawer, inputs |
| `--silo-border` | `#E3E6EB` | Default hairline border |
| `--silo-border-strong` | `#D3D8E0` | Input borders, emphasis dividers |
| `--silo-text` | `#12151C` | Primary text |
| `--silo-text-muted` | `#626B79` | Secondary text, labels |
| `--silo-text-faint` | `#98A1AF` | Placeholder, meta, disabled |
| `--silo-ink` | `#1E2A4A` | Brand mark, login background, headings accent |
| `--silo-ink-deep` | `#141C36` | Darker end of ink gradients (login) |
| `--silo-on-ink` | `var(--silo-surface)` | Text on `--silo-ink` surfaces |
| `--silo-on-ink-muted` | `rgba(255,255,255,.72)` | Secondary text on ink |
| `--silo-on-ink-line` | `rgba(255,255,255,.12)` | Hairlines on ink |
| `--silo-accent` | `#2F5AA8` | Primary actions, links, active nav, focus ring |
| `--silo-accent-hover` | `#284C8F` | Hover state of the above |
| `--silo-accent-tint` | `#EAF0FA` | Active nav background, selected-row tint, user chat bubble |
| `--silo-amber` | `#C98A2C` | Credit balances, value figures **only** |
| `--silo-amber-tint` | `#FBF1DF` | "Expiring soon" pill background |
| `--silo-green` | `#1E8A5B` | Success, active status |
| `--silo-green-tint` | `#E6F5EE` | Active-status pill background |
| `--silo-red` | `#C23B3B` | Danger, revoked, zero/negative credit |
| `--silo-red-tint` | `#FBEAEA` | Revoked-status pill background |
| `--chart-1 … --chart-8` | see `_tokens.scss` | Categorical data series: chart lines, `.customer-dot`, share bars |

**Rule:** never introduce a new hex value in a page or component file. If a shade is missing, add it here first.

**Chart palette rules**
1. Series colors come from `--chart-N` only. The C# palette (`ChartPalette.Colors`) mirrors the same hex values in the same order, with a comment pointing to `_tokens.scss`, because Telerik Chart receives colors as strings. No hex literal may live anywhere else in `.cs` or `.razor`.
2. The palette avoids amber, red and green as dominant hues, because those carry meaning (value, danger, success). Do not use framework default palettes.
3. A customer keeps the same color on the chart, the dot and the share bar (same index).

### 2.2 Typography

| Token | Value |
|---|---|
| `--font-sans` | `'IRANSans', Tahoma, 'Segoe UI', sans-serif` |
| `--font-mono` | `ui-monospace, 'JetBrains Mono', 'Cascadia Code', Consolas, 'Courier New', monospace` |

IRANSans is already vendored (`abstracts/font.scss`) — no new font files required. `--font-mono` uses a system-stack fallback chain so it renders correctly with zero new assets; teams that want pixel-perfect JetBrains Mono can self-host it later and the token is the only place that needs to change.

Available IRANSans weights: `400` (regular), `500` (medium), `700` (bold), `900` (black). There is no `600` — don't reference it. In Bootstrap terms: use `fw-medium` (500) or `fw-bold` (700); never `fw-semibold`.

| Token | Size | Use |
|---|---|---|
| `--fs-xs` | `.76rem` | Meta text, pill labels, table meta |
| `--fs-sm` | `.82rem` | Secondary buttons, small labels |
| `--fs-base` | `.875rem` | Body text, table cells |
| `--fs-md` | `.95rem` | Panel titles, form labels |
| `--fs-lg` | `1.1rem` | Page subheads |
| `--fs-xl` | `1.3rem` | Page titles (`<h2>`) |
| `--fs-2xl` | `1.7rem` | Stat-strip numbers |

### 2.3 Spacing

4px base unit: `--space-1: 4px` … `--space-8: 40px` (see token file for the full scale: 4/8/12/16/20/24/32/40).

### 2.4 Radius & shadow

| Token | Value | Use |
|---|---|---|
| `--radius-sm` | `6px` | Nav items, pills-adjacent controls |
| `--radius-md` | `10px` | Buttons, inputs, panels, stat strip |
| `--radius-lg` | `16px` | Login card, modals, dialogs |
| `--radius-pill` | `999px` | Status badges |
| `--shadow-sm` | `0 1px 2px rgba(18,21,28,.04)` | Rarely used — flat design avoids card shadows |
| `--shadow-md` | `0 8px 24px rgba(18,21,28,.08)` | Dropdowns, popovers, modals, Telerik notification |
| `--shadow-login` | `0 20px 60px rgba(18,21,28,.25)` | Login card only |

### 2.5 Control sizes

`--control-h-sm: 30px` · `--control-h: 38px` · `--control-h-lg: 48px` (login only). Bootstrap `.form-control`, `.btn`, and Telerik inputs/buttons must resolve to the same height so they can sit in one row.

### 2.6 Motion

`--dur-fast: .15s` (hover, focus), `--dur-base: .2s` (collapse, drawer), `--ease: ease`.
Rules: no motion without a state change, except one one-shot entrance (the login logo ping). Every animation is disabled under `prefers-reduced-motion`.

### 2.7 Z-index

`--z-sidebar 1020 · --z-modal 1055 · --z-notification 1080 · --z-popup 1090 · --z-loader 1100`. The page loader must always sit above modals; Telerik popups (dropdowns) must sit above modals. `--z-sidebar` is used by the drawer.

---

## 3. Components

### 3.1 Navigation shell — top bar + drawer (`.top-bar`, `.drawer`)
- **Top bar** (`.top-bar`): brand on the inline-start edge (`.top-bar-brand`: 3-bar "silo cross-section" glyph, not an icon font, plus name and a muted subtitle); user block on the inline-end edge (`.top-bar-user`: name + logout). `.menu-toggle` is rendered only for authenticated users.
- **Drawer** (`.drawer`): slides in from the inline-start edge (right in RTL). Light surface, 1px `border-inline-start`, width `--sidebar-width` (240px), `z-index: var(--z-sidebar)`. `.drawer.open` shows it; `.menu-backdrop.show` dims the content and closes the drawer on click.
- **Nav items** (`.nav-item-ai`): muted text, `border-inline-start: 3px solid transparent`; `.active` gets `--silo-accent-tint` background, `--silo-accent` text/border, `font-weight:500`.
- **Logout**: `.btn-sm .btn-outline-secondary`, icon-only, with `aria-label` and `title`. Never red: it is not destructive.
- **Accessibility**: the toggle has `aria-expanded` and `aria-controls="app-drawer"`; the nav has `aria-label`; the backdrop is `aria-hidden`.
- **Naming**: `.top-bar` (the shell) is not the page header of §3.2.

### 3.2 Page header
No border, generous top padding, page `<h2>` + one-line muted description, primary action button trailing edge.

This is a layout pattern, not required as a class. Trailing actions: one `.btn-primary`, others `.btn-outline-primary`. Home may use the hero band (§3.16) instead.

### 3.3 Stat strip (`.stat-card` container → new `.stat-strip` wrapper)
One `.stat-strip` panel, children `.stat-card` divided by `border-inline-start: 1px solid var(--silo-border)` (first child none). Numbers in `--font-mono`, `--fs-2xl`, `font-weight:700`. Default number color is `--silo-text`; use `.stat-card.accent`, `.stat-card.amber`, `.stat-card.red` modifiers to color specific figures (e.g., active keys = accent, revoked = red).

- No icons in the strip: do not use `.stat-icon` or the `.stat-card.green` modifier.
- Mapping: credit/balance → `amber`; "active" counts → `accent`; plain usage/counts → no modifier; `red` only for revoked / zero-credit counts.
- Never colour a stat number with `.text-danger` / `.text-success`.

### 3.4 Panel / card (`.card-ai`)
Flat surface, 1px border, `--radius-md`, **no shadow**. Header (`.card-ai-header`): bottom border, `--fs-md` semibold-weight(700) title, optional trailing meta text in `--silo-text-faint`.

The header's trailing area may hold meta text (`.text-muted .small`), a `.btn-group.btn-group-sm` toggle (active = `.btn-primary`, inactive = `.btn-outline-primary`), or one `.btn-sm`. A card never carries `shadow-sm` or `border-0`.

### 3.5 Table (Bootstrap `.table` inside `.table-responsive`)
Header row: `--silo-bg` background, `--fs-xs` `--silo-text-faint` text, bottom border. Body rows: hairline bottom border, no zebra striping, hover = `--silo-bg`. Any cell holding an ID/date/key/amount gets `--font-mono` via a `.mono` helper class (used inline where the razor markup already isolates that cell — e.g. wrap key labels).

Telerik Grid is the standard data table; Bootstrap `.table` is kept for legacy pages only. The same visual rules above apply to `.k-grid` (skin in §3.12).
- `Class=` on a grid is only for layout modifiers. The skin lives on `.k-grid`; the aliases `grid-center`, `customers-grid` and `silo-telerik-grid` are retired.
- Modifier `.grid-flush`: a grid placed directly inside `.card-ai` (no outer border/radius, to avoid a double border).

**Cell conventions**

| Value | Markup |
|---|---|
| Name / title | `fw-medium` |
| ID, key, model name, idempotency key, tags | `mono text-muted` + `dir="ltr"` (add `small` in dense grids) |
| Amount (toman / usd / token counts) | `mono` + `dir="ltr"` |
| Credit balance | amount + `fw-bold` + `text-amber`; `<= 0` → `text-danger` |
| Date / timestamp | `mono text-muted`, **no** `dir="ltr"` (Jalali order must not flip) |
| Count | `mono` |
| Type / feature label | `badge-neutral` |
| Status | `badge-*` per §3.6 |
| Row actions | `.btn-sm .btn-outline-*`, icon + label, last column, `text-end` |

### 3.6 Status badges (`.badge-active` / `.badge-expired` / `.badge-revoked`)
Pill (`--radius-pill`), tint background + solid text using the green/amber/red tint pairs above, small leading dot via `::before`. Replaces the previous bright flat `#dcfce7`/`#fee2e2` Tailwind-style colors with the token set so they match the rest of the palette exactly.

Two label badges follow the same pill spec **without** the leading dot (they are labels, not states):

| Class | Look | Use |
|---|---|---|
| `.badge-neutral` | `--silo-bg` bg, 1px `--silo-border`, `--silo-text-muted`, `--fs-xs`, weight 500 | doc type, feature, kind, inactive, processing, counters |
| `.badge-accent` | `--silo-accent-tint` bg, `--silo-accent` text | "Default" marker, RAG model kind |

**Status mapping — single source of truth**

| Domain | State | Badge |
|---|---|---|
| API key | Active / Expired / Revoked | `badge-active` / `badge-expired` / `badge-revoked` |
| Document processing | Completed / Processing / Failed | `badge-active` / `badge-neutral` / `badge-revoked` |
| Instruction | Active / Inactive | `badge-active` / `badge-neutral` |
| AI model | Active / Inactive | `badge-active` / `badge-revoked` |
| Ledger transaction | TopUp / anything else | `badge-active` / `badge-revoked` (a new type needs an explicit row here) |
| Model kind | RAG / Normal | `badge-accent` / `badge-neutral` |
| Model | Default RAG | `badge-accent` |

Never combine Bootstrap `.badge` with these classes, and never use `bg-success`, `bg-info`, `bg-secondary`, `bg-danger`, `bg-light` badges. The amber badge (`badge-expired`) is only for expiry.

### 3.7 Buttons
- `.btn-primary` → solid `--silo-accent`, hover `--silo-accent-hover`, white text, `--radius-md`.
- `.btn-outline-primary/secondary/danger/info` → bordered, `--silo-border-strong`, colored text matching intent, fills on hover.
- `.btn-ai-primary` (login) → same primary treatment, full width.
- All buttons: `--radius-md`, no uppercase, no letter-spacing tricks.
- `.btn-success` is **not** part of the system. Confirming an inline edit uses `.btn-primary`.
- `.btn-light` / `.btn-outline-light` exist only on ink surfaces (hero band, §3.16).
- Icon-only buttons need `aria-label` and `title`; size = `--control-h-sm` square.
- Disabled = `opacity: .6`, `cursor: not-allowed`. A busy button is disabled but never shows a spinner (§3.15).
- Telerik `ThemeColor="primary"` → `--silo-accent` (§3.12).

### 3.8 Forms
`.form-control` / `.form-select`: `--silo-border-strong` border, `--radius-md`, focus = 2px `--silo-accent-tint` ring + `--silo-accent` border. Labels: `--fs-sm`, `font-weight:500`, `--silo-text-muted`.

- Labels: `.form-label` only. Do not add `fw-semibold`, `text-secondary`, `small`, `mb-1`.
- Optional hint inside a label: `<span class="text-muted fw-normal">(اختیاری)</span>`.
- Hint under an input: `.form-text` → `--fs-xs`, `--silo-text-faint`.
- Small variant: `.form-control-sm` = `--control-h-sm` (inline editing in grids).
- Checkbox: `.form-check` with `accent-color: var(--silo-accent)`; label `fw-medium`.
- Textarea: `.form-control` (+ `.rag-content-editor` for the instruction editor, which sets the minimum height). Prose stays in sans, not mono.
- Validation: `.field-error` / Blazor `.validation-message` → `--fs-xs`, `--silo-red`; an invalid field gets `--ring-danger`.
- **Parity rule:** Telerik inputs and `.form-control` side by side must match in height, border, radius and focus ring.
- **Icon field** (`.field-group`, `.field-label`, `.field`, `.field-icon`, `.field-input`, `.field-toggle`, `.field-error`) is used by the login only. Every other form uses `.form-control` / Telerik inputs.
- `<kbd>`: `--font-mono`, `--fs-xs`, `--silo-surface` bg, 1px `--silo-border-strong`, `--radius-sm`.

### 3.9 Alerts
Re-skinned to tint/solid-text pairs matching the badge system (`alert-success` → green pair, `alert-danger` → red pair, `alert-warning` → amber pair) instead of Bootstrap defaults, so a success alert and an active-status badge read as the same "success" visually.

**Secret-reveal pattern** (a new API key shown once): `.alert-success` containing a bold sentence, a `.secret-box`, and a muted hint line.
`.secret-box` (replaces `.customer-api-key`): `--font-mono`, `--fs-sm`, `--silo-surface` bg, 1px `--silo-border`, `--radius-sm`, padding `--space-2`, `word-break: break-all`, `user-select: all`, always `dir="ltr"`.

### 3.10 Chat (`.chat-row`, `.chat-bubble`, `.chat-citation`)
Bot bubble: surface + border. User bubble: `--silo-accent-tint` background. Citation badge: solid `--silo-ink` chip with the citation index in `--font-mono`. Similarity score and chunk index render in `--font-mono`, `--silo-text-faint`.

**Full anatomy**

| Block | Classes | Spec |
|---|---|---|
| Settings bar | `.rag-settings`, `.rag-setting-title`, `.rag-setting-icon`, `.rag-setting-heading`, `.rag-setting-description`, `.rag-setting-fields`, `.rag-field`, `.rag-top-k`, `.rag-setting-action`, `.rag-new-chat-btn` | Same surface as `.card-ai` (border, `--radius-md`, no shadow). Heading `--fs-md`/700; description `--fs-sm` muted; field labels per §3.8; icon = icon tile `md` |
| Chat card | `.rag-chat-card` | = `.card-ai` |
| Header | `.rag-chat-header`, `.rag-chat-header-left`, `.rag-agent-avatar`, `.rag-agent-name`, `.rag-agent-status`, `.rag-message-count` | Avatar = icon tile `md`, `--silo-ink` bg; status dot `--silo-green` (meaning: ready); count `--fs-xs` faint |
| Window | `.chat-window`, `.rag-chat-window` | `--silo-bg` bg, scrolls internally |
| Empty state | `.rag-empty`, `.rag-empty-orbit`, `.rag-empty-icon`, `.rag-empty-title`, `.rag-empty-description`, `.rag-suggestions`, `.rag-suggestion` | Icon tile `lg` on `--silo-accent-tint`; title 700; description muted; suggestions are `badge-neutral`-style pills. No orbit/glow decoration (principle 1) |
| Message | `.chat-row`, `.chat-row-user`, `.chat-row-bot`, `.rag-message-avatar`, `.chat-bubble`, `.chat-bubble-user`, `.chat-bubble-bot`, `.chat-bubble-header`, `.chat-sender`, `.chat-time`, `.chat-bubble-body` | Bubble colors as above. Time `.mono`, `--fs-xs`, faint. Body `--fs-base`, `white-space: pre-wrap` |
| Citations | `.chat-citations`, `.chat-citations-title`, `.citation-title-left`, `.citation-icon`, `.citation-count`, `.citation-list`, `.chat-citation`, `.chat-citation-number`, `.chat-citation-content`, `.chat-citation-meta`, `.chat-citation-file`, `.chat-citation-chunk`, `.chat-citation-score`, `.chat-citation-snippet` | Top hairline, no nested shadow. Number chip: `--silo-ink` bg, `--silo-on-ink`, mono. File name 500. Chunk and score mono, faint. Snippet `--fs-sm`, muted, clamped to 3 lines |
| Thinking | `.rag-thinking-bubble` | Bot bubble with an inline `TelerikLoader` (not a `TelerikLoaderContainer`) |
| Input | `.rag-input-area`, `.rag-input-wrapper`, `.rag-input-icon`, `.rag-textarea`, `.rag-send-btn` | Wrapper behaves like a field (`--silo-border-strong`, `--radius-md`, `--ring-focus` on `:focus-within`). Send = icon-only `.btn-primary`, `--control-h` square |
| Hints | `.rag-input-hint`, `.rag-input-security`, `kbd` | `--fs-xs`, faint |

### 3.11 Login (`.login-bg`, `.login-card`)
Background gradient uses `--silo-ink` (was a near-identical dark gray before — now explicitly tied to the brand token instead of a one-off value). Card: white, `--radius-lg`, `--shadow-login`.

The login partial contains no SCSS variables or hex literals. Token mapping:

| Element | Token |
|---|---|
| Background gradient | `--silo-ink` → `--silo-ink-deep` |
| Primary button / hover | `--silo-accent` / `--silo-accent-hover` |
| Logo glyph and ping ring | `--silo-accent-tint` |
| Background glow | `color-mix(in srgb, var(--silo-accent) 25%, transparent)` |
| Title / helper text | `--silo-text` / `--silo-text-muted` |
| Field border / field background | `--silo-border-strong` / `--silo-bg` |
| Validation error | `--silo-red` |
| Card radius / field radius | `--radius-lg` / `--radius-md` |
| Field and button height | `--control-h-lg` |
| Title weight | 700 |
| Transitions | `--dur-fast` |
| Focus ring | `--ring-focus` |

The page title is an `<h2>`; it must not receive focus from `FocusOnNavigate`.

### 3.12 Telerik (notification, grid, inputs, tabs, dialogs, loader, chart)
`.k-notification` re-skinned to the token palette (success/error/warning map to the green/red/amber pairs) so Telerik popups don't look like a foreign component. `.k-button` inherits `--font-sans`; a primary Telerik button maps to `--silo-accent`. All Telerik components are skinned in `pages/components/_overrides.scss`, tokens only:

| Component | Spec |
|---|---|
| `.k-notification` | success/error/warning → green/red/amber pairs, `--shadow-md`, `--z-notification` |
| `.k-grid` | 1px `--silo-border`, `--radius-md`, no shadow; header `--silo-bg`, `--fs-xs`, faint, 500; no zebra; row hover `--silo-bg`; pager `--fs-sm` |
| Inputs (`.k-input`, `.k-picker`) | `--silo-border-strong`, `--radius-md`, height `--control-h`, `--ring-focus` on `:focus-within`, disabled = `--silo-bg` bg + faint text |
| `.k-button-solid-primary` | `--silo-accent` / hover `--silo-accent-hover` |
| `.k-tabstrip` | active tab `--silo-accent` text and border; wrapper bordered like a card |
| `.k-dialog` and custom `Modal` | `--radius-lg`, `--shadow-md`, title `--fs-md`/700, hairline header/footer, backdrop `rgba(18,21,28,.45)`, `--z-modal` |
| `.k-loader-container` | `z-index: var(--z-loader)`, backdrop `rgba(246,247,249,.7)` |
| `.k-chart` | text `--font-sans`, `--silo-text-muted`; gridlines `--silo-border`; no marker dots by default; series from `--chart-N` |

Dialog success pattern: green `bi-check-circle-fill fs-1`, title `fw-bold`, optional `badge-neutral` for a detail. Telerik class names vary by version (`k-table-*` in newer ones); verify selectors in DevTools.

### 3.13 File upload
- `.file-drop`: dashed 1.5px `--silo-border-strong`, `--silo-bg` bg, `--radius-md`, padding `--space-4`. `.file-drop.has-file`: solid `--silo-accent` border, `--silo-accent-tint` bg.
- Compact variant: `.file-drop` combined with `d-flex` for one-row use inside forms.
- `.file-chip`: icon tile `sm`, name (500, ellipsis), meta row (`.file-chip-ext` as `badge-neutral`, size as `.mono`), `.file-chip-remove` icon button.
- The dropzone icon uses `text-primary`; no opacity tricks.

### 3.14 Icon tile
One component for every square icon container: `.icon-tile` + size modifier `-sm | -md | -lg` (`--tile-sm/md/lg`), `--radius-md`, centered. Variants: default `--silo-accent-tint` bg + `--silo-accent` icon; `.icon-tile-ink` `--silo-ink` bg + `--silo-on-ink` icon.
Used by: `.logo-icon`, `.welcome-icon`, `.header-icon`, `.rag-setting-icon`, `.rag-agent-avatar`, `.rag-message-avatar`, `.citation-icon`, `.file-chip-icon`.

### 3.15 Loading & empty states
**Loading**
1. Page / data / save / upload operations: one `TelerikLoaderContainer` at the end of the page, rendered with `@if (busy)`; text from `TextResources.APP_StringKeys_Loading` or a specific action text.
2. A small inline `TelerikLoader` is allowed only in the chat send button and the chat "thinking" bubble.
3. Bootstrap `.spinner-border` is forbidden.
4. Buttons never swap their label for a spinner; they become `disabled`.

**Empty states**
- Card/modal body: `text-center text-muted py-5`, icon `fs-2 d-block mb-2`, one sentence.
- Grid `NoDataTemplate`: `text-center text-muted py-4`, inline icon `me-2`.

### 3.16 Hero band (Home only)
Spec: `--silo-ink` bg (optionally gradient to `--silo-ink-deep`), `--radius-md`, no shadow, text `--silo-on-ink`, date `.mono` in `--silo-on-ink-muted`, actions `.btn-light` (primary) + `.btn-outline-light`. Classes: `.welcome-banner`, `.welcome-content`, `.welcome-icon` (→ icon tile `-ink`), `.welcome-date`, `.welcome-actions`. No other page may use it; other pages use the standard page header (§3.2).

### 3.17 Data color markers
`.customer-dot` (10px circle), `.share-cell` / `.share-bar` / `.share-fill` / `.share-text` (track `--silo-border`, 6px high, fill = series color, percent in mono). Color and width come from data through inline `style`: **this is the only allowed use of `style=` for color.**

---

## 4. File architecture

```
wwwroot/styles/
├─ Site.scss                     entry point — import order matters
├─ abstracts/
│  ├─ _tokens.scss               Single source of truth for all tokens.
│  ├─ colors.scss                unchanged (legacy $variables, still used by legacy CSS in general.scss)
│  ├─ font.scss                  unchanged (IRANSans @font-face + inherited legacy utility/glyphicon CSS)
│  └─ measures.scss               unchanged
├─ libraries/
│  └─ bootstrap.rtl.css           unchanged
└─ pages/
   ├─ pages.scss                 unchanged (chat → layout → general, same as before)
   ├─ components/
   │  └─ _overrides.scss         .mono, .text-amber, .fw-medium, .badge-neutral/.badge-accent,
   │                             .secret-box, .icon-tile, .file-drop/.file-chip, Telerik skin, z-index rules
   ├─ layout/
   │  ├─ layout.scss             REWRITTEN — drawer/top bar/stat-strip/card-ai/badges/login/buttons on tokens
   │  └─ _login.scss             login partial, tokens only
   ├─ chat/
   │  └─ chat.scss               REWRITTEN — same tokens applied to chat bubbles/citations
   └─ general.scss               EDITED — see note below
```

`colors.scss` is a legacy palette. It carries one header comment: *"Legacy palette. Do not use in SiloAI. Use `_tokens.scss`."* No new variables go there.

### Import order (`Site.scss` → `pages/pages.scss`)

```
abstracts/font.scss
abstracts/_tokens.scss   ← loads before colors so tokens are available everywhere after
abstracts/colors.scss
abstracts/measures.scss
libraries/bootstrap.rtl.css
pages/chat/chat.scss
pages/layout/layout.scss
pages/components/_overrides.scss   ← after layout, before general
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

- **Do** use `.mono` (utility defined in `_overrides.scss`) on any cell/value that is an ID, key label, date, timestamp, or currency amount.
- **Do** use the `accent` / `amber` / `red` modifier classes on `.stat-card` — don't invent new stat colors.
- **Do** read series colors from `--chart-N` (CSS) or `ChartPalette` (C#, mirrored 1:1).
- **Do** add a row to the status mapping (§3.6) before introducing a new status.
- **Do** use one page-level `TelerikLoaderContainer` (§3.15).
- **Don't** use amber for anything except credit/value figures — it will stop meaning "money" if it's reused as a generic warning color (use the existing `--silo-amber-tint` pill for "expiring" status, which is still value-adjacent).
- **Don't** colour non-credit numbers amber (prices, rates, multipliers, token counts).
- **Don't** add box-shadow to panels/cards; the system is intentionally flat. Shadow tokens exist only for floating elements (notifications, dropdowns) and the login card.
- **Don't** hardcode hex colors in page-level `.razor` `style="..."` attributes — every color need should be satisfiable by an existing class or token.
- **Don't** use `fw-semibold`, `bg-*` badges, `.btn-success`, `.spinner-border`, `.stat-icon`, `.stat-card.green`.
- **Don't** use Bootstrap `.fs-*` for text; it is allowed only to size icons.
- **Don't** apply `dir="ltr"` to Jalali dates.
- **Deprecated:** `.customer-credit`, `.customer-credit-negative` (use `.text-amber` / `.text-danger`), `.customer-api-key` (use `.secret-box`), `grid-center`, `customers-grid`, `silo-telerik-grid`, `.stat-icon`, `.stat-card.green`.
- **Legacy pages:** `/dashboard` and `/api-keys` (English, not in navigation) are outside the system. Delete them, or migrate them before the next release.