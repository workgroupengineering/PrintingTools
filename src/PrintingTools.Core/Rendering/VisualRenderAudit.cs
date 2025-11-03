using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;

namespace PrintingTools.Core.Rendering;

/// <summary>
/// Provides lightweight metadata about the Avalonia visual tree to aid pagination and rendering diagnostics.
/// </summary>
public static class VisualRenderAudit
{
    public static IReadOnlyList<VisualRenderMetadata> Collect(Visual root)
    {
        ArgumentNullException.ThrowIfNull(root);

        var results = new List<VisualRenderMetadata>();
        CollectCore(root, Matrix.Identity, results);
        return results;
    }

    private static void CollectCore(Visual visual, Matrix parentTransform, IList<VisualRenderMetadata> results)
    {
        var localTransform = CalculateLocalTransform(visual);
        var worldTransform = parentTransform * localTransform;

        var transformedBounds = visual.Bounds.TransformToAABB(worldTransform);
        var name = visual switch
        {
            Control control => control.Name,
            _ => null
        };

        var children = visual.GetVisualChildren().ToList();

        var metadata = new VisualRenderMetadata(
            visual.GetType().FullName ?? visual.GetType().Name,
            name,
            visual.Bounds,
            transformedBounds,
            worldTransform,
            visual.Opacity,
            visual.HasMirrorTransform,
            visual.RenderTransform is not null,
            visual.ClipToBounds,
            visual.Clip is not null,
            visual.OpacityMask is not null,
            children.Count);

        results.Add(metadata);

        foreach (var child in children)
        {
            CollectCore(child, worldTransform, results);
        }
    }

    private static Matrix CalculateLocalTransform(Visual visual)
    {
        var bounds = visual.Bounds;
        var translation = Matrix.CreateTranslation(bounds.Position);

        var renderTransformMatrix = Matrix.Identity;

        if (visual.HasMirrorTransform)
        {
            var mirrorMatrix = new Matrix(-1.0, 0.0, 0.0, 1.0, bounds.Width, 0);
            renderTransformMatrix = mirrorMatrix * renderTransformMatrix;
        }

        if (visual.RenderTransform is { } renderTransform)
        {
            var origin = visual.RenderTransformOrigin.ToPixels(bounds.Size);
            var offset = Matrix.CreateTranslation(origin);
            var finalTransform = (-offset) * renderTransform.Value * offset;
            renderTransformMatrix = finalTransform * renderTransformMatrix;
        }

        return renderTransformMatrix * translation;
    }
}

public sealed record VisualRenderMetadata(
    string TypeName,
    string? Name,
    Rect LocalBounds,
    Rect WorldBounds,
    Matrix WorldTransform,
    double Opacity,
    bool HasMirrorTransform,
    bool HasRenderTransform,
    bool ClipToBounds,
    bool HasGeometryClip,
    bool HasOpacityMask,
    int ChildCount);
