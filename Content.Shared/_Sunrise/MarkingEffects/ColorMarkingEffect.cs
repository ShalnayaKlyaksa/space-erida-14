using System.Linq;
using Robust.Shared.Serialization;

namespace Content.Shared._Sunrise.MarkingEffects;

[Serializable, NetSerializable]
public sealed partial class ColorMarkingEffect : MarkingEffect
{
    public override MarkingEffectType Type => MarkingEffectType.Color;

    public Color GetColor()
        => Colors.TryGetValue("base", out var col) ? col : Color.White;

    public ColorMarkingEffect(Color color) : base(color)
    {
    }

    public static readonly ColorMarkingEffect White = new(Color.White);

    public override string ToString()
    {
        Dictionary<string, string> dict = new();

        var color = GetColor();
        dict.Add("color.base", color.ToHex());

        var result = string.Join(",", dict.Select(kvp => $"{kvp.Key}={kvp.Value}"));
        return $"{Type.ToString()}=={result}";
    }

    public static ColorMarkingEffect? Parse(Dictionary<string, string> dict)
    {
        var color = Color.White;

        foreach (var (type, value) in dict)
        {
            if (type == "color.base")
                color = Color.TryFromHex(value) ?? Color.White;
        }

        return new ColorMarkingEffect(color);
    }

    public override ColorMarkingEffect Clone()
    {
        return new ColorMarkingEffect(GetColor());
    }

    public override bool Equals(MarkingEffect? maybeOther)
    {
        return maybeOther is ColorMarkingEffect other && DictionaryEquals(Colors, other.Colors);
    }
}
