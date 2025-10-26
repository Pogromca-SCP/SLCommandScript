namespace SLCommandScript.FileScriptsLoader.Events;

/// <summary>
/// Represents event types.
/// </summary>
public enum EventType : byte
{
    PlayerJoin,
    PlayerLeave,
    PlayerBan,
    PlayerKick,
    PlayerCheaterReport,
    PlayerChangeRole,
    PlayerPreauth,
    PlayerGetGroup,
    PlayerReport,
    RagdollSpawn,
    PlayerMute,
    PlayerUnmute,
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
    PlayerUseIntercom,
    PlayerCloseGenerator,
    PlayerDamageShootingTarget,
    PlayerDamageWindow,
    PlayerDeactivateGenerator,
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
    PlayerShootWeapon,
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
    PlayerUseItem,
    PlayerInteractScp330,
    PlayerThrowProjectile,
    PlayerCoinFlip,
    PlayerReceiveLoadout,


    PlayerChangeRadioRange,
    PlayerToggleRadio,
    PlayerUseRadio,


    LczDecontaminationStart,
    LczDecontaminationAnnouncement,


    MapGenerate,


    ItemSpawn,
    CreatePickup,
    DestroyPickup,


    GeneratorActivate,
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
    TeamRespawnSelection,
    TeamRespawn,


    WarheadStart,
    WarheadStop,
    WarheadDetonation,


    ExecuteCommand,


    CassieQueueScpTermination,


    Scp914Activate,
    Scp914KnobChange,
    Scp914InventoryItemUpgrade,
    Scp914PickupUpgrade,
    Scp914ProcessPlayer,


    Scp106Stalk,
    Scp106Submerge,
    Scp106VigorChange,
    Scp106TeleportPlayer,
    Scp106UseHunterAtlas,


    Scp173PlaySound,
    Scp173BreakneckSpeeds,
    Scp173NewObserver,
    Scp173RemoveObserver,
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
    Scp079Recontainment,


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


    RoundEndingConditionsCheck,
    Scp079Ping,


    ObjectiveActivatedGeneratorComplete,
    ObjectiveComplete,
    ObjectiveDamagedScpComplete,
    ObjectiveEscapedComplete,
    ObjectiveKilledEnemyComplete,
    ObjectivePickedScpItemComplete,
    PlayerChangeAttachments,
    PlayerChangeBadgeVisibility,
    PlayerDetectedByScp1344,
    PlayerInspectKeycard,
    PlayerInteractWarheadLever,
    PlayerJump,
    PlayerMovementStateChange,
    PlayerProcessJailbirdMessage,
    PlayerRaPlayerListAddPlayer,
    PlayerReceiveAchievement,
    PlayerRequestCustomRaInfo,
    PlayerRequestRaPlayerInfo,
    PlayerRequestRaPlayerList,
    PlayerRequestRaPlayersInfo,
    PlayerRoomChange,
    PlayerSendAttachmentsPrefs,
    PlayerSpinRevolver,
    PlayerToggleDisruptorFiringMode,
    PlayerZoneChange,
    Scp127GainExperience,
    Scp127LevelUp,
    Scp127Talk,
    Scp3114Dance,
    Scp3114Disguise,
    Scp3114Reveal,
    Scp3114StrangleAbort,
    Scp3114StrangleStart,
    ServerElevatorSequenceChange,


    Scp173Teleport,


    BlastDoorChange,
    RoomLightChange,
    RoomColorChange,
    DoorLockChange,
    CheckpointDoorSequenceChange,
    DoorDamage,
    DoorRepair,
    Scp939Focus,
    Scp939MimickEnvironment,
    Scp049Attack,
    Scp049SenseLooseTarget,
    Scp049SenseKillTarget,
    Scp173Snap,
    PlayerCheckHitmarker,
    PlayerSendHitmarker,
    HumeShieldBreak,
    FactionInfluenceChange,
    MilestoneAchievement,
    DeadmanSequenceActivation,
    ServerShutdown,
}
