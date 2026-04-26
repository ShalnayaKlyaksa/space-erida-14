using System.Linq;
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

    [DataField("markingGradient")]
    private List<MarkingGradient> _markingGradients;

    /// <summary>
    /// The colors taken on by the marking
    /// </summary>
    public IReadOnlyList<Color> MarkingColors => _markingColors;

    public IReadOnlyList<MarkingGradient> MarkingGradients => _markingGradients ?? [];

    /// <summary>
    /// Whether the marking is forced regardless of points
    /// </summary>
    public bool Forced;

    public Marking()
    {
        _markingColors = new();
        _markingGradients = new();
    }

    public Marking(ProtoId<MarkingPrototype> markingId, IEnumerable<Color> colors)
    {
        MarkingId = markingId;
        _markingColors = colors.ToList();
        _markingGradients = Enumerable.Repeat(MarkingGradient.None, _markingColors.Count).ToList();
    }

    public Marking(ProtoId<MarkingPrototype> markingId, IEnumerable<Color> colors, IEnumerable<MarkingGradient> gradients)
    {
        MarkingId = markingId;
        _markingColors = colors.ToList();
        _markingGradients = gradients.ToList();

        if (_markingGradients.Count != _markingColors.Count)
            _markingGradients = Enumerable.Repeat(MarkingGradient.None, _markingColors.Count).ToList();
    }

    public Marking(ProtoId<MarkingPrototype> markingId, int colorsCount) : this(markingId,
        Enumerable.Repeat(Color.White, colorsCount).ToList())
    {
    }

    public bool Equals(Marking other)
    {
        return MarkingId.Equals(other.MarkingId)
            && MarkingColors.SequenceEqual(other.MarkingColors)
            && MarkingGradients.SequenceEqual(other.MarkingGradients)
            && Forced.Equals(other.Forced);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(MarkingId, MarkingColors, MarkingGradients, Forced);
    }

    public Marking WithColor(Color color) =>
        this with { _markingColors = Enumerable.Repeat(color, MarkingColors.Count).ToList() };

    public Marking WithColorAt(int index, Color color)
    {
        var newColors = _markingColors.ShallowClone();
        newColors[index] = color;
        return this with { _markingColors = newColors };
    }

    public Marking WithGradientAt(int index, MarkingGradient gradient)
    {
        var newGradients = _markingGradients?.ShallowClone() ??
            Enumerable.Repeat(MarkingGradient.None, MarkingColors.Count).ToList();
        newGradients[index] = gradient;
        return this with { _markingGradients = newGradients };
    }

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

        return $"{sanitizedName}@{String.Join(',', colorStringList)}";
    }

    public static Marking? ParseFromDbString(string input)
    {
        if (input.Length == 0) return null;
        var split = input.Split('@');
        if (split.Length != 2) return null;
        List<Color> colorList = new();
        foreach (string color in split[1].Split(','))
        {
            colorList.Add(Color.FromHex(color));
        }

        return new Marking(split[0], colorList);
    }
}

[DataDefinition, Serializable, NetSerializable]
public partial record struct MarkingGradient
{
    public static readonly MarkingGradient None = new(MarkingGradientType.None, Color.White, 90f, 0f, 1f);

    [DataField]
    public MarkingGradientType Type;

    [DataField]
    public Color Color;

    /// <summary>
    /// Direction of the gradient in degrees. Radial gradients ignore this.
    /// </summary>
    [DataField]
    public float Angle;

    /// <summary>
    /// Shifts the gradient center along its direction. Range is expected to be -1..1.
    /// </summary>
    [DataField]
    public float Offset;

    /// <summary>
    /// Controls how soft the transition is. Lower values are sharper, higher values are smoother.
    /// </summary>
    [DataField]
    public float Softness;

    public MarkingGradient(MarkingGradientType type, Color color)
        : this(type, color, DefaultAngle(type), 0f, 1f)
    {
    }

    public MarkingGradient(MarkingGradientType type, Color color, float angle, float offset, float softness)
    {
        Type = type;
        Color = color;
        Angle = angle;
        Offset = offset;
        Softness = softness;
    }

    public static float DefaultAngle(MarkingGradientType type) => type switch
    {
        MarkingGradientType.Horizontal => 0f,
        MarkingGradientType.Diagonal => 45f,
        MarkingGradientType.ReverseDiagonal => 135f,
        _ => 90f,
    };
}

[Serializable, NetSerializable]
public enum MarkingGradientType : byte
{
    None,
    Vertical,
    Horizontal,
    Diagonal,
    ReverseDiagonal,
    Radial,
}
