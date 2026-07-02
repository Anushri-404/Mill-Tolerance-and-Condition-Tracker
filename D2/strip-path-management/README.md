# Strip Path Management — Log Observation (UI Recreation)

A pixel-close React recreation of the "Strip Path Management - Log
Observation" screen, built with Vite + React. **Frontend only — there is no
backend, API call, or data persistence.** All form state lives in memory via
`useState` and resets on page reload.

---

## 1. Assumptions made while recreating the screen

The original screenshot is a cropped browser window, so a few things aren't
fully visible. Where that happened, the closest professional guess was made
and is called out here and in code comments:

| Area | What was unclear | Assumption made |
|---|---|---|
| Sidebar | Only the right edge of each menu label is visible (`...tion`, `...cking`, `...trip Path - Log Observatic`) | Reconstructed a plausible steel-mill operations menu, with **"Strip Path - Log Observation"** kept as the active item to match the visible page. See `src/components/layout/Sidebar.jsx`. |
| Dropdown option lists | Only the *currently selected* value is shown for closed dropdowns (Section, Equipment Level1/2, Observation Type, Affected Portion, Severity) | Populated each `<select>` with a small, domain-appropriate option list. See `src/data/formOptions.js`. |
| Exact colors/fonts | Screenshot is a raster image, not code | Approximated the navy top bar, light-blue title bar, and pale-yellow input fields (a very common ASP.NET/jQuery admin-UI look) using named CSS variables in `src/index.css`, so they're easy to retune against the real brand palette. |
| Date pickers | Screenshot shows a small calendar icon inside each date field | Used native `<input type="date">` with an overlaid calendar emoji as a lightweight stand-in — swap for a real date-picker library if the project needs one. |
| "Choose File" button | Native browser file input styling varies by OS/browser | Used a plain `<input type="file">`, which renders as "Choose File" in Chrome (as in the screenshot) without extra styling/JS. |
| Live clock in top bar | Screenshot shows a specific timestamp | Passed in as a prop (`dateTime`) with the screenshot's value as the default, rather than wiring a real `setInterval` clock, since there's no backend/timezone source in this recreation. |

---

## 2. Project structure

```
strip-path-management/
├── index.html
├── package.json
├── vite.config.js
├── .eslintrc.cjs
├── README.md
├── public/
└── src/
    ├── main.jsx                 # React entry point
    ├── App.jsx / App.css        # Root layout (TopBar + Sidebar + Page)
    ├── index.css                # Global design tokens & resets
    ├── components/
    │   ├── layout/
    │   │   ├── TopBar.jsx/.css        # Navy "Welcome / Date / Logout" bar
    │   │   ├── Sidebar.jsx/.css       # Left navigation menu
    │   │   ├── PageHeader.jsx/.css    # Light-blue page title bar
    │   │   └── ActionLinks.jsx/.css   # "Save | Refresh | Cancel" row
    │   ├── common/
    │   │   ├── FormField.jsx/.css     # "Label : control" wrapper
    │   │   ├── TextInput.jsx          # Styled <input type="text/number">
    │   │   ├── SelectField.jsx        # Styled <select>
    │   │   ├── DateInput.jsx          # Date field w/ calendar icon
    │   │   ├── FileInput.jsx          # File upload field
    │   │   ├── TextArea.jsx           # Defect details textarea
    │   │   └── inputs.css             # Shared input styling
    │   └── form/
    │       ├── LogObservationForm.jsx # All form fields + local state
    │       └── LogObservationForm.css
    ├── pages/
    │   ├── LogObservationPage.jsx     # Composes header+actions+form
    │   └── LogObservationPage.css
    └── data/
        └── formOptions.js             # Dropdown option lists
```

---

## 3. Beginner-friendly setup guide

### 3.1 Software to install (one-time)

1. **Node.js** — install the **LTS version, 20.x or newer** (this project
   was written against Node 22, but anything ≥ 18 works fine).
   Download from https://nodejs.org and run the installer for your OS.
   Verify it installed correctly:

   ```bash
   node -v
   npm -v
   ```

   You should see version numbers printed (e.g. `v20.11.1` and `10.2.4`).
   If you get "command not found," restart your terminal, or your terminal,
   or reinstall and make sure "Add to PATH" was checked during install.

2. **VS Code** — download from https://code.visualstudio.com

3. **Recommended VS Code extensions** (open VS Code → Extensions icon in the
   left sidebar → search for each, click Install):
   - **ESLint** (`dbaeumer.vscode-eslint`) — shows lint errors inline
   - **Prettier – Code formatter** (`esbenp.prettier-vscode`) — auto-formats code
   - **ES7+ React/Redux/React-Native snippets** (`dsznajder.es7-react-js-snippets`)
   - **Auto Rename Tag** (`formulahendry.auto-rename-tag`)
   - **Path Intellisense** (`christian-kohler.path-intellisense`)

### 3.2 Getting the project onto your machine

If you received this project as a folder/zip:

```bash
# Navigate into wherever you unzipped/downloaded it
cd path/to/strip-path-management
```

If you're starting completely from scratch instead (for future reference,
this is how the project itself was created):

```bash
npm create vite@latest strip-path-management -- --template react
cd strip-path-management
```

Then you'd copy the `src/` files from this project into the newly scaffolded
one.

### 3.3 Install dependencies

From inside the project folder:

```bash
npm install
```

This reads `package.json` and downloads React, Vite, and ESLint into a new
`node_modules/` folder. It only needs to be run once (or again whenever
`package.json` changes).

### 3.4 Start the development server

```bash
npm run dev
```

You'll see output like:

```
  VITE v5.x.x  ready in 300 ms
  ➜  Local:   http://localhost:5173/
```

Open that URL in your browser (it should also open automatically). Any time
you save a file, the page updates instantly — no manual refresh needed.

To stop the server, press `Ctrl + C` in the terminal.

### 3.5 Build for production

When you're ready to deploy:

```bash
npm run build
```

This creates an optimized `dist/` folder containing static HTML/CSS/JS you
can upload to any static host (Netlify, Vercel, S3, nginx, etc.).

To preview that production build locally before deploying:

```bash
npm run preview
```

### 3.6 Useful terminal / folder navigation commands

| Command | What it does |
|---|---|
| `pwd` | Print current folder path |
| `ls` (Mac/Linux) or `dir` (Windows) | List files in current folder |
| `cd folder-name` | Move into a folder |
| `cd ..` | Move up one folder |
| `cd ~` | Jump to your home folder |
| `code .` | Open the current folder in VS Code |

### 3.7 Fixing common errors

- **`'vite' is not recognized` / `command not found: vite`**
  Run `npm install` again — the `vite` command comes from `node_modules`,
  and `npm run dev` (not typing `vite` directly) is the command to use.

- **`EADDRINUSE: address already in use :::5173`**
  Another process is already using port 5173. Either stop that process, or
  run `npm run dev -- --port 5174` to use a different port.

- **Blank white page in the browser**
  Open the browser DevTools console (F12 or right-click → Inspect →
  Console tab) and look for a red error — it almost always names the exact
  file and line. Common causes: a typo in an `import` path, or a missing
  closing tag in JSX.

- **`Module not found: Can't resolve './SomeFile'`**
  Check the spelling and capitalization of the import — file systems on
  Mac/Linux are case-sensitive even though the file might "look" the same.

- **Styles not showing up**
  Make sure the component's `.css` file is actually imported at the top of
  its `.jsx` file (e.g. `import './TopBar.css'`) — Vite only bundles CSS
  that's explicitly imported somewhere.

- **`npm install` fails with permission errors (Mac/Linux)**
  Avoid `sudo npm install`. Instead, fix npm's default folder permissions
  by following https://docs.npmjs.com/resolving-eacces-permissions-errors,
  or use a Node version manager like `nvm`.

### 3.8 General debugging tips

- Keep the browser DevTools open (`F12`) while developing — the Console tab
  surfaces JavaScript errors, and the Elements tab lets you inspect exactly
  which CSS rule is being applied to any element.
- React errors in the terminal running `npm run dev` are usually more
  detailed than what shows in the browser — check both.
- If a change doesn't seem to appear, do a hard refresh (`Ctrl+Shift+R` /
  `Cmd+Shift+R`) in case the browser cached an old version.
- Break large components down and use `console.log()` liberally inside
  event handlers (e.g. `onChange`) to confirm state is updating as expected.

---

## 4. Notes on the recreation approach

- All visual values (colors, spacing, font sizes) are centralized as CSS
  custom properties in `src/index.css`, so retuning the palette to match a
  real brand guide only requires editing one file.
- Form controls (`TextInput`, `SelectField`, `DateInput`, `FileInput`,
  `TextArea`) are intentionally small and prop-driven so they can be reused
  on other forms in a larger app, rather than being one-off, page-specific
  markup.
- No client or server validation, routing, or state management library was
  added — the brief asked for UI only, so `useState` inside
  `LogObservationForm` is sufficient and avoids unnecessary dependencies.
