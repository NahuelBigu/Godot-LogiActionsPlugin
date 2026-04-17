using System.Text.Json;

namespace Loupedeck.GodotMxBridge;

internal static class JsonElementExtensions
{
    public static string GetStringOrDefault(this JsonElement el, string prop, string def = "")
    {
        if (el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.String)
            return v.GetString() ?? def;
        return def;
    }

    public static bool GetBoolOrDefault(this JsonElement el, string prop, bool def = false)
    {
        if (!el.TryGetProperty(prop, out var v))
            return def;
        if (v.ValueKind == JsonValueKind.True) return true;
        if (v.ValueKind == JsonValueKind.False) return false;
        if (v.ValueKind == JsonValueKind.Number)
            return v.GetInt32() != 0;
        return def;
    }

    public static double GetDoubleOrDefault(this JsonElement el, string prop, double def = 0.0)
    {
        if (el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.Number)
            return v.GetDouble();
        return def;
    }

    public static int GetIntOrDefault(this JsonElement el, string prop, int def = 0)
    {
        if (el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.Number)
            return v.GetInt32();
        return def;
    }

    /// <summary>Physics layer/mask values are uint32 bitmasks; JSON may exceed <see cref="int.MaxValue"/>.</summary>
    public static int GetPhysicsLayerMaskInt32(this JsonElement el, string prop, int def = 0)
    {
        if (!el.TryGetProperty(prop, out var v) || v.ValueKind != JsonValueKind.Number)
            return def;
        if (v.TryGetInt32(out var i32))
            return i32;
        if (v.TryGetUInt32(out var u32))
            return unchecked((int)u32);
        var d = v.GetDouble();
        if (d is >= 0 and <= uint.MaxValue)
            return unchecked((int)(uint)d);
        return def;
    }

    public static double[] GetDoubleArray(this JsonElement el, string prop, int expectedLen)
    {
        var result = new double[expectedLen];
        if (!el.TryGetProperty(prop, out var arr) || arr.ValueKind != JsonValueKind.Array)
            return result;
        var i = 0;
        foreach (var item in arr.EnumerateArray())
        {
            if (i >= expectedLen) break;
            result[i++] = item.GetDouble();
        }
        return result;
    }

    public static string[] GetStringArray(this JsonElement el, string prop, int expectedLen)
    {
        var result = new string[expectedLen];
        if (!el.TryGetProperty(prop, out var arr) || arr.ValueKind != JsonValueKind.Array)
            return result;
        var i = 0;
        foreach (var item in arr.EnumerateArray())
        {
            if (i >= expectedLen) break;
            result[i++] = item.ValueKind == JsonValueKind.String ? (item.GetString() ?? "") : "";
        }
        return result;
    }

    public static bool HasNonEmptyString(this JsonElement el, string prop)
    {
        return el.TryGetProperty(prop, out var v)
               && v.ValueKind == JsonValueKind.String
               && !string.IsNullOrWhiteSpace(v.GetString());
    }
}
