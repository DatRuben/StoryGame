using UnityEngine;

public enum BaseRace
{
    Animali,
    Canispar,
    Drakken,
    Eastern,
    Griffin,
    Human,
    SoulChip,
    WesternDragon
}

public enum RaceSize
{
    Size1,
    Size2,
    TallerSize2,
    Size3,
    Size1Feral,
    Size2Feral,
    Size3Feral,
    Dragon,
    BigDragon
}

public static class RaceSizeBodyScale
{
    public static float GetDefault(
        RaceSize raceSize)
    {
        switch (raceSize)
        {
            case RaceSize.Size1:
                return 0.7f;

            case RaceSize.Size2:
                return 1f;

            case RaceSize.TallerSize2:
                return 1.25f;

            case RaceSize.Size3:
                return 1.3f;

            case RaceSize.Size1Feral:
                return 0.5f;

            case RaceSize.Size2Feral:
                return 1f;

            case RaceSize.Size3Feral:
                return 1.5f;

            case RaceSize.Dragon:
                return 1.75f;

            case RaceSize.BigDragon:
                return 2f;

            default:
                return 1f;
        }
    }

    public static Vector2 GetRange(
        RaceSize raceSize)
    {
        switch (raceSize)
        {
            case RaceSize.Size1:
                return new Vector2(
                    0.6f,
                    0.8f
                );

            case RaceSize.Size2:
                return new Vector2(
                    0.9f,
                    1.1f
                );

            case RaceSize.TallerSize2:
                return new Vector2(
                    1.15f,
                    1.35f
                );

            case RaceSize.Size3:
                return new Vector2(
                    1.2f,
                    1.4f
                );

            case RaceSize.Size1Feral:
                return new Vector2(
                    0.4f,
                    0.6f
                );

            case RaceSize.Size2Feral:
                return new Vector2(
                    0.9f,
                    1.1f
                );

            case RaceSize.Size3Feral:
                return new Vector2(
                    1.4f,
                    1.6f
                );

            case RaceSize.Dragon:
                return new Vector2(
                    1.65f,
                    1.85f
                );

            case RaceSize.BigDragon:
                return new Vector2(
                    1.9f,
                    2.1f
                );

            default:
                return new Vector2(
                    0.9f,
                    1.1f
                );
        }
    }
}

public enum BodyType
{
    Humanoid,
    Quadruped,
    StanceSwitching
}

public enum MovementBaseType
{
    Size2Humanoid,
    Size2Feral
}