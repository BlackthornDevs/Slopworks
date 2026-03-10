using UnityEngine;

public enum BeltValidationError
{
    None,
    TooShort,
    TooLong,
    TooSteep,
    TurnTooSharp
}

public struct BeltValidationResult
{
    public bool IsValid;
    public BeltValidationError Error;

    public static BeltValidationResult Valid()
    {
        return new BeltValidationResult { IsValid = true, Error = BeltValidationError.None };
    }

    public static BeltValidationResult Invalid(BeltValidationError error)
    {
        return new BeltValidationResult { IsValid = false, Error = error };
    }
}

/// <summary>
/// Validates belt placement parameters before sending to server.
/// Pure math -- no MonoBehaviour, no side effects.
/// </summary>
public static class BeltPlacementValidator
{
    public const float MinLength = 0.5f;
    public const float MaxLength = 56f;
    public const float MaxSlopeAngle = 45f;
    public const float MinTurnAngle = 30f; // minimum angle between startDir and endDir

    public static BeltValidationResult Validate(
        Vector3 startPos, Vector3 startDir,
        Vector3 endPos, Vector3 endDir)
    {
        // Zero endDir signals an invalid direction (e.g. straight backward)
        if (endDir.sqrMagnitude < 0.001f)
            return BeltValidationResult.Invalid(BeltValidationError.TurnTooSharp);

        float distance = Vector3.Distance(startPos, endPos);

        if (distance < MinLength)
            return BeltValidationResult.Invalid(BeltValidationError.TooShort);

        if (distance > MaxLength)
            return BeltValidationResult.Invalid(BeltValidationError.TooLong);

        float horizontalDist = new Vector2(endPos.x - startPos.x, endPos.z - startPos.z).magnitude;
        float verticalDist = Mathf.Abs(endPos.y - startPos.y);

        if (horizontalDist > 0.001f)
        {
            float slopeAngle = Mathf.Atan2(verticalDist, horizontalDist) * Mathf.Rad2Deg;
            if (slopeAngle > MaxSlopeAngle)
                return BeltValidationResult.Invalid(BeltValidationError.TooSteep);
        }
        else if (verticalDist > 0.001f)
        {
            return BeltValidationResult.Invalid(BeltValidationError.TooSteep);
        }

        // Reject belts where start and end tangents diverge too sharply.
        // Angle between tangents below MinTurnAngle would create a jagged kink.
        var startFlat = new Vector3(startDir.x, 0, startDir.z).normalized;
        var endFlat = new Vector3(endDir.x, 0, endDir.z).normalized;
        if (startFlat.sqrMagnitude > 0.001f && endFlat.sqrMagnitude > 0.001f)
        {
            float dot = Vector3.Dot(startFlat, endFlat);
            float angle = Mathf.Acos(Mathf.Clamp(dot, -1f, 1f)) * Mathf.Rad2Deg;
            if (angle > (180f - MinTurnAngle))
                return BeltValidationResult.Invalid(BeltValidationError.TurnTooSharp);
        }

        return BeltValidationResult.Valid();
    }
}
