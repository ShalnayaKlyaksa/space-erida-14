using Robust.Shared.Audio;
using Robust.Shared.Utility;

namespace Content.Server._Erida.GameTicking.Components;

[RegisterComponent, Access(typeof(EridaApocalypseRuleSystem))]
public sealed partial class EridaApocalypseRuleComponent : Component
{
    [DataField]
    public float AnnouncementDelay = 600f;

    [DataField]
    public float TimeUntilAnnouncement;

    [DataField]
    public bool Announced;

    [DataField]
    public LocId Announcement = "erida-apocalypse-warning";

    [DataField]
    public Color AnnouncementColor = Color.FromHex("#d10000");

    [DataField]
    public SoundSpecifier AnnouncementSound = new SoundPathSpecifier("/Audio/Voice/Cluwne/cluwnelaugh1.ogg");

    [DataField]
    public string? AnnouncementSender;
}
