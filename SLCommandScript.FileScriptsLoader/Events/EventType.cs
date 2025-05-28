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
    PlayerToggleNoclip,
    PlayerChangeNickname,
    PlayerValidateVisibility,


    PlayerDeath,
    PlayerChangeSpectator,
    PlayerEscape,
    PlayerDamage,
    PlayerUpdateEffect,
    PlayerSpawn,
    PlayerEnterPocketDimension,
    PlayerExitPocketDimension,
    PlayerHandcuff,
    PlayerRemoveHandcuffs,


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
    PlayerInteractToy,
    PlayerSearchToy,
    PlayerSearchToyAbort,


    PlayerAimWeapon,
    PlayerDryfireWeapon,
    PlayerReloadWeapon,
    PlayerShotWeapon,
    PlayerUnloadWeapon,
    PlayerToggleWeaponFlashlight,


    PlayerCancelUsingItem,
    PlayerChangeItem,
    PlayerDropAmmo,
    PlayerDropItem,
    PlayerPickupItem,
    PlayerPickupAmmo,
    PlayerPickupArmor,
    PlayerPickupScp330,
    PlayerSearchPickup,
    PlayerSearchAmmo,
    PlayerSearchArmor,
    PlayerThrowItem,
    PlayerToggleFlashlight,
    PlayerUsedItem,
    PlayerInteractScp330,
    PlayerThrowProjectile,
    PlayerCoinFlip,
    PlayerReceiveLoadout,


    PlayerChangeRadioRange,
    PlayerRadioToggle,
    PlayerUsingRadio,


    LczDecontaminationStart,
    LczDecontaminationAnnouncement,


    MapGenerated,


    ItemSpawned,
    PickupCreated,
    PickupDestroyed,


    GeneratorActivated,
    PlaceBlood,
    PlaceBulletHole,
    PlayerEnterHazard,
    PlayerLeaveHazard,
    PlayerIdleTesla,
    PlayerTriggerTesla,
    PlayerUnlockWarhead,
    CassieAnnouncement,
    ExplosionSpawn,
    ProjectileExplode,
    AdminChat,


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
    Scp914InventoryItemUpgraded,
    Scp914PickupUpgraded,
    Scp914ProcessPlayer,


    Scp106Stalking,
    Scp106Submerging,
    Scp106VigorChange,
    Scp106TeleportPlayer,
    Scp106UsedHunterAtlas,


    Scp173PlaySound,
    Scp173BreakneckSpeeds,
    Scp173NewObserver,
    Scp173RemovedObserver,
    Scp173CreateTantrum,


    Scp939CreateAmnesticCloud,
    Scp939Lunge,
    Scp939Attack,


    Scp079GainExperience,
    Scp079LevelUpTier,
    Scp079UseTesla,
    Scp079LockdownRoom,
    Scp079CancelRoomLockdown,
    Scp079LockDoor,
    Scp079UnlockDoor,
    Scp079BlackoutZone,
    Scp079BlackoutRoom,
    Scp079ChangeCamera,
    Scp079Recontained,


    Scp049ResurrectBody,
    Scp049UseDoctorsCall,
    Scp049UseSense,


    Scp049_2ConsumeCorpse,
    Scp049_2StartConsumingCorpse,


    Scp096AddTarget,
    Scp096Enrage,
    Scp096ChangeState,
    Scp096Charge,
    Scp096PryGate,
    Scp096TryNotCry,
    Scp096StartCrying,


    BanIssued,
    BanRevoked,
    BanUpdated,
}
