# Phase 1 Progress – Printing Support Initiative

## 1.1 Inventory of WPF Printing Assets

- `exten/wpf/src/Microsoft.DotNet.Wpf/src/System.Printing/` exposes the managed API surface (ref assemblies) and native C++/CLI pipeline (`CPP/src/PrintQueue.cpp`) for spooler interaction, print queue management, and ticket serialization.
- `exten/wpf/src/Microsoft.DotNet.Wpf/src/PresentationFramework/System/Windows/Controls/PrintDialog.cs` + `MS/Internal/Printing/Win32PrintDialog.cs` implement the desktop dialog flow, ticket editing, and direct `Visual` / `DocumentPaginator` submission via `XpsDocumentWriter`.
- `exten/wpf/src/Microsoft.DotNet.Wpf/src/ReachFramework/` supplies print schema orchestration (`PrintSchema.cs`, `ReachPrintTicketSerializer.cs`) and plumbing for `PrintTicket`, capability discovery, and document packaging.
- Tests and telemetry (`System.Printing.Tests`, `TraceLoggers/XpsOMPrintingTraceLogger.cs`) demonstrate expected behaviors, async status handling, and diagnostics hooks we can mirror.

## 1.2 Avalonia Rendering/Printing Internals & Gaps

- Rendering capture today relies on `Avalonia.Media.Imaging.RenderTargetBitmap` (`exten/Avalonia/src/Avalonia.Base/Media/Imaging/RenderTargetBitmap.cs`) and `Avalonia.Rendering.ImmediateRenderer` (`.../Rendering/ImmediateRenderer.cs`) which traverse visuals into a `DrawingContext` but target raster outputs with platform `IRenderTargetBitmapImpl`.
- Scene composition flows through `Avalonia.Rendering.Composition.CompositingRenderer` and the `SceneGraph` infrastructure, none of which surface print-oriented hooks (no paginator abstraction, job/session lifecycle).
- Platform backends (e.g., `native/Avalonia.Native/src/OSX/`) lack printing-specific interop; no bindings to `NSPrintOperation` or spooler APIs, indicating we must introduce native bridges.
- Internal APIs (e.g., `Visual.Render`, `DrawingContext`), while powerful, are `internal`/`protected` heavy; enabling cross-assembly access for `PrintingTools` will require adjustments to `build/ExternalConsumers.props` or custom `InternalsVisibleTo` keys.

## 1.3 macOS Printing Requirements Snapshot

- Apple’s stack centers around `NSPrintInfo` for ticket/state, `NSPrintOperation` for job orchestration, and `NSView` drawing callbacks into Quartz contexts (PDF/vector friendly) with accessory panels for setup and preview.
- Need to bridge Avalonia visuals into Quartz: capture vector primitives or high-DPI raster into `CGContextRef` provided during `NSView.drawRect(_: )` invoked by print/preview.
- Print-to-PDF and sandbox compliance require handling `NSPrintPanel`, `NSSharingService` or file destination flows, plus entitlement checks on Apple Silicon (M3 Pro) hardware.
- macOS expects pagination, margins, and duplex to be precomputed before invoking the operation; our abstraction must translate Avalonia layout to `NSPrintPageRenderer`-style callbacks.

## 1.4 Target Scenarios & Non-Goals (Initial Milestone)

- **Supported visuals**: single `Visual`, logical subtree (rooted at `Control`), or pre-paginated document surfaces produced by `PrintingTools.PrintDocument` abstraction.
- **Capabilities**: interactive print dialog & preview, paper selection, orientation, scaling, multi-page support, and print-to-PDF.
- **Non-goals (initial)**: XPS spooler integration, Windows/Linux backends, full fidelity for complex 3D compositions, or API parity with WPF `PrintQueue` management.
- **Assumptions**: Application integrates `PrintingTools` services, accepts macOS-first rollout, and tolerates friend-assembly linking to Avalonia internals during preview phase.

---

**Outstanding Phase 1 Follow-ups**

- Validate whether existing Avalonia diagnostics (`VisualTreeDebug`) can assist print preview instrumentation.
- Confirm licensing boundaries when referencing WPF implementation specifics in documentation and naming.

