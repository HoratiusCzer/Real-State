# Design System Master File

> **LOGIC:** When building a specific page, first check `design-system/pages/[page-name].md`.
> If that file exists, its rules **override** this Master file.
> If not, strictly follow the rules below.

---

**Project:** REAK
**Generated:** 2026-09-16 16:49:52
**Curated override:** 2026-09-16 — the auto-matched "Real Estate/Property" category (teal
marketplace colors + Cinzel luxury serif + Exaggerated Minimalism style) directly contradicts
this project's own brief: `docs/REAK-requirements.md` §5 explicitly says *avoid fake luxury
imagery, avoid generic AI-SaaS aesthetics*, and asks for *authoritative, professional,
premium, modern, trustworthy, clean, Nepal-relevant, association-focused* — i.e. an
institutional/government-adjacent association, not a boutique property marketplace. Replaced
below with the "Government/Public Service" color match and "Corporate Trust" typography match
from targeted `--domain color` / `--domain typography` searches, which fit that brief
directly. Treat the values below as authoritative for REAK, not the auto-generated ones above
this note.
**Category:** Government/Public Service × Corporate Trust (curated, not auto-matched)
**Design Dials:** Variance 3/10 (Centered / Minimal) | Density 6/10 (Standard)

---

## Global Rules

### Color Palette

| Role | Hex | CSS Variable |
|------|-----|--------------|
| Primary | `#0F172A` | `--color-primary` |
| On Primary | `#FFFFFF` | `--color-on-primary` |
| Secondary | `#334155` | `--color-secondary` |
| Accent/CTA | `#0369A1` | `--color-accent` |
| Background | `#F8FAFC` | `--color-background` |
| Foreground | `#020617` | `--color-foreground` |
| Muted | `#E8ECF1` | `--color-muted` |
| Border | `#E2E8F0` | `--color-border` |
| Destructive | `#DC2626` | `--color-destructive` |
| Ring | `#0F172A` | `--color-ring` |

**Color Notes:** High-contrast navy + professional blue accent ("Government/Public Service"
match). Deliberately restrained — no teal/marketplace vibrancy, no gradients. This is a
placeholder professional palette, not REAK's official brand color; swap it once REAK supplies
real brand colors (spec §36 lists "official colors" as something REAK itself must provide).

### Typography

- **Heading Font:** Lexend
- **Body Font:** Source Sans 3
- **Mood:** corporate, trustworthy, accessible, readable, professional, clean
- **Why**: "Corporate Trust" pairing, matched against "government, healthcare, finance,
  accessibility-focused" — fits an association/institutional platform far better than the
  auto-matched luxury-real-estate serif pairing. Lexend is specifically designed to reduce
  visual stress and improve reading performance, which also supports the WCAG 2.2 AA target
  in `docs/REAK-requirements.md` §24.
- **Google Fonts:** [Lexend + Source Sans 3](https://fonts.googleapis.com/css2?family=Lexend:wght@300;400;500;600;700&family=Source+Sans+3:wght@300;400;500;600;700&display=swap)

**CSS Import:**
```css
@import url('https://fonts.googleapis.com/css2?family=Lexend:wght@300;400;500;600;700&family=Source+Sans+3:wght@300;400;500;600;700&display=swap');
```

### Spacing Variables

*Density: 6/10 — Standard*

| Token | Value | Usage |
|-------|-------|-------|
| `--space-xs` | `4px` / `0.25rem` | Tight gaps |
| `--space-sm` | `8px` / `0.5rem` | Icon gaps, inline spacing |
| `--space-md` | `16px` / `1rem` | Standard padding |
| `--space-lg` | `24px` / `1.5rem` | Section padding |
| `--space-xl` | `32px` / `2rem` | Large gaps |
| `--space-2xl` | `48px` / `3rem` | Section margins |
| `--space-3xl` | `64px` / `4rem` | Hero padding |

### Shadow Depths

| Level | Value | Usage |
|-------|-------|-------|
| `--shadow-sm` | `0 1px 2px rgba(0,0,0,0.05)` | Subtle lift |
| `--shadow-md` | `0 4px 6px rgba(0,0,0,0.1)` | Cards, buttons |
| `--shadow-lg` | `0 10px 15px rgba(0,0,0,0.1)` | Modals, dropdowns |
| `--shadow-xl` | `0 20px 25px rgba(0,0,0,0.15)` | Hero images, featured cards |

---

## Component Specs

### Buttons

```css
/* Primary Button */
.btn-primary {
  background: #0369A1;
  color: white;
  padding: 12px 24px;
  border-radius: 8px;
  font-weight: 600;
  transition: all 200ms ease;
  cursor: pointer;
}

.btn-primary:hover {
  opacity: 0.9;
  transform: translateY(-1px);
}

/* Secondary Button */
.btn-secondary {
  background: transparent;
  color: #0F172A;
  border: 1.5px solid #0F172A;
  padding: 12px 24px;
  border-radius: 8px;
  font-weight: 600;
  transition: all 200ms ease;
  cursor: pointer;
}
```

### Cards

```css
.card {
  background: #FFFFFF;
  border: 1px solid #E2E8F0;
  border-radius: 10px;
  padding: 24px;
  box-shadow: var(--shadow-sm);
  transition: all 200ms ease;
}

.card:hover {
  box-shadow: var(--shadow-md);
}
```

### Inputs

```css
.input {
  padding: 12px 16px;
  border: 1px solid #E2E8F0;
  border-radius: 8px;
  font-size: 16px;
  background: #FFFFFF;
  transition: border-color 200ms ease;
}

.input:focus {
  border-color: #0369A1;
  outline: none;
  box-shadow: 0 0 0 3px #0369A120;
}
```

### Modals

```css
.modal-overlay {
  background: rgba(0, 0, 0, 0.5);
  backdrop-filter: blur(4px);
}

.modal {
  background: white;
  border-radius: 16px;
  padding: 32px;
  box-shadow: var(--shadow-xl);
  max-width: 500px;
  width: 90%;
}
```

---

## Style Guidelines

**Style:** Minimalism & Swiss Style *(curated — replaces auto-matched "Exaggerated
Minimalism", which is for fashion/editorial/luxury brands, not an association platform)*

**Keywords:** Clean, simple, spacious, functional, white space, high contrast, geometric,
sans-serif, grid-based, essential

**Best For:** Enterprise apps, dashboards, documentation sites, SaaS platforms, professional
tools — matches REAK's Member Portal and Admin Portal directly; apply the same restraint to
the public site rather than a marketing-site "loud minimal" treatment.

**Key Effects:** Subtle hover (200–250ms), smooth transitions, sharp/minimal shadows
(`--shadow-sm`/`--shadow-md` only — avoid the `--shadow-xl` hero treatment for routine UI),
clear type hierarchy, fast loading. No oversized display type, no massive negative-space
hero statements — this is an institutional association site, not an agency portfolio.

### Page Pattern

REAK's homepage sections are specified directly in `docs/REAK-requirements.md` §4.1 (Header,
Hero, REAK introduction, Mission/Vision, Association benefits, Property Exchange explanation,
How the network works, Verified members, News, Notices, Events, Membership CTA, Public
property section behind a feature flag, Contact CTA, Footer) — use that order, not a
generic auto-matched pattern. The closest generic pattern for reference is **Marketplace /
Directory** (search-focused hero, categories, featured listings, trust signals, CTA) for the
member/public property-exchange pages specifically (Stage 6+), not the homepage.

---

## Anti-Patterns (Do NOT Use)

- ❌ Poor photos
- ❌ No virtual tours

### Additional Forbidden Patterns

- ❌ **Emojis as icons** — Use SVG icons (Heroicons, Lucide, Simple Icons)
- ❌ **Missing cursor:pointer** — All clickable elements must have cursor:pointer
- ❌ **Layout-shifting hovers** — Avoid scale transforms that shift layout
- ❌ **Low contrast text** — Maintain 4.5:1 minimum contrast ratio
- ❌ **Instant state changes** — Always use transitions (150-300ms)
- ❌ **Invisible focus states** — Focus states must be visible for a11y

---

## Pre-Delivery Checklist

Before delivering any UI code, verify:

- [ ] No emojis used as icons (use SVG instead)
- [ ] All icons from consistent icon set (Heroicons/Lucide)
- [ ] `cursor-pointer` on all clickable elements
- [ ] Hover states with smooth transitions (150-300ms)
- [ ] Light mode: text contrast 4.5:1 minimum
- [ ] Focus states visible for keyboard navigation
- [ ] `prefers-reduced-motion` respected
- [ ] Responsive: 375px, 768px, 1024px, 1440px
- [ ] No content hidden behind fixed navbars
- [ ] No horizontal scroll on mobile
