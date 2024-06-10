using CommandSystem;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Enums;
using PluginAPI.Events;
using System.Collections.Generic;

namespace SLCommandScript.FileScriptsLoader.Events;

/// <summary>
/// Handles server events using script files.
/// </summary>
public class FileScriptsEventHandler
{
    /// <summary>
    /// Contains registered event handling scripts.
    /// </summary>
    public Dictionary<ServerEventType, ICommand> EventScripts { get; } = [];

    /// <summary>
    /// Executes a command handler.
    /// </summary>
    /// <param name="eventType">Type of event to handle.</param>
    /// <param name="args">Event arguments to use.</param>
    private void HandleEvent(ServerEventType eventType, params string[] args)
    {
        if (!EventScripts.ContainsKey(eventType))
        {
            return;
        }

        var cmd = EventScripts[eventType];
        var result = cmd.Execute(new(args, 1, args.Length - 1), ServerConsole.Scs, out var message);

        if (!result)
        {
            Log.Error(message, "FileScriptsEventHandler: ");
        }
    }

    #region Players
    #region Technical
    [PluginEvent(ServerEventType.PlayerJoined)]
    private void OnPlayerJoin(PlayerJoinedEvent args) => HandleEvent(ServerEventType.PlayerJoined, "PlayerJoined", args.Player.PlayerId.ToString(), args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerLeft)]
    private void OnPlayerLeave(PlayerLeftEvent args) => HandleEvent(ServerEventType.PlayerLeft, "PlayerLeft", args.Player.PlayerId.ToString(), args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerBanned)]
    private void OnPlayerBanned(PlayerBannedEvent args) => HandleEvent(ServerEventType.PlayerBanned, "PlayerBanned", args.Player.Nickname, args.Reason);

    [PluginEvent(ServerEventType.PlayerKicked)]
    private void OnPlayerKicked(PlayerKickedEvent args) => HandleEvent(ServerEventType.PlayerKicked, "PlayerKicked", args.Player.PlayerId.ToString(), args.Player.DisplayNickname,
        args.Reason);

    [PluginEvent(ServerEventType.PlayerCheaterReport)]
    private void OnCheaterReport(PlayerCheaterReportEvent args) => HandleEvent(ServerEventType.PlayerCheaterReport, "PlayerCheaterReport", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname, args.Target.PlayerId.ToString(), args.Target.DisplayNickname, args.Reason);

    [PluginEvent(ServerEventType.PlayerChangeRole)]
    private void OnChangeRole(PlayerChangeRoleEvent args) => HandleEvent(ServerEventType.PlayerChangeRole, "PlayerChangeRole", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname, args.OldRole.RoleTypeId.ToString(), args.NewRole.ToString(), args.ChangeReason.ToString());

    [PluginEvent(ServerEventType.PlayerPreauth)]
    private void OnPreauth(PlayerPreauthEvent args) => HandleEvent(ServerEventType.PlayerPreauth, "PlayerPreauth", args.Region);

    [PluginEvent(ServerEventType.PlayerGetGroup)]
    private void OnPlayerChangeGroup(PlayerGetGroupEvent args) => HandleEvent(ServerEventType.PlayerGetGroup, "PlayerGetGroup", args.UserId, args.Group.BadgeText);

    [PluginEvent(ServerEventType.PlayerReport)]
    private void OnReport(PlayerReportEvent args) => HandleEvent(ServerEventType.PlayerReport, "PlayerReport", args.Player.PlayerId.ToString(), args.Player.DisplayNickname,
        args.Target.PlayerId.ToString(), args.Target.DisplayNickname, args.Reason);

    [PluginEvent(ServerEventType.RagdollSpawn)]
    private void OnRagdollSpawn(RagdollSpawnEvent args) => HandleEvent(ServerEventType.RagdollSpawn, "RagdollSpawn", args.Player.PlayerId.ToString(), args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerMuted)]
    private void OnPlayerMuted(PlayerMutedEvent args) => HandleEvent(ServerEventType.PlayerMuted, "PlayerMuted", args.Issuer.PlayerId.ToString(), args.Issuer.DisplayNickname,
        args.Player.PlayerId.ToString(), args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerUnmuted)]
    private void OnPlayerUnmuted(PlayerUnmutedEvent args) => HandleEvent(ServerEventType.PlayerUnmuted, "PlayerUnmuted", args.Issuer.PlayerId.ToString(), args.Issuer.DisplayNickname,
        args.Player.PlayerId.ToString(), args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerCheckReservedSlot)]
    private void OnCheckReservedSlot(PlayerCheckReservedSlotEvent args) => HandleEvent(ServerEventType.PlayerCheckReservedSlot, "PlayerCheckReservedSlot", args.Userid,
        args.HasReservedSlot.ToString());
    #endregion

    #region State
    [PluginEvent(ServerEventType.PlayerDeath)]
    private void OnPlayerDied(PlayerDeathEvent args)
    {
        const ServerEventType eventType = ServerEventType.PlayerDeath;
        const string eventName = "PlayerDeath";

        if (args.Attacker is null)
        {
            HandleEvent(eventType, eventName, args.Player.PlayerId.ToString(), args.Player.DisplayNickname);
        }
        else
        {
            HandleEvent(eventType, eventName, args.Player.PlayerId.ToString(), args.Player.DisplayNickname, args.Attacker.PlayerId.ToString(), args.Attacker.DisplayNickname);
        }
    }

    [PluginEvent(ServerEventType.PlayerChangeSpectator)]
    private void OnPlayerChangesSpectatedPlayer(PlayerChangeSpectatorEvent args)
    {
        const ServerEventType eventType = ServerEventType.PlayerChangeSpectator;
        const string eventName = "PlayerChangeSpectator";

        if (args.OldTarget is null)
        {
            HandleEvent(eventType, eventName, args.Player.PlayerId.ToString(), args.Player.DisplayNickname, args.NewTarget.PlayerId.ToString(),
                args.NewTarget.DisplayNickname);
        }
        else
        {
            HandleEvent(eventType, eventName, args.Player.PlayerId.ToString(), args.Player.DisplayNickname, args.NewTarget.PlayerId.ToString(),
                args.NewTarget.DisplayNickname, args.OldTarget.PlayerId.ToString(), args.OldTarget.DisplayNickname);
        }
    }

    [PluginEvent(ServerEventType.PlayerEscape)]
    private void OnPlayerEscaped(PlayerEscapeEvent args) => HandleEvent(ServerEventType.PlayerEscape, "PlayerEscape", args.Player.PlayerId.ToString(), args.Player.DisplayNickname,
        args.NewRole.ToString());

    [PluginEvent(ServerEventType.PlayerHandcuff)]
    private void OnPlayerHandcuffed(PlayerHandcuffEvent args) => HandleEvent(ServerEventType.PlayerHandcuff, "PlayerHandcuff", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname, args.Target.PlayerId.ToString(), args.Target.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerRemoveHandcuffs)]
    private void OnPlayerUncuffed(PlayerRemoveHandcuffsEvent args) => HandleEvent(ServerEventType.PlayerRemoveHandcuffs, "PlayerRemoveHandcuffs", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname, args.Target.PlayerId.ToString(), args.Target.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerDamage)]
    private void OnPlayerDamage(PlayerDamageEvent args)
    {
        if (args.Target is null)
        {
            return;
        }

        const ServerEventType eventType = ServerEventType.PlayerDamage;
        const string eventName = "PlayerDamage";

        if (args.Player is null)
        {
            HandleEvent(eventType, eventName, args.Target.PlayerId.ToString(), args.Target.DisplayNickname);
        }
        else
        {
            HandleEvent(eventType, eventName, args.Target.PlayerId.ToString(), args.Target.DisplayNickname, args.Player.PlayerId.ToString(), args.Player.DisplayNickname);
        }
    }

    [PluginEvent(ServerEventType.PlayerReceiveEffect)]
    private void OnReceiveEffect(PlayerReceiveEffectEvent args) => HandleEvent(ServerEventType.PlayerReceiveEffect, "PlayerReceiveEffect", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname, args.Effect.Classification.ToString(), args.Intensity.ToString(), args.Duration.ToString());

    [PluginEvent(ServerEventType.PlayerSpawn)]
    private void OnSpawn(PlayerSpawnEvent args) => HandleEvent(ServerEventType.PlayerSpawn, "PlayerSpawn", args.Player.PlayerId.ToString(), args.Player.DisplayNickname,
        args.Role.ToString());

    [PluginEvent(ServerEventType.PlayerEnterPocketDimension)]
    private void OnPlayerEnterPocketDimension(PlayerEnterPocketDimensionEvent args) => HandleEvent(ServerEventType.PlayerEnterPocketDimension, "PlayerEnterPocketDimension",
        args.Player.PlayerId.ToString(), args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerExitPocketDimension)]
    private void OnPlayerExitPocketDimension(PlayerExitPocketDimensionEvent args) => HandleEvent(ServerEventType.PlayerExitPocketDimension, "PlayerExitPocketDimension",
        args.Player.PlayerId.ToString(), args.Player.DisplayNickname, args.IsSuccessful.ToString());
    #endregion

    #region Environment
    [PluginEvent(ServerEventType.PlayerActivateGenerator)]
    private void OnPlayerActivateGenerator(PlayerActivateGeneratorEvent args) => HandleEvent(ServerEventType.PlayerActivateGenerator, "PlayerActivateGenerator",
        args.Player.PlayerId.ToString(), args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerUsingIntercom)]
    private void OnPlayerUsingIntercom(PlayerUsingIntercomEvent args) => HandleEvent(ServerEventType.PlayerUsingIntercom, "PlayerUsingIntercom", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname, args.IntercomState.ToString());

    [PluginEvent(ServerEventType.PlayerCloseGenerator)]
    private void OnPlayerClosesGenerator(PlayerCloseGeneratorEvent args) => HandleEvent(ServerEventType.PlayerCloseGenerator, "PlayerCloseGenerator", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerDamagedShootingTarget)]
    private void OnPlayerDamagedShootingTarget(PlayerDamagedShootingTargetEvent args) => HandleEvent(ServerEventType.PlayerDamagedShootingTarget, "PlayerDamagedShootingTarget",
        args.Player.PlayerId.ToString(), args.Player.DisplayNickname, args.DamageAmount.ToString());

    [PluginEvent(ServerEventType.PlayerDamagedWindow)]
    private void OnPlayerDamagedWindow(PlayerDamagedWindowEvent args) => HandleEvent(ServerEventType.PlayerDamagedWindow, "PlayerDamagedWindow", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname, args.DamageAmount.ToString());

    [PluginEvent(ServerEventType.PlayerDeactivatedGenerator)]
    private void OnPlayerDeactivatedGenerator(PlayerDeactivatedGeneratorEvent args) => HandleEvent(ServerEventType.PlayerDeactivatedGenerator, "PlayerDeactivatedGenerator",
        args.Player.PlayerId.ToString(), args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerOpenGenerator)]
    private void OnPlayerOpenedGenerator(PlayerOpenGeneratorEvent args) => HandleEvent(ServerEventType.PlayerOpenGenerator, "PlayerOpenGenerator", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerInteractShootingTarget)]
    private void OnInteractWithShootingTarget(PlayerInteractShootingTargetEvent args) => HandleEvent(ServerEventType.PlayerInteractShootingTarget, "PlayerInteractShootingTarget",
        args.Player.PlayerId.ToString(), args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerInteractLocker)]
    private void OnInteractWithLocker(PlayerInteractLockerEvent args) => HandleEvent(ServerEventType.PlayerInteractLocker, "PlayerInteractLocker", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerInteractElevator)]
    private void OnInteractWithElevator(PlayerInteractElevatorEvent args) => HandleEvent(ServerEventType.PlayerInteractElevator, "PlayerInteractElevator",
        args.Player.PlayerId.ToString(), args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerUnlockGenerator)]
    private void OnUnlockGenerator(PlayerUnlockGeneratorEvent args) => HandleEvent(ServerEventType.PlayerUnlockGenerator, "PlayerUnlockGenerator", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerInteractDoor)]
    private void OnPlayerInteractDoor(PlayerInteractDoorEvent args) => HandleEvent(ServerEventType.PlayerInteractDoor, "PlayerInteractDoor", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerInteractGenerator)]
    private void OnPlayerInteractGenerator(PlayerInteractGeneratorEvent args) => HandleEvent(ServerEventType.PlayerInteractGenerator, "PlayerInteractGenerator",
        args.Player.PlayerId.ToString(), args.Player.DisplayNickname);
    #endregion

    #region Weapons
    [PluginEvent(ServerEventType.PlayerAimWeapon)]
    private void OnPlayerAimsWeapon(PlayerAimWeaponEvent args) => HandleEvent(ServerEventType.PlayerAimWeapon, "PlayerAimWeapon", args.IsAiming.ToString(),
        args.Firearm.ItemTypeId.ToString(), args.Player.PlayerId.ToString(), args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerDryfireWeapon)]
    private void OnPlayerDryfireWeapon(PlayerDryfireWeaponEvent args) => HandleEvent(ServerEventType.PlayerDryfireWeapon, "PlayerDryfireWeapon", args.Firearm.ItemTypeId.ToString(),
        args.Player.PlayerId.ToString(), args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerReloadWeapon)]
    private void OnReloadWeapon(PlayerReloadWeaponEvent args) => HandleEvent(ServerEventType.PlayerReloadWeapon, "PlayerReloadWeapon", args.Firearm.ItemTypeId.ToString(),
        args.Player.PlayerId.ToString(), args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerShotWeapon)]
    private void OnShotWeapon(PlayerShotWeaponEvent args) => HandleEvent(ServerEventType.PlayerShotWeapon, "PlayerShotWeapon", args.Firearm.ItemTypeId.ToString(),
        args.Player.PlayerId.ToString(), args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerUnloadWeapon)]
    private void OnUnloadWeapon(PlayerUnloadWeaponEvent args) => HandleEvent(ServerEventType.PlayerUnloadWeapon, "PlayerUnloadWeapon", args.Firearm.ItemTypeId.ToString(),
        args.Player.PlayerId.ToString(), args.Player.DisplayNickname);
    #endregion

    #region Items
    [PluginEvent(ServerEventType.PlayerCancelUsingItem)]
    private void OnPlayerCancelsUsingItem(PlayerCancelUsingItemEvent args) => HandleEvent(ServerEventType.PlayerCancelUsingItem, "PlayerCancelUsingItem",
        args.Item.ItemTypeId.ToString(), args.Player.PlayerId.ToString(), args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerChangeItem)]
    private void OnPlayerChangesItem(PlayerChangeItemEvent args) => HandleEvent(ServerEventType.PlayerChangeItem, "PlayerChangeItem", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerDropAmmo)]
    private void OnPlayerDropAmmo(PlayerDropAmmoEvent args) => HandleEvent(ServerEventType.PlayerDropAmmo, "PlayerDropAmmo", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname, args.Item.ToString(), args.Amount.ToString());

    [PluginEvent(ServerEventType.PlayerDropItem)]
    private void OnPlayerDropItem(PlayerDropItemEvent args) => HandleEvent(ServerEventType.PlayerDropItem, "PlayerDropItem", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname, args.Item.ItemTypeId.ToString());

    [PluginEvent(ServerEventType.PlayerDroppedAmmo)]
    private void OnPlayerDroppedAmmo(PlayerDroppedAmmoEvent args) => HandleEvent(ServerEventType.PlayerDroppedAmmo, "PlayerDroppedAmmo", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname, args.Item.Info.ItemId.ToString(), args.Amount.ToString());

    [PluginEvent(ServerEventType.PlayerDropedpItem)]
    private void OnPlayerDroppedItem(PlayerDroppedItemEvent args) => HandleEvent(ServerEventType.PlayerDropedpItem, "PlayerDroppedItem", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname, args.Item.Info.ItemId.ToString());

    [PluginEvent(ServerEventType.PlayerPickupAmmo)]
    private void OnPlayerPickupAmmo(PlayerPickupAmmoEvent args) => HandleEvent(ServerEventType.PlayerPickupAmmo, "PlayerPickupAmmo", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname, args.Item.Info.ItemId.ToString());

    [PluginEvent(ServerEventType.PlayerPickupArmor)]
    private void OnPlayerPickupArmor(PlayerPickupArmorEvent args) => HandleEvent(ServerEventType.PlayerPickupArmor, "PlayerPickupArmor", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname, args.Item.Info.ItemId.ToString());

    [PluginEvent(ServerEventType.PlayerPickupScp330)]
    private void OnPlayerPickupScp330(PlayerPickupScp330Event args) => HandleEvent(ServerEventType.PlayerPickupScp330, "PlayerPickupScp330", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname, args.Item.Info.ItemId.ToString());

    [PluginEvent(ServerEventType.PlayerSearchPickup)]
    private void OnSearchPickup(PlayerSearchPickupEvent args) => HandleEvent(ServerEventType.PlayerSearchPickup, "PlayerSearchPickup", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname, args.Item.Info.ItemId.ToString());

    [PluginEvent(ServerEventType.PlayerSearchedPickup)]
    private void OnSearchedPickup(PlayerSearchedPickupEvent args) => HandleEvent(ServerEventType.PlayerSearchedPickup, "PlayerSearchedPickup", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname, args.Item.Info.ItemId.ToString());

    [PluginEvent(ServerEventType.PlayerThrowItem)]
    private void OnThrowItem(PlayerThrowItemEvent args) => HandleEvent(ServerEventType.PlayerThrowItem, "PlayerThrowItem", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname, args.Item.ItemTypeId.ToString());

    [PluginEvent(ServerEventType.PlayerToggleFlashlight)]
    private void OnToggleFlashlight(PlayerToggleFlashlightEvent args) => HandleEvent(ServerEventType.PlayerToggleFlashlight, "PlayerToggleFlashlight", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname, args.IsToggled.ToString());

    [PluginEvent(ServerEventType.PlayerUsedItem)]
    private void OnPlayerUsedItem(PlayerUsedItemEvent args) => HandleEvent(ServerEventType.PlayerUsedItem, "PlayerUsedItem", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname, args.Item.ItemTypeId.ToString());

    [PluginEvent(ServerEventType.PlayerUseHotkey)]
    private void OnPlayerUsedHotkey(PlayerUseHotkeyEvent args) => HandleEvent(ServerEventType.PlayerUseHotkey, "PlayerUseHotkey", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname, args.Action.ToString());

    [PluginEvent(ServerEventType.PlayerUseItem)]
    private void OnPlayerStartedUsingItem(PlayerUseItemEvent args) => HandleEvent(ServerEventType.PlayerUseItem, "PlayerUseItem", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname, args.Item.ItemTypeId.ToString());

    [PluginEvent(ServerEventType.PlayerInteractScp330)]
    private void OnInteractWithScp330(PlayerInteractScp330Event args) => HandleEvent(ServerEventType.PlayerInteractScp330, "PlayerInteractScp330", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerThrowProjectile)]
    private void OnPlayerThrowProjectile(PlayerThrowProjectileEvent args) => HandleEvent(ServerEventType.PlayerThrowProjectile, "PlayerThrowProjectile",
        args.Thrower.PlayerId.ToString(), args.Thrower.DisplayNickname, args.Item.ItemTypeId.ToString());

    [PluginEvent(ServerEventType.PlayerPreCoinFlip)]
    private void OnPlayerPreCoinFlip(PlayerPreCoinFlipEvent args) => HandleEvent(ServerEventType.PlayerPreCoinFlip, "PlayerPreCoinFlip", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerCoinFlip)]
    private void OnPlayerCoinFlip(PlayerCoinFlipEvent args) => HandleEvent(ServerEventType.PlayerCoinFlip, "PlayerCoinFlip", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname, args.IsTails.ToString());
    #endregion

    #region Radio
    [PluginEvent(ServerEventType.PlayerChangeRadioRange)]
    private void OnPlayerChangesRadioRange(PlayerChangeRadioRangeEvent args) => HandleEvent(ServerEventType.PlayerChangeRadioRange, "PlayerChangeRadioRange",
        args.Player.PlayerId.ToString(), args.Player.DisplayNickname, args.Range.ToString());

    [PluginEvent(ServerEventType.PlayerRadioToggle)]
    private void OnPlayerRadioToggle(PlayerRadioToggleEvent args) => HandleEvent(ServerEventType.PlayerRadioToggle, "PlayerRadioToggle", args.NewState.ToString(),
        args.Player.PlayerId.ToString(), args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerUsingRadio)]
    private void OnPlayerUsingRadio(PlayerUsingRadioEvent args) => HandleEvent(ServerEventType.PlayerUsingRadio, "PlayerUsingRadio", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname);
    #endregion
    #endregion

    #region Decontamination
    [PluginEvent(ServerEventType.LczDecontaminationStart)]
    private void OnLczDecontaminationStarts(LczDecontaminationStartEvent args) => HandleEvent(ServerEventType.LczDecontaminationStart, "LczDecontaminationStart");

    [PluginEvent(ServerEventType.LczDecontaminationAnnouncement)]
    private void OnAnnounceLczDecontamination(LczDecontaminationAnnouncementEvent args) => HandleEvent(ServerEventType.LczDecontaminationAnnouncement,
        "LczDecontaminationAnnouncement", args.Id.ToString());
    #endregion

    #region Map Generation
    [PluginEvent(ServerEventType.MapGenerated)]
    private void OnMapGenerated(MapGeneratedEvent args) => HandleEvent(ServerEventType.MapGenerated, "MapGenerated");
    #endregion

    #region Items
    [PluginEvent(ServerEventType.GrenadeExploded)]
    private void OnGrenadeExploded(GrenadeExplodedEvent args) => HandleEvent(ServerEventType.GrenadeExploded, "GrenadeExploded", args.Grenade.Info.ItemId.ToString());

    [PluginEvent(ServerEventType.ItemSpawned)]
    private void OnItemSpawned(ItemSpawnedEvent args) => HandleEvent(ServerEventType.ItemSpawned, "ItemSpawned", args.Item.ToString());
    #endregion

    #region Environment
    [PluginEvent(ServerEventType.GeneratorActivated)]
    private void OnGeneratorActivated(GeneratorActivatedEvent args) => HandleEvent(ServerEventType.GeneratorActivated, "GeneratorActivated");

    [PluginEvent(ServerEventType.PlaceBlood)]
    private void OnPlaceBlood(PlaceBloodEvent args) => HandleEvent(ServerEventType.PlaceBlood, "PlaceBlood", args.Player.PlayerId.ToString(), args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.PlaceBulletHole)]
    private void OnPlaceBulletHole(PlaceBulletHoleEvent args) => HandleEvent(ServerEventType.PlaceBulletHole, "PlaceBulletHole");
    #endregion

    #region Round Events
    [PluginEvent(ServerEventType.RoundEnd)]
    private void OnRoundEnded(RoundEndEvent args) => HandleEvent(ServerEventType.RoundEnd, "RoundEnd", args.LeadingTeam.ToString());

    [PluginEvent(ServerEventType.RoundRestart)]
    private void OnRoundRestart(RoundRestartEvent args) => HandleEvent(ServerEventType.RoundRestart, "RoundRestart");

    [PluginEvent(ServerEventType.RoundStart)]
    private void OnRoundStart(RoundStartEvent args) => HandleEvent(ServerEventType.RoundStart, "RoundStart");

    [PluginEvent(ServerEventType.WaitingForPlayers)]
    private void OnWaitingForPlayers(WaitingForPlayersEvent args) => HandleEvent(ServerEventType.WaitingForPlayers, "WaitingForPlayers");

    [PluginEvent(ServerEventType.TeamRespawnSelected)]
    private void OnTeamSelected(TeamRespawnSelectedEvent args) => HandleEvent(ServerEventType.TeamRespawnSelected, "TeamRespawnSelected", args.Team.ToString());

    [PluginEvent(ServerEventType.TeamRespawn)]
    private void OnTeamRespawn(TeamRespawnEvent args) => HandleEvent(ServerEventType.TeamRespawn, "TeamRespawn", args.Team.ToString(), args.NextWaveMaxSize.ToString());

    [PluginEvent(ServerEventType.RoundEndConditionsCheck)]
    private void OnRoundEndConditionsCheck(RoundEndConditionsCheckEvent args) => HandleEvent(ServerEventType.RoundEndConditionsCheck, "RoundEndConditionsCheck");
    #endregion

    #region Warhead
    [PluginEvent(ServerEventType.WarheadStart)]
    private void OnWarheadStart(WarheadStartEvent args) => HandleEvent(ServerEventType.WarheadStart, "WarheadStart", args.Player.PlayerId.ToString(), args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.WarheadStop)]
    private void OnWarheadStop(WarheadStopEvent args) => HandleEvent(ServerEventType.WarheadStop, "WarheadStop", args.Player.PlayerId.ToString(), args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.WarheadDetonation)]
    private void OnWarheadDetonation(WarheadDetonationEvent args) => HandleEvent(ServerEventType.WarheadDetonation, "WarheadDetonation");
    #endregion

    #region Commands
    [PluginEvent(ServerEventType.PlayerGameConsoleCommand)]
    private void OnPlayerGameconsoleCommand(PlayerGameConsoleCommandEvent args) => HandleEvent(ServerEventType.PlayerGameConsoleCommand, "PlayerGameConsoleCommand",
        args.Player.PlayerId.ToString(), args.Player.DisplayNickname, args.Command);

    [PluginEvent(ServerEventType.RemoteAdminCommand)]
    private void OnRemoteadminCommand(RemoteAdminCommandEvent args) => HandleEvent(ServerEventType.RemoteAdminCommand, "RemoteAdminCommand", args.Command);

    [PluginEvent(ServerEventType.ConsoleCommand)]
    private void OnConsoleCommand(ConsoleCommandEvent args) => HandleEvent(ServerEventType.ConsoleCommand, "ConsoleCommand", args.Command);

    [PluginEvent(ServerEventType.RemoteAdminCommandExecuted)]
    private void OnRemoteadminCommandExecuted(RemoteAdminCommandExecutedEvent args) => HandleEvent(ServerEventType.RemoteAdminCommandExecuted, "RemoteAdminCommandExecuted",
        args.Command);

    [PluginEvent(ServerEventType.PlayerGameConsoleCommandExecuted)]
    private void OnPlayerGameconsoleCommandExecuted(PlayerGameConsoleCommandExecutedEvent args) => HandleEvent(ServerEventType.PlayerGameConsoleCommandExecuted,
        "PlayerGameConsoleCommandExecuted", args.Player.PlayerId.ToString(), args.Player.DisplayNickname, args.Command);

    [PluginEvent(ServerEventType.ConsoleCommandExecuted)]
    private void OnConsoleCommandExecuted(ConsoleCommandExecutedEvent args) => HandleEvent(ServerEventType.ConsoleCommandExecuted, "ConsoleCommandExecuted", args.Command);
    #endregion

    #region SCPs
    #region General
    [PluginEvent(ServerEventType.CassieAnnouncesScpTermination)]
    private void OnCassieAnnouncScpTermination(CassieAnnouncesScpTerminationEvent args) => HandleEvent(ServerEventType.CassieAnnouncesScpTermination, "CassieAnnouncesScpTermination",
        args.Player.RoleName, args.Announcement);
    #endregion

    #region 914
    [PluginEvent(ServerEventType.Scp914Activate)]
    private void OnScp914Activate(Scp914ActivateEvent args) => HandleEvent(ServerEventType.Scp914Activate, "Scp914Activate", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname, args.KnobSetting.ToString());

    [PluginEvent(ServerEventType.Scp914KnobChange)]
    private void OnScp914KnobChange(Scp914KnobChangeEvent args) => HandleEvent(ServerEventType.Scp914KnobChange, "Scp914KnobChange", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname, args.KnobSetting.ToString());

    [PluginEvent(ServerEventType.Scp914UpgradeInventory)]
    private void OnScp914UpgradeInventory(Scp914UpgradeInventoryEvent args) => HandleEvent(ServerEventType.Scp914UpgradeInventory, "Scp914UpgradeInventory",
        args.Player.PlayerId.ToString(), args.Player.DisplayNickname, args.Item.ItemTypeId.ToString(), args.KnobSetting.ToString());

    [PluginEvent(ServerEventType.Scp914UpgradePickup)]
    private void OnScp914UpgradePickup(Scp914UpgradePickupEvent args) => HandleEvent(ServerEventType.Scp914UpgradePickup, "Scp914UpgradePickup",
        args.Item.Info.ItemId.ToString(), args.KnobSetting.ToString());

    [PluginEvent(ServerEventType.Scp914PickupUpgraded)]
    private void OnScp914PickupUpgraded(Scp914PickupUpgradedEvent args) => HandleEvent(ServerEventType.Scp914PickupUpgraded, "Scp914PickupUpgraded",
        args.Item.Info.ItemId.ToString(), args.KnobSetting.ToString());

    [PluginEvent(ServerEventType.Scp914InventoryItemUpgraded)]
    private void OnScp914InventoryItemUpgraded(Scp914InventoryItemUpgradedEvent args) => HandleEvent(ServerEventType.Scp914InventoryItemUpgraded,
        "Scp914InventoryItemUpgraded", args.Player.PlayerId.ToString(), args.Player.DisplayNickname, args.Item.ItemTypeId.ToString(), args.KnobSetting.ToString());

    [PluginEvent(ServerEventType.Scp914ProcessPlayer)]
    private void OnScp914ProcessPlayer(Scp914ProcessPlayerEvent args) => HandleEvent(ServerEventType.Scp914ProcessPlayer, "Scp914ProcessPlayer",
        args.Player.PlayerId.ToString(), args.Player.DisplayNickname, args.KnobSetting.ToString());
    #endregion

    #region 106
    [PluginEvent(ServerEventType.Scp106Stalking)]
    private void OnScp106Stalking(Scp106StalkingEvent args) => HandleEvent(ServerEventType.Scp106Stalking, "Scp106Stalking", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.Scp106TeleportPlayer)]
    private void OnScp106TeleportPlayer(Scp106TeleportPlayerEvent args) => HandleEvent(ServerEventType.Scp106TeleportPlayer, "Scp106TeleportPlayer",
        args.Player.PlayerId.ToString(), args.Player.DisplayNickname, args.Target.PlayerId.ToString(), args.Target.DisplayNickname);
    #endregion

    #region 173
    [PluginEvent(ServerEventType.Scp173PlaySound)]
    private void OnScp173PlaySound(Scp173PlaySoundEvent args) => HandleEvent(ServerEventType.Scp173PlaySound, "Scp173PlaySound", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.Scp173BreakneckSpeeds)]
    private void OnScp173BreakneckSpeeds(Scp173BreakneckSpeedsEvent args) => HandleEvent(ServerEventType.Scp173BreakneckSpeeds, "Scp173BreakneckSpeeds",
        args.Player.PlayerId.ToString(), args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.Scp173NewObserver)]
    private void OnScp173NewObserver(Scp173NewObserverEvent args) => HandleEvent(ServerEventType.Scp173NewObserver, "Scp173NewObserver", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname, args.Target.PlayerId.ToString(), args.Target.DisplayNickname);

    [PluginEvent(ServerEventType.Scp173SnapPlayer)]
    private void OnScp173SnapPlayer(Scp173SnapPlayerEvent args) => HandleEvent(ServerEventType.Scp173SnapPlayer, "Scp173SnapPlayer", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname, args.Target.PlayerId.ToString(), args.Target.DisplayNickname);

    [PluginEvent(ServerEventType.Scp173CreateTantrum)]
    private void OnScp173CreateTantrum(Scp173CreateTantrumEvent args) => HandleEvent(ServerEventType.Scp173CreateTantrum, "Scp173CreateTantrum",
        args.Player.PlayerId.ToString(), args.Player.DisplayNickname);
    #endregion

    #region 939
    [PluginEvent(ServerEventType.Scp939CreateAmnesticCloud)]
    private void OnScp939CreateAmnesticCloud(Scp939CreateAmnesticCloudEvent args) => HandleEvent(ServerEventType.Scp939CreateAmnesticCloud, "Scp939CreateAmnesticCloud",
        args.Player.PlayerId.ToString(), args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.Scp939Lunge)]
    private void OnScp939Lunge(Scp939LungeEvent args) => HandleEvent(ServerEventType.Scp939Lunge, "Scp939Lunge", args.Player.PlayerId.ToString(), args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.Scp939Attack)]
    private void OnScp939Attack(Scp939AttackEvent args) => HandleEvent(ServerEventType.Scp939Attack, "Scp939Attack", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname);
    #endregion

    #region 079
    [PluginEvent(ServerEventType.Scp079GainExperience)]
    private void OnScp079GainExperience(Scp079GainExperienceEvent args) => HandleEvent(ServerEventType.Scp079GainExperience, "Scp079GainExperience",
        args.Player.PlayerId.ToString(), args.Player.DisplayNickname, args.Amount.ToString(), args.Reason.ToString());

    [PluginEvent(ServerEventType.Scp079LevelUpTier)]
    private void OnScp079LevelUpTier(Scp079LevelUpTierEvent args) => HandleEvent(ServerEventType.Scp079LevelUpTier, "Scp079LevelUpTier", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname, args.Tier.ToString());

    [PluginEvent(ServerEventType.Scp079UseTesla)]
    private void OnScp079UseTesla(Scp079UseTeslaEvent args) => HandleEvent(ServerEventType.Scp079UseTesla, "Scp079UseTesla", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.Scp079LockdownRoom)]
    private void OnScp079LockdownRoom(Scp079LockdownRoomEvent args) => HandleEvent(ServerEventType.Scp079LockdownRoom, "Scp079LockdownRoom", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname, args.Room.Name.ToString());

    [PluginEvent(ServerEventType.Scp079CancelRoomLockdown)]
    private void OnScp079CancelRoomLockdown(Scp079CancelRoomLockdownEvent args) => HandleEvent(ServerEventType.Scp079CancelRoomLockdown, "Scp079CancelRoomLockdown",
        args.Player.PlayerId.ToString(), args.Player.DisplayNickname, args.Room.Name.ToString());

    [PluginEvent(ServerEventType.Scp079LockDoor)]
    private void OnScp079LockDoor(Scp079LockDoorEvent args) => HandleEvent(ServerEventType.Scp079LockDoor, "Scp079LockDoor", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.Scp079UnlockDoor)]
    private void OnScp079UnLockDoor(Scp079UnlockDoorEvent args) => HandleEvent(ServerEventType.Scp079UnlockDoor, "Scp079UnlockDoor", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.Scp079BlackoutZone)]
    private void OnScp079BlackoutZone(Scp079BlackoutZoneEvent args) => HandleEvent(ServerEventType.Scp079BlackoutZone, "Scp079BlackoutZone", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname, args.Zone.ToString());

    [PluginEvent(ServerEventType.Scp079BlackoutRoom)]
    private void OnScp079BlackoutRoom(Scp079BlackoutRoomEvent args) => HandleEvent(ServerEventType.Scp079BlackoutRoom, "Scp079BlackoutRoom", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname, args.Room.Name.ToString());

    [PluginEvent(ServerEventType.Scp079CameraChanged)]
    private void OnScp079ChangedCamera(Scp079CameraChangedEvent args) => HandleEvent(ServerEventType.Scp079CameraChanged, "Scp079CameraChanged", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname, args.Camera.Label);
    #endregion

    #region 049
    [PluginEvent(ServerEventType.Scp049ResurrectBody)]
    private void OnScp049ResurrectBody(Scp049ResurrectBodyEvent args) => HandleEvent(ServerEventType.Scp049ResurrectBody, "Scp049ResurrectBody", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname, args.Target.PlayerId.ToString(), args.Target.DisplayNickname);

    [PluginEvent(ServerEventType.Scp049StartResurrectingBody)]
    private void OnScp049StartResurrectingBody(Scp049StartResurrectingBodyEvent args) => HandleEvent(ServerEventType.Scp049StartResurrectingBody, "Scp049StartResurrectingBody",
        args.Player.PlayerId.ToString(), args.Player.DisplayNickname, args.Target.PlayerId.ToString(), args.Target.DisplayNickname);
    #endregion

    #region 096
    [PluginEvent(ServerEventType.Scp096AddingTarget)]
    private void OnScp096AddTarget(Scp096AddingTargetEvent args) => HandleEvent(ServerEventType.Scp096AddingTarget, "Scp096AddingTarget", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname, args.Target.PlayerId.ToString(), args.Target.DisplayNickname);

    [PluginEvent(ServerEventType.Scp096Enraging)]
    private void OnScp096Enrage(Scp096EnragingEvent args) => HandleEvent(ServerEventType.Scp096Enraging, "Scp096Enraging", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.Scp096ChangeState)]
    private void OnScp096CalmDown(Scp096ChangeStateEvent args) => HandleEvent(ServerEventType.Scp096ChangeState, "Scp096ChangeState", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname, args.RageState.ToString());

    [PluginEvent(ServerEventType.Scp096Charging)]
    private void OnScp096Charge(Scp096ChargingEvent args) => HandleEvent(ServerEventType.Scp096Charging, "Scp096Charging", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.Scp096PryingGate)]
    private void OnScp096PryGate(Scp096PryingGateEvent args) => HandleEvent(ServerEventType.Scp096PryingGate, "Scp096PryingGate", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.Scp096TryNotCry)]
    private void OnScp096TryingNotCry(Scp096TryNotCryEvent args) => HandleEvent(ServerEventType.Scp096TryNotCry, "Scp096TryNotCry", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.Scp096StartCrying)]
    private void OnScp096StartCrying(Scp096StartCryingEvent args) => HandleEvent(ServerEventType.Scp096StartCrying, "Scp096StartCrying", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname);
    #endregion
    #endregion

    #region Bans
    [PluginEvent(ServerEventType.BanIssued)]
    private void OnBanIssued(BanIssuedEvent args) => HandleEvent(ServerEventType.BanIssued, "BanIssued", args.BanDetails.Id, args.BanType.ToString());

    [PluginEvent(ServerEventType.BanRevoked)]
    private void OnBanRevoked(BanRevokedEvent args) => HandleEvent(ServerEventType.BanRevoked, "BanRevoked", args.Id, args.BanType.ToString());

    [PluginEvent(ServerEventType.BanUpdated)]
    private void OnBanUpdated(BanUpdatedEvent args) => HandleEvent(ServerEventType.BanUpdated, "BanUpdated", args.BanDetails.Id, args.BanType.ToString());
    #endregion
}
