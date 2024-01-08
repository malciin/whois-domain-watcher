namespace DomainWatcher.Cli.Formatters.Values;

public class TabularColumnSpec
{
    public string? Header { get; set; }

    public int Width
    {
        get => Math.Max(Header?.Length ?? 0, width);
        set => width = value;
    }

    public int PaddingLeft { get; set; } = 0;

    public int PaddingRight { get; set; } = 1;

    public int Padding
    {
        set
        {
            PaddingLeft = value;
            PaddingRight = value;
        }
    }

    public int Align { get; set; }

    private int width = 0;

    public void UpdateWidthIfBigger(int width)
    {
        Width = Math.Max(Width, width);
    }
}
