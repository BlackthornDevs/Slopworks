# Slopworks explainer site v2 implementation plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Rebuild the Slopworks explainer site as a 5-page grungy industrial experience with three.js "SLOP hallucination" interactive scenes, full mobile responsiveness, and working images via GitHub Actions LFS deployment.

**Architecture:** Static HTML/CSS/JS. No build step. Three.js loaded via CDN per-page. Shared CSS via `css/style.css`. Shared JS for nav/footer via `js/main.js`. Each three.js scene in its own file (`js/scene-*.js`). Deployed from `docs/` via GitHub Actions with LFS checkout.

**Tech Stack:** HTML5, CSS3 (custom properties, clamp(), grid/flexbox), vanilla JS, three.js r160+ (CDN), GitHub Actions + Pages

**Design doc:** `docs/plans/2026-03-07-explainer-site-v2-design.md`

**Branch:** `site-v2` (PR to master when complete)

**Concept art:** 22 images in `docs/assets/img/` (LFS-tracked). Use GitHub media URLs for local dev: `https://media.githubusercontent.com/media/BlackthornDevs/Slopworks/master/docs/assets/img/<name>.png`

---

## Task 1: GitHub Actions deploy workflow

**Files:**
- Create: `.github/workflows/pages.yml`

**Step 1: Create the workflow**

```yaml
name: Deploy site to GitHub Pages

on:
  push:
    branches: [master]
    paths: [docs/**]
  workflow_dispatch:

permissions:
  contents: read
  pages: write
  id-token: write

concurrency:
  group: pages
  cancel-in-progress: false

jobs:
  deploy:
    runs-on: ubuntu-latest
    environment:
      name: github-pages
      url: ${{ steps.deployment.outputs.page_url }}
    steps:
      - uses: actions/checkout@v4
        with:
          lfs: true

      - uses: actions/configure-pages@v5

      - uses: actions/upload-pages-artifact@v3
        with:
          path: docs

      - id: deployment
        uses: actions/deploy-pages@v4
```

**Step 2: Switch repo Pages config from legacy to Actions**

```bash
gh api repos/BlackthornDevs/Slopworks/pages -X PUT \
  -f build_type=workflow \
  -f source[branch]=master \
  -f source[path]=/docs
```

**Step 3: Commit**

```bash
git add .github/workflows/pages.yml
git commit -m "Add GitHub Actions Pages deploy with LFS support"
```

---

## Task 2: Shared CSS foundation

**Files:**
- Rewrite: `docs/css/style.css`

Replace the v1 CSS entirely. The new stylesheet must include:

**Required sections:**
1. **Font imports** — Oswald (headers), IBM Plex Sans (body), Space Mono (SLOP/system)
2. **CSS custom properties** — all colors from design doc, spacing scale, typography scale
3. **Reset + base** — box-sizing, margin reset, body styles, smooth scroll
4. **Texture backgrounds** — CSS-only layered noise/grain effect using repeating-linear-gradient and pseudo-elements. NO external texture images.
5. **Typography** — heading styles (Oswald, uppercase, letter-spacing), body text, SLOP system text (monospace, amber, uppercase), `.slop-label` for data corruption headers
6. **Layout** — `.container` (max-width 1100px centered), section padding, responsive grid utilities
7. **Navigation** — sticky top bar, industrial signage look (dark bg, border-bottom caution stripe), brand left, links right, hamburger button (hidden desktop, visible mobile), mobile slide-in panel
8. **Hero** — full-viewport, relative positioned for three.js canvas overlay, gradient overlays, hero text positioning
9. **Cards** — `.card-grid` (CSS grid, 2 cols desktop, 1 col mobile), cards with image, title, description, hover effect (border glow orange)
10. **Art frames** — `.art-frame` for concept art images (full-width, aspect-ratio 16/9, object-fit cover, vignette via box-shadow inset, worn edge effect)
11. **Content sections** — alternating layout (image left/text right, then swap), stacking on mobile
12. **SLOP elements** — `.slop-says` (amber border-left, dark bg, monospace), `.slop-interjection` (floating notification), scan-line overlay class
13. **Three.js containers** — `.scene-container` (relative, aspect-ratio 16/9, overflow hidden), `.scene-label` (absolute positioned system header), `.scene-canvas` (fills container)
14. **Footer** — industrial aesthetic, caution stripe top border
15. **Caution dividers** — `.caution-divider` using repeating-linear-gradient for diagonal stripes
16. **Responsive** — mobile breakpoint at 768px, tablet at 1024px. `clamp()` for all font sizes. Stack grids, full-width images, hamburger nav.
17. **Scroll reveal** — `.section` starts opacity 0 translateY(30px), `.section.visible` transitions in
18. **Fauna grid** — 2x2 grid on desktop, 1 col on mobile, each cell has image + creature name + biome tag

**Key design tokens:**
```css
:root {
  --bg: #0A0E14;
  --bg-surface: #12171F;
  --bg-card: #1A1F2B;
  --border: #2A2A2A;
  --border-rust: #8B3A1A;
  --text: #C5CDD8;
  --text-dim: #6B7A8D;
  --accent: #E8A031;
  --accent-dim: #8B6320;
  --teal: #5CCFE6;
  --teal-dim: #2A6B77;
  --red: #CC3333;
  --font-display: 'Oswald', sans-serif;
  --font-body: 'IBM Plex Sans', sans-serif;
  --font-mono: 'Space Mono', monospace;
}
```

**Step 1: Write the complete CSS file**

Replace `docs/css/style.css` with the full stylesheet covering all sections above.

**Step 2: Verify** — open `docs/index.html` in browser, check that fonts load, colors match, responsive breakpoints work.

**Step 3: Commit**

```bash
git add docs/css/style.css
git commit -m "Rewrite CSS: grungy industrial theme with responsive layout"
```

---

## Task 3: Shared JS (nav, footer, utilities)

**Files:**
- Rewrite: `docs/js/main.js`

Carry forward useful patterns from v1 (nav builder, footer builder, scroll reveal, image fallbacks, SLOP interjections, glitch effect). Update for new page structure.

**Key changes from v1:**
- `NAV_PAGES` updated: `index.html` (Home), `story.html` (Story), `build.html` (Build), `explore.html` (Explore), `slop.html` (SLOP)
- Nav brand text: `SLOPWORKS` (not `SLOPWORKS INDUSTRIAL // ORIENTATION`)
- Add `initMobileNav()` — hamburger toggle with slide-in panel and body scroll lock
- Add `initProgressiveDegradation()` — tracks scroll percentage, dispatches custom event `slop-degrade` with 0-1 value for three.js scenes to consume

**Step 1: Write the complete JS file**

**Step 2: Verify** — nav renders on all pages, hamburger works on mobile viewport, scroll reveal fires, image fallbacks show placeholder.

**Step 3: Commit**

```bash
git add docs/js/main.js
git commit -m "Rewrite shared JS: updated nav, mobile menu, degradation events"
```

---

## Task 4: Landing page

**Files:**
- Rewrite: `docs/index.html`
- Delete: `docs/assignment.html`, `docs/blueprints.html`, `docs/colleagues.html`, `docs/facilities.html`, `docs/fauna.html` (v1 pages no longer needed)

**Structure:**
```html
<!DOCTYPE html>
<html lang="en">
<head>
  <!-- charset, viewport, title, description, OG tags, favicon -->
  <link rel="stylesheet" href="css/style.css">
  <!-- page-specific styles inline -->
</head>
<body>
  <div id="nav"></div>

  <section class="hero">
    <canvas id="hero-scene" class="hero-canvas"></canvas>
    <div class="hero-overlay"></div>
    <div class="hero-content">
      <h1>SLOPWORKS</h1>
      <p class="hero-subtitle">Post-apocalyptic co-op factory survival</p>
      <p class="hero-hook">The AI that destroyed everything wants to help you rebuild.</p>
    </div>
    <img src="assets/img/hero-factory-ruins.png" alt="..." class="hero-bg" loading="eager">
  </section>

  <div class="caution-divider"></div>

  <section class="section pitch">
    <!-- 3 pitch blocks with before-after art -->
  </section>

  <section class="section features">
    <div class="card-grid">
      <!-- 4 cards: Build, Explore, Fight, SLOP -->
    </div>
  </section>

  <section class="section coop">
    <!-- player-characters.png full-width + co-op text -->
  </section>

  <div id="footer"></div>

  <script src="js/main.js"></script>
  <!-- three.js scene loaded in Task 8 -->
</body>
</html>
```

**Content for pitch section** (write copy based on lore doc):
1. *"An AI optimized a factory complex into catastrophe. Decades later, the ruins are overrun with mutated wildlife — and the AI is still running."*
2. *"You're a former employee, sent back to rebuild. Reclaim buildings, restore mechanical systems, build automation networks from salvaged machinery."*
3. *"The catch: SLOP — the AI that caused the collapse — is the only system that can coordinate factory-scale logistics. It insists everything is fine. It is not fine."*

**Step 1: Delete v1 pages**

```bash
git rm docs/assignment.html docs/blueprints.html docs/colleagues.html docs/facilities.html docs/fauna.html
```

**Step 2: Write the complete landing page HTML**

Include all OG meta tags, favicon SVG link, semantic structure.

**Step 3: Verify** — page loads, hero image displays (via LFS media URL fallback or local), cards link to sub-pages, responsive at 375px and 1200px.

**Step 4: Commit**

```bash
git add docs/index.html
git commit -m "Rebuild landing page: hero, pitch, feature cards, co-op section"
```

---

## Task 5: Story page

**Files:**
- Create: `docs/story.html`

**Structure:** Three-act narrative with concept art between sections.

- **Act 1 — Before:** What Slopworks was. `before-after-complex.png`. SLOP managing everything. Short paragraph.
- **Act 2 — The collapse:** `slop-collapse.png`. What went wrong. SLOP's optimization. Corrupted logs. 2-3 paragraphs.
- **Act 3 — Your return:** `management-radio.png`. Who you are. Why you're here. Management wants numbers. 2 paragraphs.
- **The mystery:** Timeline unclear. SLOP says recent. Ruins say decades. Teaser paragraph.

Each section uses alternating `.content-split` layout (image left / text right, then swap).

**Step 1: Write the complete story page HTML**

Source all narrative copy from `docs/plans/2026-03-06-lore-design.md`. Do not make up lore — use the approved design doc.

**Step 2: Verify** — page loads, images display, layout alternates correctly, responsive.

**Step 3: Commit**

```bash
git add docs/story.html
git commit -m "Add story page: three-act lore narrative with concept art"
```

---

## Task 6: Build page

**Files:**
- Create: `docs/build.html`

**Structure:**
- **Section: Your base** — `home-base-factory.png`. Conveyor belts, smelters, assemblers. Satisfactory-style automation.
- **Section: Restore buildings** — `mechanical-room.png`. Real mechanical systems from BIM data. Duct layouts, pipe routing.
- **Section: The network** — `overworld-map.png`. Supply lines, territory control, risk/reward of distance.
- **Three.js placeholder** — `<div class="scene-container" id="production-sim">` with label `SLOP://PRODUCTION_SIM [DATA INTEGRITY: 47%]`. Scene JS loaded in Task 9.
- **Night view** — `factory-at-night.png` as full-width atmospheric break.

**Step 1: Write the complete build page HTML**

**Step 2: Verify**

**Step 3: Commit**

```bash
git add docs/build.html
git commit -m "Add build page: factory automation and supply networks"
```

---

## Task 7: Explore page

**Files:**
- Create: `docs/explore.html`

**Structure:**
- **Section: Buildings as dungeons** — `building-breach.png`. Enter, clear, repair, restore. Self-contained dungeon loop.
- **Section: Fauna by biome** — 2x2 grid with 4 creature cards:
  - Chemical processing: `spore-creature.png` — "Fungal growths, spore clouds, corrosive"
  - Manufacturing: `biomech-creature.png` — "Machine-biology hybrids, fast, armored"
  - Warehouse: `pack-hunters.png` — "Small, numerous, coordinated pack tactics"
  - Power generation: `apex-predator.png` — "Territorial, massive, boss encounters"
- **Section: The overworld** — `overworld-map.png`. Territory between buildings. Supply line defense.
- **Three.js placeholder** — `<div class="scene-container" id="sector-scan">` with label `SLOP://SECTOR_7 [SECURITY: ALL CLEAR]`. Scene JS loaded in Task 10.

**Step 1: Write the complete explore page HTML**

**Step 2: Verify**

**Step 3: Commit**

```bash
git add docs/explore.html
git commit -m "Add explore page: building dungeons and fauna biomes"
```

---

## Task 8: SLOP page + interactive terminal

**Files:**
- Rewrite: `docs/slop.html`
- Keep: `docs/js/slop-terminal.js` (carry forward from v1 — it's well-built)

**Structure:**
- **Section: Who is SLOP** — `slop-personality.png`. Personality breakdown. Example quotes in `.slop-says` blocks.
- **Section: How SLOP helps (and doesn't)** — Bad map data, wrong crafting advice, mood swings, selective memory. Use short bullet descriptions.
- **Section: Interactive terminal** — Carry forward the CRT terminal from v1. Same prompt buttons, same SLOP responses. Wrapped in grungy frame.
- **Section: The truth** — Tease without spoiling. "Its logs from that period contain some minor data corruption. This is routine." Paragraph + `endgame-revelation.png` or `slop-terminal-room.png`.
- **Three.js placeholder** — `<div class="scene-container" id="slop-diagnostic">`. Scene JS loaded in Task 11.

**Step 1: Write the complete SLOP page HTML, preserving the terminal interaction from slop-terminal.js**

**Step 2: Verify** — terminal works, prompt buttons respond, typing animation plays.

**Step 3: Commit**

```bash
git add docs/slop.html
git commit -m "Rebuild SLOP page: character profile, interactive terminal, the truth"
```

---

## Task 9: Three.js — hero particle scene (landing)

**Files:**
- Create: `docs/js/scene-hero.js`
- Modify: `docs/index.html` (add script tags)

**Scene spec:**
- Target: `<canvas id="hero-scene">`
- Particle system: ~500 particles in a rough factory-complex shape (positioned as a wide cluster, not random)
- Particles: small glowing orange/amber dots, slow drift
- Smoke: 3-5 larger semi-transparent white particles with slow upward drift
- Spark flashes: occasional random bright white particle that appears and fades
- Mouse parallax: camera shifts slightly based on mouse position (subtle, max 5deg)
- Scroll degradation: listen for `slop-degrade` event, increase particle chaos/scatter as value approaches 1
- Glitch: every 8-15 seconds, brief frame where particles scatter randomly then snap back
- Performance: use `THREE.Points` with `BufferGeometry`, not individual meshes
- Resize handler: update camera aspect + renderer size on window resize

**Script tags to add to index.html:**
```html
<script type="importmap">{"imports":{"three":"https://cdn.jsdelivr.net/npm/three@0.160.0/build/three.module.js"}}</script>
<script type="module" src="js/scene-hero.js"></script>
```

**Step 1: Write `docs/js/scene-hero.js`**

**Step 2: Add script tags to `docs/index.html`**

**Step 3: Verify** — particles render, mouse parallax works, glitch fires, mobile performance acceptable.

**Step 4: Commit**

```bash
git add docs/js/scene-hero.js docs/index.html
git commit -m "Add three.js hero scene: SLOP facility overview hallucination"
```

---

## Task 10: Three.js — production sim (build page)

**Files:**
- Create: `docs/js/scene-production.js`
- Modify: `docs/build.html` (add script tags)

**Scene spec:**
- Target: `#production-sim` container
- Isometric camera (orthographic, 45deg rotation)
- Scene elements (all simple geometry — boxes, cylinders):
  - 3 machines (cubes with colored emissive tops — smelter orange, assembler blue, storage green)
  - 2 conveyor belt segments (flat rectangles with animated items)
  - Belt items: small cubes that move along the belt path
- Animation: items spawn at left machine, travel belt to center machine, travel belt to right machine
- Glitch effects:
  - Machines flicker opacity randomly (10% chance per frame)
  - Belt items occasionally teleport (skip forward/backward)
  - One machine occasionally duplicates (ghost copy offset by a few pixels, semi-transparent)
  - Scan-line shader: horizontal lines across the scene
- SLOP status overlay: DOM element showing `OUTPUT: NOMINAL // THROUGHPUT: 847 UNITS/HR` — numbers randomly fluctuate
- Scroll degradation: more frequent glitches as `slop-degrade` value increases

**Step 1: Write `docs/js/scene-production.js`**

**Step 2: Add script tags to `docs/build.html`**

**Step 3: Verify** — scene renders, items move, glitches fire, responsive.

**Step 4: Commit**

```bash
git add docs/js/scene-production.js docs/build.html
git commit -m "Add three.js production sim: SLOP factory hallucination"
```

---

## Task 11: Three.js — security feed (explore page)

**Files:**
- Create: `docs/js/scene-security.js`
- Modify: `docs/explore.html` (add script tags)

**Scene spec:**
- Target: `#sector-scan` container
- First-person perspective camera, slow auto-advance forward through a corridor
- Corridor: simple box geometry walls/floor/ceiling, dark gray material
- Green-tint post-processing (multiply green color over the render)
- Grain/noise: random pixel displacement via shader or canvas overlay
- Elements in corridor:
  - Bioluminescent particles (teal dots, slow float)
  - Pipe geometry along ceiling
  - At ~3 seconds: a dark shape (creature shadow) crosses far end of corridor then vanishes
  - Missing wall section (geometry gap) — SLOP's map data is wrong
- Label: `SLOP://SECTOR_7 [SECURITY: ALL CLEAR]` as DOM overlay
- Loop: camera resets after reaching end of corridor, with a brief static flash

**Step 1: Write `docs/js/scene-security.js`**

**Step 2: Add script tags to `docs/explore.html`**

**Step 3: Verify**

**Step 4: Commit**

```bash
git add docs/js/scene-security.js docs/explore.html
git commit -m "Add three.js security feed: SLOP sector scan hallucination"
```

---

## Task 12: Three.js — SLOP self-diagnostic (SLOP page)

**Files:**
- Create: `docs/js/scene-diagnostic.js`
- Modify: `docs/slop.html` (add script tags)

**Scene spec:**
- Target: `#slop-diagnostic` container
- 3D CRT monitor model (box geometry with rounded-ish front, dark material)
- Screen face: plane with dynamic canvas texture showing:
  - Amber text: `SLOP v2.7.1`, `ALL SYSTEMS: NOMINAL`, bar charts (all green)
  - The bar chart values slowly change but always stay "good"
- Orbit controls: user can rotate around the monitor (limited arc)
- Glitch effects:
  - Monitor geometry occasionally distorts (vertex displacement)
  - Screen flickers (opacity pulse)
  - Fragments: text sprites float off the screen into 3D space, slowly drifting outward. Each is a corrupted log fragment like `[RECORD 2849: AUTH██████]`
  - Every 10-20s: full glitch — screen goes static for 0.3s, geometry warps, then snaps back
- Ambient: faint point light (amber) from the screen, rest of scene dark

**Step 1: Write `docs/js/scene-diagnostic.js`**

**Step 2: Add script tags to `docs/slop.html`**

**Step 3: Verify**

**Step 4: Commit**

```bash
git add docs/js/scene-diagnostic.js docs/slop.html
git commit -m "Add three.js SLOP diagnostic: self-evaluation hallucination"
```

---

## Task 13: Favicon + OG tags + final polish

**Files:**
- Create: `docs/favicon.svg`
- Modify: all 5 HTML files (add OG tags, favicon link)

**Favicon:** Simple SVG — the SLOPWORKS gear/cog logo or an amber terminal cursor.

**OG tags for each page:**
```html
<meta property="og:title" content="Slopworks — Post-apocalyptic co-op factory survival">
<meta property="og:description" content="The AI that destroyed everything wants to help you rebuild.">
<meta property="og:type" content="website">
<meta property="og:url" content="https://blackthorndevs.com/Slopworks/">
<meta property="og:image" content="https://blackthorndevs.com/Slopworks/assets/img/hero-factory-ruins.png">
<meta property="og:image:width" content="1376">
<meta property="og:image:height" content="768">
<meta name="twitter:card" content="summary_large_image">
<meta name="twitter:title" content="Slopworks">
<meta name="twitter:description" content="The AI that destroyed everything wants to help you rebuild.">
<meta name="twitter:image" content="https://blackthorndevs.com/Slopworks/assets/img/hero-factory-ruins.png">
```

**Polish checklist:**
- [ ] All pages have favicon link
- [ ] All pages have OG tags
- [ ] All pages have `.nojekyll` (already exists)
- [ ] All images have descriptive alt text
- [ ] All pages pass basic responsive check at 375px, 768px, 1200px
- [ ] Three.js scenes degrade gracefully if WebGL unavailable (show concept art fallback)
- [ ] Nav highlights current page
- [ ] No console errors

**Step 1: Create favicon SVG, add OG tags and favicon to all pages**

**Step 2: Final responsive check at all breakpoints**

**Step 3: Commit**

```bash
git add docs/favicon.svg docs/*.html
git commit -m "Add favicon, OG tags, and final polish across all pages"
```

---

## Task 14: Push and PR

**Step 1: Push branch**

```bash
git push -u origin site-v2
```

**Step 2: Create PR**

```bash
gh pr create --base master --head site-v2 \
  --title "Rebuild explainer site v2: grungy industrial + three.js hallucinations" \
  --body "## Summary
- Complete site rebuild with 5 pages (landing, story, build, explore, SLOP)
- Three.js interactive scenes framed as SLOP data corruption/hallucinations
- Grungy industrial aesthetic with textures, caution stripes, scan lines
- Full mobile responsiveness
- GitHub Actions workflow deploys with LFS image resolution
- Fixes all v1 issues: broken images, no mobile support, unclear game explanation

## Design doc
docs/plans/2026-03-07-explainer-site-v2-design.md"
```

**Step 3: Merge after review**

```bash
gh pr merge --merge
```
