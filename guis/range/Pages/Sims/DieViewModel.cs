public sealed record DieViewModel(int Face, bool Spinning)
{
    public string Glyph => Face switch
    {
        1 => "⚀", 2 => "⚁", 3 => "⚂", 4 => "⚃", 5 => "⚄", 6 => "⚅",
        _ => "?"
    };
}