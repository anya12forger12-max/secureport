# Accessibility Guide

SecurePort targets WCAG 2.2 Level AA compliance. This guide documents the accessibility features and design principles.

## Standards

| Standard | Level | Status |
|----------|-------|--------|
| WCAG 2.2 | AA | Target compliance |
| Section 508 | — | Aligned |
| EN 301 549 | — | Aligned |

## Keyboard Navigation

Every interactive element in SecurePort is reachable and operable via keyboard:

| Action | Key |
|--------|-----|
| Move between panels | `Tab` / `Shift+Tab` |
| Activate button/link | `Enter` or `Space` |
| Navigate lists | `Arrow Up` / `Arrow Down` |
| Start scan | `Ctrl+Enter` |
| Stop scan | `Escape` |
| Search | `Ctrl+F` |
| Export | `Ctrl+E` |
| Toggle sidebar | `Ctrl+B` |
| Open settings | `Ctrl+,` |

### Focus Indicators

All focusable elements display a visible focus ring with sufficient contrast (minimum 3:1 against adjacent colors). Focus order follows the logical reading order of the interface.

### Focus Trapping

Modal dialogs (settings, export, backup) trap focus within the dialog. `Escape` closes the modal and returns focus to the triggering element.

## Screen Reader Support

### ARIA Labels

Every interactive control has an accessible label:

- Buttons: descriptive labels (`Start Scan`, `Export Results`)
- Panels: `aria-label` identifying the panel purpose
- Status indicators: `aria-live` regions for real-time updates
- Charts: `aria-label` with a text summary of the data

### Semantic Structure

- The UI uses semantic Avalonia controls (not custom-drawn elements where possible).
- Headings are hierarchical (`HeadingLevel` 1–4).
- Lists use list semantics, not visual-only formatting.

### Live Regions

Scan progress and status changes are announced via `Live` regions:

- Scan start: "Scan started for 192.168.1.1"
- Scan complete: "Scan complete. 12 open ports found."
- Errors: "Scan failed. Target unreachable."

## High Contrast Mode

Enable via **Settings → Appearance → High Contrast**.

High contrast mode:

- Increases text contrast to meet WCAG AAA ratios (7:1 for normal text).
- Replaces subtle color differences with distinct borders and patterns.
- Ensures all interactive elements remain visible against any background.
- Respects the system high-contrast setting when "System" is selected.

## Reduced Motion

Enable via **Settings → Appearance → Reduced Motion** or detected from the OS preference.

Reduced motion mode:

- Disables all transition animations.
- Replaces animated progress indicators with static alternatives.
- Eliminates scroll-snap behavior.
- Instant state changes instead of fades or slides.

## Font Scaling

The UI font size is adjustable from 75% to 200%:

- **Settings → Appearance → Font Size** — select a percentage or use the slider.
- `Ctrl++` / `Ctrl+-` to increase/decrease incrementally.
- `Ctrl+0` to reset to default.

All layout containers reflow to accommodate larger text without clipping or overlap.

## Focus Management

### Scan Workflow

1. User enters target and presses `Tab` to the **Start Scan** button.
2. On scan start, focus moves to the progress indicator (announced via live region).
3. On scan complete, focus moves to the results list.
4. Results can be navigated with arrow keys.

### Modal Dialogs

1. When a modal opens, focus moves to the first interactive element inside it.
2. `Tab` cycles through modal controls.
3. On close, focus returns to the element that triggered the modal.

### Panel Navigation

The sidebar, main content, and results panel are navigable as landmark regions. Screen readers can jump between them directly.

## Accessible Charts

The dashboard charts include:

- Text alternatives (`aria-label`) summarizing the data.
- Data tables as an alternative view for every chart.
- Patterns and shapes (not just color) to distinguish data series.
- Sufficient color contrast for all data elements.

## Testing Accessibility

### Automated

Run accessibility checks using browser dev tools or dedicated scanners against the HTML export format.

### Manual

1. Navigate the entire application using only the keyboard.
2. Test with a screen reader (NVDA on Windows, VoiceOver on macOS, Orca on Linux).
3. Verify all content is readable at 200% zoom.
4. Check color contrast with a contrast analyzer.
5. Enable reduced motion and verify no animations play.

### Resources

- [WCAG 2.2 Guidelines](https://www.w3.org/TR/WCAG22/)
- [WAI-ARIA Authoring Practices](https://www.w3.org/WAI/ARIA/apg/)
- [Avalonia Accessibility](https://docs.avaloniaui.net/docs/stay/articles/accessibility)
