using System.Linq;
using Content.Shared._Sunrise.MarkingEffects; //Erida edit
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.Humanoid.Markings;

/// <summary>
///     Represents a marking ID and its colors
/// </summary>
[DataDefinition, Serializable, NetSerializable]
public partial record struct Marking
{
    /// <summary>
    /// The <see cref="MarkingPrototype"/> referred to by this marking
    /// </summary>
    [DataField(required: true)]
    public ProtoId<MarkingPrototype> MarkingId;

    [DataField("markingColor")]
    private List<Color> _markingColors;

    //Erida start
    private List<MarkingEffect> _markingEffects;
    //Erida end

    /// <summary>
    /// The colors taken on by the marking
    /// </summary>
    public IReadOnlyList<Color> MarkingColors => _markingColors;

    //Erida start
    public IReadOnlyList<MarkingEffect> MarkingEffects => GetMarkingEffects();
    //Erida end

    /// <summary>
    /// Whether the marking is forced regardless of points
    /// </summary>
    public bool Forced;

    public Marking()
    {
        _markingColors = new();
        _markingEffects = new(); //Erida edit
    }

    public Marking(ProtoId<MarkingPrototype> markingId, IEnumerable<Color> colors)
    {
        MarkingId = markingId;
        _markingColors = colors.ToList();
        //Erida start
        _markingEffects = _markingColors.Select(color => (MarkingEffect) new ColorMarkingEffect(color)).ToList();
        //Erida end
    }

    //Erida start
    public Marking(ProtoId<MarkingPrototype> markingId, IEnumerable<Color> colors, IEnumerable<MarkingEffect> effects)
    {
        MarkingId = markingId;
        _markingColors = colors.ToList();
        _markingEffects = effects.Select(effect => effect.Clone()).ToList();

        EnsureEffectCount();
    }
    //Erida end

    public Marking(ProtoId<MarkingPrototype> markingId, int colorsCount) : this(markingId,
        Enumerable.Repeat(Color.White, colorsCount).ToList())
    {
    }

    public bool Equals(Marking other)
    {
        return MarkingId.Equals(other.MarkingId)
            && MarkingColors.SequenceEqual(other.MarkingColors)
            && MarkingEffects.Select(effect => effect.ToString()).SequenceEqual(other.MarkingEffects.Select(effect => effect.ToString())) //Erida edit
            && Forced.Equals(other.Forced);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(MarkingId, MarkingColors, string.Join(';', MarkingEffects.Select(effect => effect.ToString())), Forced); //Erida edit
    }

    //Erida start
    private List<MarkingEffect> GetMarkingEffects()
    {
        if (_markingEffects?.Count == _markingColors.Count)
            return _markingEffects;

        return _markingColors.Select(color => (MarkingEffect) new ColorMarkingEffect(color)).ToList();
    }

    public Marking WithColor(Color color) =>
        this with
        {
            _markingColors = Enumerable.Repeat(color, MarkingColors.Count).ToList(),
            _markingEffects = Enumerable.Repeat<MarkingEffect>(new ColorMarkingEffect(color), MarkingColors.Count)
                .Select(effect => effect.Clone())
                .ToList(),
        };
    //Erida end

    public Marking WithColorAt(int index, Color color)
    {
        var newColors = _markingColors.ShallowClone();
        newColors[index] = color;

        //Erida start
        var newEffects = CloneEffects();
        if (index < newEffects.Count)
        {
            if (newEffects[index] is ColorMarkingEffect)
                newEffects[index] = new ColorMarkingEffect(color);
            else
                newEffects[index].Colors["base"] = color;
        }

        var marking = this with { _markingColors = newColors, _markingEffects = newEffects };
        //Erida end

        return marking;
    }

    //Erida start
    public Marking WithMarkingEffectAt(int index, MarkingEffect effect)
    {
        var newEffects = CloneEffects();
        newEffects[index] = effect.Clone();

        var newColors = _markingColors.ShallowClone();
        if (effect.Colors.TryGetValue("base", out var color))
            newColors[index] = color;

        return this with { _markingColors = newColors, _markingEffects = newEffects };
    }
    //Erida end

    // look this could be better but I don't think serializing
    // colors is the correct thing to do
    //
    // this is still janky imo but serializing a color and feeding
    // it into the default JSON serializer (which is just *fine*)
    // doesn't seem to have compatible interfaces? this 'works'
    // for now but should eventually be improved so that this can,
    // in fact just be serialized through a convenient interface
    public string ToLegacyDbString()
    {
        // reserved character
        string sanitizedName = MarkingId.Id.Replace('@', '_');
        List<string> colorStringList = new();
        foreach (var color in MarkingColors)
            colorStringList.Add(color.ToHex());

        //Erida start
        if (MarkingEffects.Count == 0)
            return $"{sanitizedName}@{String.Join(',', colorStringList)}";

        var effectString = string.Join(';', MarkingEffects.Select(effect => effect.ToString()));
        return $"{sanitizedName}@{String.Join(',', colorStringList)}@{effectString}";
        //Erida end
    }

    public static Marking? ParseFromDbString(string input)
    {
        if (input.Length == 0) return null;
        var split = input.Split('@');
        if (split.Length < 2) return null; //Erida edit
        List<Color> colorList = new();
        foreach (string color in split[1].Split(','))
        {
            colorList.Add(Color.FromHex(color));
        }

        if (split.Length == 2)
            return new Marking(split[0], colorList);

        //Erida start
        var effects = new List<MarkingEffect>();
        foreach (var effectString in split[2].Split(';'))
        {
            if (MarkingEffect.Parse(effectString) is { } effect)
                effects.Add(effect);
        }

        return new Marking(split[0], colorList, effects);
    }

    private void EnsureEffectCount()
    {
        if (_markingEffects.Count == _markingColors.Count)
            return;

        _markingEffects = _markingColors.Select(color => (MarkingEffect) new ColorMarkingEffect(color)).ToList();
    }

    private List<MarkingEffect> CloneEffects()
    {
        var effects = _markingEffects?.Select(effect => effect.Clone()).ToList() ??
                      _markingColors.Select(color => (MarkingEffect) new ColorMarkingEffect(color)).ToList();

        if (effects.Count != _markingColors.Count)
            effects = _markingColors.Select(color => (MarkingEffect) new ColorMarkingEffect(color)).ToList();

        return effects;
    }
    //Erida end
}
