using System;
using System.Threading;
using System.Threading.Tasks;

namespace PrintingTools.Core;

public interface IPrintManager
{
    Task PrintAsync(PrintSession session, CancellationToken cancellationToken = default);

    Task<PrintPreviewModel> CreatePreviewAsync(PrintSession session, CancellationToken cancellationToken = default);
}

public interface IPrintAdapter
{
    Task PrintAsync(PrintSession session, CancellationToken cancellationToken = default);

    Task<PrintPreviewModel> CreatePreviewAsync(PrintSession session, CancellationToken cancellationToken = default);
}

public interface IPrintAdapterResolver
{
    IPrintAdapter Resolve(PrintSession session);
}

public sealed class PrintManager : IPrintManager
{
    private readonly IPrintAdapterResolver _resolver;
    private readonly PrintingToolsOptions _options;

    public PrintManager(IPrintAdapterResolver resolver, PrintingToolsOptions options)
    {
        _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public Task PrintAsync(PrintSession session, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(session);

        var adapter = _resolver.Resolve(session);
        return adapter.PrintAsync(session, cancellationToken);
    }

    public async Task<PrintPreviewModel> CreatePreviewAsync(PrintSession session, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(session);

        if (!_options.EnablePreview)
        {
            throw new NotSupportedException("Print preview is disabled by configuration.");
        }

        var adapter = _resolver.Resolve(session);
        return await adapter.CreatePreviewAsync(session, cancellationToken).ConfigureAwait(false);
    }
}

public sealed class DefaultPrintAdapterResolver : IPrintAdapterResolver
{
    private readonly PrintingToolsOptions _options;
    private readonly IPrintAdapter _fallback = new UnsupportedPrintAdapter();

    public DefaultPrintAdapterResolver(PrintingToolsOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public IPrintAdapter Resolve(PrintSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        if (_options.AdapterFactory is { } factory)
        {
            var adapter = factory();
            if (adapter is not null)
            {
                return adapter;
            }
        }

        return _fallback;
    }
}

internal sealed class UnsupportedPrintAdapter : IPrintAdapter
{
    private static readonly Task<PrintPreviewModel> UnsupportedPreviewTask =
        Task.FromException<PrintPreviewModel>(new NotSupportedException("Print preview is not available on this platform."));

    public Task PrintAsync(PrintSession session, CancellationToken cancellationToken = default) =>
        Task.FromException(new NotSupportedException("Printing is not available on this platform."));

    public Task<PrintPreviewModel> CreatePreviewAsync(PrintSession session, CancellationToken cancellationToken = default) =>
        UnsupportedPreviewTask;
}
