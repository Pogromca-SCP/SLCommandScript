using CommandSystem;
using LabApi.Events.Arguments.ObjectiveEvents;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.Scp0492Events;
using LabApi.Events.Arguments.Scp049Events;
using LabApi.Events.Arguments.Scp079Events;
using LabApi.Events.Arguments.Scp096Events;
using LabApi.Events.Arguments.Scp106Events;
using LabApi.Events.Arguments.Scp127Events;
using LabApi.Events.Arguments.Scp173Events;
using LabApi.Events.Arguments.Scp3114Events;
using LabApi.Events.Arguments.Scp914Events;
using LabApi.Events.Arguments.Scp939Events;
using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.Arguments.WarheadEvents;
using LabApi.Events.CustomHandlers;
using LabApi.Features.Console;
using System.Collections.Generic;

namespace SLCommandScript.FileScriptsLoader.Events;

/// <summary>
/// Handles server events using script files.
/// </summary>
public class FileScriptsEventHandler : CustomEventsHandler
{
    /// <summary>
    /// Contains registered event handling scripts.
    /// </summary>
    public Dictionary<EventType, ICommand?> EventScripts { get; } = [];

    /// <summary>
    /// Executes a command handler.
    /// </summary>
    /// <param name="eventType">Type of event to handle.</param>
    /// <param name="args">Event arguments to use.</param>
    private void HandleEvent(EventType eventType, params string[] args)
    {
        if (!EventScripts.TryGetValue(eventType, out var cmd) || cmd is null)
        {
            return;
        }

        var result = cmd.Execute(new(args, 1, args.Length - 1), ServerConsole.Scs, out var message);

        if (!result)
        {
            Logger.Error(message);
        }
    }

    public override void OnPlayerJoined(PlayerJoinedEventArgs ev) => HandleEvent(EventType.PlayerJoin, nameof(EventType.PlayerJoin), ev.Player.PlayerId.ToString(),
        ev.Player.DisplayName);

    public override void OnPlayerLeft(PlayerLeftEventArgs ev) => HandleEvent(EventType.PlayerLeave, nameof(EventType.PlayerLeave), ev.Player.PlayerId.ToString(),
        ev.Player.DisplayName);

    public override void OnPlayerBanned(PlayerBannedEventArgs ev)
    {
        if (ev.Player is null)
        {
            return;
        }

        HandleEvent(EventType.PlayerBan, nameof(EventType.PlayerBan), ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.Reason);
    }

    public override void OnPlayerKicked(PlayerKickedEventArgs ev) => HandleEvent(EventType.PlayerKick, nameof(EventType.PlayerKick), ev.Player.PlayerId.ToString(),
        ev.Player.DisplayName, ev.Reason);

    public override void OnPlayerReportedCheater(PlayerReportedCheaterEventArgs ev) => HandleEvent(EventType.PlayerCheaterReport, nameof(EventType.PlayerCheaterReport),
        ev.Target.PlayerId.ToString(), ev.Target.DisplayName, ev.Reason);

    public override void OnPlayerChangedRole(PlayerChangedRoleEventArgs ev) => HandleEvent(EventType.PlayerChangeRole, nameof(EventType.PlayerChangeRole),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.OldRole.ToString(), ev.NewRole.RoleTypeId.ToString(), ev.ChangeReason.ToString());

    public override void OnPlayerPreAuthenticated(PlayerPreAuthenticatedEventArgs ev) => HandleEvent(EventType.PlayerPreauth, nameof(EventType.PlayerPreauth),
        ev.Region);

    public override void OnPlayerGroupChanged(PlayerGroupChangedEventArgs ev) => HandleEvent(EventType.PlayerGetGroup, nameof(EventType.PlayerGetGroup),
        ev.Player.PlayerId.ToString(), ev.Group.BadgeText);

    public override void OnPlayerReportedPlayer(PlayerReportedPlayerEventArgs ev) => HandleEvent(EventType.PlayerReport, nameof(EventType.PlayerReport),
        ev.Target.PlayerId.ToString(), ev.Target.DisplayName, ev.Reason);

    public override void OnPlayerSpawnedRagdoll(PlayerSpawnedRagdollEventArgs ev) => HandleEvent(EventType.RagdollSpawn, nameof(EventType.RagdollSpawn),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.Ragdoll.Role.ToString());

    public override void OnPlayerMuted(PlayerMutedEventArgs ev) => HandleEvent(EventType.PlayerMute, nameof(EventType.PlayerMute), ev.Issuer.PlayerId.ToString(),
        ev.Issuer.DisplayName, ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.IsIntercom.ToString());

    public override void OnPlayerUnmuted(PlayerUnmutedEventArgs ev) => HandleEvent(EventType.PlayerUnmute, nameof(EventType.PlayerUnmute),
        ev.Issuer.PlayerId.ToString(), ev.Issuer.DisplayName, ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.IsIntercom.ToString());

    public override void OnPlayerToggledNoclip(PlayerToggledNoclipEventArgs ev) => HandleEvent(EventType.PlayerToggleNoclip, nameof(EventType.PlayerToggleNoclip),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.IsNoclipping.ToString());

    public override void OnPlayerChangedNickname(PlayerChangedNicknameEventArgs ev) => HandleEvent(EventType.PlayerChangeNickname, nameof(EventType.PlayerChangeNickname),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.OldNickname ?? string.Empty, ev.NewNickname ?? string.Empty);

    public override void OnPlayerValidatedVisibility(PlayerValidatedVisibilityEventArgs ev) => HandleEvent(EventType.PlayerValidateVisibility,
        nameof(EventType.PlayerValidateVisibility), ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.Target.PlayerId.ToString(), ev.Target.DisplayName,
        ev.IsVisible.ToString());

    public override void OnPlayerDeath(PlayerDeathEventArgs ev)
    {
        const EventType eventType = EventType.PlayerDeath;
        const string eventName = nameof(EventType.PlayerDeath);

        if (ev.Attacker is null)
        {
            HandleEvent(eventType, eventName, ev.Player.PlayerId.ToString(), ev.Player.DisplayName);
        }
        else
        {
            HandleEvent(eventType, eventName, ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.Attacker.PlayerId.ToString(), ev.Attacker.DisplayName);
        }
    }

    public override void OnPlayerChangedSpectator(PlayerChangedSpectatorEventArgs ev)
    {
        const EventType eventType = EventType.PlayerChangeSpectator;
        const string eventName = nameof(EventType.PlayerChangeSpectator);

        if (ev.OldTarget is null)
        {
            HandleEvent(eventType, eventName, ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.NewTarget.PlayerId.ToString(), ev.NewTarget.DisplayName);
}
        else
        {
            HandleEvent(eventType, eventName, ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.NewTarget.PlayerId.ToString(), ev.NewTarget.DisplayName,
                ev.OldTarget.PlayerId.ToString(), ev.OldTarget.DisplayName);
        }
    }

    public override void OnPlayerEscaped(PlayerEscapedEventArgs ev) => HandleEvent(EventType.PlayerEscape, nameof(EventType.PlayerEscape),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.NewRole.ToString(), ev.EscapeScenarioType.ToString(), ev.OldRole.ToString());

    public override void OnPlayerHurt(PlayerHurtEventArgs ev)
    {
        const EventType eventType = EventType.PlayerDamage;
        const string eventName = nameof(EventType.PlayerDamage);

        if (ev.Attacker is null)
        {
            HandleEvent(eventType, eventName, ev.Player.PlayerId.ToString(), ev.Player.DisplayName);
        }
        else
        {
            HandleEvent(eventType, eventName, ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.Attacker.PlayerId.ToString(), ev.Attacker.DisplayName);
        }
    }

    public override void OnPlayerUpdatedEffect(PlayerEffectUpdatedEventArgs ev) => HandleEvent(EventType.PlayerUpdateEffect, nameof(EventType.PlayerUpdateEffect),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.Effect.Classification.ToString(), ev.Intensity.ToString(), ev.Duration.ToString());

    public override void OnPlayerSpawned(PlayerSpawnedEventArgs ev) => HandleEvent(EventType.PlayerSpawn, nameof(EventType.PlayerSpawn), ev.Player.PlayerId.ToString(),
        ev.Player.DisplayName, ev.Role.RoleName, ev.UseSpawnPoint.ToString());

    public override void OnPlayerEnteredPocketDimension(PlayerEnteredPocketDimensionEventArgs ev) => HandleEvent(EventType.PlayerEnterPocketDimension,
        nameof(EventType.PlayerEnterPocketDimension), ev.Player.PlayerId.ToString(), ev.Player.DisplayName);

    public override void OnPlayerLeftPocketDimension(PlayerLeftPocketDimensionEventArgs ev) => HandleEvent(EventType.PlayerExitPocketDimension,
        nameof(EventType.PlayerExitPocketDimension), ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.IsSuccessful.ToString());

    public override void OnPlayerCuffed(PlayerCuffedEventArgs ev) => HandleEvent(EventType.PlayerHandcuff, nameof(EventType.PlayerHandcuff),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.Target.PlayerId.ToString(), ev.Target.DisplayName);

    public override void OnPlayerUncuffed(PlayerUncuffedEventArgs ev) => HandleEvent(EventType.PlayerRemoveHandcuffs, nameof(EventType.PlayerRemoveHandcuffs),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.Target.PlayerId.ToString(), ev.Target.DisplayName);

    public override void OnPlayerActivatedGenerator(PlayerActivatedGeneratorEventArgs ev) => HandleEvent(EventType.PlayerActivateGenerator,
        nameof(EventType.PlayerActivateGenerator), ev.Player.PlayerId.ToString(), ev.Player.DisplayName);

    public override void OnPlayerUsedIntercom(PlayerUsedIntercomEventArgs ev)
    {
        if (ev.Player is null)
        {
            return;
        }

        HandleEvent(EventType.PlayerUseIntercom, nameof(EventType.PlayerUseIntercom), ev.Player.PlayerId.ToString(), ev.Player.DisplayName,
            ev.State.ToString());
    }

    public override void OnPlayerClosedGenerator(PlayerClosedGeneratorEventArgs ev) => HandleEvent(EventType.PlayerCloseGenerator,
        nameof(EventType.PlayerCloseGenerator), ev.Player.PlayerId.ToString(), ev.Player.DisplayName);

    public override void OnPlayerDamagedShootingTarget(PlayerDamagedShootingTargetEventArgs ev) => HandleEvent(EventType.PlayerDamageShootingTarget,
        nameof(EventType.PlayerDamageShootingTarget), ev.Player.PlayerId.ToString(), ev.Player.DisplayName);

    public override void OnPlayerDamagedWindow(PlayerDamagedWindowEventArgs ev) => HandleEvent(EventType.PlayerDamageWindow, nameof(EventType.PlayerDamageWindow),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName);

    public override void OnPlayerDeactivatedGenerator(PlayerDeactivatedGeneratorEventArgs ev) => HandleEvent(EventType.PlayerDeactivateGenerator,
        nameof(EventType.PlayerDeactivateGenerator), ev.Player.PlayerId.ToString(), ev.Player.DisplayName);

    public override void OnPlayerOpenedGenerator(PlayerOpenedGeneratorEventArgs ev) => HandleEvent(EventType.PlayerOpenGenerator, nameof(EventType.PlayerOpenGenerator),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName);

    public override void OnPlayerInteractedShootingTarget(PlayerInteractedShootingTargetEventArgs ev) => HandleEvent(EventType.PlayerInteractShootingTarget,
        nameof(EventType.PlayerInteractShootingTarget), ev.Player.PlayerId.ToString(), ev.Player.DisplayName);

    public override void OnPlayerInteractedLocker(PlayerInteractedLockerEventArgs ev) => HandleEvent(EventType.PlayerInteractLocker,
        nameof(EventType.PlayerInteractLocker), ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.CanOpen.ToString());

    public override void OnPlayerInteractedElevator(PlayerInteractedElevatorEventArgs ev) => HandleEvent(EventType.PlayerInteractElevator,
        nameof(EventType.PlayerInteractElevator), ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.Elevator.CurrentSequence.ToString(),
        ev.Elevator.GoingUp.ToString());

    public override void OnPlayerUnlockedGenerator(PlayerUnlockedGeneratorEventArgs ev) => HandleEvent(EventType.PlayerUnlockGenerator,
        nameof(EventType.PlayerUnlockGenerator), ev.Player.PlayerId.ToString(), ev.Player.DisplayName);

    public override void OnPlayerInteractedDoor(PlayerInteractedDoorEventArgs ev) => HandleEvent(EventType.PlayerInteractDoor, nameof(EventType.PlayerInteractDoor),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.Door.DoorName.ToString(), ev.CanOpen.ToString());

    public override void OnPlayerInteractedGenerator(PlayerInteractedGeneratorEventArgs ev) => HandleEvent(EventType.PlayerInteractGenerator,
        nameof(EventType.PlayerInteractGenerator), ev.Player.PlayerId.ToString(), ev.Player.DisplayName);

    public override void OnPlayerInteractedToy(PlayerInteractedToyEventArgs ev) => HandleEvent(EventType.PlayerInteractToy, nameof(EventType.PlayerInteractToy),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.Interactable.CanSearch.ToString());

    public override void OnPlayerSearchedToy(PlayerSearchedToyEventArgs ev) => HandleEvent(EventType.PlayerSearchToy, nameof(EventType.PlayerSearchToy),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.Interactable.CanSearch.ToString());

    public override void OnPlayerSearchToyAborted(PlayerSearchToyAbortedEventArgs ev) => HandleEvent(EventType.PlayerSearchToyAbort,
        nameof(EventType.PlayerSearchToyAbort), ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.Interactable.CanSearch.ToString());

    public override void OnPlayerAimedWeapon(PlayerAimedWeaponEventArgs ev) => HandleEvent(EventType.PlayerAimWeapon, nameof(EventType.PlayerAimWeapon),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.FirearmItem.Type.ToString(), ev.Aiming.ToString());

    public override void OnPlayerDryFiredWeapon(PlayerDryFiredWeaponEventArgs ev) => HandleEvent(EventType.PlayerDryfireWeapon, nameof(EventType.PlayerDryfireWeapon),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.FirearmItem.Type.ToString());

    public override void OnPlayerReloadedWeapon(PlayerReloadedWeaponEventArgs ev) => HandleEvent(EventType.PlayerReloadWeapon, nameof(EventType.PlayerReloadWeapon),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.FirearmItem.Type.ToString());

    public override void OnPlayerShotWeapon(PlayerShotWeaponEventArgs ev) => HandleEvent(EventType.PlayerShootWeapon, nameof(EventType.PlayerShootWeapon),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.FirearmItem.Type.ToString());

    public override void OnPlayerUnloadedWeapon(PlayerUnloadedWeaponEventArgs ev) => HandleEvent(EventType.PlayerUnloadWeapon, nameof(EventType.PlayerUnloadWeapon),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.FirearmItem.Type.ToString());

    public override void OnPlayerToggledWeaponFlashlight(PlayerToggledWeaponFlashlightEventArgs ev) => HandleEvent(EventType.PlayerToggleWeaponFlashlight,
        nameof(EventType.PlayerToggleWeaponFlashlight), ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.FirearmItem.Type.ToString(),
        ev.NewState.ToString());

    public override void OnPlayerCancelledUsingItem(PlayerCancelledUsingItemEventArgs ev) => HandleEvent(EventType.PlayerCancelUsingItem,
        nameof(EventType.PlayerCancelUsingItem), ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.UsableItem.Type.ToString());

    public override void OnPlayerChangedItem(PlayerChangedItemEventArgs ev) => HandleEvent(EventType.PlayerChangeItem, nameof(EventType.PlayerChangeItem),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.OldItem?.Type.ToString() ?? string.Empty, ev.NewItem?.Type.ToString() ?? string.Empty);

    public override void OnPlayerDroppedAmmo(PlayerDroppedAmmoEventArgs ev) => HandleEvent(EventType.PlayerDropAmmo, nameof(EventType.PlayerDropAmmo),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.AmmoPickup.Type.ToString(), ev.Amount.ToString());

    public override void OnPlayerDroppedItem(PlayerDroppedItemEventArgs ev) => HandleEvent(EventType.PlayerDropItem, nameof(EventType.PlayerDropItem),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.Pickup.Type.ToString(), ev.Throw.ToString());

    public override void OnPlayerPickedUpItem(PlayerPickedUpItemEventArgs ev) => HandleEvent(EventType.PlayerPickupItem, nameof(EventType.PlayerPickupItem),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.Item.Type.ToString());

    public override void OnPlayerPickedUpAmmo(PlayerPickedUpAmmoEventArgs ev) => HandleEvent(EventType.PlayerPickupAmmo, nameof(EventType.PlayerPickupAmmo),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.AmmoType.ToString(), ev.AmmoAmount.ToString());

    public override void OnPlayerPickedUpArmor(PlayerPickedUpArmorEventArgs ev)
    {
        if (ev.BodyArmorItem is null)
        {
            return;
        }

        HandleEvent(EventType.PlayerPickupArmor, nameof(EventType.PlayerPickupArmor), ev.Player.PlayerId.ToString(), ev.Player.DisplayName,
            ev.BodyArmorItem.Type.ToString());
    }

    public override void OnPlayerPickedUpScp330(PlayerPickedUpScp330EventArgs ev) => HandleEvent(EventType.PlayerPickupScp330, nameof(EventType.PlayerPickupScp330),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.CandyPickup.Type.ToString());

    public override void OnPlayerSearchedPickup(PlayerSearchedPickupEventArgs ev) => HandleEvent(EventType.PlayerSearchPickup, nameof(EventType.PlayerSearchPickup),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.Pickup.Type.ToString());

    public override void OnPlayerSearchedAmmo(PlayerSearchedAmmoEventArgs ev) => HandleEvent(EventType.PlayerSearchAmmo, nameof(EventType.PlayerSearchAmmo),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.AmmoPickup.Type.ToString());

    public override void OnPlayerSearchedArmor(PlayerSearchedArmorEventArgs ev) => HandleEvent(EventType.PlayerSearchArmor, nameof(EventType.PlayerSearchArmor),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.BodyArmorPickup.Type.ToString());

    public override void OnPlayerThrewItem(PlayerThrewItemEventArgs ev) => HandleEvent(EventType.PlayerThrowItem, nameof(EventType.PlayerThrowItem),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.Pickup.Type.ToString());

    public override void OnPlayerToggledFlashlight(PlayerToggledFlashlightEventArgs ev) => HandleEvent(EventType.PlayerToggleFlashlight,
        nameof(EventType.PlayerToggleFlashlight), ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.NewState.ToString());

    public override void OnPlayerUsedItem(PlayerUsedItemEventArgs ev) => HandleEvent(EventType.PlayerUseItem, nameof(EventType.PlayerUseItem),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.UsableItem.Type.ToString());

    public override void OnPlayerInteractedScp330(PlayerInteractedScp330EventArgs ev) => HandleEvent(EventType.PlayerInteractScp330,
        nameof(EventType.PlayerInteractScp330), ev.Player.PlayerId.ToString(), ev.Player.DisplayName);

    public override void OnPlayerThrewProjectile(PlayerThrewProjectileEventArgs ev) => HandleEvent(EventType.PlayerThrowProjectile,
        nameof(EventType.PlayerThrowProjectile), ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.ThrowableItem.Type.ToString());

    public override void OnPlayerFlippedCoin(PlayerFlippedCoinEventArgs ev) => HandleEvent(EventType.PlayerCoinFlip, nameof(EventType.PlayerCoinFlip),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.IsTails.ToString());

    public override void OnPlayerReceivedLoadout(PlayerReceivedLoadoutEventArgs ev) => HandleEvent(EventType.PlayerReceiveLoadout,
        nameof(EventType.PlayerReceiveLoadout), ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.InventoryReset.ToString());

    public override void OnPlayerChangedRadioRange(PlayerChangedRadioRangeEventArgs ev) => HandleEvent(EventType.PlayerChangeRadioRange,
        nameof(EventType.PlayerChangeRadioRange), ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.Range.ToString());

    public override void OnPlayerToggledRadio(PlayerToggledRadioEventArgs ev) => HandleEvent(EventType.PlayerToggleRadio, nameof(EventType.PlayerToggleRadio),
        ev.NewState.ToString(), ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.NewState.ToString());

    public override void OnPlayerUsedRadio(PlayerUsedRadioEventArgs ev) => HandleEvent(EventType.PlayerUseRadio, nameof(EventType.PlayerUseRadio),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.Drain.ToString());

    public override void OnServerLczDecontaminationStarted() => HandleEvent(EventType.LczDecontaminationStart, nameof(EventType.LczDecontaminationStart));

    public override void OnServerLczDecontaminationAnnounced(LczDecontaminationAnnouncedEventArgs ev) => HandleEvent(EventType.LczDecontaminationAnnouncement,
        nameof(EventType.LczDecontaminationAnnouncement), ev.Phase.ToString());

    public override void OnServerMapGenerated(MapGeneratedEventArgs ev) => HandleEvent(EventType.MapGenerate, nameof(EventType.MapGenerate), ev.Seed.ToString());

    public override void OnServerItemSpawned(ItemSpawnedEventArgs ev) => HandleEvent(EventType.ItemSpawn, nameof(EventType.ItemSpawn),
        ev.Pickup.Type.ToString());

    public override void OnServerPickupCreated(PickupCreatedEventArgs ev) => HandleEvent(EventType.CreatePickup, nameof(EventType.CreatePickup),
        ev.Pickup.Type.ToString());

    public override void OnServerPickupDestroyed(PickupDestroyedEventArgs ev) => HandleEvent(EventType.DestroyPickup, nameof(EventType.DestroyPickup),
        ev.Pickup.Type.ToString());

    public override void OnServerGeneratorActivated(GeneratorActivatedEventArgs ev) => HandleEvent(EventType.GeneratorActivate, nameof(EventType.GeneratorActivate));

    public override void OnPlayerPlacedBlood(PlayerPlacedBloodEventArgs ev) => HandleEvent(EventType.PlaceBlood, nameof(EventType.PlaceBlood),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.Attacker.PlayerId.ToString(), ev.Attacker.DisplayName);

    public override void OnPlayerPlacedBulletHole(PlayerPlacedBulletHoleEventArgs ev) => HandleEvent(EventType.PlaceBulletHole, nameof(EventType.PlaceBulletHole),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.DecalType.ToString());

    public override void OnPlayerEnteredHazard(PlayerEnteredHazardEventArgs ev) => HandleEvent(EventType.PlayerEnterHazard, nameof(EventType.PlayerEnterHazard),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.Hazard.IsActive.ToString());

    public override void OnPlayerLeftHazard(PlayerLeftHazardEventArgs ev) => HandleEvent(EventType.PlayerLeaveHazard, nameof(EventType.PlayerLeaveHazard),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.Hazard.IsActive.ToString());

    public override void OnPlayerIdledTesla(PlayerIdledTeslaEventArgs ev) => HandleEvent(EventType.PlayerIdleTesla, nameof(EventType.PlayerIdleTesla),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName);

    public override void OnPlayerTriggeredTesla(PlayerTriggeredTeslaEventArgs ev) => HandleEvent(EventType.PlayerTriggerTesla, nameof(EventType.PlayerTriggerTesla),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName);

    public override void OnPlayerUnlockedWarheadButton(PlayerUnlockedWarheadButtonEventArgs ev) => HandleEvent(EventType.PlayerUnlockWarhead,
        nameof(EventType.PlayerUnlockWarhead), ev.Player.PlayerId.ToString(), ev.Player.DisplayName);

    public override void OnServerCassieAnnounced(CassieAnnouncedEventArgs ev) => HandleEvent(EventType.CassieAnnouncement, nameof(EventType.CassieAnnouncement),
        ev.CustomAnnouncement.ToString(), ev.MakeHold.ToString(), ev.MakeNoise.ToString(), ev.Words);

    public override void OnServerExplosionSpawned(ExplosionSpawnedEventArgs ev)
    {
        const EventType eventType = EventType.ExplosionSpawn;
        const string eventName = nameof(EventType.ExplosionSpawn);

        if (ev.Player is null)
        {
            HandleEvent(eventType, eventName, ev.ExplosionType.ToString());
}
        else
        {
            HandleEvent(eventType, eventName, ev.ExplosionType.ToString(), ev.Player.PlayerId.ToString(), ev.Player.DisplayName);
        }
    }

    public override void OnServerProjectileExploded(ProjectileExplodedEventArgs ev)
    {
        const EventType eventType = EventType.ProjectileExplode;
        const string eventName = nameof(EventType.ProjectileExplode);

        if (ev.Player is null)
        {
            HandleEvent(eventType, eventName, ev.TimedGrenade.Type.ToString());
        }
        else
        {
            HandleEvent(eventType, eventName, ev.TimedGrenade.Type.ToString(), ev.Player.PlayerId.ToString(), ev.Player.DisplayName);
        }
    }

    public override void OnServerSentAdminChat(SentAdminChatEventArgs ev) => HandleEvent(EventType.AdminChat, nameof(EventType.AdminChat));

    public override void OnServerRoundEnded(RoundEndedEventArgs ev) => HandleEvent(EventType.RoundEnd, nameof(EventType.RoundEnd), ev.LeadingTeam.ToString(),
        ev.ShowSummary.ToString());

    public override void OnServerRoundRestarted() => HandleEvent(EventType.RoundRestart, nameof(EventType.RoundRestart));

    public override void OnServerRoundStarted() => HandleEvent(EventType.RoundStart, nameof(EventType.RoundStart));

    public override void OnServerWaitingForPlayers() => HandleEvent(EventType.WaitingForPlayers, nameof(EventType.WaitingForPlayers));

    public override void OnServerWaveTeamSelected(WaveTeamSelectedEventArgs ev) => HandleEvent(EventType.TeamRespawnSelection, nameof(EventType.TeamRespawnSelection),
        ev.Wave.Faction.ToString());

    public override void OnServerWaveRespawned(WaveRespawnedEventArgs ev) => HandleEvent(EventType.TeamRespawn, nameof(EventType.TeamRespawn),
        ev.Wave.Faction.ToString(), ev.Players.Count.ToString());

    public override void OnWarheadStarted(WarheadStartedEventArgs ev) => HandleEvent(EventType.WarheadStart, nameof(EventType.WarheadStart),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName);

    public override void OnWarheadStopped(WarheadStoppedEventArgs ev) => HandleEvent(EventType.WarheadStop, nameof(EventType.WarheadStop),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName);

    public override void OnWarheadDetonated(WarheadDetonatedEventArgs ev) => HandleEvent(EventType.WarheadDetonation, nameof(EventType.WarheadDetonation),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName);

    public override void OnServerCommandExecuted(CommandExecutedEventArgs ev)
    {
        const EventType eventType = EventType.ExecuteCommand;
        const string eventName = nameof(EventType.ExecuteCommand);

        if (ev.Sender is null)
        {
            HandleEvent(eventType, eventName, ev.Command.Command);
        }
        else
        {
            HandleEvent(eventType, eventName, ev.Command.Command, ev.Sender.SenderId, ev.Sender.Nickname);
        }
    }

    public override void OnServerCassieQueuedScpTermination(CassieQueuedScpTerminationEventArgs ev) => HandleEvent(EventType.CassieQueueScpTermination,
        nameof(EventType.CassieQueueScpTermination), ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.Player.Role.ToString(), ev.Announcement);

    public override void OnScp914Activated(Scp914ActivatedEventArgs ev) => HandleEvent(EventType.Scp914Activate, nameof(EventType.Scp914Activate),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.KnobSetting.ToString());

    public override void OnScp914KnobChanged(Scp914KnobChangedEventArgs ev) => HandleEvent(EventType.Scp914KnobChange, nameof(EventType.Scp914KnobChange),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.KnobSetting.ToString());

    public override void OnScp914ProcessedInventoryItem(Scp914ProcessedInventoryItemEventArgs ev) => HandleEvent(EventType.Scp914InventoryItemUpgrade,
        nameof(EventType.Scp914InventoryItemUpgrade), ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.Item.Type.ToString(), ev.KnobSetting.ToString());

    public override void OnScp914ProcessedPickup(Scp914ProcessedPickupEventArgs ev)
    {
        if (ev.Pickup is null)
        {
            return;
        }

        HandleEvent(EventType.Scp914PickupUpgrade, nameof(EventType.Scp914PickupUpgrade), ev.Pickup.Type.ToString(), ev.KnobSetting.ToString());
    }

    public override void OnScp914ProcessedPlayer(Scp914ProcessedPlayerEventArgs ev) => HandleEvent(EventType.Scp914ProcessPlayer,
        nameof(EventType.Scp914ProcessPlayer), ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.Player.Role.ToString(), ev.KnobSetting.ToString());

    public override void OnScp106ChangedStalkMode(Scp106ChangedStalkModeEventArgs ev) => HandleEvent(EventType.Scp106Stalk, nameof(EventType.Scp106Stalk),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.IsStalkActive.ToString());

    public override void OnScp106ChangedSubmersionStatus(Scp106ChangedSubmersionStatusEventArgs ev) => HandleEvent(EventType.Scp106Submerge,
        nameof(EventType.Scp106Submerge), ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.IsSubmerging.ToString());

    public override void OnScp106ChangedVigor(Scp106ChangedVigorEventArgs ev) => HandleEvent(EventType.Scp106VigorChange, nameof(EventType.Scp106VigorChange),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.OldValue.ToString(), ev.Value.ToString());

    public override void OnScp106TeleportedPlayer(Scp106TeleportedPlayerEvent ev) => HandleEvent(EventType.Scp106TeleportPlayer, nameof(EventType.Scp106TeleportPlayer),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.Target.PlayerId.ToString(), ev.Target.DisplayName);

    public override void OnScp106UsedHunterAtlas(Scp106UsedHunterAtlasEventArgs ev) => HandleEvent(EventType.Scp106UseHunterAtlas,
        nameof(EventType.Scp106UseHunterAtlas), ev.Player.PlayerId.ToString(), ev.Player.DisplayName);

    public override void OnScp173PlayedSound(Scp173PlayedSoundEventArgs ev) => HandleEvent(EventType.Scp173PlaySound, nameof(EventType.Scp173PlaySound),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.SoundId.ToString());

    public override void OnScp173BreakneckSpeedChanged(Scp173BreakneckSpeedChangedEventArgs ev) => HandleEvent(EventType.Scp173BreakneckSpeeds,
        nameof(EventType.Scp173BreakneckSpeeds), ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.Active.ToString());

    public override void OnScp173AddedObserver(Scp173AddedObserverEventArgs ev) => HandleEvent(EventType.Scp173NewObserver, nameof(EventType.Scp173NewObserver),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.Target.PlayerId.ToString(), ev.Target.DisplayName);

    public override void OnScp173RemovedObserver(Scp173RemovedObserverEventArgs ev) => HandleEvent(EventType.Scp173RemoveObserver,
        nameof(EventType.Scp173RemoveObserver), ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.Target.PlayerId.ToString(), ev.Target.DisplayName);

    public override void OnScp173CreatedTantrum(Scp173CreatedTantrumEventArgs ev) => HandleEvent(EventType.Scp173CreateTantrum, nameof(EventType.Scp173CreateTantrum),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName);

    public override void OnScp939CreatedAmnesticCloud(Scp939CreatedAmnesticCloudEventArgs ev) => HandleEvent(EventType.Scp939CreateAmnesticCloud,
        nameof(EventType.Scp939CreateAmnesticCloud), ev.Player.PlayerId.ToString(), ev.Player.DisplayName);

    public override void OnScp939Lunged(Scp939LungedEventArgs ev) => HandleEvent(EventType.Scp939Lunge, nameof(EventType.Scp939Lunge), ev.Player.PlayerId.ToString(),
        ev.Player.DisplayName, ev.LungeState.ToString());

    public override void OnScp939Attacked(Scp939AttackedEventArgs ev) => HandleEvent(EventType.Scp939Attack, nameof(EventType.Scp939Attack),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.Target.PlayerId.ToString(), ev.Target.DisplayName, ev.Damage.ToString());

    public override void OnScp079GainedExperience(Scp079GainedExperienceEventArgs ev) => HandleEvent(EventType.Scp079GainExperience,
        nameof(EventType.Scp079GainExperience), ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.Amount.ToString(), ev.Reason.ToString());

    public override void OnScp079LeveledUp(Scp079LeveledUpEventArgs ev) => HandleEvent(EventType.Scp079LevelUpTier, nameof(EventType.Scp079LevelUpTier),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.Tier.ToString());

    public override void OnScp079UsedTesla(Scp079UsedTeslaEventArgs ev) => HandleEvent(EventType.Scp079UseTesla, nameof(EventType.Scp079UseTesla),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName);

    public override void OnScp079LockedDownRoom(Scp079LockedDownRoomEventArgs ev) => HandleEvent(EventType.Scp079LockdownRoom, nameof(EventType.Scp079LockdownRoom),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.Room.Name.ToString());

    public override void OnScp079CancelledRoomLockdown(Scp079CancelledRoomLockdownEventArgs ev) => HandleEvent(EventType.Scp079CancelRoomLockdown,
        nameof(EventType.Scp079CancelRoomLockdown), ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.Room.Name.ToString());

    public override void OnScp079LockedDoor(Scp079LockedDoorEventArgs ev) => HandleEvent(EventType.Scp079LockDoor, nameof(EventType.Scp079LockDoor),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.Door.DoorName.ToString());

    public override void OnScp079UnlockedDoor(Scp079UnlockedDoorEventArgs ev) => HandleEvent(EventType.Scp079UnlockDoor, nameof(EventType.Scp079UnlockDoor),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.Door.DoorName.ToString());

    public override void OnScp079BlackedOutZone(Scp079BlackedOutZoneEventArgs ev) => HandleEvent(EventType.Scp079BlackoutZone, nameof(EventType.Scp079BlackoutZone),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.Zone.ToString());

    public override void OnScp079BlackedOutRoom(Scp079BlackedOutRoomEventArgs ev) => HandleEvent(EventType.Scp079BlackoutRoom, nameof(EventType.Scp079BlackoutRoom),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.Room.Name.ToString());

    public override void OnScp079ChangedCamera(Scp079ChangedCameraEventArgs ev) => HandleEvent(EventType.Scp079ChangeCamera, nameof(EventType.Scp079ChangeCamera),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.Camera.Base.Label);

    public override void OnScp079Recontained(Scp079RecontainedEventArgs ev) => HandleEvent(EventType.Scp079Recontainment, nameof(EventType.Scp079Recontainment),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName);

    public override void OnScp049ResurrectedBody(Scp049ResurrectedBodyEventArgs ev) => HandleEvent(EventType.Scp049ResurrectBody, nameof(EventType.Scp049ResurrectBody),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.Target.PlayerId.ToString(), ev.Target.DisplayName);

    public override void OnScp049UsedDoctorsCall(Scp049UsedDoctorsCallEventArgs ev) => HandleEvent(EventType.Scp049UseDoctorsCall,
        nameof(EventType.Scp049UseDoctorsCall), ev.Player.PlayerId.ToString(), ev.Player.DisplayName);

    public override void OnScp049UsedSense(Scp049UsedSenseEventArgs ev) => HandleEvent(EventType.Scp049UseSense, nameof(EventType.Scp049UseSense),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.Target.PlayerId.ToString(), ev.Target.DisplayName);

    public override void OnScp0492ConsumedCorpse(Scp0492ConsumedCorpseEventArgs ev) => HandleEvent(EventType.Scp049_2ConsumeCorpse,
        nameof(EventType.Scp049_2ConsumeCorpse), ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.Ragdoll.Role.ToString());

    public override void OnScp0492StartedConsumingCorpse(Scp0492StartedConsumingCorpseEventArgs ev) => HandleEvent(EventType.Scp049_2StartConsumingCorpse,
        nameof(EventType.Scp049_2StartConsumingCorpse), ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.Ragdoll.Role.ToString());

    public override void OnScp096AddedTarget(Scp096AddedTargetEventArgs ev) => HandleEvent(EventType.Scp096AddTarget, nameof(EventType.Scp096AddTarget),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.Target.PlayerId.ToString(), ev.Target.DisplayName);

    public override void OnScp096Enraged(Scp096EnragedEventArgs ev) => HandleEvent(EventType.Scp096Enrage, nameof(EventType.Scp096Enrage),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.InitialDuration.ToString());

    public override void OnScp096ChangedState(Scp096ChangedStateEventArgs ev) => HandleEvent(EventType.Scp096ChangeState, nameof(EventType.Scp096ChangeState),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.State.ToString());

    public override void OnScp096Charged(Scp096ChargedEventArgs ev) => HandleEvent(EventType.Scp096Charge, nameof(EventType.Scp096Charge),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName);

    public override void OnScp096PriedGate(Scp096PriedGateEventArgs ev) => HandleEvent(EventType.Scp096PryGate, nameof(EventType.Scp096PryGate),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.Gate.DoorName.ToString());

    public override void OnScp096TriedNotToCry(Scp096TriedNotToCryEventArgs ev) => HandleEvent(EventType.Scp096TryNotCry, nameof(EventType.Scp096TryNotCry),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName);

    public override void OnScp096StartedCrying(Scp096StartedCryingEventArgs ev) => HandleEvent(EventType.Scp096StartCrying, nameof(EventType.Scp096StartCrying),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName);

    public override void OnServerBanIssued(BanIssuedEventArgs ev) => HandleEvent(EventType.BanIssued, nameof(EventType.BanIssued), ev.BanDetails.Id,
        ev.BanType.ToString());

    public override void OnServerBanRevoked(BanRevokedEventArgs ev) => HandleEvent(EventType.BanRevoked, nameof(EventType.BanRevoked), ev.BanDetails.Id,
        ev.BanType.ToString());

    public override void OnServerBanUpdated(BanUpdatedEventArgs ev) => HandleEvent(EventType.BanUpdated, nameof(EventType.BanUpdated), ev.BanDetails.Id,
        ev.BanType.ToString());

    public override void OnServerRoundEndingConditionsCheck(RoundEndingConditionsCheckEventArgs ev) => HandleEvent(EventType.RoundEndingConditionsCheck,
        nameof(EventType.RoundEndingConditionsCheck), ev.CanEnd.ToString());

    public override void OnScp079Pinged(Scp079PingedEventArgs ev) => HandleEvent(EventType.Scp079Ping, nameof(EventType.Scp079Ping), ev.Player.PlayerId.ToString(),
        ev.Player.DisplayName, ev.PingType.ToString());

    public override void OnObjectiveActivatedGeneratorCompleted(GeneratorActivatedObjectiveEventArgs ev) => HandleEvent(
        EventType.ObjectiveActivatedGeneratorComplete, nameof(EventType.ObjectiveActivatedGeneratorComplete), ev.Player.PlayerId.ToString(), ev.Player.DisplayName,
        ev.Faction.ToString());

    public override void OnObjectiveCompleted(ObjectiveCompletedBaseEventArgs ev) => HandleEvent(EventType.ObjectiveComplete, nameof(EventType.ObjectiveComplete),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.Faction.ToString());

    public override void OnObjectiveDamagedScpCompleted(ScpDamagedObjectiveEventArgs ev) => HandleEvent(EventType.ObjectiveDamagedScpComplete,
        nameof(EventType.ObjectiveDamagedScpComplete), ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.Target.PlayerId.ToString(), ev.Target.DisplayName,
        ev.Faction.ToString());

    public override void OnObjectiveEscapedCompleted(EscapedObjectiveEventArgs ev) => HandleEvent(EventType.ObjectiveEscapedComplete,
        nameof(EventType.ObjectiveEscapedComplete), ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.Faction.ToString(), ev.OldRole.ToString(),
        ev.NewRole.ToString());

    public override void OnObjectiveKilledEnemyCompleted(EnemyKilledObjectiveEventArgs ev) => HandleEvent(EventType.ObjectiveKilledEnemyComplete,
        nameof(EventType.ObjectiveKilledEnemyComplete), ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.Target.PlayerId.ToString(), ev.Target.DisplayName,
        ev.Faction.ToString());

    public override void OnObjectivePickedScpItemCompleted(ScpItemPickedObjectiveEventArgs ev) => HandleEvent(EventType.ObjectivePickedScpItemComplete,
        nameof(EventType.ObjectivePickedScpItemComplete), ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.Faction.ToString(), ev.Item.Type.ToString());

    public override void OnPlayerChangedAttachments(PlayerChangedAttachmentsEventArgs ev) => HandleEvent(EventType.PlayerChangeAttachments,
        nameof(EventType.PlayerChangeAttachments), ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.FirearmItem.Type.ToString());

    public override void OnPlayerChangedBadgeVisibility(PlayerChangedBadgeVisibilityEventArgs ev) => HandleEvent(EventType.PlayerChangeBadgeVisibility,
        nameof(EventType.PlayerChangeBadgeVisibility), ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.NewVisibility.ToString());

    public override void OnPlayerDetectedByScp1344(PlayerDetectedByScp1344EventArgs ev) => HandleEvent(EventType.PlayerDetectedByScp1344,
        nameof(EventType.PlayerDetectedByScp1344), ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.Target.PlayerId.ToString(), ev.Target.DisplayName);

    public override void OnPlayerInspectedKeycard(PlayerInspectedKeycardEventArgs ev) => HandleEvent(EventType.PlayerInspectKeycard,
        nameof(EventType.PlayerInspectKeycard), ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.KeycardItem.Type.ToString());

    public override void OnPlayerInteractedWarheadLever(PlayerInteractedWarheadLeverEventArgs ev) => HandleEvent(EventType.PlayerInteractWarheadLever,
        nameof(EventType.PlayerInteractWarheadLever), ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.Enabled.ToString());

    public override void OnPlayerJumped(PlayerJumpedEventArgs ev) => HandleEvent(EventType.PlayerJump, nameof(EventType.PlayerJump),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName);

    public override void OnPlayerMovementStateChanged(PlayerMovementStateChangedEventArgs ev) => HandleEvent(EventType.PlayerMovementStateChange,
        nameof(EventType.PlayerMovementStateChange), ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.OldState.ToString(), ev.NewState.ToString());

    public override void OnPlayerProcessedJailbirdMessage(PlayerProcessedJailbirdMessageEventArgs ev) => HandleEvent(EventType.PlayerProcessJailbirdMessage,
        nameof(EventType.PlayerProcessJailbirdMessage), ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.Message.ToString());

    public override void OnPlayerRaPlayerListAddedPlayer(PlayerRaPlayerListAddedPlayerEventArgs ev) => HandleEvent(EventType.PlayerRaPlayerListAddPlayer,
        nameof(EventType.PlayerRaPlayerListAddPlayer), ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.Target.PlayerId.ToString(), ev.Target.DisplayName);

    public override void OnPlayerReceivedAchievement(PlayerReceivedAchievementEventArgs ev)
    {
        if (ev.Player is null)
        {
            return;
        }

        HandleEvent(EventType.PlayerReceiveAchievement, nameof(EventType.PlayerReceiveAchievement), ev.Player.PlayerId.ToString(), ev.Player.DisplayName,
            ev.Achievement.ToString());
    }

    public override void OnPlayerRequestedCustomRaInfo(PlayerRequestedCustomRaInfoEventArgs ev) => HandleEvent(EventType.PlayerRequestCustomRaInfo,
        nameof(EventType.PlayerRequestCustomRaInfo), ev.Player.PlayerId.ToString(), ev.Player.DisplayName);

    public override void OnPlayerRequestedRaPlayerInfo(PlayerRequestedRaPlayerInfoEventArgs ev) => HandleEvent(EventType.PlayerRequestRaPlayerInfo,
        nameof(EventType.PlayerRequestRaPlayerInfo), ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.Target.PlayerId.ToString(), ev.Target.DisplayName);

    public override void OnPlayerRequestedRaPlayerList(PlayerRequestedRaPlayerListEventArgs ev) => HandleEvent(EventType.PlayerRequestRaPlayerList,
        nameof(EventType.PlayerRequestRaPlayerList), ev.Player.PlayerId.ToString(), ev.Player.DisplayName);

    public override void OnPlayerRequestedRaPlayersInfo(PlayerRequestedRaPlayersInfoEventArgs ev) => HandleEvent(EventType.PlayerRequestRaPlayersInfo,
        nameof(EventType.PlayerRequestRaPlayersInfo), ev.Player.PlayerId.ToString(), ev.Player.DisplayName);

    public override void OnPlayerRoomChanged(PlayerRoomChangedEventArgs ev) => HandleEvent(EventType.PlayerRoomChange, nameof(EventType.PlayerRoomChange),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName);

    public override void OnPlayerSentAttachmentsPrefs(PlayerSentAttachmentsPrefsEventArgs ev) => HandleEvent(EventType.PlayerSendAttachmentsPrefs,
        nameof(EventType.PlayerSendAttachmentsPrefs), ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.FirearmType.ToString());

    public override void OnPlayerSpinnedRevolver(PlayerSpinnedRevolverEventArgs ev) => HandleEvent(EventType.PlayerSpinRevolver,
        nameof(EventType.PlayerSpinRevolver), ev.Player.PlayerId.ToString(), ev.Player.DisplayName);

    public override void OnPlayerToggledDisruptorFiringMode(PlayerToggledDisruptorFiringModeEventArgs ev) => HandleEvent(EventType.PlayerToggleDisruptorFiringMode,
        nameof(EventType.PlayerToggleDisruptorFiringMode), ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.SingleShotMode.ToString());

    public override void OnPlayerZoneChanged(PlayerZoneChangedEventArgs ev) => HandleEvent(EventType.PlayerZoneChange,
        nameof(EventType.PlayerZoneChange), ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.OldZone.ToString(), ev.NewZone.ToString());

    public override void OnScp127GainExperience(Scp127GainExperienceEventArgs ev) => HandleEvent(EventType.Scp127GainExperience,
        nameof(EventType.Scp127GainExperience), ev.ExperienceGain.ToString());

    public override void OnScp127LevelUp(Scp127LevelUpEventArgs ev) => HandleEvent(EventType.Scp127LevelUp, nameof(EventType.Scp127LevelUp), ev.Tier.ToString());

    public override void OnScp127Talked(Scp127TalkedEventArgs ev) => HandleEvent(EventType.Scp127Talk, nameof(EventType.Scp127Talk), ev.VoiceLine.ToString());

    public override void OnScp3114Dance(Scp3114StartedDanceEventArgs ev) => HandleEvent(EventType.Scp3114Dance, nameof(EventType.Scp3114Dance),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.DanceId.ToString());

    public override void OnScp3114Disguised(Scp3114DisguisedEventArgs ev) => HandleEvent(EventType.Scp3114Disguise, nameof(EventType.Scp3114Disguise),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName);

    public override void OnScp3114Revealed(Scp3114RevealedEventArgs ev) => HandleEvent(EventType.Scp3114Reveal, nameof(EventType.Scp3114Reveal),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.Forced.ToString());

    public override void OnScp3114StrangleAborted(Scp3114StrangleAbortedEventArgs ev) => HandleEvent(EventType.Scp3114StrangleAbort,
        nameof(EventType.PlayerDetectedByScp1344), ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.Target.PlayerId.ToString(), ev.Target.DisplayName);

    public override void OnScp3114StrangleStarted(Scp3114StrangleStartedEventArgs ev) => HandleEvent(EventType.Scp3114StrangleStart,
        nameof(EventType.PlayerDetectedByScp1344), ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.Target.PlayerId.ToString(), ev.Target.DisplayName);

    public override void OnServerElevatorSequenceChanged(ElevatorSequenceChangedEventArgs ev) => HandleEvent(EventType.ServerElevatorSequenceChange,
        nameof(EventType.ServerElevatorSequenceChange));

    public override void OnScp173Teleported(Scp173TeleportedEventArgs ev) => HandleEvent(EventType.Scp173Teleport, nameof(EventType.Scp173Teleport),
        ev.Player.PlayerId.ToString(), ev.Player.DisplayName, ev.IsAllowed.ToString());
}
