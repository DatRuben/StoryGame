using UnityEngine;

public enum BaseRace
{
    Animali,
    Canispar,
    Drakken,
    Eastern,
    Griffin,
    Human,
    WesternDragon
}

public enum RaceSize
{
    Size1,
    Size2,
    Size3,
    Size1Feral,
    Size2Feral,
    Size3Feral
}

public enum BodyType
{
    Humanoid,
    Quadruped,
    StanceSwitching
}

public enum CapsuleDirection
{
    XAxis = 0,
    YAxis = 1,
    ZAxis = 2
}

[CreateAssetMenu(menuName = "Game/Race Profile")]
public class RaceProfile : ScriptableObject
{
    [Header("Identity")]
    public string displayName;
    public BaseRace baseRace;
    public string subraceName;
    public RaceSize size;
    public BodyType bodyType;

    [TextArea]
    public string description;

    [Header("Body / Collider")]
    public float capsuleRadius = 0.5f;
    public float capsuleHeight = 2f;
    public Vector3 capsuleCenter = Vector3.zero;
    public CapsuleDirection capsuleDirection = CapsuleDirection.YAxis;

    [Header("Player Setup")]
    public Vector3 groundCheckLocalPosition = new Vector3(0f, -.9f, 0f);
    public Vector3 cameraPivotLocalPosition = new Vector3(0f, .9f, 0f);

    [Header("Movement")]
    public float walkSpeed = 10f;
    public float sprintSpeed = 15f;
    public float groundAcceleration = 8f;
    public float airAcceleration = 2f;
    public float deceleration = 16f;
    public float jumpForce = 7f;

    [Header("Size / Stamina Modifiers")]
    public float staminaMultiplier = 1f;
    public float dodgeDistanceMultiplier = 1f;
    public float dodgeSpeedMultiplier = 1f;

    [Header("Equipment Rules")]
    public bool usesSize1Armor = false;
    public bool usesSize2Armor = true;
    public bool usesSize3Armor = false;
    public bool usesSize1FeralArmor = false;
    public bool usesSize2FeralArmor = false;
    public bool usesSize3FeralArmor = false;

    public bool usesSize1Weapons = true;
    public bool usesSize2Weapons = true;
    public bool usesSize3Weapons = false;
    public bool usesSize1FeralWeapons = false;
    public bool usesSize2FeralWeapons = false;
    public bool usesSize3FeralWeapons = false;

    [Header("Magic Rules")]
    public bool naturalCatalyst = false;

    [Header("Racial Skill Tree")]
    public string racialSkillTreeName;

    [TextArea]
    public string racialSkillTreeTheme;

    [Header("Lineage")]
    public bool usesLineageProfiles = true;
    public LineageProfile[] allowedLineage;
    public int maxLineageCount = 1;

    [Header("Shapeshifting")]
    public bool canShapeshift = false;
    public ShapeshiftProfile[] shapeshiftForms;

    [Header("Aether Inventory")]
    public bool hasAetherInventory = false;
    //public float naturalInventoryWeightCapacity = 0f;
    //public int naturalInventorySlotCapacity = 0;

    [Header("Mount / Riding Rules")]
    public bool canBeRidden = false;
    public bool canRideMounts = true;

    [Header("Holding Rules")]
    public bool canHoldItemInMouth = false;
    public bool canUseMouthWeapons = false;

    [Header("Special Body Parts")]
    public bool hasWings = false;

    [Header("Special Mechanics")]
    public bool canFly = false;

    [TextArea]
    public string notes;
}