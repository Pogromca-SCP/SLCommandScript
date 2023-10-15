using System.Collections.Generic;
using PluginAPI.Enums;
using CommandSystem;
using System;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PluginAPI.Events;

namespace SLCommandScript.FileScriptsLoader.Events;

/// <summary>
/// Handles server events using script files.
/// </summary>
public class FileScriptsEventHandler
{
    /// <summary>
    /// Contains registered event handling scripts.
    /// </summary>
    public Dictionary<ServerEventType, ICommand> EventScripts { get; } = new();

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
    void OnPlayerJoin(PlayerJoinedEvent args) => HandleEvent(ServerEventType.PlayerJoined, "PlayerJoined", args.Player.PlayerId.ToString(), args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerLeft)]
    void OnPlayerLeave(PlayerLeftEvent args) => HandleEvent(ServerEventType.PlayerLeft, "PlayerLeft", args.Player.PlayerId.ToString(), args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerBanned)]
    void OnPlayerBanned(PlayerBannedEvent args) => HandleEvent(ServerEventType.PlayerBanned, "PlayerBanned", args.Player.Nickname, args.Reason);

    [PluginEvent(ServerEventType.PlayerKicked)]
    void OnPlayerKicked(PlayerKickedEvent args) => HandleEvent(ServerEventType.PlayerKicked, "PlayerKicked", args.Player.PlayerId.ToString(), args.Player.DisplayNickname,
        args.Reason);

    [PluginEvent(ServerEventType.PlayerCheaterReport)]
    void OnCheaterReport(PlayerCheaterReportEvent args) => HandleEvent(ServerEventType.PlayerCheaterReport, "PlayerCheaterReport", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname, args.Target.PlayerId.ToString(), args.Target.DisplayNickname, args.Reason);

    [PluginEvent(ServerEventType.PlayerChangeRole)]
    void OnChangeRole(PlayerChangeRoleEvent args) => HandleEvent(ServerEventType.PlayerChangeRole, "PlayerChangeRole", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname, args.OldRole.RoleTypeId.ToString(), args.NewRole.ToString(), args.ChangeReason.ToString());

    [PluginEvent(ServerEventType.PlayerPreauth)]
    void OnPreauth(PlayerPreauthEvent args) => HandleEvent(ServerEventType.PlayerPreauth, "PlayerPreauth", args.Region);

    [PluginEvent(ServerEventType.PlayerGetGroup)]
    void OnPlayerChangeGroup(PlayerGetGroupEvent args) => HandleEvent(ServerEventType.PlayerGetGroup, "PlayerGetGroup", args.UserId, args.Group.BadgeText);

    [PluginEvent(ServerEventType.PlayerReport)]
    void OnReport(PlayerReportEvent args) => HandleEvent(ServerEventType.PlayerReport, "PlayerReport", args.Player.PlayerId.ToString(), args.Player.DisplayNickname,
        args.Target.PlayerId.ToString(), args.Target.DisplayNickname, args.Reason);

    [PluginEvent(ServerEventType.RagdollSpawn)]
    void OnRagdollSpawn(RagdollSpawnEvent args) => HandleEvent(ServerEventType.RagdollSpawn, "RagdollSpawn", args.Player.PlayerId.ToString(), args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerMuted)]
    void OnPlayerMuted(PlayerMutedEvent args) => HandleEvent(ServerEventType.PlayerMuted, "PlayerMuted", args.Issuer.PlayerId.ToString(), args.Issuer.DisplayNickname,
        args.Player.PlayerId.ToString(), args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerUnmuted)]
    void OnPlayerUnmuted(PlayerUnmutedEvent args) => HandleEvent(ServerEventType.PlayerUnmuted, "PlayerUnmuted", args.Issuer.PlayerId.ToString(), args.Issuer.DisplayNickname,
        args.Player.PlayerId.ToString(), args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerCheckReservedSlot)]
    void OnCheckReservedSlot(PlayerCheckReservedSlotEvent args) => HandleEvent(ServerEventType.PlayerCheckReservedSlot, "PlayerCheckReservedSlot", args.Userid,
        args.HasReservedSlot.ToString());
    #endregion

    #region State
    [PluginEvent(ServerEventType.PlayerDeath)]
    void OnPlayerDied(PlayerDeathEvent args)
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
    void OnPlayerChangesSpectatedPlayer(PlayerChangeSpectatorEvent args)
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
    void OnPlayerEscaped(PlayerEscapeEvent args) => HandleEvent(ServerEventType.PlayerEscape, "PlayerEscape", args.Player.PlayerId.ToString(), args.Player.DisplayNickname,
        args.NewRole.ToString());

    [PluginEvent(ServerEventType.PlayerHandcuff)]
    void OnPlayerHandcuffed(PlayerHandcuffEvent args) => HandleEvent(ServerEventType.PlayerHandcuff, "PlayerHandcuff", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname, args.Target.PlayerId.ToString(), args.Target.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerRemoveHandcuffs)]
    void OnPlayerUncuffed(PlayerRemoveHandcuffsEvent args) => HandleEvent(ServerEventType.PlayerRemoveHandcuffs, "PlayerRemoveHandcuffs", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname, args.Target.PlayerId.ToString(), args.Target.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerDamage)]
    void OnPlayerDamage(PlayerDamageEvent args)
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
    void OnReceiveEffect(PlayerReceiveEffectEvent args) => HandleEvent(ServerEventType.PlayerReceiveEffect, "PlayerReceiveEffect", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname, args.Effect.Classification.ToString(), args.Intensity.ToString(), args.Duration.ToString());

    [PluginEvent(ServerEventType.PlayerSpawn)]
    void OnSpawn(PlayerSpawnEvent args) => HandleEvent(ServerEventType.PlayerSpawn, "PlayerSpawn", args.Player.PlayerId.ToString(), args.Player.DisplayNickname,
        args.Role.ToString());

    [PluginEvent(ServerEventType.PlayerEnterPocketDimension)]
    void OnPlayerEnterPocketDimension(PlayerEnterPocketDimensionEvent args) => HandleEvent(ServerEventType.PlayerEnterPocketDimension, "PlayerEnterPocketDimension",
        args.Player.PlayerId.ToString(), args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerExitPocketDimension)]
    void OnPlayerExitPocketDimension(PlayerExitPocketDimensionEvent args) => HandleEvent(ServerEventType.PlayerExitPocketDimension, "PlayerExitPocketDimension",
        args.Player.PlayerId.ToString(), args.Player.DisplayNickname, args.IsSuccessful.ToString());
    #endregion

    #region Environment
    [PluginEvent(ServerEventType.PlayerActivateGenerator)]
    void OnPlayerActivateGenerator(PlayerActivateGeneratorEvent args) => HandleEvent(ServerEventType.PlayerActivateGenerator, "PlayerActivateGenerator",
        args.Player.PlayerId.ToString(), args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerUsingIntercom)]
    void OnPlayerUsingIntercom(PlayerUsingIntercomEvent args) => HandleEvent(ServerEventType.PlayerUsingIntercom, "PlayerUsingIntercom", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname, args.IntercomState.ToString());

    [PluginEvent(ServerEventType.PlayerCloseGenerator)]
    void OnPlayerClosesGenerator(PlayerCloseGeneratorEvent args) => HandleEvent(ServerEventType.PlayerCloseGenerator, "PlayerCloseGenerator", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerDamagedShootingTarget)]
    void OnPlayerDamagedShootingTarget(PlayerDamagedShootingTargetEvent args) => HandleEvent(ServerEventType.PlayerDamagedShootingTarget, "PlayerDamagedShootingTarget",
        args.Player.PlayerId.ToString(), args.Player.DisplayNickname, args.DamageAmount.ToString());

    [PluginEvent(ServerEventType.PlayerDamagedWindow)]
    void OnPlayerDamagedWindow(PlayerDamagedWindowEvent args) => HandleEvent(ServerEventType.PlayerDamagedWindow, "PlayerDamagedWindow", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname, args.DamageAmount.ToString());

    [PluginEvent(ServerEventType.PlayerDeactivatedGenerator)]
    void OnPlayerDeactivatedGenerator(PlayerDeactivatedGeneratorEvent args) => HandleEvent(ServerEventType.PlayerDeactivatedGenerator, "PlayerDeactivatedGenerator",
        args.Player.PlayerId.ToString(), args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerOpenGenerator)]
    void OnPlayerOpenedGenerator(PlayerOpenGeneratorEvent args) => HandleEvent(ServerEventType.PlayerOpenGenerator, "PlayerOpenGenerator", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerInteractShootingTarget)]
    void OnInteractWithShootingTarget(PlayerInteractShootingTargetEvent args) => HandleEvent(ServerEventType.PlayerInteractShootingTarget, "PlayerInteractShootingTarget",
        args.Player.PlayerId.ToString(), args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerInteractLocker)]
    void OnInteractWithLocker(PlayerInteractLockerEvent args) => HandleEvent(ServerEventType.PlayerInteractLocker, "PlayerInteractLocker", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerInteractElevator)]
    void OnInteractWithElevator(PlayerInteractElevatorEvent args) => HandleEvent(ServerEventType.PlayerInteractElevator, "PlayerInteractElevator",
        args.Player.PlayerId.ToString(), args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerUnlockGenerator)]
    void OnUnlockGenerator(PlayerUnlockGeneratorEvent args) => HandleEvent(ServerEventType.PlayerUnlockGenerator, "PlayerUnlockGenerator", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerInteractDoor)]
    void OnPlayerInteractDoor(PlayerInteractDoorEvent args) => HandleEvent(ServerEventType.PlayerInteractDoor, "PlayerInteractDoor", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerInteractGenerator)]
    void OnPlayerInteractGenerator(PlayerInteractGeneratorEvent args) => HandleEvent(ServerEventType.PlayerInteractGenerator, "PlayerInteractGenerator",
        args.Player.PlayerId.ToString(), args.Player.DisplayNickname);
    #endregion

    #region Weapons
    [PluginEvent(ServerEventType.PlayerAimWeapon)]
    void OnPlayerAimsWeapon(PlayerAimWeaponEvent args) => HandleEvent(ServerEventType.PlayerAimWeapon, "PlayerAimWeapon", args.IsAiming.ToString(),
        args.Firearm.ItemTypeId.ToString(), args.Player.PlayerId.ToString(), args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerDryfireWeapon)]
    void OnPlayerDryfireWeapon(PlayerDryfireWeaponEvent args) => HandleEvent(ServerEventType.PlayerDryfireWeapon, "PlayerDryfireWeapon", args.Firearm.ItemTypeId.ToString(),
        args.Player.PlayerId.ToString(), args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerReloadWeapon)]
    void OnReloadWeapon(PlayerReloadWeaponEvent args) => HandleEvent(ServerEventType.PlayerReloadWeapon, "PlayerReloadWeapon", args.Firearm.ItemTypeId.ToString(),
        args.Player.PlayerId.ToString(), args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerShotWeapon)]
    void OnShotWeapon(PlayerShotWeaponEvent args) => HandleEvent(ServerEventType.PlayerShotWeapon, "PlayerShotWeapon", args.Firearm.ItemTypeId.ToString(),
        args.Player.PlayerId.ToString(), args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerUnloadWeapon)]
    void OnUnloadWeapon(PlayerUnloadWeaponEvent args) => HandleEvent(ServerEventType.PlayerUnloadWeapon, "PlayerUnloadWeapon", args.Firearm.ItemTypeId.ToString(),
        args.Player.PlayerId.ToString(), args.Player.DisplayNickname);
    #endregion

    #region Items
    [PluginEvent(ServerEventType.PlayerCancelUsingItem)]
    void OnPlayerCancelsUsingItem(PlayerCancelUsingItemEvent args) => HandleEvent(ServerEventType.PlayerCancelUsingItem, "PlayerCancelUsingItem",
        args.Item.ItemTypeId.ToString(), args.Player.PlayerId.ToString(), args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerChangeItem)]
    void OnPlayerChangesItem(PlayerChangeItemEvent args) => HandleEvent(ServerEventType.PlayerChangeItem, "PlayerChangeItem", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerDropAmmo)]
    void OnPlayerDropAmmo(PlayerDropAmmoEvent args) => HandleEvent(ServerEventType.PlayerDropAmmo, "PlayerDropAmmo", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname, args.Item.ToString(), args.Amount.ToString());

    [PluginEvent(ServerEventType.PlayerDropItem)]
    void OnPlayerDropItem(PlayerDropItemEvent args) => HandleEvent(ServerEventType.PlayerDropItem, "PlayerDropItem", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname, args.Item.ItemTypeId.ToString());

    [PluginEvent(ServerEventType.PlayerDroppedAmmo)]
    void OnPlayerDroppedAmmo(PlayerDroppedAmmoEvent args) => HandleEvent(ServerEventType.PlayerDroppedAmmo, "PlayerDroppedAmmo", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname, args.Item.Info.ItemId.ToString(), args.Amount.ToString());

    [PluginEvent(ServerEventType.PlayerDropedpItem)]
    void OnPlayerDroppedItem(PlayerDroppedItemEvent args) => HandleEvent(ServerEventType.PlayerDropedpItem, "PlayerDroppedItem", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname, args.Item.Info.ItemId.ToString());

    [PluginEvent(ServerEventType.PlayerPickupAmmo)]
    void OnPlayerPickupAmmo(PlayerPickupAmmoEvent args) => HandleEvent(ServerEventType.PlayerPickupAmmo, "PlayerPickupAmmo", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname, args.Item.Info.ItemId.ToString());

    [PluginEvent(ServerEventType.PlayerPickupArmor)]
    void OnPlayerPickupArmor(PlayerPickupArmorEvent args) => HandleEvent(ServerEventType.PlayerPickupArmor, "PlayerPickupArmor", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname, args.Item.Info.ItemId.ToString());

    [PluginEvent(ServerEventType.PlayerPickupScp330)]
    void OnPlayerPickupScp330(PlayerPickupScp330Event args) => HandleEvent(ServerEventType.PlayerPickupScp330, "PlayerPickupScp330", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname, args.Item.Info.ItemId.ToString());

    [PluginEvent(ServerEventType.PlayerSearchPickup)]
    void OnSearchPickup(PlayerSearchPickupEvent args) => HandleEvent(ServerEventType.PlayerSearchPickup, "PlayerSearchPickup", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname, args.Item.Info.ItemId.ToString());

    [PluginEvent(ServerEventType.PlayerSearchedPickup)]
    void OnSearchedPickup(PlayerSearchedPickupEvent args) => HandleEvent(ServerEventType.PlayerSearchedPickup, "PlayerSearchedPickup", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname, args.Item.Info.ItemId.ToString());

    [PluginEvent(ServerEventType.PlayerThrowItem)]
    void OnThrowItem(PlayerThrowItemEvent args) => HandleEvent(ServerEventType.PlayerThrowItem, "PlayerThrowItem", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname, args.Item.ItemTypeId.ToString());

    [PluginEvent(ServerEventType.PlayerToggleFlashlight)]
    void OnToggleFlashlight(PlayerToggleFlashlightEvent args) => HandleEvent(ServerEventType.PlayerToggleFlashlight, "PlayerToggleFlashlight", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname, args.IsToggled.ToString());

    [PluginEvent(ServerEventType.PlayerUsedItem)]
    void OnPlayerUsedItem(PlayerUsedItemEvent args) => HandleEvent(ServerEventType.PlayerUsedItem, "PlayerUsedItem", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname, args.Item.ItemTypeId.ToString());

    [PluginEvent(ServerEventType.PlayerUseHotkey)]
    void OnPlayerUsedHotkey(PlayerUseHotkeyEvent args) => HandleEvent(ServerEventType.PlayerUseHotkey, "PlayerUseHotkey", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname, args.Action.ToString());

    [PluginEvent(ServerEventType.PlayerUseItem)]
    void OnPlayerStartedUsingItem(PlayerUseItemEvent args) => HandleEvent(ServerEventType.PlayerUseItem, "PlayerUseItem", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname, args.Item.ItemTypeId.ToString());

    [PluginEvent(ServerEventType.PlayerInteractScp330)]
    void OnInteractWithScp330(PlayerInteractScp330Event args) => HandleEvent(ServerEventType.PlayerInteractScp330, "PlayerInteractScp330", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerThrowProjectile)]
    void OnPlayerThrowProjectile(PlayerThrowProjectileEvent args) => HandleEvent(ServerEventType.PlayerThrowProjectile, "PlayerThrowProjectile",
        args.Thrower.PlayerId.ToString(), args.Thrower.DisplayNickname, args.Item.ItemTypeId.ToString());

    [PluginEvent(ServerEventType.PlayerPreCoinFlip)]
    void OnPlayerPreCoinFlip(PlayerPreCoinFlipEvent args) => HandleEvent(ServerEventType.PlayerPreCoinFlip, "PlayerPreCoinFlip", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerCoinFlip)]
    void OnPlayerCoinFlip(PlayerCoinFlipEvent args) => HandleEvent(ServerEventType.PlayerCoinFlip, "PlayerCoinFlip", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname, args.IsTails.ToString());
    #endregion

    #region Radio
    [PluginEvent(ServerEventType.PlayerChangeRadioRange)]
    void OnPlayerChangesRadioRange(PlayerChangeRadioRangeEvent args) => HandleEvent(ServerEventType.PlayerChangeRadioRange, "PlayerChangeRadioRange",
        args.Player.PlayerId.ToString(), args.Player.DisplayNickname, args.Range.ToString());

    [PluginEvent(ServerEventType.PlayerRadioToggle)]
    void OnPlayerRadioToggle(PlayerRadioToggleEvent args) => HandleEvent(ServerEventType.PlayerRadioToggle, "PlayerRadioToggle", args.NewState.ToString(),
        args.Player.PlayerId.ToString(), args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerUsingRadio)]
    void OnPlayerUsingRadio(PlayerUsingRadioEvent args) => HandleEvent(ServerEventType.PlayerUsingRadio, "PlayerUsingRadio", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname);
    #endregion
    #endregion

    #region Decontamination
    [PluginEvent(ServerEventType.LczDecontaminationStart)]
    void OnLczDecontaminationStarts(LczDecontaminationStartEvent args) => HandleEvent(ServerEventType.LczDecontaminationStart, "LczDecontaminationStart");

    [PluginEvent(ServerEventType.LczDecontaminationAnnouncement)]
    void OnAnnounceLczDecontamination(LczDecontaminationAnnouncementEvent args) => HandleEvent(ServerEventType.LczDecontaminationAnnouncement,
        "LczDecontaminationAnnouncement", args.Id.ToString());
    #endregion

    #region Map Generation
    [PluginEvent(ServerEventType.MapGenerated)]
    void OnMapGenerated(MapGeneratedEvent args) => HandleEvent(ServerEventType.MapGenerated, "MapGenerated");
    #endregion

    #region Items
    [PluginEvent(ServerEventType.GrenadeExploded)]
    void OnGrenadeExploded(GrenadeExplodedEvent args) => HandleEvent(ServerEventType.GrenadeExploded, "GrenadeExploded", args.Grenade.Info.ItemId.ToString());

    [PluginEvent(ServerEventType.ItemSpawned)]
    void OnItemSpawned(ItemSpawnedEvent args) => HandleEvent(ServerEventType.ItemSpawned, "ItemSpawned", args.Item.ToString());
    #endregion

    #region Environment
    [PluginEvent(ServerEventType.GeneratorActivated)]
    void OnGeneratorActivated(GeneratorActivatedEvent args) => HandleEvent(ServerEventType.GeneratorActivated, "GeneratorActivated");

    [PluginEvent(ServerEventType.PlaceBlood)]
    void OnPlaceBlood(PlaceBloodEvent args) => HandleEvent(ServerEventType.PlaceBlood, "PlaceBlood", args.Player.PlayerId.ToString(), args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.PlaceBulletHole)]
    void OnPlaceBulletHole(PlaceBulletHoleEvent args) => HandleEvent(ServerEventType.PlaceBulletHole, "PlaceBulletHole");
    #endregion

    #region Round Events
    [PluginEvent(ServerEventType.RoundEnd)]
    void OnRoundEnded(RoundEndEvent args) => HandleEvent(ServerEventType.RoundEnd, "RoundEnd", args.LeadingTeam.ToString());

    [PluginEvent(ServerEventType.RoundRestart)]
    void OnRoundRestart(RoundRestartEvent args) => HandleEvent(ServerEventType.RoundRestart, "RoundRestart");

    [PluginEvent(ServerEventType.RoundStart)]
    void OnRoundStart(RoundStartEvent args) => HandleEvent(ServerEventType.RoundStart, "RoundStart");

    [PluginEvent(ServerEventType.WaitingForPlayers)]
    void OnWaitingForPlayers(WaitingForPlayersEvent args) => HandleEvent(ServerEventType.WaitingForPlayers, "WaitingForPlayers");

    [PluginEvent(ServerEventType.TeamRespawnSelected)]
    void OnTeamSelected(TeamRespawnSelectedEvent args) => HandleEvent(ServerEventType.TeamRespawnSelected, "TeamRespawnSelected", args.Team.ToString());

    [PluginEvent(ServerEventType.TeamRespawn)]
    void OnTeamRespawn(TeamRespawnEvent args) => HandleEvent(ServerEventType.TeamRespawn, "TeamRespawn", args.Team.ToString(), args.NextWaveMaxSize.ToString());

    [PluginEvent(ServerEventType.RoundEndConditionsCheck)]
    void OnRoundEndConditionsCheck(RoundEndConditionsCheckEvent args) => HandleEvent(ServerEventType.RoundEndConditionsCheck, "RoundEndConditionsCheck");
    #endregion

    #region Warhead
    [PluginEvent(ServerEventType.WarheadStart)]
    void OnWarheadStart(WarheadStartEvent args) => HandleEvent(ServerEventType.WarheadStart, "WarheadStart", args.Player.PlayerId.ToString(), args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.WarheadStop)]
    void OnWarheadStop(WarheadStopEvent args) => HandleEvent(ServerEventType.WarheadStop, "WarheadStop", args.Player.PlayerId.ToString(), args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.WarheadDetonation)]
    void OnWarheadDetonation(WarheadDetonationEvent args) => HandleEvent(ServerEventType.WarheadDetonation, "WarheadDetonation");
    #endregion

    #region Commands
    [PluginEvent(ServerEventType.PlayerGameConsoleCommand)]
    void OnPlayerGameconsoleCommand(PlayerGameConsoleCommandEvent args) => HandleEvent(ServerEventType.PlayerGameConsoleCommand, "PlayerGameConsoleCommand",
        args.Player.PlayerId.ToString(), args.Player.DisplayNickname, args.Command);

    [PluginEvent(ServerEventType.RemoteAdminCommand)]
    void OnRemoteadminCommand(RemoteAdminCommandEvent args) => HandleEvent(ServerEventType.RemoteAdminCommand, "RemoteAdminCommand", args.Command);

    [PluginEvent(ServerEventType.ConsoleCommand)]
    void OnConsoleCommand(ConsoleCommandEvent args) => HandleEvent(ServerEventType.ConsoleCommand, "ConsoleCommand", args.Command);

    [PluginEvent(ServerEventType.RemoteAdminCommandExecuted)]
    void OnRemoteadminCommandExecuted(RemoteAdminCommandExecutedEvent args) => HandleEvent(ServerEventType.RemoteAdminCommandExecuted, "RemoteAdminCommandExecuted",
        args.Command);

    [PluginEvent(ServerEventType.PlayerGameConsoleCommandExecuted)]
    void OnPlayerGameconsoleCommandExecuted(PlayerGameConsoleCommandExecutedEvent args) => HandleEvent(ServerEventType.PlayerGameConsoleCommandExecuted,
        "PlayerGameConsoleCommandExecuted", args.Player.PlayerId.ToString(), args.Player.DisplayNickname, args.Command);

    [PluginEvent(ServerEventType.ConsoleCommandExecuted)]
    void OnConsoleCommandExecuted(ConsoleCommandExecutedEvent args) => HandleEvent(ServerEventType.ConsoleCommandExecuted, "ConsoleCommandExecuted", args.Command);
    #endregion

    #region SCPs
    #region General
    [PluginEvent(ServerEventType.CassieAnnouncesScpTermination)]
    void OnCassieAnnouncScpTermination(CassieAnnouncesScpTerminationEvent args) => HandleEvent(ServerEventType.CassieAnnouncesScpTermination, "CassieAnnouncesScpTermination",
        args.Player.RoleName, args.Announcement);
    #endregion

    #region 914
    [PluginEvent(ServerEventType.Scp914Activate)]
    public void OnScp914Activate(Scp914ActivateEvent args) => HandleEvent(ServerEventType.Scp914Activate, "Scp914Activate", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname, args.KnobSetting.ToString());

    [PluginEvent(ServerEventType.Scp914KnobChange)]
    public void OnScp914KnobChange(Scp914KnobChangeEvent args) => HandleEvent(ServerEventType.Scp914KnobChange, "Scp914KnobChange", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname, args.KnobSetting.ToString());

    [PluginEvent(ServerEventType.Scp914UpgradeInventory)]
    public void OnScp914UpgradeInventory(Scp914UpgradeInventoryEvent args) => HandleEvent(ServerEventType.Scp914UpgradeInventory, "Scp914UpgradeInventory",
        args.Player.PlayerId.ToString(), args.Player.DisplayNickname, args.Item.ItemTypeId.ToString(), args.KnobSetting.ToString());

    [PluginEvent(ServerEventType.Scp914UpgradePickup)]
    public void OnScp914UpgradePickup(Scp914UpgradePickupEvent args) => HandleEvent(ServerEventType.Scp914UpgradePickup, "Scp914UpgradePickup",
        args.Item.Info.ItemId.ToString(), args.KnobSetting.ToString());

    [PluginEvent(ServerEventType.Scp914PickupUpgraded)]
    public void OnScp914PickupUpgraded(Scp914PickupUpgradedEvent args) => HandleEvent(ServerEventType.Scp914PickupUpgraded, "Scp914PickupUpgraded",
        args.Item.Info.ItemId.ToString(), args.KnobSetting.ToString());

    [PluginEvent(ServerEventType.Scp914InventoryItemUpgraded)]
    public void OnScp914InventoryItemUpgraded(Scp914InventoryItemUpgradedEvent args) => HandleEvent(ServerEventType.Scp914InventoryItemUpgraded,
        "Scp914InventoryItemUpgraded", args.Player.PlayerId.ToString(), args.Player.DisplayNickname, args.Item.ItemTypeId.ToString(), args.KnobSetting.ToString());

    [PluginEvent(ServerEventType.Scp914ProcessPlayer)]
    public void OnScp914ProcessPlayer(Scp914ProcessPlayerEvent args) => HandleEvent(ServerEventType.Scp914ProcessPlayer, "Scp914ProcessPlayer",
        args.Player.PlayerId.ToString(), args.Player.DisplayNickname, args.KnobSetting.ToString());
    #endregion

    #region 106
    [PluginEvent(ServerEventType.Scp106Stalking)]
    public void OnScp106Stalking(Scp106StalkingEvent args) => HandleEvent(ServerEventType.Scp106Stalking, "Scp106Stalking", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.Scp106TeleportPlayer)]
    public void OnScp106TeleportPlayer(Scp106TeleportPlayerEvent args) => HandleEvent(ServerEventType.Scp106TeleportPlayer, "Scp106TeleportPlayer",
        args.Player.PlayerId.ToString(), args.Player.DisplayNickname, args.Target.PlayerId.ToString(), args.Target.DisplayNickname);
    #endregion

    #region 173
    [PluginEvent(ServerEventType.Scp173PlaySound)]
    public void OnScp173PlaySound(Scp173PlaySoundEvent args) => HandleEvent(ServerEventType.Scp173PlaySound, "Scp173PlaySound", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.Scp173BreakneckSpeeds)]
    public void OnScp173BreakneckSpeeds(Scp173BreakneckSpeedsEvent args) => HandleEvent(ServerEventType.Scp173BreakneckSpeeds, "Scp173BreakneckSpeeds",
        args.Player.PlayerId.ToString(), args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.Scp173NewObserver)]
    public void OnScp173NewObserver(Scp173NewObserverEvent args) => HandleEvent(ServerEventType.Scp173NewObserver, "Scp173NewObserver", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname, args.Target.PlayerId.ToString(), args.Target.DisplayNickname);

    [PluginEvent(ServerEventType.Scp173SnapPlayer)]
    public void OnScp173SnapPlayer(Scp173SnapPlayerEvent args) => HandleEvent(ServerEventType.Scp173SnapPlayer, "Scp173SnapPlayer", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname, args.Target.PlayerId.ToString(), args.Target.DisplayNickname);

    [PluginEvent(ServerEventType.Scp173CreateTantrum)]
    public void OnScp173CreateTantrum(Scp173CreateTantrumEvent args) => HandleEvent(ServerEventType.Scp173CreateTantrum, "Scp173CreateTantrum",
        args.Player.PlayerId.ToString(), args.Player.DisplayNickname);
    #endregion

    #region 939
    [PluginEvent(ServerEventType.Scp939CreateAmnesticCloud)]
    public void OnScp939CreateAmnesticCloud(Scp939CreateAmnesticCloudEvent args) => HandleEvent(ServerEventType.Scp939CreateAmnesticCloud, "Scp939CreateAmnesticCloud",
        args.Player.PlayerId.ToString(), args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.Scp939Lunge)]
    public void OnScp939Lunge(Scp939LungeEvent args) => HandleEvent(ServerEventType.Scp939Lunge, "Scp939Lunge", args.Player.PlayerId.ToString(), args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.Scp939Attack)]
    public void OnScp939Attack(Scp939AttackEvent args) => HandleEvent(ServerEventType.Scp939Attack, "Scp939Attack", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname);
    #endregion

    #region 079
    [PluginEvent(ServerEventType.Scp079GainExperience)]
    public void OnScp079GainExperience(Scp079GainExperienceEvent args) => HandleEvent(ServerEventType.Scp079GainExperience, "Scp079GainExperience",
        args.Player.PlayerId.ToString(), args.Player.DisplayNickname, args.Amount.ToString(), args.Reason.ToString());

    [PluginEvent(ServerEventType.Scp079LevelUpTier)]
    public void OnScp079LevelUpTier(Scp079LevelUpTierEvent args) => HandleEvent(ServerEventType.Scp079LevelUpTier, "Scp079LevelUpTier", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname, args.Tier.ToString());

    [PluginEvent(ServerEventType.Scp079UseTesla)]
    public void OnScp079UseTesla(Scp079UseTeslaEvent args) => HandleEvent(ServerEventType.Scp079UseTesla, "Scp079UseTesla", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.Scp079LockdownRoom)]
    public void OnScp079LockdownRoom(Scp079LockdownRoomEvent args) => HandleEvent(ServerEventType.Scp079LockdownRoom, "Scp079LockdownRoom", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname, args.Room.Name.ToString());

    [PluginEvent(ServerEventType.Scp079CancelRoomLockdown)]
    public void OnScp079CancelRoomLockdown(Scp079CancelRoomLockdownEvent args) => HandleEvent(ServerEventType.Scp079CancelRoomLockdown, "Scp079CancelRoomLockdown",
        args.Player.PlayerId.ToString(), args.Player.DisplayNickname, args.Room.Name.ToString());

    [PluginEvent(ServerEventType.Scp079LockDoor)]
    public void OnScp079LockDoor(Scp079LockDoorEvent args) => HandleEvent(ServerEventType.Scp079LockDoor, "Scp079LockDoor", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.Scp079UnlockDoor)]
    public void OnScp079UnLockDoor(Scp079UnlockDoorEvent args) => HandleEvent(ServerEventType.Scp079UnlockDoor, "Scp079UnlockDoor", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.Scp079BlackoutZone)]
    public void OnScp079BlackoutZone(Scp079BlackoutZoneEvent args) => HandleEvent(ServerEventType.Scp079BlackoutZone, "Scp079BlackoutZone", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname, args.Zone.ToString());

    [PluginEvent(ServerEventType.Scp079BlackoutRoom)]
    public void OnScp079BlackoutRoom(Scp079BlackoutRoomEvent args) => HandleEvent(ServerEventType.Scp079BlackoutRoom, "Scp079BlackoutRoom", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname, args.Room.Name.ToString());

    [PluginEvent(ServerEventType.Scp079CameraChanged)]
    public void OnScp079ChangedCamera(Scp079CameraChangedEvent args) => HandleEvent(ServerEventType.Scp079CameraChanged, "Scp079CameraChanged", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname, args.Camera.Label);
    #endregion

    #region 049
    [PluginEvent(ServerEventType.Scp049ResurrectBody)]
    public void OnScp049ResurrectBody(Scp049ResurrectBodyEvent args) => HandleEvent(ServerEventType.Scp049ResurrectBody, "Scp049ResurrectBody", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname, args.Target.PlayerId.ToString(), args.Target.DisplayNickname);

    [PluginEvent(ServerEventType.Scp049StartResurrectingBody)]
    public void OnScp049StartResurrectingBody(Scp049StartResurrectingBodyEvent args) => HandleEvent(ServerEventType.Scp049StartResurrectingBody, "Scp049StartResurrectingBody",
        args.Player.PlayerId.ToString(), args.Player.DisplayNickname, args.Target.PlayerId.ToString(), args.Target.DisplayNickname);
    #endregion

    #region 096
    [PluginEvent(ServerEventType.Scp096AddingTarget)]
    public void OnScp096AddTarget(Scp096AddingTargetEvent args) => HandleEvent(ServerEventType.Scp096AddingTarget, "Scp096AddingTarget", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname, args.Target.PlayerId.ToString(), args.Target.DisplayNickname);

    [PluginEvent(ServerEventType.Scp096Enraging)]
    public void OnScp096Enrage(Scp096EnragingEvent args) => HandleEvent(ServerEventType.Scp096Enraging, "Scp096Enraging", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.Scp096ChangeState)]
    public void OnScp096CalmDown(Scp096ChangeStateEvent args) => HandleEvent(ServerEventType.Scp096ChangeState, "Scp096ChangeState", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname, args.RageState.ToString());

    [PluginEvent(ServerEventType.Scp096Charging)]
    public void OnScp096Charge(Scp096ChargingEvent args) => HandleEvent(ServerEventType.Scp096Charging, "Scp096Charging", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.Scp096PryingGate)]
    public void OnScp096PryGate(Scp096PryingGateEvent args) => HandleEvent(ServerEventType.Scp096PryingGate, "Scp096PryingGate", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.Scp096TryNotCry)]
    public void OnScp096TryingNotCry(Scp096TryNotCryEvent args) => HandleEvent(ServerEventType.Scp096TryNotCry, "Scp096TryNotCry", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname);

    [PluginEvent(ServerEventType.Scp096StartCrying)]
    public void OnScp096StartCrying(Scp096StartCryingEvent args) => HandleEvent(ServerEventType.Scp096StartCrying, "Scp096StartCrying", args.Player.PlayerId.ToString(),
        args.Player.DisplayNickname);
    #endregion
    #endregion

    #region Bans
    [PluginEvent(ServerEventType.BanIssued)]
    void OnBanIssued(BanIssuedEvent args) => HandleEvent(ServerEventType.BanIssued, "BanIssued", args.BanDetails.Id, args.BanType.ToString());

    [PluginEvent(ServerEventType.BanRevoked)]
    void OnBanRevoked(BanRevokedEvent args) => HandleEvent(ServerEventType.BanRevoked, "BanRevoked", args.Id, args.BanType.ToString());

    [PluginEvent(ServerEventType.BanUpdated)]
    void OnBanUpdated(BanUpdatedEvent args) => HandleEvent(ServerEventType.BanUpdated, "BanUpdated", args.BanDetails.Id, args.BanType.ToString());
    #endregion
}
