public sealed class DiceHtmlRenderer
{
    public string Die(int face, bool spinning) =>
        $"""
         <div id="dice" class="die{(spinning ? " spin" : "")}" hx-swap-oob="true">
             {Glyph(face)}
         </div>
         """;

    public string Status(string text) =>
        $"""<div id="status" hx-swap-oob="true">{text}</div>""";

    private static string Glyph(int n) => n switch
    {
        1 => "⚀", 2 => "⚁", 3 => "⚂", 4 => "⚃", 5 => "⚄", 6 => "⚅",
        _ => "?"
    };
}