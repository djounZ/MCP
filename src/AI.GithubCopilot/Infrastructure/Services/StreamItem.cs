namespace AI.GithubCopilot.Infrastructure.Services;

public sealed record StreamItem<TOut>
{

    private StreamItem(TOut? Value, string? Ignored, string? Ended, bool isIgnored, bool isEnded)
    {
        this.Value = Value;
        this.Ignored = Ignored;
        this.Ended = Ended;
        IsIgnored = isIgnored;
        IsEnded = isEnded;
    }


    public static StreamItem<TOut> BuildContent(TOut content)
    {
        return new StreamItem<TOut>(content, null, null, false, false);
    }

    public static StreamItem<TOut> BuildIgnored(string? content)
    {
        return new StreamItem<TOut>(default,content, null, true, false);
    }

    public static StreamItem<TOut> BuildEnded(string? content)
    {
        return new StreamItem<TOut>(default,null, content, false, true);
    }


    public TOut? Value { get; }
    public string? Ignored { get; }
    public string? Ended { get; }

    public bool IsIgnored { get; }
    public bool IsEnded { get; }
}
