using CommandSystem;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.Scp914Events;
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
    #region Events Management
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
    #endregion

    #region Players
    #region Technical
    public override void OnPlayerJoined(PlayerJoinedEventArgs args) => HandleEvent(EventType.PlayerJoined, nameof(EventType.PlayerJoined), args.Player.PlayerId.ToString(),
        args.Player.DisplayName);

    public override void OnPlayerLeft(PlayerLeftEventArgs args) => HandleEvent(EventType.PlayerLeft, nameof(EventType.PlayerLeft), args.Player.PlayerId.ToString(),
        args.Player.DisplayName);

    public override void OnPlayerBanned(PlayerBannedEventArgs args)
    {
        if (args.Player is null)
        {
            return;
        }

        HandleEvent(EventType.PlayerBanned, nameof(EventType.PlayerBanned), args.Player.DisplayName, args.Reason);
    }

    public override void OnPlayerKicked(PlayerKickedEventArgs args) => HandleEvent(EventType.PlayerKicked, nameof(EventType.PlayerKicked), args.Player.PlayerId.ToString(),
        args.Player.DisplayName, args.Reason);

    public override void OnPlayerReportedCheater(PlayerReportedCheaterEventArgs args) => HandleEvent(EventType.PlayerCheaterReport, nameof(EventType.PlayerCheaterReport),
        args.Player.PlayerId.ToString(), args.Player.DisplayName, args.Target.PlayerId.ToString(), args.Target.DisplayName, args.Reason);

    public override void OnPlayerChangedRole(PlayerChangedRoleEventArgs args) => HandleEvent(EventType.PlayerChangeRole, nameof(EventType.PlayerChangeRole),
        args.Player.PlayerId.ToString(), args.Player.DisplayName, args.OldRole.ToString(), args.NewRole.RoleName, args.ChangeReason.ToString());

    public override void OnPlayerPreAuthenticated(PlayerPreAuthenticatedEventArgs args) => HandleEvent(EventType.PlayerPreauth, nameof(EventType.PlayerPreauth),
        args.Region);

    public override void OnPlayerGroupChanged(PlayerGroupChangedEventArgs args) => HandleEvent(EventType.PlayerGetGroup, nameof(EventType.PlayerGetGroup),
        args.Player.PlayerId.ToString(), args.Group.BadgeText);

    public override void OnPlayerReportedPlayer(PlayerReportedPlayerEventArgs args) => HandleEvent(EventType.PlayerReport, nameof(EventType.PlayerReport),
        args.Player.PlayerId.ToString(), args.Player.DisplayName, args.Target.PlayerId.ToString(), args.Target.DisplayName, args.Reason);

    public override void OnPlayerSpawnedRagdoll(PlayerSpawnedRagdollEventArgs args) => HandleEvent(EventType.RagdollSpawn, nameof(EventType.RagdollSpawn),
        args.Player.PlayerId.ToString(), args.Player.DisplayName);

    public override void OnPlayerMuted(PlayerMutedEventArgs args) => HandleEvent(EventType.PlayerMuted, nameof(EventType.PlayerMuted), args.Issuer.PlayerId.ToString(),
        args.Issuer.DisplayName, args.Player.PlayerId.ToString(), args.Player.DisplayName);

    public override void OnPlayerUnmuted(PlayerUnmutedEventArgs args) => HandleEvent(EventType.PlayerUnmuted, nameof(EventType.PlayerUnmuted),
        args.Issuer.PlayerId.ToString(), args.Issuer.DisplayName, args.Player.PlayerId.ToString(), args.Player.DisplayName);
    #endregion

    #region State
    public override void OnPlayerDeath(PlayerDeathEventArgs args)
    {
        const EventType eventType = EventType.PlayerDeath;
        const string eventName = nameof(EventType.PlayerDeath);

        if (args.Attacker is null)
        {
            HandleEvent(eventType, eventName, args.Player.PlayerId.ToString(), args.Player.DisplayName);
        }
        else
        {
            HandleEvent(eventType, eventName, args.Player.PlayerId.ToString(), args.Player.DisplayName, args.Attacker.PlayerId.ToString(), args.Attacker.DisplayName);
        }
    }

    public override void OnPlayerChangedSpectator(PlayerChangedSpectatorEventArgs args) => HandleEvent(EventType.PlayerChangeSpectator,
        nameof(EventType.PlayerChangeSpectator), args.Player.PlayerId.ToString(), args.Player.DisplayName, args.NewTarget.PlayerId.ToString(), args.NewTarget.DisplayName,
        args.OldTarget.PlayerId.ToString(), args.OldTarget.DisplayName);

    public override void OnPlayerEscaped(PlayerEscapedEventArgs args) => HandleEvent(EventType.PlayerEscape, nameof(EventType.PlayerEscape),
        args.Player.PlayerId.ToString(), args.Player.DisplayName, args.NewRole.ToString());

    public override void OnPlayerHurt(PlayerHurtEventArgs args)
    {
        const EventType eventType = EventType.PlayerDamage;
        const string eventName = nameof(EventType.PlayerDamage);

        if (args.Attacker is null)
        {
            HandleEvent(eventType, eventName, args.Player.PlayerId.ToString(), args.Player.DisplayName);
        }
        else
        {
            HandleEvent(eventType, eventName, args.Player.PlayerId.ToString(), args.Player.DisplayName, args.Attacker.PlayerId.ToString(), args.Attacker.DisplayName);
        }
    }

    public override void OnPlayerUpdatedEffect(PlayerEffectUpdatedEventArgs args) => HandleEvent(EventType.PlayerUpdateEffect, nameof(EventType.PlayerUpdateEffect),
        args.Player.PlayerId.ToString(), args.Player.DisplayName, args.Effect.Classification.ToString(), args.Intensity.ToString(), args.Duration.ToString());

    public override void OnPlayerSpawned(PlayerSpawnedEventArgs args) => HandleEvent(EventType.PlayerSpawn, nameof(EventType.PlayerSpawn), args.Player.PlayerId.ToString(),
        args.Player.DisplayName, args.Role.RoleName);

    public override void OnPlayerEnteredPocketDimension(PlayerEnteredPocketDimensionEventArgs args) => HandleEvent(EventType.PlayerEnterPocketDimension,
        nameof(EventType.PlayerEnterPocketDimension), args.Player.PlayerId.ToString(), args.Player.DisplayName);

    public override void OnPlayerLeftPocketDimension(PlayerLeftPocketDimensionEventArgs args) => HandleEvent(EventType.PlayerExitPocketDimension,
        nameof(EventType.PlayerExitPocketDimension), args.Player.PlayerId.ToString(), args.Player.DisplayName, args.IsSuccessful.ToString());
    #endregion

    #region Environment
    public override void OnPlayerActivatedGenerator(PlayerActivatedGeneratorEventArgs args) => HandleEvent(EventType.PlayerActivateGenerator,
        nameof(EventType.PlayerActivateGenerator), args.Player.PlayerId.ToString(), args.Player.DisplayName);

    public override void OnPlayerUsedIntercom(PlayerUsedIntercomEventArgs args)
    {
        if (args.Player is null)
        {
            return;
        }

        HandleEvent(EventType.PlayerUsedIntercom, nameof(EventType.PlayerUsedIntercom), args.Player.PlayerId.ToString(), args.Player.DisplayName,
            args.State.ToString());
    }

    public override void OnPlayerClosedGenerator(PlayerClosedGeneratorEventArgs args) => HandleEvent(EventType.PlayerCloseGenerator,
        nameof(EventType.PlayerCloseGenerator), args.Player.PlayerId.ToString(), args.Player.DisplayName);

    public override void OnPlayerDamagedShootingTarget(PlayerDamagedShootingTargetEventArgs args) => HandleEvent(EventType.PlayerDamagedShootingTarget,
        nameof(EventType.PlayerDamagedShootingTarget), args.Player.PlayerId.ToString(), args.Player.DisplayName);

    public override void OnPlayerDamagedWindow(PlayerDamagedWindowEventArgs args) => HandleEvent(EventType.PlayerDamagedWindow, nameof(EventType.PlayerDamagedWindow),
        args.Player.PlayerId.ToString(), args.Player.DisplayName);

    public override void OnPlayerDeactivatedGenerator(PlayerDeactivatedGeneratorEventArgs args) => HandleEvent(EventType.PlayerDeactivatedGenerator,
        nameof(EventType.PlayerDeactivatedGenerator), args.Player.PlayerId.ToString(), args.Player.DisplayName);

    public override void OnPlayerOpenedGenerator(PlayerOpenedGeneratorEventArgs args) => HandleEvent(EventType.PlayerOpenGenerator, nameof(EventType.PlayerOpenGenerator),
        args.Player.PlayerId.ToString(), args.Player.DisplayName);

    public override void OnPlayerInteractedShootingTarget(PlayerInteractedShootingTargetEventArgs args) => HandleEvent(EventType.PlayerInteractShootingTarget,
        nameof(EventType.PlayerInteractShootingTarget), args.Player.PlayerId.ToString(), args.Player.DisplayName);

    public override void OnPlayerInteractedLocker(PlayerInteractedLockerEventArgs args) => HandleEvent(EventType.PlayerInteractLocker,
        nameof(EventType.PlayerInteractLocker), args.Player.PlayerId.ToString(), args.Player.DisplayName);

    public override void OnPlayerInteractedElevator(PlayerInteractedElevatorEventArgs args) => HandleEvent(EventType.PlayerInteractElevator,
        nameof(EventType.PlayerInteractElevator), args.Player.PlayerId.ToString(), args.Player.DisplayName);

    public override void OnPlayerUnlockedGenerator(PlayerUnlockedGeneratorEventArgs args) => HandleEvent(EventType.PlayerUnlockGenerator,
        nameof(EventType.PlayerUnlockGenerator), args.Player.PlayerId.ToString(), args.Player.DisplayName);

    public override void OnPlayerInteractedDoor(PlayerInteractedDoorEventArgs args) => HandleEvent(EventType.PlayerInteractDoor, nameof(EventType.PlayerInteractDoor),
        args.Player.PlayerId.ToString(), args.Player.DisplayName);

    public override void OnPlayerInteractedGenerator(PlayerInteractedGeneratorEventArgs args) => HandleEvent(EventType.PlayerInteractGenerator,
        nameof(EventType.PlayerInteractGenerator), args.Player.PlayerId.ToString(), args.Player.DisplayName);
    #endregion

    #region Weapons
    public override void OnPlayerAimedWeapon(PlayerAimedWeaponEventArgs args) => HandleEvent(EventType.PlayerAimWeapon, nameof(EventType.PlayerAimWeapon),
        args.Aiming.ToString(), args.FirearmItem.Type.ToString(), args.Player.PlayerId.ToString(), args.Player.DisplayName);

    public override void OnPlayerDryFiredWeapon(PlayerDryFiredWeaponEventArgs args) => HandleEvent(EventType.PlayerDryfireWeapon, nameof(EventType.PlayerDryfireWeapon),
        args.FirearmItem.Type.ToString(), args.Player.PlayerId.ToString(), args.Player.DisplayName);

    public override void OnPlayerReloadedWeapon(PlayerReloadedWeaponEventArgs args) => HandleEvent(EventType.PlayerReloadWeapon, nameof(EventType.PlayerReloadWeapon),
        args.FirearmItem.Type.ToString(), args.Player.PlayerId.ToString(), args.Player.DisplayName);

    public override void OnPlayerShotWeapon(PlayerShotWeaponEventArgs args) => HandleEvent(EventType.PlayerShotWeapon, nameof(EventType.PlayerShotWeapon),
        args.FirearmItem.Type.ToString(), args.Player.PlayerId.ToString(), args.Player.DisplayName);

    public override void OnPlayerUnloadedWeapon(PlayerUnloadedWeaponEventArgs args) => HandleEvent(EventType.PlayerUnloadWeapon, nameof(EventType.PlayerUnloadWeapon),
        args.FirearmItem.Type.ToString(), args.Player.PlayerId.ToString(), args.Player.DisplayName);
    #endregion

    #region Items
    public override void OnPlayerCancelledUsingItem(PlayerCancelledUsingItemEventArgs args) => HandleEvent(EventType.PlayerCancelUsingItem,
        nameof(EventType.PlayerCancelUsingItem), args.UsableItem.Type.ToString(), args.Player.PlayerId.ToString(), args.Player.DisplayName);

    public override void OnPlayerChangedItem(PlayerChangedItemEventArgs args) => HandleEvent(EventType.PlayerChangeItem, nameof(EventType.PlayerChangeItem),
        args.Player.PlayerId.ToString(), args.Player.DisplayName);

    public override void OnPlayerDroppedAmmo(PlayerDroppedAmmoEventArgs args) => HandleEvent(EventType.PlayerDropAmmo, nameof(EventType.PlayerDropAmmo),
        args.Player.PlayerId.ToString(), args.Player.DisplayName, args.AmmoPickup.Type.ToString(), args.Amount.ToString());

    public override void OnPlayerDroppedItem(PlayerDroppedItemEventArgs args) => HandleEvent(EventType.PlayerDropItem, nameof(EventType.PlayerDropItem),
        args.Player.PlayerId.ToString(), args.Player.DisplayName, args.Pickup.Type.ToString());

    public override void OnPlayerPickedUpAmmo(PlayerPickedUpAmmoEventArgs args)
    {
        if (args.AmmoPickup is null)
        {
            return;
        }

        HandleEvent(EventType.PlayerPickupAmmo, nameof(EventType.PlayerPickupAmmo), args.Player.PlayerId.ToString(), args.Player.DisplayName,
            args.AmmoPickup.Type.ToString());
    }

    public override void OnPlayerPickedUpArmor(PlayerPickedUpArmorEventArgs args)
    {
        if (args.BodyArmorItem is null)
        {
            return;
        }

        HandleEvent(EventType.PlayerPickupArmor, nameof(EventType.PlayerPickupArmor), args.Player.PlayerId.ToString(), args.Player.DisplayName,
            args.BodyArmorItem.Type.ToString());
    }

    public override void OnPlayerPickedUpScp330(PlayerPickedUpScp330EventArgs args) => HandleEvent(EventType.PlayerPickupScp330, nameof(EventType.PlayerPickupScp330),
        args.Player.PlayerId.ToString(), args.Player.DisplayName, args.CandyPickup.Type.ToString());

    public override void OnPlayerSearchedPickup(PlayerSearchedPickupEventArgs args) => HandleEvent(EventType.PlayerSearchPickup, nameof(EventType.PlayerSearchPickup),
        args.Player.PlayerId.ToString(), args.Player.DisplayName, args.Pickup.Type.ToString());

    public override void OnPlayerThrewItem(PlayerThrewItemEventArgs args) => HandleEvent(EventType.PlayerThrowItem, nameof(EventType.PlayerThrowItem),
        args.Player.PlayerId.ToString(), args.Player.DisplayName, args.Pickup.Type.ToString());

    public override void OnPlayerToggledFlashlight(PlayerToggledFlashlightEventArgs args) => HandleEvent(EventType.PlayerToggleFlashlight,
        nameof(EventType.PlayerToggleFlashlight), args.Player.PlayerId.ToString(), args.Player.DisplayName, args.LightItem.IsEmitting.ToString());

    public override void OnPlayerUsedItem(PlayerUsedItemEventArgs args) => HandleEvent(EventType.PlayerUsedItem, nameof(EventType.PlayerUsedItem),
        args.Player.PlayerId.ToString(), args.Player.DisplayName, args.UsableItem.Type.ToString());

    public override void OnPlayerInteractedScp330(PlayerInteractedScp330EventArgs args) => HandleEvent(EventType.PlayerInteractScp330,
        nameof(EventType.PlayerInteractScp330), args.Player.PlayerId.ToString(), args.Player.DisplayName);

    public override void OnPlayerThrewProjectile(PlayerThrewProjectileEventArgs args) => HandleEvent(EventType.PlayerThrowProjectile,
        nameof(EventType.PlayerThrowProjectile), args.Player.PlayerId.ToString(), args.Player.DisplayName, args.ThrowableItem.Type.ToString());

    public override void OnPlayerFlippedCoin(PlayerFlippedCoinEventArgs args) => HandleEvent(EventType.PlayerCoinFlip, nameof(EventType.PlayerCoinFlip),
        args.Player.PlayerId.ToString(), args.Player.DisplayName, args.IsTails.ToString());
    #endregion

    #region Radio
    public override void OnPlayerChangedRadioRange(PlayerChangedRadioRangeEventArgs args) => HandleEvent(EventType.PlayerChangeRadioRange,
        nameof(EventType.PlayerChangeRadioRange), args.Player.PlayerId.ToString(), args.Player.DisplayName, args.Range.ToString());

    public override void OnPlayerToggledRadio(PlayerToggledRadioEventArgs args) => HandleEvent(EventType.PlayerRadioToggle, nameof(EventType.PlayerRadioToggle),
        args.NewState.ToString(), args.Player.PlayerId.ToString(), args.Player.DisplayName);

    public override void OnPlayerUsedRadio(PlayerUsedRadioEventArgs args) => HandleEvent(EventType.PlayerUsingRadio, nameof(EventType.PlayerUsingRadio),
        args.Player.PlayerId.ToString(), args.Player.DisplayName);
    #endregion
    #endregion

    #region Decontamination
    public override void OnServerLczDecontaminationStarted() => HandleEvent(EventType.LczDecontaminationStart, nameof(EventType.LczDecontaminationStart));

    public override void OnServerLczDecontaminationAnnounced(LczDecontaminationAnnouncedEventArgs args) => HandleEvent(EventType.LczDecontaminationAnnouncement,
        nameof(EventType.LczDecontaminationAnnouncement), args.Phase.ToString());
    #endregion

    #region Map Generation
    public override void OnServerMapGenerated(MapGeneratedEventArgs args) => HandleEvent(EventType.MapGenerated, nameof(EventType.MapGenerated), args.Seed.ToString());
    #endregion

    #region Items
    public override void OnServerItemSpawned(ItemSpawnedEventArgs args) => HandleEvent(EventType.ItemSpawned, nameof(EventType.ItemSpawned),
        args.Pickup.Type.ToString());
    #endregion

    #region Environment
    public override void OnServerGeneratorActivated(GeneratorActivatedEventArgs args) => HandleEvent(EventType.GeneratorActivated, nameof(EventType.GeneratorActivated));

    public override void OnPlayerPlacedBlood(PlayerPlacedBloodEventArgs args) => HandleEvent(EventType.PlaceBlood, nameof(EventType.PlaceBlood),
        args.Player.PlayerId.ToString(), args.Player.DisplayName);

    public override void OnPlayerPlacedBulletHole(PlayerPlacedBulletHoleEventArgs args) => HandleEvent(EventType.PlaceBulletHole, nameof(EventType.PlaceBulletHole));
    #endregion

    #region Round Events
    public override void OnServerRoundEnded(RoundEndedEventArgs args) => HandleEvent(EventType.RoundEnd, nameof(EventType.RoundEnd), args.LeadingTeam.ToString());

    public override void OnServerRoundRestarted() => HandleEvent(EventType.RoundRestart, nameof(EventType.RoundRestart));

    public override void OnServerRoundStarted() => HandleEvent(EventType.RoundStart, nameof(EventType.RoundStart));

    public override void OnServerWaitingForPlayers() => HandleEvent(EventType.WaitingForPlayers, nameof(EventType.WaitingForPlayers));

    public override void OnServerWaveTeamSelected(WaveTeamSelectedEventArgs args) => HandleEvent(EventType.TeamRespawnSelected, nameof(EventType.TeamRespawnSelected),
        args.Wave.Faction.ToString());

    public override void OnServerWaveRespawned(WaveRespawnedEventArgs args) => HandleEvent(EventType.TeamRespawn, nameof(EventType.TeamRespawn),
        args.Wave.Faction.ToString(), args.Players.Count.ToString());
    #endregion

    #region Warhead
    public override void OnWarheadStarted(WarheadStartedEventArgs args) => HandleEvent(EventType.WarheadStart, nameof(EventType.WarheadStart),
        args.Player.PlayerId.ToString(), args.Player.DisplayName);

    public override void OnWarheadStopped(WarheadStoppedEventArgs args) => HandleEvent(EventType.WarheadStop, nameof(EventType.WarheadStop),
        args.Player.PlayerId.ToString(), args.Player.DisplayName);

    public override void OnWarheadDetonated(WarheadDetonatedEventArgs args) => HandleEvent(EventType.WarheadDetonation, nameof(EventType.WarheadDetonation));
    #endregion

    #region Commands
    public override void OnServerCommandExecuted(CommandExecutedEventArgs args)
    {
        const EventType eventType = EventType.CommandExecuted;
        const string eventName = nameof(EventType.CommandExecuted);

        if (args.Sender is null)
        {
            HandleEvent(eventType, eventName, args.Command.Command);
        }
        else
        {
            HandleEvent(eventType, eventName, args.Command.Command, args.Sender.SenderId, args.Sender.Nickname);
        }
    }
    #endregion

    #region SCPs
    #region General
    public override void OnServerCassieQueuedScpTermination(CassieQueuedScpTerminationEventArgs args) => HandleEvent(EventType.CassieQueueScpTermination,
        nameof(EventType.CassieQueueScpTermination), args.Player.Role.ToString(), args.Announcement);
    #endregion

    #region 914
    public override void OnScp914Activated(Scp914ActivatedEventArgs args) => HandleEvent(EventType.Scp914Activate, nameof(EventType.Scp914Activate),
        args.Player.PlayerId.ToString(), args.Player.DisplayName, args.KnobSetting.ToString());

    public override void OnScp914KnobChanged(Scp914KnobChangedEventArgs args) => HandleEvent(EventType.Scp914KnobChange, nameof(EventType.Scp914KnobChange),
        args.Player.PlayerId.ToString(), args.Player.DisplayName, args.KnobSetting.ToString());
    #endregion

    #region 106

    #endregion

    #region 173

    #endregion

    #region 939

    #endregion

    #region 079

    #endregion

    #region 049

    #endregion

    #region 096

    #endregion
    #endregion

    #region Bans
    public override void OnServerBanIssued(BanIssuedEventArgs args) => HandleEvent(EventType.BanIssued, nameof(EventType.BanIssued), args.BanDetails.Id,
        args.BanType.ToString());

    public override void OnServerBanRevoked(BanRevokedEventArgs args) => HandleEvent(EventType.BanRevoked, nameof(EventType.BanRevoked), args.BanDetails.Id,
        args.BanType.ToString());

    public override void OnServerBanUpdated(BanUpdatedEventArgs args) => HandleEvent(EventType.BanUpdated, nameof(EventType.BanUpdated), args.BanDetails.Id,
        args.BanType.ToString());
    #endregion
}
