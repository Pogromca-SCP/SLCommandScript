namespace SLCommandScript.FileScriptsLoader.Events;

/// <summary>
/// Represents event types.
/// </summary>
public enum EventType : byte
{
    PlayerJoined,
    PlayerLeft,
    PlayerBanned,
    PlayerKicked,
    PlayerCheaterReport,
    PlayerChangeRole,
    PlayerPreauth,
    PlayerGetGroup,
    PlayerReport,
    RagdollSpawn,
    PlayerMuted,
    PlayerUnmuted,


    PlayerDeath,
    PlayerChangeSpectator,
    PlayerEscape,
    PlayerDamage,
    PlayerUpdateEffect,
    PlayerSpawn,
    PlayerEnterPocketDimension,
    PlayerExitPocketDimension,


    PlayerActivateGenerator,
    PlayerUsedIntercom,
    PlayerCloseGenerator,
    PlayerDamagedShootingTarget,
    PlayerDamagedWindow,
    PlayerDeactivatedGenerator,
    PlayerOpenGenerator,
    PlayerInteractShootingTarget,
    PlayerInteractLocker,
    PlayerInteractElevator,
    PlayerUnlockGenerator,
    PlayerInteractDoor,
    PlayerInteractGenerator,


    PlayerAimWeapon,
    PlayerDryfireWeapon,
    PlayerReloadWeapon,
    PlayerShotWeapon,
    PlayerUnloadWeapon,


    PlayerCancelUsingItem,
    PlayerChangeItem,
    PlayerDropAmmo,
    PlayerDropItem,
    PlayerPickupAmmo,
    PlayerPickupArmor,
    PlayerPickupScp330,
    PlayerSearchPickup,
    PlayerThrowItem,
    PlayerToggleFlashlight,
    PlayerUsedItem,
    PlayerInteractScp330,
    PlayerThrowProjectile,
    PlayerCoinFlip,


    PlayerChangeRadioRange,
    PlayerRadioToggle,
    PlayerUsingRadio,


    LczDecontaminationStart,
    LczDecontaminationAnnouncement,


    MapGenerated,


    ItemSpawned,


    GeneratorActivated,
    PlaceBlood,
    PlaceBulletHole,


    RoundEnd,
    RoundRestart,
    RoundStart,
    WaitingForPlayers,
    TeamRespawnSelected,
    TeamRespawn,


    WarheadStart,
    WarheadStop,
    WarheadDetonation,


    CommandExecuted,


    CassieQueueScpTermination,


    Scp914Activate,
    Scp914KnobChange,


    BanIssued,
    BanRevoked,
    BanUpdated,
}
