namespace Carabia.Automation.Framework.Browser;

/// <summary>Named window sizes used by responsive tests.</summary>
public sealed record Viewport(string Name, int Width, int Height)
{
    public static readonly Viewport Desktop = new("Desktop", 1920, 1080);
    public static readonly Viewport Tablet = new("Tablet", 768, 1024);
    public static readonly Viewport Mobile = new("Mobile", 390, 844);

    public static IReadOnlyList<Viewport> All { get; } = [Desktop, Tablet, Mobile];

    /// <summary>Carabia's header script switches to mobile behaviour at window.innerWidth &lt;= 768.</summary>
    public bool IsMobileLayout => Width <= 768;

    public override string ToString() => $"{Name} {Width}x{Height}";
}
