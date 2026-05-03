using Content.Server._Erida.GameTicking.Components;
using Content.Server.Chat.Managers;
using Content.Server.Chat.Systems;
using Content.Server.GameTicking.Rules;
using Content.Shared.GameTicking.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;

namespace Content.Server._Erida.GameTicking;

public sealed class EridaApocalypseRuleSystem : GameRuleSystem<EridaApocalypseRuleComponent>
{
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    protected override void Started(EntityUid uid, EridaApocalypseRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);
        component.TimeUntilAnnouncement = component.AnnouncementDelay;
        component.Announced = false;

        if (component.TimeUntilAnnouncement <= 0f)
            SendAnnouncement(component);
    }

    protected override void ActiveTick(EntityUid uid, EridaApocalypseRuleComponent component, GameRuleComponent gameRule, float frameTime)
    {
        base.ActiveTick(uid, component, gameRule, frameTime);

        if (component.Announced)
            return;

        component.TimeUntilAnnouncement -= frameTime;
        if (component.TimeUntilAnnouncement > 0f)
            return;

        SendAnnouncement(component);
    }

    private void SendAnnouncement(EridaApocalypseRuleComponent component)
    {
        if (component.Announced)
            return;

        component.Announced = true;
        _chatManager.DispatchServerAnnouncement(
            Loc.GetString(component.Announcement),
            colorOverride: component.AnnouncementColor);
        _audio.PlayGlobal(component.AnnouncementSound, Filter.Broadcast(), true);
    }
}
