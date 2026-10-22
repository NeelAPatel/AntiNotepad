namespace AntiQuickPad.Design;

internal sealed record VariantRenderOptions(string Output, string? Only, bool NoCapture)
{
    internal const string Usage = "Usage: --render-variants <outdir> [--only <variantId>] [--no-capture]";

    internal static VariantRenderOptions Parse(string[] args)
    {
        if (args.Length < 2 || string.IsNullOrWhiteSpace(args[1])) throw new ArgumentException(Usage);
        string? only = null;
        var noCapture = false;
        for (var i = 2; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--only" when i + 1 < args.Length && !args[i + 1].StartsWith("--", StringComparison.Ordinal):
                    only = args[++i];
                    break;
                case "--no-capture":
                    noCapture = true;
                    break;
                default:
                    throw new ArgumentException(Usage);
            }
        }
        if (only is not null) _ = VariantRenderSpec.Get(only);
        return new(System.IO.Path.GetFullPath(args[1]), only, noCapture);
    }
}

internal sealed record GlassCheckOptions(string Output)
{
    internal const string Usage = "Usage: --glass-check <outdir>";

    internal static GlassCheckOptions Parse(string[] args)
    {
        if (args.Length != 2 || string.IsNullOrWhiteSpace(args[1])) throw new ArgumentException(Usage);
        return new(System.IO.Path.GetFullPath(args[1]));
    }
}
