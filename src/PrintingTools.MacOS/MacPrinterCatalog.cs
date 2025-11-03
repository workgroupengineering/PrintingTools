using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using PrintingTools.Core;
using PrintingTools.MacOS.Native;

namespace PrintingTools.MacOS;

public static class MacPrinterCatalog
{
    private const string DiagnosticsCategory = "MacPrinterCatalog";

    public static IReadOnlyList<string> GetInstalledPrinters()
    {
        if (!OperatingSystem.IsMacOS())
        {
            return Array.Empty<string>();
        }

        var result = new List<string>();
        var native = new PrintingToolsInterop.StringArray();

        try
        {
            native = PrintingToolsInterop.GetPrinterNames();
            if (native.Count <= 0 || native.Items == IntPtr.Zero)
            {
                return result;
            }

            var pointerSize = IntPtr.Size;
            for (var i = 0; i < native.Count; i++)
            {
                var itemPtr = Marshal.ReadIntPtr(native.Items, i * pointerSize);
                var length = native.Lengths != IntPtr.Zero
                    ? Marshal.ReadInt32(native.Lengths, i * sizeof(int))
                    : 0;

                if (itemPtr == IntPtr.Zero || length <= 0)
                {
                    continue;
                }

                var name = Marshal.PtrToStringUni(itemPtr, length);
                if (!string.IsNullOrWhiteSpace(name))
                {
                    result.Add(name);
                }
            }

            PrintDiagnostics.Report(
                DiagnosticsCategory,
                $"Enumerated {result.Count} printer(s) from macOS.",
                context: new { PrinterCount = result.Count });

            return result;
        }
        catch (Exception ex)
        {
            PrintDiagnostics.Report(DiagnosticsCategory, "Failed to enumerate printers.", ex);
            return Array.Empty<string>();
        }
        finally
        {
            PrintingToolsInterop.FreePrinterNames(native);
        }
    }
}
