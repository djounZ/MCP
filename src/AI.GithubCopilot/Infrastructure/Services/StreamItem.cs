namespace AI.GithubCopilot.Infrastructure.Services;

public sealed record StreamItem<TOut>
{
    public StreamItem(TOut? content) : this(content, false, false)
    {

    }

    private StreamItem(TOut? Value, bool Ignored, bool Ended)
    {
        this.Value = Value;
        this.Ignored = Ignored;
        this.Ended = Ended;
    }

    public static StreamItem<TOut> IgnoredItem => new(default, true, false);
    public static StreamItem<TOut> EndedItem => new(default, false, true);
    public TOut? Value { get; init; }
    public bool Ignored { get; init; }
    public bool Ended { get; init; }
}