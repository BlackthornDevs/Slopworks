using UnityEngine;

/// <summary>
/// Runtime-tunable belt parameters. Drop on a GameObject in the scene
/// and adjust values in the inspector during play. Changes push to
/// BeltRouteBuilder statics immediately via OnValidate.
/// </summary>
public class BeltSettings : MonoBehaviour
{
    [Header("Constraints")]
    [Tooltip("Minimum straight segment at connectors and between turns")]
    [SerializeField] private float _minStraight = 0.5f;

    [Tooltip("Maximum total belt length")]
    [SerializeField] private float _maxLength = 56f;

    [Header("Geometry")]
    [Tooltip("Fixed turn radius for Straight mode arcs")]
    [SerializeField] private float _turnRadius = 1.0f;

    [Tooltip("Maximum ramp angle in degrees")]
    [SerializeField] private float _maxRampAngle = 30f;

    [Tooltip("Minimum turn angle between start and end directions")]
    [SerializeField] private float _minTurnAngle = 30f;

    private void Awake()
    {
        PushValues();
    }

    private void OnValidate()
    {
        PushValues();
    }

    private void PushValues()
    {
        BeltRouteBuilder.MinStraight = _minStraight;
        BeltRouteBuilder.MaxLength = _maxLength;
        BeltRouteBuilder.TurnRadius = _turnRadius;
        BeltRouteBuilder.MaxRampAngle = _maxRampAngle;
        BeltRouteBuilder.MinTurnAngle = _minTurnAngle;
    }
}
