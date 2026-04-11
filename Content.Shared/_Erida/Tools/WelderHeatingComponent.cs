using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Content.Shared.Tools;
using JetBrains.Annotations;
using Robust.Shared.Audio;

namespace Content.Shared._Erida.WelderHeating;


[RegisterComponent, NetworkedComponent]
public sealed partial class WelderHeatingComponent : Component
{

    // Ой жоске нагревайка
    [DataField]
    public float HeatPerUse = 5000f;

    // Чо за аппарат..?
    [DataField]
    public ProtoId<ToolQualityPrototype> RequiredQuality = "Welding";


    public SoundSpecifier HeatSound = new SoundPathSpecifier("/Audio/Effects/Chemistry/bubbles.ogg");
    public LocId Popup = "warming-with-welder";

    [DataField]
    public float SoundCooldown = 1.5f;

    public TimeSpan NextSoundAt;
}
