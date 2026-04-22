namespace Loupedeck.GodotMxBridge;

/// <summary>
/// Centralizes Node2D/Node3D transform logic shared by transform dials and dynamic folders.
/// </summary>
internal static class NodeTransformHelper
{
    private const int    BaseSteps       = 100;
    private const double FallbackPosStep = 0.01;
    private const double FallbackRotStep = 0.1;
    private const double FallbackSclStep = 0.01;
    private const double VelocityStart   = 3.5;
    private const double VelocityEnd     = 12;
    private const double VelocityMaxMul  = 50;

    public const double PosStep   = FallbackPosStep;
    public const double RotStep   = FallbackRotStep;
    public const double ScaleStep = FallbackSclStep;

    public const double RotationDegMin = -360.0;
    public const double RotationDegMax =  360.0;

    public static readonly string[] AllKeys = [ActionKeys.TfScale, ActionKeys.TfPosX, ActionKeys.TfPosY, ActionKeys.TfPosZ, ActionKeys.TfRotX, ActionKeys.TfRotY, ActionKeys.TfRotZ];

    private static readonly string[] Keys2D = [ActionKeys.TfScale, ActionKeys.TfPosX, ActionKeys.TfPosY, ActionKeys.TfRotZ];

    public static IReadOnlyList<String> AxisKeysFor(ContextSnapshot snap) =>
        snap.TransformKind == NodeTransformKind.Node2D ? Keys2D : AllKeys;

    public static Boolean AxisApplies(String key, ContextSnapshot snap) =>
        snap.TransformKind != NodeTransformKind.Node2D || (key == ActionKeys.TfScale || key == ActionKeys.TfPosX || key == ActionKeys.TfPosY || key == ActionKeys.TfRotZ);

    public static Boolean IsAxisKey(String key)
    {
        if (String.IsNullOrEmpty(key)) return false;
        foreach (var k in AllKeys)
        {
            if (String.Equals(k, key, StringComparison.Ordinal))
                return true;
        }

        return false;
    }

    public static double GetStep(string key, ContextSnapshot snap)
    {
        if (!snap.HasRangeHints)
            return key switch
            {
                ActionKeys.TfRotX or ActionKeys.TfRotY or ActionKeys.TfRotZ => FallbackRotStep,
                ActionKeys.TfScale               => FallbackSclStep,
                _                     => FallbackPosStep,
            };

        double range = key switch
        {
            ActionKeys.TfPosX    => snap.PositionMax[0] - snap.PositionMin[0],
            ActionKeys.TfPosY    => snap.PositionMax[1] - snap.PositionMin[1],
            ActionKeys.TfPosZ    => snap.PositionMax[2] - snap.PositionMin[2],
            ActionKeys.TfRotX    => snap.RotationMax[0] - snap.RotationMin[0],
            ActionKeys.TfRotY    => snap.RotationMax[1] - snap.RotationMin[1],
            ActionKeys.TfRotZ    => snap.RotationMax[2] - snap.RotationMin[2],
            ActionKeys.TfScale => snap.ScaleMax - snap.ScaleMin,
            _       => 1.0,
        };
        return Math.Max(0.01, range / BaseSteps);
    }

    public static int VelocityTicks(int diff)
    {
        if (diff == 0) return 0;
        int speed = Math.Abs(diff);
        double multiplier = SmoothVelocityMultiplier(speed);
        return (int)Math.Round(Math.Sign(diff) * multiplier);
    }

    public static double VelocityDelta(double baseStep, int diff)
    {
        if (diff == 0) return 0;
        int    speed      = Math.Abs(diff);
        double multiplier = SmoothVelocityMultiplier(speed);
        return Math.Sign(diff) * baseStep * multiplier;
    }

    private static double SmoothVelocityMultiplier(int speed)
    {
        var t = (speed - VelocityStart) / (VelocityEnd - VelocityStart);
        t = Math.Clamp(t, 0.0, 1.0);
        t = t * t * (3.0 - 2.0 * t); // smoothstep
        return 1.0 + (VelocityMaxMul - 1.0) * t;
    }

    public static double GetScalar(string key, ContextSnapshot snap) => key switch
    {
        ActionKeys.TfScale => snap.ScaleUniform,
        ActionKeys.TfPosX    => snap.Position[0],
        ActionKeys.TfPosY    => snap.Position[1],
        ActionKeys.TfPosZ    => snap.Position[2],
        ActionKeys.TfRotX    => snap.RotationDeg[0],
        ActionKeys.TfRotY    => snap.RotationDeg[1],
        ActionKeys.TfRotZ    => snap.RotationDeg[2],
        _       => 0.0,
    };

    public static void ApplyEncoderDelta(string key, int diff, IBridgeTransport bridge, ContextSnapshot snap)
    {
        ApplyDelta(key, diff, bridge, snap);
    }

    public static void ApplyDelta(string key, int ticks, IBridgeTransport bridge, ContextSnapshot snap)
    {
        if (!AxisApplies(key, snap) || ticks == 0) return;
        NodeTransformAdjustmentTracker.ClearPendingResetForKey(key);
        
        var delta = VelocityDelta(GetStep(key, snap), ticks);
        
        switch (key)
        {
            case ActionKeys.TfScale:
                bridge.SendRelativeFloat(EventIds.TfScale, delta);
                break;
            case ActionKeys.TfPosX:
                bridge.SendRelativeFloat(EventIds.TfPosX, delta);
                break;
            case ActionKeys.TfPosY:
                bridge.SendRelativeFloat(EventIds.TfPosY, delta);
                break;
            case ActionKeys.TfPosZ:
                bridge.SendRelativeFloat(EventIds.TfPosZ, delta);
                break;
            case ActionKeys.TfRotX:
                bridge.SendRelativeFloat(EventIds.TfRotX, delta);
                break;
            case ActionKeys.TfRotY:
                bridge.SendRelativeFloat(EventIds.TfRotY, delta);
                break;
            case ActionKeys.TfRotZ:
                bridge.SendRelativeFloat(EventIds.TfRotZ, delta);
                break;
        }
    }

    public static String FormatScalarDisplay(String key, Double value) => key switch
    {
        ActionKeys.TfScale => value.ToString("F2"),
        ActionKeys.TfPosX or ActionKeys.TfPosY or ActionKeys.TfPosZ => value.ToString("F2"),
        ActionKeys.TfRotX or ActionKeys.TfRotY or ActionKeys.TfRotZ => value.ToString("F1") + "°",
        _       => value.ToString("F2"),
    };

    public static string? GetDisplayValue(string key, ContextSnapshot snap) => key switch
    {
        ActionKeys.TfScale => snap.ScaleUniform.ToString("F2"),
        ActionKeys.TfPosX    => snap.Position[0].ToString("F2"),
        ActionKeys.TfPosY    => snap.Position[1].ToString("F2"),
        ActionKeys.TfPosZ    => snap.Position[2].ToString("F2"),
        ActionKeys.TfRotX    => snap.RotationDeg[0].ToString("F1") + "°",
        ActionKeys.TfRotY    => snap.RotationDeg[1].ToString("F1") + "°",
        ActionKeys.TfRotZ    => snap.RotationDeg[2].ToString("F1") + "°",
        _       => null,
    };

    public static string? GetDisplayName(string key) => key switch
    {
        ActionKeys.TfScale => "Scale",
        ActionKeys.TfPosX    => "Pos X",
        ActionKeys.TfPosY    => "Pos Y",
        ActionKeys.TfPosZ    => "Pos Z",
        ActionKeys.TfRotX    => "Rot X",
        ActionKeys.TfRotY    => "Rot Y",
        ActionKeys.TfRotZ    => "Rot Z",
        _       => null,
    };
}
