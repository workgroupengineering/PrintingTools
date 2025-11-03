# Pagination Roadmap (Phase 3.3)

1. [ ] Collect Layout Metadata
   - Inventory Avalonia rendering helpers (`Visual.Render`, `ImmediateRenderer`, `SceneGraph`, `DrawingContextHelper`) that expose size, clip, transforms, and raster hooks needed for high-DPI capture.
   - Verify `PrintLayoutHints.TargetPageSize`, `Scale`, and `Margins` flow into `PrintPageMetrics`/`PrintSettings` (macOS path complete); add diagnostics to surface effective values during pagination.
   - `PrintPageMetrics` now captures effective page/margin geometry, DPI, and incremental offsets; extend it as pagination matures (horizontal flow, column hints).

2. [ ] Design Pagination Engine
   - Create a `Paginator` service that walks the Avalonia visual tree and measures content against page bounds.
   - Integrate with `PagedCompositeEnumerator` so pagination decisions are cached and reusable. (Baseline implementation exists via `DefaultPrintPaginator`; extend it with measurement logic.)
   - Support multi-page overflow for tall visuals by cloning/transforming visuals into logical slices.

3. [ ] Rendering Strategy
   - Decide between vector re-rendering and high-DPI raster exports per page; expose configuration for apps.
   - Build prototype capture adapters for both high-DPI raster (`RenderTargetBitmap`) and PDF (`SkiaSharp` picture/`CGContext`) targets; note any required internal access or build flags. *Done for sample: Skia path complements existing raster thumbnails, either can be toggled per platform.*
   - Provide diagnostics (bounding boxes, layout traces) to validate page splits.

**Progress Notes**
- `PrintPaginationUtilities.ExpandPage` now slices tall visuals into multiple `PrintPage` instances using `ContentOffset`, and `MacPrintAdapter` consumes the expanded list (with PDF export reusing the same path). Additional work remains for horizontal flow/columns and richer diagnostics.
- `SkiaPdfExporter` prototype renders visuals directly to Skia PDF documents when managed export is requested, exercising the vector pipeline.
- Preview UI consumes the cached bitmaps with zoom + paging, demonstrating virtualization over the expanded page set.
- Preview overlay now uses the same page metrics as the macOS print path, drawing printable margins so discrepancies are visible before running native preview.
- Diagnostics pipeline (`PrintDiagnostics`) now surfaces render/exception events from the macOS adapter and Skia exporter, enabling host applications to observe print fidelity issues without attaching debuggers.

4. [ ] Preview Integration
   - Feed paginated pages into the upcoming Avalonia preview controls with virtualization and zoom.
   - Ensure preview uses the exact same paginator results used by native print adapters. (Managed/native callback plumbing in place for macOS bridge.)

5. [ ] Testing & Validation
   - Add integration tests covering simple page breaks, explicit target sizes, and overflow scenarios.
   - Capture edge cases (mixed DPI, transforms, clip regions) and document known limitations.

---

**Dependencies**
- `PrintLayoutHints` (core hints mechanism) – implemented.
- `PagedCompositeEnumerator` – initial support landed; will evolve alongside the paginator.
- macOS adapter rendering path – managed/native callback plumbing now renders via Quartz blits; remaining work focuses on advanced pagination and overflow handling.
