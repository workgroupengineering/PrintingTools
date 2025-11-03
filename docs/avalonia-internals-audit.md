# Avalonia Internals Audit for PrintingTools (Phase 3.1)

## Summary

To deliver high-fidelity printing from Avalonia visuals we rely on a small set of internal APIs today and have identified a handful of upstream changes that will make the integration sustainable. This note captures the current touch points, why they are required, and the proposed upstream actions.

## Required Internal APIs

| Area | Internal Type/Member | Usage within PrintingTools | Notes / Risk |
|------|---------------------|-----------------------------|--------------|
| Rendering | `ImmediateRenderer.Render` | Needed for future vector export when bypassing `RenderTargetBitmap` rasterization. | Requires `InternalsVisibleTo` or formal extension point. |
| Scene graph | `Scene` + `SceneGraphFactory` members | Inspect visual tree bounds and transforms when generating pagination diagnostics. | Currently accessed via `VisualRenderAudit`; consider exposing a diagnostic surface upstream. |
| Visual helpers | `VisualExtensions.GetTransformedBounds` | Accurate layout bounds for pagination slices and margin calculations. | Candidate for promotion to public API. |
| Drawing infrastructure | `DrawingContextHelper.RenderAsync` | Used by `SkiaPdfExporter` to replay visuals into Skia PDF documents without re-laying out controls. | Public but lives in `Avalonia.Skia.Helpers`; confirm long-term contract status. |
| Layout contracts | `IRenderRoot.Renderer` | Extraction point for hooking live visuals during asynchronous pagination. | Future vector pipeline will require a stable façade. |

## Upstream Changes to Track

1. **Friend Assembly Coverage** – Extend Avalonia’s `ExternalConsumers.props` so `PrintingTools.Core` and `PrintingTools.MacOS` receive friend access while printing APIs stabilize.
2. **Public Pagination Hooks** – Propose a lightweight pagination/measurement service in Avalonia to remove direct reliance on `VisualExtensions` internals.
3. **Renderer Service Abstraction** – Discuss exposing a publicly supported renderer interface (e.g., `IVisualRenderer`) that enables high-DPI re-render without private entry points.
4. **Diagnostic Contracts** – Upstream an officially supported diagnostic callback for render traces, reducing the need for bespoke helpers like `VisualRenderAudit`.

## Next Steps

- Raise an upstream issue summarising the above requirements and propose incremental API openings (Phase 3.1 exit criterion).
- Prototype vector pipeline changes against the current friend-access approach while tracking deltas against upcoming Avalonia releases.
