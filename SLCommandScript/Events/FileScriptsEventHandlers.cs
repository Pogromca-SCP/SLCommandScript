using System.Collections.Generic;
using PluginAPI.Enums;
using SLCommandScript.Commands;
using System;
using PluginAPI.Core;
using PluginAPI.Core.Attributes;
using PlayerStatsSystem;
using MapGeneration.Distributors;
using InventorySystem.Items.Firearms;
using CommandSystem;
using InventorySystem.Items.Usables;
using InventorySystem.Items.Radio;
using PlayerRoles.Voice;
using AdminToys;
using InventorySystem.Items;
using PlayerRoles;
using InventorySystem.Items.Pickups;
using CustomPlayerEffects;
using LiteNetLib;
using UnityEngine;
using Interactables.Interobjects;
using InventorySystem.Items.ThrowableProjectiles;
using Interactables.Interobjects.DoorUtils;
using Footprinting;
using Respawning;
using PlayerRoles.PlayableScps.Scp173;
using PlayerRoles.PlayableScps.Scp939;
using PlayerRoles.PlayableScps.Scp079;
using MapGeneration;
using PlayerRoles.PlayableScps.Scp079.Cameras;
using PlayerRoles.PlayableScps.Scp096;
using Scp914;

namespace SLCommandScript.Events;

/// <summary>
/// Handles server events using script files.
/// </summary>
public class FileScriptsEventHandlers
{
    /// <summary>
    /// Contains registered event handlers.
    /// </summary>
    public static IDictionary<ServerEventType, FileScriptCommand> EventScripts { get; set; } = null;

    /// <summary>
    /// Executes a command handler.
    /// </summary>
    /// <param name="eventType">Type of event to handle.</param>
    /// <param name="args">Event arguments to use.</param>
    private static void HandleEvent(ServerEventType eventType, params string[] args)
    {
        if (EventScripts is null || !EventScripts.ContainsKey(eventType))
        {
            return;
        }

        var cmd = EventScripts[eventType];

        if (cmd is null)
        {
            return;
        }

        var result = cmd.Execute(new ArraySegment<string>(args, 1, args.Length - 1), new ServerConsoleSender(), out var message);

        if (!result)
        {
            Log.Error(message, "FileScriptsEventHandlers: ");
        }
    }

    #region Players
    [PluginEvent(ServerEventType.PlayerJoined)]
    void OnPlayerJoin(Player player) => HandleEvent(ServerEventType.PlayerJoined, "PlayerJoined", player?.PlayerId.ToString(), player?.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerLeft)]
    void OnPlayerLeave(Player player) => HandleEvent(ServerEventType.PlayerLeft, "PlayerLeft", player?.PlayerId.ToString(), player?.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerDeath)]
    void OnPlayerDied(Player player, Player attacker, DamageHandlerBase damageHandler) =>
        HandleEvent(ServerEventType.PlayerDeath, "PlayerDeath", player?.PlayerId.ToString(), player?.DisplayNickname,
            attacker?.PlayerId.ToString(), attacker?.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerActivateGenerator)]
    void OnPlayerActivateGenerator(Player player, Scp079Generator gen) =>
        HandleEvent(ServerEventType.PlayerActivateGenerator, "PlayerActivateGenerator", player?.PlayerId.ToString(), player?.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerAimWeapon)]
    void OnPlayerAimsWeapon(Player player, Firearm gun, bool isAiming) =>
        HandleEvent(ServerEventType.PlayerAimWeapon, "PlayerAimWeapon", isAiming.ToString(), gun?.ItemTypeId.ToString(),
            player?.PlayerId.ToString(), player?.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerBanned)]
    void OnPlayerBanned(Player player, Player issuer, string reason, long duration) =>
        HandleEvent(ServerEventType.PlayerBanned, "PlayerBanned", player?.PlayerId.ToString(), player?.DisplayNickname, issuer?.PlayerId.ToString(),
            issuer?.DisplayNickname, reason);

    [PluginEvent(ServerEventType.PlayerCancelUsingItem)]
    void OnPlayerCancelsUsingItem(Player player, UsableItem item) =>
        HandleEvent(ServerEventType.PlayerCancelUsingItem, "PlayerCancelUsingItem", item.ItemTypeId.ToString(),
            player?.PlayerId.ToString(), player?.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerChangeItem)]
    void OnPlayerChangesItem(Player player, ushort oldItem, ushort newItem) =>
        HandleEvent(ServerEventType.PlayerChangeItem, "PlayerChangeItem", player?.PlayerId.ToString(), player?.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerChangeRadioRange)]
    void OnPlayerChangesRadioRange(Player player, RadioItem item, RadioMessages.RadioRangeLevel range) =>
        HandleEvent(ServerEventType.PlayerChangeRadioRange, "PlayerChangeRadioRange", player?.PlayerId.ToString(), player?.DisplayNickname,
            range.ToString());

    [PluginEvent(ServerEventType.PlayerRadioToggle)]
    void OnPlayerRadioToggle(Player player, RadioItem item, bool newState) =>
        HandleEvent(ServerEventType.PlayerRadioToggle, "PlayerRadioToggle", newState.ToString(), player?.PlayerId.ToString(), player?.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerUsingRadio)]
    void OnPlayerUsingRadio(Player player, RadioItem radio, float drain) =>
        HandleEvent(ServerEventType.PlayerUsingRadio, "PlayerUsingRadio", player?.PlayerId.ToString(), player?.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerGetGroup)]
    void OnPlayerChangeGroup(string userID, UserGroup group) =>
        HandleEvent(ServerEventType.PlayerGetGroup, "PlayerGetGroup", userID, group?.BadgeText);

    [PluginEvent(ServerEventType.PlayerUsingIntercom)]
    void OnPlayerUsingIntercom(Player player, IntercomState state) =>
        HandleEvent(ServerEventType.PlayerUsingIntercom, "PlayerUsingIntercom", player?.PlayerId.ToString(), player?.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerChangeSpectator)]
    void OnPlayerChangesSpectatedPlayer(Player player, Player oldTarget, Player newTarget) =>
        HandleEvent(ServerEventType.PlayerChangeSpectator, "PlayerChangeSpectator", player?.PlayerId.ToString(), player?.DisplayNickname,
            oldTarget?.PlayerId.ToString(), oldTarget?.DisplayNickname, newTarget?.PlayerId.ToString(), newTarget?.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerCloseGenerator)]
    void OnPlayerClosesGenerator(Player player, Scp079Generator gen) =>
        HandleEvent(ServerEventType.PlayerCloseGenerator, "PlayerCloseGenerator", player?.PlayerId.ToString(), player?.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerDamagedShootingTarget)]
    void OnPlayerDamagedShootingTarget(Player player, ShootingTarget target, DamageHandlerBase damageHandler, float amount) =>
        HandleEvent(ServerEventType.PlayerDamagedShootingTarget, "PlayerDamagedShootingTarget", player?.PlayerId.ToString(), player?.DisplayNickname,
            amount.ToString());

    [PluginEvent(ServerEventType.PlayerDamagedWindow)]
    void OnPlayerDamagedWindow(Player player, BreakableWindow window, DamageHandlerBase damageHandler, float amount) =>
        HandleEvent(ServerEventType.PlayerDamagedWindow, "PlayerDamagedWindow", player?.PlayerId.ToString(), player?.DisplayNickname, amount.ToString());

    [PluginEvent(ServerEventType.PlayerDeactivatedGenerator)]
    void OnPlayerDeactivatedGenerator(Player player, Scp079Generator gen) =>
        HandleEvent(ServerEventType.PlayerDeactivatedGenerator, "PlayerDeactivatedGenerator", player?.PlayerId.ToString(), player?.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerDropAmmo)]
    void OnPlayerDroppedAmmo(Player player, ItemType ammoType, int amount) =>
        HandleEvent(ServerEventType.PlayerDropAmmo, "PlayerDropAmmo", player?.PlayerId.ToString(), player?.DisplayNickname, ammoType.ToString(),
            amount.ToString());

    [PluginEvent(ServerEventType.PlayerDropItem)]
    void OnPlayerDroppedItem(Player player, ItemBase item) =>
        HandleEvent(ServerEventType.PlayerDropItem, "PlayerDropItem", player?.PlayerId.ToString(), player?.DisplayNickname, item?.ItemTypeId.ToString());

    [PluginEvent(ServerEventType.PlayerDryfireWeapon)]
    void OnPlayerDryfireWeapon(Player player, Firearm item) =>
        HandleEvent(ServerEventType.PlayerDryfireWeapon, "PlayerDryfireWeapon", player?.PlayerId.ToString(), player?.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerEscape)]
    void OnPlayerEscaped(Player player, RoleTypeId role) =>
        HandleEvent(ServerEventType.PlayerEscape, "PlayerEscape", player?.PlayerId.ToString(), player?.DisplayNickname, role.ToString());

    [PluginEvent(ServerEventType.PlayerHandcuff)]
    void OnPlayerHandcuffed(Player player, Player target) =>
        HandleEvent(ServerEventType.PlayerHandcuff, "PlayerHandcuff", player?.PlayerId.ToString(), player?.DisplayNickname,
            target?.PlayerId.ToString(), target?.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerRemoveHandcuffs)]
    void OnPlayerUncuffed(Player player, Player target, bool success) =>
        HandleEvent(ServerEventType.PlayerRemoveHandcuffs, "PlayerRemoveHandcuffs", player?.PlayerId.ToString(), player?.DisplayNickname,
            target?.PlayerId.ToString(), target?.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerDamage)]
    void OnPlayerDamage(Player player, Player attacker, DamageHandlerBase damageHandler) =>
        HandleEvent(ServerEventType.PlayerDamage, "PlayerDamage", player?.PlayerId.ToString(), player?.DisplayNickname, attacker?.PlayerId.ToString(),
            attacker?.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerKicked)]
    void OnPlayerKicked(Player player, ICommandSender issuer, string reason) =>
        HandleEvent(ServerEventType.PlayerKicked, "PlayerKicked", player?.PlayerId.ToString(), player?.DisplayNickname, reason);

    [PluginEvent(ServerEventType.PlayerOpenGenerator)]
    void OnPlayerOpenedGenerator(Player player, Scp079Generator gen) =>
        HandleEvent(ServerEventType.PlayerOpenGenerator, "PlayerOpenGenerator", player?.PlayerId.ToString(), player?.DisplayNickname);


    [PluginEvent(ServerEventType.PlayerPickupAmmo)]
    void OnPlayerPickupAmmo(Player player, ItemPickupBase pickup) =>
        HandleEvent(ServerEventType.PlayerPickupAmmo, "PlayerPickupAmmo", player?.PlayerId.ToString(), player?.DisplayNickname,
            pickup?.Info.ItemId.ToString());

    [PluginEvent(ServerEventType.PlayerPickupArmor)]
    void OnPlayerPickupArmor(Player player, ItemPickupBase pickup) =>
        HandleEvent(ServerEventType.PlayerPickupArmor, "PlayerPickupArmor", player?.PlayerId.ToString(), player?.DisplayNickname,
            pickup?.Info.ItemId.ToString());

    [PluginEvent(ServerEventType.PlayerPickupScp330)]
    void OnPlayerPickupScp330(Player player, ItemPickupBase pickup) =>
        HandleEvent(ServerEventType.PlayerPickupScp330, "PlayerPickupScp330", player?.PlayerId.ToString(), player?.DisplayNickname,
            pickup?.Info.ItemId.ToString());

    [PluginEvent(ServerEventType.PlayerPreauth)]
    void OnPreauth(string userid, string ipAddress, long expiration, CentralAuthPreauthFlags flags, string country, byte[] signature, ConnectionRequest req, int index) =>
        HandleEvent(ServerEventType.PlayerPreauth, "PlayerPreauth", country);

    [PluginEvent(ServerEventType.PlayerReceiveEffect)]
    void OnReceiveEffect(Player player, StatusEffectBase effect, byte intensity, float duration) =>
        HandleEvent(ServerEventType.PlayerReceiveEffect, "PlayerReceiveEffect", player?.PlayerId.ToString(), player?.DisplayNickname, intensity.ToString(),
            duration.ToString());

    [PluginEvent(ServerEventType.PlayerReloadWeapon)]
    void OnReloadWeapon(Player player, Firearm gun) =>
        HandleEvent(ServerEventType.PlayerReloadWeapon, "PlayerReloadWeapon", player?.PlayerId.ToString(), player?.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerChangeRole)]
    void OnChangeRole(Player player, PlayerRoleBase oldRole, RoleTypeId newRole, RoleChangeReason reason) =>
        HandleEvent(ServerEventType.PlayerChangeRole, "PlayerChangeRole", player?.PlayerId.ToString(), player?.DisplayNickname, newRole.ToString());

    [PluginEvent(ServerEventType.PlayerSearchPickup)]
    void OnSearchPickup(Player player, ItemPickupBase pickup) =>
        HandleEvent(ServerEventType.PlayerSearchPickup, "PlayerSearchPickup", player?.PlayerId.ToString(), player?.DisplayNickname,
            pickup?.Info.ItemId.ToString());

    [PluginEvent(ServerEventType.PlayerSearchedPickup)]
    void OnSearchedPickup(Player player, ItemPickupBase pickup) =>
        HandleEvent(ServerEventType.PlayerSearchedPickup, "PlayerSearchedPickup", player?.PlayerId.ToString(), player?.DisplayNickname,
            pickup?.Info.ItemId.ToString());

    [PluginEvent(ServerEventType.PlayerShotWeapon)]
    void OnShotWeapon(Player player, Firearm gun) =>
        HandleEvent(ServerEventType.PlayerShotWeapon, "PlayerShotWeapon", player?.PlayerId.ToString(), player?.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerSpawn)]
    void OnSpawn(Player player, RoleTypeId role) =>
        HandleEvent(ServerEventType.PlayerSpawn, "PlayerSpawn", player?.PlayerId.ToString(), player?.DisplayNickname, role.ToString());

    [PluginEvent(ServerEventType.PlayerThrowItem)]
    void OnThrowItem(Player player, ItemBase item, Rigidbody rb) =>
        HandleEvent(ServerEventType.PlayerThrowItem, "PlayerThrowItem", player?.PlayerId.ToString(), player?.DisplayNickname, item?.ItemTypeId.ToString());

    [PluginEvent(ServerEventType.PlayerToggleFlashlight)]
    void OnToggleFlashlight(Player player, ItemBase item, bool isToggled) =>
        HandleEvent(ServerEventType.PlayerToggleFlashlight, "PlayerToggleFlashlight", player?.PlayerId.ToString(), player?.DisplayNickname,
            isToggled.ToString());

    [PluginEvent(ServerEventType.PlayerUnloadWeapon)]
    void OnUnloadWeapon(Player player, Firearm gun) =>
        HandleEvent(ServerEventType.PlayerUnloadWeapon, "PlayerUnloadWeapon", player?.PlayerId.ToString(), player?.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerUnlockGenerator)]
    void OnUnlockGenerator(Player player, Scp079Generator gen) =>
        HandleEvent(ServerEventType.PlayerUnlockGenerator, "PlayerUnlockGenerator", player?.PlayerId.ToString(), player?.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerUsedItem)]
    void OnPlayerUsedItem(Player player, ItemBase item) =>
        HandleEvent(ServerEventType.PlayerUsedItem, "PlayerUsedItem", player?.PlayerId.ToString(), player?.DisplayNickname, item?.ItemTypeId.ToString());

    [PluginEvent(ServerEventType.PlayerUseHotkey)]
    void OnPlayerUsedHotkey(Player player, ActionName action) =>
        HandleEvent(ServerEventType.PlayerUseHotkey, "PlayerUseHotkey", player?.PlayerId.ToString(), player?.DisplayNickname, action.ToString());

    [PluginEvent(ServerEventType.PlayerUseItem)]
    void OnPlayerStartedUsingItem(Player player, UsableItem item) =>
        HandleEvent(ServerEventType.PlayerUseItem, "PlayerUseItem", player?.PlayerId.ToString(), player?.DisplayNickname, item?.ItemTypeId.ToString());

    [PluginEvent(ServerEventType.PlayerCheaterReport)]
    void OnCheaterReport(Player player, Player target, string reason) =>
        HandleEvent(ServerEventType.PlayerCheaterReport, "PlayerCheaterReport", player?.PlayerId.ToString(), player?.DisplayNickname,
            target?.PlayerId.ToString(), target?.DisplayNickname, reason);

    [PluginEvent(ServerEventType.PlayerReport)]
    void OnReport(Player player, Player target, string reason) =>
        HandleEvent(ServerEventType.PlayerReport, "PlayerReport", player?.PlayerId.ToString(), player?.DisplayNickname, target?.PlayerId.ToString(),
            target?.DisplayNickname, reason);

    [PluginEvent(ServerEventType.PlayerInteractShootingTarget)]
    void OnInteractWithShootingTarget(Player player, ShootingTarget target) =>
        HandleEvent(ServerEventType.PlayerInteractShootingTarget, "PlayerInteractShootingTarget", player?.PlayerId.ToString(), player?.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerInteractLocker)]
    void OnInteractWithLocker(Player player, Locker locker, LockerChamber chamber, bool canAccess) =>
        HandleEvent(ServerEventType.PlayerInteractLocker, "PlayerInteractLocker", player?.PlayerId.ToString(), player?.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerInteractElevator)]
    void OnInteractWithElevator(Player player, ElevatorChamber elevator) =>
        HandleEvent(ServerEventType.PlayerInteractElevator, "PlayerInteractElevator", player?.PlayerId.ToString(), player?.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerInteractScp330)]
    void OnInteractWithScp330(Player player, int candy, bool first, bool second) =>
        HandleEvent(ServerEventType.PlayerInteractScp330, "PlayerInteractScp330", player?.PlayerId.ToString(), player?.DisplayNickname);

    [PluginEvent(ServerEventType.RagdollSpawn)]
    void OnRagdollSpawn(Player player, IRagdollRole ragdoll, DamageHandlerBase damageHandler) =>
        HandleEvent(ServerEventType.RagdollSpawn, "RagdollSpawn", player?.PlayerId.ToString(), player?.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerMuted)]
    public void OnPlayerMuted(Player player, Player target, bool isIntercom) =>
        HandleEvent(ServerEventType.PlayerMuted, "PlayerMuted", player?.PlayerId.ToString(), player?.DisplayNickname, target?.PlayerId.ToString(),
            target?.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerUnmuted)]
    public void OnPlayerUnmuted(Player player, Player target, bool isIntercom) =>
        HandleEvent(ServerEventType.PlayerUnmuted, "PlayerUnmuted", player?.PlayerId.ToString(), player?.DisplayNickname, target?.PlayerId.ToString(),
            target?.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerCheckReservedSlot)]
    public void OnCheckReservedSlot(string userid, bool hasReservedSlot) =>
        HandleEvent(ServerEventType.PlayerCheckReservedSlot, "PlayerCheckReservedSlot", userid, hasReservedSlot.ToString());

    [PluginEvent(ServerEventType.PlayerEnterPocketDimension)]
    public void OnPlayerEnterPocketDimension(Player player) =>
        HandleEvent(ServerEventType.PlayerEnterPocketDimension, "PlayerEnterPocketDimension", player?.PlayerId.ToString(), player?.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerExitPocketDimension)]
    public void OnPlayerExitPocketDimension(Player player, bool isSuccsefull) =>
        HandleEvent(ServerEventType.PlayerExitPocketDimension, "PlayerExitPocketDimension", player?.PlayerId.ToString(), player?.DisplayNickname,
            isSuccsefull.ToString());

    [PluginEvent(ServerEventType.PlayerThrowProjectile)]
    public void OnPlayerThrowProjectile(Player player, ThrowableItem item, ThrowableItem.ProjectileSettings projectileSettings, bool fullForce) =>
        HandleEvent(ServerEventType.PlayerThrowProjectile, "PlayerThrowProjectile", player?.PlayerId.ToString(), player?.DisplayNickname,
            item.ItemTypeId.ToString());

    [PluginEvent(ServerEventType.PlayerInteractDoor)]
    public void OnPlayerInteractDoor(Player player, DoorVariant door, bool canOpen) =>
        HandleEvent(ServerEventType.PlayerInteractDoor, "PlayerInteractDoor", player?.PlayerId.ToString(), player?.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerPreCoinFlip)]
    public void OnPlayerPreCoinFlip(Player player) =>
        HandleEvent(ServerEventType.PlayerPreCoinFlip, "PlayerPreCoinFlip", player?.PlayerId.ToString(), player?.DisplayNickname);

    [PluginEvent(ServerEventType.PlayerCoinFlip)]
    public void OnPlayerCoinFlip(Player player, bool isTails) =>
        HandleEvent(ServerEventType.PlayerCoinFlip, "PlayerCoinFlip", player?.PlayerId.ToString(), player?.DisplayNickname, isTails.ToString());

    [PluginEvent(ServerEventType.PlayerInteractGenerator)]
    public void OnPlayerInteractGenerator(Player player, Scp079Generator gen, Scp079Generator.GeneratorColliderId colliderId) =>
        HandleEvent(ServerEventType.PlayerInteractGenerator, "PlayerInteractGenerator", player?.PlayerId.ToString(), player?.DisplayNickname);
    #endregion

    #region Decontamination
    [PluginEvent(ServerEventType.LczDecontaminationStart)]
    void OnLczDecontaminationStarts() => HandleEvent(ServerEventType.LczDecontaminationStart, "LczDecontaminationStart");

    [PluginEvent(ServerEventType.LczDecontaminationAnnouncement)]
    void OnAnnounceLczDecontamination(int id) => HandleEvent(ServerEventType.LczDecontaminationAnnouncement, "LczDecontaminationAnnouncement", id.ToString());
    #endregion

    #region Map Generation
    [PluginEvent(ServerEventType.MapGenerated)]
    void OnMapGenerated() => HandleEvent(ServerEventType.MapGenerated, "MapGenerated");
    #endregion

    #region Items
    [PluginEvent(ServerEventType.GrenadeExploded)]
    void OnGrenadeExploded(Footprint owner, Vector3 position, ItemPickupBase item) => HandleEvent(ServerEventType.GrenadeExploded, "GrenadeExploded",
        item?.Info.ItemId.ToString());

    [PluginEvent(ServerEventType.ItemSpawned)]
    void OnItemSpawned(ItemType item, Vector3 position) => HandleEvent(ServerEventType.ItemSpawned, "ItemSpawned", item.ToString());
    #endregion

    #region Environment
    [PluginEvent(ServerEventType.GeneratorActivated)]
    void OnGeneratorActivated(Scp079Generator gen) => HandleEvent(ServerEventType.GeneratorActivated, "GeneratorActivated");

    [PluginEvent(ServerEventType.PlaceBlood)]
    void OnPlaceBlood(Player player, Vector3 position) => HandleEvent(ServerEventType.PlaceBlood, "PlaceBlood", player?.PlayerId.ToString(),
        player?.DisplayNickname);

    [PluginEvent(ServerEventType.PlaceBulletHole)]
    void OnPlaceBulletHole(Vector3 position) => HandleEvent(ServerEventType.PlaceBulletHole, "PlaceBulletHole");
    #endregion

    #region Round Events
    [PluginEvent(ServerEventType.RoundEnd)]
    void OnRoundEnded(RoundSummary.LeadingTeam leadingTeam) => HandleEvent(ServerEventType.RoundEnd, "RoundEnd", leadingTeam.ToString());

    [PluginEvent(ServerEventType.RoundRestart)]
    void OnRoundRestart() => HandleEvent(ServerEventType.RoundRestart, "RoundRestart");

    [PluginEvent(ServerEventType.RoundStart)]
    void OnRoundStart() => HandleEvent(ServerEventType.RoundStart, "RoundStart");

    [PluginEvent(ServerEventType.WaitingForPlayers)]
    void WaitingForPlayers() => HandleEvent(ServerEventType.WaitingForPlayers, "WaitingForPlayers");

    [PluginEvent(ServerEventType.TeamRespawnSelected)]
    public void OnTeamSelected(SpawnableTeamType team) => HandleEvent(ServerEventType.TeamRespawnSelected, "TeamRespawnSelected", team.ToString());

    [PluginEvent(ServerEventType.TeamRespawn)]
    public void OnTeamRespawn(SpawnableTeamType team, List<Player> players, int amount) => HandleEvent(ServerEventType.TeamRespawn, "TeamRespawn",
        team.ToString(), amount.ToString());

    [PluginEvent(ServerEventType.RoundEndConditionsCheck)]
    public void OnRoundEndConditionsCheck(bool baseGameConditionsSatisfied) => HandleEvent(ServerEventType.RoundEndConditionsCheck,
        "RoundEndConditionsCheck");
    #endregion

    #region Warhead
    [PluginEvent(ServerEventType.WarheadStart)]
    public void OnWarheadStart(bool isAutomatic, Player player, bool isResumed) => HandleEvent(ServerEventType.WarheadStart, "WarheadStart",
        player?.PlayerId.ToString(), player?.DisplayNickname);

    [PluginEvent(ServerEventType.WarheadStop)]
    public void OnWarheadStop(Player player) => HandleEvent(ServerEventType.WarheadStop, "WarheadStop", player?.PlayerId.ToString(),
        player?.DisplayNickname);

    [PluginEvent(ServerEventType.WarheadDetonation)]
    public void OnWarheadDetonation() => HandleEvent(ServerEventType.WarheadDetonation, "WarheadDetonation");
    #endregion

    #region Commands
    [PluginEvent(ServerEventType.PlayerGameConsoleCommand)]
    public void OnPlayerGameconsoleCommand(Player player, string command, string[] arguments) =>
        HandleEvent(ServerEventType.PlayerGameConsoleCommand, "PlayerGameConsoleCommand", player?.PlayerId.ToString(), player?.DisplayNickname,
            command);

    [PluginEvent(ServerEventType.RemoteAdminCommand)]
    public void OnRemoteadminCommand(ICommandSender sender, string command, string[] arguments) => HandleEvent(ServerEventType.RemoteAdminCommand,
        "RemoteAdminCommand", command);

    [PluginEvent(ServerEventType.ConsoleCommand)]
    public void OnConsoleCommand(ICommandSender sender, string command, string[] arguments) => HandleEvent(ServerEventType.ConsoleCommand,
        "ConsoleCommand", command);

    [PluginEvent(ServerEventType.RemoteAdminCommandExecuted)]
    public void OnRemoteadminCommandExecuted(ICommandSender sender, string command, string[] arguments, bool result, string response) =>
        HandleEvent(ServerEventType.RemoteAdminCommandExecuted, "RemoteAdminCommandExecuted", command);

    [PluginEvent(ServerEventType.PlayerGameConsoleCommandExecuted)]
    public void OnPlayerGameconsoleCommandExecuted(Player player, string command, string[] arguments, bool result, string response) =>
        HandleEvent(ServerEventType.PlayerGameConsoleCommandExecuted, "PlayerGameConsoleCommandExecuted", command);

    [PluginEvent(ServerEventType.ConsoleCommandExecuted)]
    public void OnConsoleCommandExecuted(ICommandSender sender, string command, string[] arguments, bool result, string response) =>
        HandleEvent(ServerEventType.ConsoleCommandExecuted, "ConsoleCommandExecuted", command);
    #endregion

    #region SCPs
    [PluginEvent(ServerEventType.CassieAnnouncesScpTermination)]
    void OnCassieAnnouncScpTermination(Player scp, DamageHandlerBase damageHandler, string announcement) =>
        HandleEvent(ServerEventType.CassieAnnouncesScpTermination, "CassieAnnouncesScpTermination", scp?.RoleName, announcement);

    [PluginEvent(ServerEventType.Scp914Activate)]
    public void OnScp914Activate(Player player, Scp914KnobSetting knobSetting) => HandleEvent(ServerEventType.Scp914Activate, "Scp914Activate",
        player?.PlayerId.ToString(), player?.DisplayNickname, knobSetting.ToString());

    [PluginEvent(ServerEventType.Scp914KnobChange)]
    public void OnScp914KnobChange(Player player, Scp914KnobSetting knobSetting, Scp914KnobSetting previousKnobSetting) =>
        HandleEvent(ServerEventType.Scp914KnobChange, "Scp914KnobChange", player?.PlayerId.ToString(), player?.DisplayNickname, knobSetting.ToString());

    [PluginEvent(ServerEventType.Scp914UpgradeInventory)]
    public void OnScp914UpgradeInventory(Player player, ItemBase item, Scp914KnobSetting knobSetting) =>
        HandleEvent(ServerEventType.Scp914UpgradeInventory, "Scp914UpgradeInventory", player?.PlayerId.ToString(), player?.DisplayNickname,
            item?.ItemTypeId.ToString(), knobSetting.ToString());

    [PluginEvent(ServerEventType.Scp914UpgradePickup)]
    public void OnScp914UpgradePickup(ItemPickupBase item, Vector3 outputPosition, Scp914KnobSetting knobSetting) =>
        HandleEvent(ServerEventType.Scp914UpgradePickup, "Scp914UpgradePickup", item?.Info.ItemId.ToString(), knobSetting.ToString());

    [PluginEvent(ServerEventType.Scp106Stalking)]
    public void OnScp106Stalking(Player player, bool activate) => HandleEvent(ServerEventType.Scp106Stalking, "Scp106Stalking",
        player?.PlayerId.ToString(), player?.DisplayNickname);

    [PluginEvent(ServerEventType.Scp173PlaySound)]
    public void OnScp173PlaySound(Player player, Scp173AudioPlayer.Scp173SoundId soundId) => HandleEvent(ServerEventType.Scp173PlaySound,
        "Scp173PlaySound", player?.PlayerId.ToString(), player?.DisplayNickname);

    [PluginEvent(ServerEventType.Scp173BreakneckSpeeds)]
    public void OnScp173BreakneckSpeeds(Player player, bool activate) => HandleEvent(ServerEventType.Scp173BreakneckSpeeds, "Scp173BreakneckSpeeds",
        player?.PlayerId.ToString(), player?.DisplayNickname);

    [PluginEvent(ServerEventType.Scp173NewObserver)]
    public void OnScp173NewObserver(Player player, Player target) => HandleEvent(ServerEventType.Scp173NewObserver, "Scp173NewObserver",
        player?.PlayerId.ToString(), player?.DisplayNickname, target?.PlayerId.ToString(), target?.DisplayNickname);

    [PluginEvent(ServerEventType.Scp173SnapPlayer)]
    public void OnScp173SnapPlayer(Player player, Player target) => HandleEvent(ServerEventType.Scp173SnapPlayer, "Scp173SnapPlayer",
        player?.PlayerId.ToString(), player?.DisplayNickname, target?.PlayerId.ToString(), target?.DisplayNickname);

    [PluginEvent(ServerEventType.Scp173CreateTantrum)]
    public void OnScp173CreateTantrum(Player player) => HandleEvent(ServerEventType.Scp173CreateTantrum, "Scp173CreateTantrum",
        player?.PlayerId.ToString(), player?.DisplayNickname);

    [PluginEvent(ServerEventType.Scp939CreateAmnesticCloud)]
    public void OnScp939CreateAmnesticCloud(Player player) => HandleEvent(ServerEventType.Scp939CreateAmnesticCloud, "Scp939CreateAmnesticCloud",
        player?.PlayerId.ToString(), player?.DisplayNickname);

    [PluginEvent(ServerEventType.Scp939Lunge)]
    public void OnScp939Lunge(Player player, Scp939LungeState state) => HandleEvent(ServerEventType.Scp939Lunge, "Scp939Lunge",
        player?.PlayerId.ToString(), player?.DisplayNickname);

    [PluginEvent(ServerEventType.Scp939Attack)]
    public void OnScp939Attack(Player player, IDestructible target) => HandleEvent(ServerEventType.Scp939Attack, "Scp939Attack",
        player?.PlayerId.ToString(), player?.DisplayNickname);

    [PluginEvent(ServerEventType.Scp079GainExperience)]
    public void OnScp079GainExperience(Player player, int amount, Scp079HudTranslation reason) => HandleEvent(ServerEventType.Scp079GainExperience,
        "Scp079GainExperience", player?.PlayerId.ToString(), player?.DisplayNickname, amount.ToString(), reason.ToString());

    [PluginEvent(ServerEventType.Scp079LevelUpTier)]
    public void OnScp079LevelUpTier(Player player, int tier) => HandleEvent(ServerEventType.Scp079LevelUpTier, "Scp079LevelUpTier",
        player?.PlayerId.ToString(), player?.DisplayNickname, tier.ToString());

    [PluginEvent(ServerEventType.Scp079UseTesla)]
    public void OnScp079UseTesla(Player player, TeslaGate tesla) => HandleEvent(ServerEventType.Scp079UseTesla, "Scp079UseTesla",
        player?.PlayerId.ToString(), player?.DisplayNickname);

    [PluginEvent(ServerEventType.Scp079LockdownRoom)]
    public void OnScp079LockdownRoom(Player player, RoomIdentifier room) => HandleEvent(ServerEventType.Scp079LockdownRoom, "Scp079LockdownRoom",
        player?.PlayerId.ToString(), player?.DisplayNickname, room?.Name.ToString());

    [PluginEvent(ServerEventType.Scp079CancelRoomLockdown)]
    public void OnScp079CancelRoomLockdown(Player player, RoomIdentifier room) => HandleEvent(ServerEventType.Scp079CancelRoomLockdown,
        "Scp079CancelRoomLockdown", player?.PlayerId.ToString(), player?.DisplayNickname, room?.Name.ToString());

    [PluginEvent(ServerEventType.Scp079LockDoor)]
    public void OnScp079LockDoor(Player player, DoorVariant door) => HandleEvent(ServerEventType.Scp079LockDoor, "Scp079LockDoor",
        player?.PlayerId.ToString(), player?.DisplayNickname);

    [PluginEvent(ServerEventType.Scp079UnlockDoor)]
    public void OnScp079UnLockDoor(Player player, DoorVariant door) => HandleEvent(ServerEventType.Scp079UnlockDoor, "Scp079UnlockDoor",
        player?.PlayerId.ToString(), player?.DisplayNickname);

    [PluginEvent(ServerEventType.Scp079BlackoutZone)]
    public void OnScp079BlackoutZone(Player player, FacilityZone zone) => HandleEvent(ServerEventType.Scp079BlackoutZone, "Scp079BlackoutZone",
        player?.PlayerId.ToString(), player?.DisplayNickname, zone.ToString());

    [PluginEvent(ServerEventType.Scp079BlackoutRoom)]
    public void OnScp079BlackoutRoom(Player player, RoomIdentifier room) => HandleEvent(ServerEventType.Scp079BlackoutRoom, "Scp079BlackoutRoom",
        player?.PlayerId.ToString(), player?.DisplayNickname, room?.Name.ToString());

    [PluginEvent(ServerEventType.Scp049ResurrectBody)]
    public void OnScp049ResurrectBody(Player player, Player target, BasicRagdoll body) => HandleEvent(ServerEventType.Scp049ResurrectBody,
        "Scp049ResurrectBody", player?.PlayerId.ToString(), player?.DisplayNickname, target?.PlayerId.ToString(), target?.DisplayNickname);

    [PluginEvent(ServerEventType.Scp079CameraChanged)]
    public void OnScp079ChangedCamera(Player player, Scp079Camera camera) => HandleEvent(ServerEventType.Scp079CameraChanged, "Scp079CameraChanged",
        player?.PlayerId.ToString(), player?.DisplayNickname, camera?.Label);

    [PluginEvent(ServerEventType.Scp049StartResurrectingBody)]
    public void OnScp049StartResurrectingBody(Player player, Player target, BasicRagdoll body, bool canResurrect) =>
        HandleEvent(ServerEventType.Scp049StartResurrectingBody, "Scp049StartResurrectingBody", player?.PlayerId.ToString(), player?.DisplayNickname,
            target?.PlayerId.ToString(), target?.DisplayNickname);

    [PluginEvent(ServerEventType.Scp106TeleportPlayer)]
    public void OnScp106TeleportPlayer(Player player, Player target) => HandleEvent(ServerEventType.Scp106TeleportPlayer, "Scp106TeleportPlayer",
        player?.PlayerId.ToString(), player?.DisplayNickname, target?.PlayerId.ToString(), target?.DisplayNickname);

    [PluginEvent(ServerEventType.Scp096AddingTarget)]
    public void OnScp096AddTarget(Player player, Player target, bool isForLooking) => HandleEvent(ServerEventType.Scp096AddingTarget, "Scp096AddingTarget",
        player?.PlayerId.ToString(), player?.DisplayNickname, target?.PlayerId.ToString(), target?.DisplayNickname);

    [PluginEvent(ServerEventType.Scp096Enraging)]
    public void OnScp096Enrage(Player player, float initialDuration) => HandleEvent(ServerEventType.Scp096Enraging, "Scp096Enraging",
        player?.PlayerId.ToString(), player?.DisplayNickname);

    [PluginEvent(ServerEventType.Scp096ChangeState)]
    public void OnScp096CalmDown(Player player, Scp096RageState rageState) => HandleEvent(ServerEventType.Scp096ChangeState, "Scp096ChangeState",
        player?.PlayerId.ToString(), player?.DisplayNickname, rageState.ToString());

    [PluginEvent(ServerEventType.Scp096Charging)]
    public void OnScp096Charge(Player player) => HandleEvent(ServerEventType.Scp096Charging, "Scp096Charging", player?.PlayerId.ToString(),
        player?.DisplayNickname);

    [PluginEvent(ServerEventType.Scp096PryingGate)]
    public void OnScp096PryGate(Player player, PryableDoor gate) => HandleEvent(ServerEventType.Scp096PryingGate, "Scp096PryingGate",
        player?.PlayerId.ToString(), player?.DisplayNickname);

    [PluginEvent(ServerEventType.Scp096TryNotCry)]
    public void OnScp096TryingNotCry(Player player) => HandleEvent(ServerEventType.Scp096TryNotCry, "Scp096TryNotCry", player?.PlayerId.ToString(),
        player?.DisplayNickname);

    [PluginEvent(ServerEventType.Scp096StartCrying)]
    public void OnScp096StartCrying(Player player) => HandleEvent(ServerEventType.Scp096StartCrying, "Scp096StartCrying", player?.PlayerId.ToString(),
        player?.DisplayNickname);

    [PluginEvent(ServerEventType.Scp914PickupUpgraded)]
    public void OnScp914PickupUpgraded(ItemPickupBase item, Vector3 newPosition, Scp914KnobSetting knobSetting) =>
        HandleEvent(ServerEventType.Scp914PickupUpgraded, "Scp914PickupUpgraded", item?.Info.ItemId.ToString(), knobSetting.ToString());

    [PluginEvent(ServerEventType.Scp914InventoryItemUpgraded)]
    public void OnScp914InventoryItemUpgraded(Player player, ItemBase item, Scp914KnobSetting knobSetting) =>
        HandleEvent(ServerEventType.Scp914InventoryItemUpgraded, "Scp914InventoryItemUpgraded", player?.PlayerId.ToString(), player?.DisplayNickname,
            item?.ItemTypeId.ToString(), knobSetting.ToString());

    [PluginEvent(ServerEventType.Scp914ProcessPlayer)]
    public void OnScp914ProcessPlayer(Player player, Scp914KnobSetting knobSetting, Vector3 outPosition) =>
        HandleEvent(ServerEventType.Scp914ProcessPlayer, "Scp914ProcessPlayer", player?.PlayerId.ToString(), player?.DisplayNickname, knobSetting.ToString());
    #endregion

    #region Bans
    [PluginEvent(ServerEventType.BanIssued)]
    public void OnBanIssued(BanDetails banDetails, BanHandler.BanType banType) => HandleEvent(ServerEventType.BanIssued, "BanIssued", banType.ToString());

    [PluginEvent(ServerEventType.BanRevoked)]
    public void OnBanRevoked(string id, BanHandler.BanType banType) => HandleEvent(ServerEventType.BanRevoked, "BanRevoked", banType.ToString());

    [PluginEvent(ServerEventType.BanUpdated)]
    public void OnBanUpdated(BanDetails banDetails, BanHandler.BanType banType) => HandleEvent(ServerEventType.BanUpdated, "BanUpdated", banType.ToString());
    #endregion
}
