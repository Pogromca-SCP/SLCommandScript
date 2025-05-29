using CommandSystem;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.Scp0492Events;
using LabApi.Events.Arguments.Scp049Events;
using LabApi.Events.Arguments.Scp079Events;
using LabApi.Events.Arguments.Scp096Events;
using LabApi.Events.Arguments.Scp106Events;
using LabApi.Events.Arguments.Scp173Events;
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

    public override void OnPlayerToggledNoclip(PlayerToggledNoclipEventArgs args) => HandleEvent(EventType.PlayerToggleNoclip, nameof(EventType.PlayerToggleNoclip),
        args.Player.PlayerId.ToString(), args.Player.DisplayName);

    public override void OnPlayerChangedNickname(PlayerChangedNicknameEventArgs args) => HandleEvent(EventType.PlayerChangeNickname, nameof(EventType.PlayerChangeNickname),
        args.Player.PlayerId.ToString(), args.Player.DisplayName);

    public override void OnPlayerValidatedVisibility(PlayerValidatedVisibilityEventArgs args) => HandleEvent(EventType.PlayerValidateVisibility,
        nameof(EventType.PlayerValidateVisibility), args.Player.PlayerId.ToString(), args.Player.DisplayName, args.Target.PlayerId.ToString(), args.Target.DisplayName);

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

    public override void OnPlayerCuffed(PlayerCuffedEventArgs args) => HandleEvent(EventType.PlayerHandcuff, nameof(EventType.PlayerHandcuff),
        args.Player.PlayerId.ToString(), args.Player.DisplayName, args.Target.PlayerId.ToString(), args.Target.DisplayName);

    public override void OnPlayerUncuffed(PlayerUncuffedEventArgs args) => HandleEvent(EventType.PlayerRemoveHandcuffs, nameof(EventType.PlayerRemoveHandcuffs),
        args.Player.PlayerId.ToString(), args.Player.DisplayName, args.Target.PlayerId.ToString(), args.Target.DisplayName);

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

    public override void OnPlayerInteractedToy(PlayerInteractedToyEventArgs args) => HandleEvent(EventType.PlayerInteractToy, nameof(EventType.PlayerInteractToy),
        args.Player.PlayerId.ToString(), args.Player.DisplayName);

    public override void OnPlayerSearchedToy(PlayerSearchedToyEventArgs args) => HandleEvent(EventType.PlayerSearchToy, nameof(EventType.PlayerSearchToy),
        args.Player.PlayerId.ToString(), args.Player.DisplayName);

    public override void OnPlayerSearchToyAborted(PlayerSearchToyAbortedEventArgs args) => HandleEvent(EventType.PlayerSearchToyAbort,
        nameof(EventType.PlayerSearchToyAbort), args.Player.PlayerId.ToString(), args.Player.DisplayName);

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

    public override void OnPlayerToggledWeaponFlashlight(PlayerToggledWeaponFlashlightEventArgs args) => HandleEvent(EventType.PlayerToggleWeaponFlashlight,
        nameof(EventType.PlayerToggleWeaponFlashlight), args.FirearmItem.Type.ToString(), args.Player.PlayerId.ToString(), args.Player.DisplayName);

    public override void OnPlayerCancelledUsingItem(PlayerCancelledUsingItemEventArgs args) => HandleEvent(EventType.PlayerCancelUsingItem,
        nameof(EventType.PlayerCancelUsingItem), args.UsableItem.Type.ToString(), args.Player.PlayerId.ToString(), args.Player.DisplayName);

    public override void OnPlayerChangedItem(PlayerChangedItemEventArgs args) => HandleEvent(EventType.PlayerChangeItem, nameof(EventType.PlayerChangeItem),
        args.Player.PlayerId.ToString(), args.Player.DisplayName);

    public override void OnPlayerDroppedAmmo(PlayerDroppedAmmoEventArgs args) => HandleEvent(EventType.PlayerDropAmmo, nameof(EventType.PlayerDropAmmo),
        args.Player.PlayerId.ToString(), args.Player.DisplayName, args.AmmoPickup.Type.ToString(), args.Amount.ToString());

    public override void OnPlayerDroppedItem(PlayerDroppedItemEventArgs args) => HandleEvent(EventType.PlayerDropItem, nameof(EventType.PlayerDropItem),
        args.Player.PlayerId.ToString(), args.Player.DisplayName, args.Pickup.Type.ToString());

    public override void OnPlayerPickedUpItem(PlayerPickedUpItemEventArgs args) => HandleEvent(EventType.PlayerPickupItem, nameof(EventType.PlayerPickupItem),
        args.Player.PlayerId.ToString(), args.Player.DisplayName);

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

    public override void OnPlayerSearchedAmmo(PlayerSearchedAmmoEventArgs args) => HandleEvent(EventType.PlayerSearchAmmo, nameof(EventType.PlayerSearchAmmo),
        args.Player.PlayerId.ToString(), args.Player.DisplayName);

    public override void OnPlayerSearchedArmor(PlayerSearchedArmorEventArgs args) => HandleEvent(EventType.PlayerSearchArmor, nameof(EventType.PlayerSearchArmor),
        args.Player.PlayerId.ToString(), args.Player.DisplayName);

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

    public override void OnPlayerReceivedLoadout(PlayerReceivedLoadoutEventArgs args) => HandleEvent(EventType.PlayerReceiveLoadout,
        nameof(EventType.PlayerReceiveLoadout), args.Player.PlayerId.ToString(), args.Player.DisplayName);

    public override void OnPlayerChangedRadioRange(PlayerChangedRadioRangeEventArgs args) => HandleEvent(EventType.PlayerChangeRadioRange,
        nameof(EventType.PlayerChangeRadioRange), args.Player.PlayerId.ToString(), args.Player.DisplayName, args.Range.ToString());

    public override void OnPlayerToggledRadio(PlayerToggledRadioEventArgs args) => HandleEvent(EventType.PlayerRadioToggle, nameof(EventType.PlayerRadioToggle),
        args.NewState.ToString(), args.Player.PlayerId.ToString(), args.Player.DisplayName);

    public override void OnPlayerUsedRadio(PlayerUsedRadioEventArgs args) => HandleEvent(EventType.PlayerUsingRadio, nameof(EventType.PlayerUsingRadio),
        args.Player.PlayerId.ToString(), args.Player.DisplayName);

    public override void OnServerLczDecontaminationStarted() => HandleEvent(EventType.LczDecontaminationStart, nameof(EventType.LczDecontaminationStart));

    public override void OnServerLczDecontaminationAnnounced(LczDecontaminationAnnouncedEventArgs args) => HandleEvent(EventType.LczDecontaminationAnnouncement,
        nameof(EventType.LczDecontaminationAnnouncement), args.Phase.ToString());

    public override void OnServerMapGenerated(MapGeneratedEventArgs args) => HandleEvent(EventType.MapGenerated, nameof(EventType.MapGenerated), args.Seed.ToString());

    public override void OnServerItemSpawned(ItemSpawnedEventArgs args) => HandleEvent(EventType.ItemSpawned, nameof(EventType.ItemSpawned),
        args.Pickup.Type.ToString());

    public override void OnServerPickupCreated(PickupCreatedEventArgs args) => HandleEvent(EventType.PickupCreated, nameof(EventType.PickupCreated),
        args.Pickup.Type.ToString());

    public override void OnServerPickupDestroyed(PickupDestroyedEventArgs args) => HandleEvent(EventType.PickupDestroyed, nameof(EventType.PickupDestroyed),
        args.Pickup.Type.ToString());

    public override void OnServerGeneratorActivated(GeneratorActivatedEventArgs args) => HandleEvent(EventType.GeneratorActivated, nameof(EventType.GeneratorActivated));

    public override void OnPlayerPlacedBlood(PlayerPlacedBloodEventArgs args) => HandleEvent(EventType.PlaceBlood, nameof(EventType.PlaceBlood),
        args.Player.PlayerId.ToString(), args.Player.DisplayName);

    public override void OnPlayerPlacedBulletHole(PlayerPlacedBulletHoleEventArgs args) => HandleEvent(EventType.PlaceBulletHole, nameof(EventType.PlaceBulletHole));

    public override void OnPlayerEnteredHazard(PlayerEnteredHazardEventArgs args) => HandleEvent(EventType.PlayerEnterHazard, nameof(EventType.PlayerEnterHazard),
        args.Player.PlayerId.ToString(), args.Player.DisplayName);

    public override void OnPlayerLeftHazard(PlayerLeftHazardEventArgs args) => HandleEvent(EventType.PlayerLeaveHazard, nameof(EventType.PlayerLeaveHazard),
        args.Player.PlayerId.ToString(), args.Player.DisplayName);

    public override void OnPlayerIdledTesla(PlayerIdledTeslaEventArgs args) => HandleEvent(EventType.PlayerIdleTesla, nameof(EventType.PlayerIdleTesla),
        args.Player.PlayerId.ToString(), args.Player.DisplayName);

    public override void OnPlayerTriggeredTesla(PlayerTriggeredTeslaEventArgs args) => HandleEvent(EventType.PlayerTriggerTesla, nameof(EventType.PlayerTriggerTesla),
        args.Player.PlayerId.ToString(), args.Player.DisplayName);

    public override void OnPlayerUnlockedWarheadButton(PlayerUnlockedWarheadButtonEventArgs args) => HandleEvent(EventType.PlayerUnlockWarhead,
        nameof(EventType.PlayerUnlockWarhead), args.Player.PlayerId.ToString(), args.Player.DisplayName);

    public override void OnServerCassieAnnounced(CassieAnnouncedEventArgs args) => HandleEvent(EventType.CassieAnnouncement, nameof(EventType.CassieAnnouncement));

    public override void OnServerExplosionSpawned(ExplosionSpawnedEventArgs args) => HandleEvent(EventType.ExplosionSpawn, nameof(EventType.ExplosionSpawn));

    public override void OnServerProjectileExploded(ProjectileExplodedEventArgs args) => HandleEvent(EventType.ProjectileExplode, nameof(EventType.ProjectileExplode));

    public override void OnServerSentAdminChat(SentAdminChatEventArgs args) => HandleEvent(EventType.AdminChat, nameof(EventType.AdminChat));

    public override void OnServerRoundEnded(RoundEndedEventArgs args) => HandleEvent(EventType.RoundEnd, nameof(EventType.RoundEnd), args.LeadingTeam.ToString());

    public override void OnServerRoundRestarted() => HandleEvent(EventType.RoundRestart, nameof(EventType.RoundRestart));

    public override void OnServerRoundStarted() => HandleEvent(EventType.RoundStart, nameof(EventType.RoundStart));

    public override void OnServerWaitingForPlayers() => HandleEvent(EventType.WaitingForPlayers, nameof(EventType.WaitingForPlayers));

    public override void OnServerWaveTeamSelected(WaveTeamSelectedEventArgs args) => HandleEvent(EventType.TeamRespawnSelected, nameof(EventType.TeamRespawnSelected),
        args.Wave.Faction.ToString());

    public override void OnServerWaveRespawned(WaveRespawnedEventArgs args) => HandleEvent(EventType.TeamRespawn, nameof(EventType.TeamRespawn),
        args.Wave.Faction.ToString(), args.Players.Count.ToString());

    public override void OnWarheadStarted(WarheadStartedEventArgs args) => HandleEvent(EventType.WarheadStart, nameof(EventType.WarheadStart),
        args.Player.PlayerId.ToString(), args.Player.DisplayName);

    public override void OnWarheadStopped(WarheadStoppedEventArgs args) => HandleEvent(EventType.WarheadStop, nameof(EventType.WarheadStop),
        args.Player.PlayerId.ToString(), args.Player.DisplayName);

    public override void OnWarheadDetonated(WarheadDetonatedEventArgs args) => HandleEvent(EventType.WarheadDetonation, nameof(EventType.WarheadDetonation));

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

    public override void OnServerCassieQueuedScpTermination(CassieQueuedScpTerminationEventArgs args) => HandleEvent(EventType.CassieQueueScpTermination,
        nameof(EventType.CassieQueueScpTermination), args.Player.Role.ToString(), args.Announcement);

    public override void OnScp914Activated(Scp914ActivatedEventArgs args) => HandleEvent(EventType.Scp914Activate, nameof(EventType.Scp914Activate),
        args.Player.PlayerId.ToString(), args.Player.DisplayName, args.KnobSetting.ToString());

    public override void OnScp914KnobChanged(Scp914KnobChangedEventArgs args) => HandleEvent(EventType.Scp914KnobChange, nameof(EventType.Scp914KnobChange),
        args.Player.PlayerId.ToString(), args.Player.DisplayName, args.KnobSetting.ToString());

    public override void OnScp914ProcessedInventoryItem(Scp914ProcessedInventoryItemEventArgs args) => HandleEvent(EventType.Scp914InventoryItemUpgraded,
        nameof(EventType.Scp914InventoryItemUpgraded), args.Player.PlayerId.ToString(), args.Player.DisplayName, args.Item.Type.ToString(), args.KnobSetting.ToString());

    public override void OnScp914ProcessedPickup(Scp914ProcessedPickupEventArgs args)
    {
        if (args.Pickup is null)
        {
            return;
        }

        HandleEvent(EventType.Scp914PickupUpgraded, nameof(EventType.Scp914PickupUpgraded), args.Pickup.Type.ToString(), args.KnobSetting.ToString());
    }

    public override void OnScp914ProcessedPlayer(Scp914ProcessedPlayerEventArgs args) => HandleEvent(EventType.Scp914ProcessPlayer,
        nameof(EventType.Scp914ProcessPlayer), args.Player.PlayerId.ToString(), args.Player.DisplayName, args.KnobSetting.ToString());

    public override void OnScp106ChangedStalkMode(Scp106ChangedStalkModeEventArgs args) => HandleEvent(EventType.Scp106Stalking, nameof(EventType.Scp106Stalking),
        args.Player.PlayerId.ToString(), args.Player.DisplayName);

    public override void OnScp106ChangedSubmersionStatus(Scp106ChangedSubmersionStatusEventArgs args) => HandleEvent(EventType.Scp106Submerging,
        nameof(EventType.Scp106Submerging), args.Player.PlayerId.ToString(), args.Player.DisplayName);

    public override void OnScp106ChangedVigor(Scp106ChangedVigorEventArgs args) => HandleEvent(EventType.Scp106VigorChange, nameof(EventType.Scp106VigorChange),
        args.Player.PlayerId.ToString(), args.Player.DisplayName, args.Value.ToString());

    public override void OnScp106TeleportedPlayer(Scp106TeleportedPlayerEvent args) => HandleEvent(EventType.Scp106VigorChange, nameof(EventType.Scp106VigorChange),
        args.Player.PlayerId.ToString(), args.Player.DisplayName, args.Target.PlayerId.ToString(), args.Target.DisplayName);

    public override void OnScp106UsedHunterAtlas(Scp106UsedHunterAtlasEventArgs args) => HandleEvent(EventType.Scp106UsedHunterAtlas,
        nameof(EventType.Scp106UsedHunterAtlas), args.Player.PlayerId.ToString(), args.Player.DisplayName);

    public override void OnScp173PlayedSound(Scp173PlayedSoundEventArgs args) => HandleEvent(EventType.Scp173PlaySound, nameof(EventType.Scp173PlaySound),
        args.Player.PlayerId.ToString(), args.Player.DisplayName);

    public override void OnScp173BreakneckSpeedChanged(Scp173BreakneckSpeedChangedEventArgs args) => HandleEvent(EventType.Scp173BreakneckSpeeds,
        nameof(EventType.Scp173BreakneckSpeeds), args.Player.PlayerId.ToString(), args.Player.DisplayName);

    public override void OnScp173AddedObserver(Scp173AddedObserverEventArgs args) => HandleEvent(EventType.Scp173NewObserver, nameof(EventType.Scp173NewObserver),
        args.Player.PlayerId.ToString(), args.Player.DisplayName, args.Target.PlayerId.ToString(), args.Target.DisplayName);

    public override void OnScp173RemovedObserver(Scp173RemovedObserverEventArgs args) => HandleEvent(EventType.Scp173RemovedObserver,
        nameof(EventType.Scp173RemovedObserver), args.Player.PlayerId.ToString(), args.Player.DisplayName, args.Target.PlayerId.ToString(), args.Target.DisplayName);

    public override void OnScp173CreatedTantrum(Scp173CreatedTantrumEventArgs args) => HandleEvent(EventType.Scp173CreateTantrum, nameof(EventType.Scp173CreateTantrum),
        args.Player.PlayerId.ToString(), args.Player.DisplayName);

    public override void OnScp939CreatedAmnesticCloud(Scp939CreatedAmnesticCloudEventArgs args) => HandleEvent(EventType.Scp939CreateAmnesticCloud,
        nameof(EventType.Scp939CreateAmnesticCloud), args.Player.PlayerId.ToString(), args.Player.DisplayName);

    public override void OnScp939Lunged(Scp939LungedEventArgs args) => HandleEvent(EventType.Scp939Lunge, nameof(EventType.Scp939Lunge), args.Player.PlayerId.ToString(),
        args.Player.DisplayName);

    public override void OnScp939Attacked(Scp939AttackedEventArgs args) => HandleEvent(EventType.Scp939Attack, nameof(EventType.Scp939Attack),
        args.Player.PlayerId.ToString(), args.Player.DisplayName);

    public override void OnScp079GainedExperience(Scp079GainedExperienceEventArgs args) => HandleEvent(EventType.Scp079GainExperience,
        nameof(EventType.Scp079GainExperience), args.Player.PlayerId.ToString(), args.Player.DisplayName, args.Amount.ToString(), args.Reason.ToString());

    public override void OnScp079LeveledUp(Scp079LeveledUpEventArgs args) => HandleEvent(EventType.Scp079LevelUpTier, nameof(EventType.Scp079LevelUpTier),
        args.Player.PlayerId.ToString(), args.Player.DisplayName, args.Tier.ToString());

    public override void OnScp079UsedTesla(Scp079UsedTeslaEventArgs args) => HandleEvent(EventType.Scp079UseTesla, nameof(EventType.Scp079UseTesla),
        args.Player.PlayerId.ToString(), args.Player.DisplayName);

    public override void OnScp079LockedDownRoom(Scp079LockedDownRoomEventArgs args) => HandleEvent(EventType.Scp079LockdownRoom, nameof(EventType.Scp079LockdownRoom),
        args.Player.PlayerId.ToString(), args.Player.DisplayName, args.Room.Name.ToString());

    public override void OnScp079CancelledRoomLockdown(Scp079CancelledRoomLockdownEventArgs args) => HandleEvent(EventType.Scp079CancelRoomLockdown,
        nameof(EventType.Scp079CancelRoomLockdown), args.Player.PlayerId.ToString(), args.Player.DisplayName, args.Room.Name.ToString());

    public override void OnScp079LockedDoor(Scp079LockedDoorEventArgs args) => HandleEvent(EventType.Scp079LockDoor, nameof(EventType.Scp079LockDoor),
        args.Player.PlayerId.ToString(), args.Player.DisplayName);

    public override void OnScp079UnlockedDoor(Scp079UnlockedDoorEventArgs args) => HandleEvent(EventType.Scp079UnlockDoor, nameof(EventType.Scp079UnlockDoor),
        args.Player.PlayerId.ToString(), args.Player.DisplayName);

    public override void OnScp079BlackedOutZone(Scp079BlackedOutZoneEventArgs args) => HandleEvent(EventType.Scp079BlackoutZone, nameof(EventType.Scp079BlackoutZone),
        args.Player.PlayerId.ToString(), args.Player.DisplayName, args.Zone.ToString());

    public override void OnScp079BlackedOutRoom(Scp079BlackedOutRoomEventArgs args) => HandleEvent(EventType.Scp079BlackoutRoom, nameof(EventType.Scp079BlackoutRoom),
        args.Player.PlayerId.ToString(), args.Player.DisplayName, args.Room.Name.ToString());

    public override void OnScp079ChangedCamera(Scp079ChangedCameraEventArgs args) => HandleEvent(EventType.Scp079ChangeCamera, nameof(EventType.Scp079ChangeCamera),
        args.Player.PlayerId.ToString(), args.Player.DisplayName, args.Camera.Base.Label);

    public override void OnScp079Recontained(Scp079RecontainedEventArgs args) => HandleEvent(EventType.Scp079Recontained, nameof(EventType.Scp079Recontained),
        args.Player.PlayerId.ToString(), args.Player.DisplayName);

    public override void OnScp049ResurrectedBody(Scp049ResurrectedBodyEventArgs args) => HandleEvent(EventType.Scp049ResurrectBody, nameof(EventType.Scp049ResurrectBody),
        args.Player.PlayerId.ToString(), args.Player.DisplayName, args.Target.PlayerId.ToString(), args.Target.DisplayName);

    public override void OnScp049UsedDoctorsCall(Scp049UsedDoctorsCallEventArgs args) => HandleEvent(EventType.Scp049UseDoctorsCall,
        nameof(EventType.Scp049UseDoctorsCall), args.Player.PlayerId.ToString(), args.Player.DisplayName);

    public override void OnScp049UsedSense(Scp049UsedSenseEventArgs args) => HandleEvent(EventType.Scp049UseSense, nameof(EventType.Scp049UseSense),
        args.Player.PlayerId.ToString(), args.Player.DisplayName, args.Target.PlayerId.ToString(), args.Target.DisplayName);

    public override void OnScp0492ConsumedCorpse(Scp0492ConsumedCorpseEventArgs args) => HandleEvent(EventType.Scp049_2ConsumeCorpse,
        nameof(EventType.Scp049_2ConsumeCorpse), args.Player.PlayerId.ToString(), args.Player.DisplayName);

    public override void OnScp0492StartedConsumingCorpse(Scp0492StartedConsumingCorpseEventArgs args) => HandleEvent(EventType.Scp049_2StartConsumingCorpse,
        nameof(EventType.Scp049_2StartConsumingCorpse), args.Player.PlayerId.ToString(), args.Player.DisplayName);

    public override void OnScp096AddedTarget(Scp096AddedTargetEventArgs args) => HandleEvent(EventType.Scp096AddTarget, nameof(EventType.Scp096AddTarget),
        args.Player.PlayerId.ToString(), args.Player.DisplayName, args.Target.PlayerId.ToString(), args.Target.DisplayName);

    public override void OnScp096Enraged(Scp096EnragedEventArgs args) => HandleEvent(EventType.Scp096Enrage, nameof(EventType.Scp096Enrage),
        args.Player.PlayerId.ToString(), args.Player.DisplayName, args.InitialDuration.ToString());

    public override void OnScp096ChangedState(Scp096ChangedStateEventArgs args) => HandleEvent(EventType.Scp096ChangeState, nameof(EventType.Scp096ChangeState),
        args.Player.PlayerId.ToString(), args.Player.DisplayName, args.State.ToString());

    public override void OnScp096Charged(Scp096ChargedEventArgs args) => HandleEvent(EventType.Scp096Charge, nameof(EventType.Scp096Charge),
        args.Player.PlayerId.ToString(), args.Player.DisplayName);

    public override void OnScp096PriedGate(Scp096PriedGateEventArgs args) => HandleEvent(EventType.Scp096PryGate, nameof(EventType.Scp096PryGate),
        args.Player.PlayerId.ToString(), args.Player.DisplayName);

    public override void OnScp096TriedNotToCry(Scp096TriedNotToCryEventArgs args) => HandleEvent(EventType.Scp096TryNotCry, nameof(EventType.Scp096TryNotCry),
        args.Player.PlayerId.ToString(), args.Player.DisplayName);

    public override void OnScp096StartedCrying(Scp096StartedCryingEventArgs args) => HandleEvent(EventType.Scp096StartCrying, nameof(EventType.Scp096StartCrying),
        args.Player.PlayerId.ToString(), args.Player.DisplayName);

    public override void OnServerBanIssued(BanIssuedEventArgs args) => HandleEvent(EventType.BanIssued, nameof(EventType.BanIssued), args.BanDetails.Id,
        args.BanType.ToString());

    public override void OnServerBanRevoked(BanRevokedEventArgs args) => HandleEvent(EventType.BanRevoked, nameof(EventType.BanRevoked), args.BanDetails.Id,
        args.BanType.ToString());

    public override void OnServerBanUpdated(BanUpdatedEventArgs args) => HandleEvent(EventType.BanUpdated, nameof(EventType.BanUpdated), args.BanDetails.Id,
        args.BanType.ToString());
}
