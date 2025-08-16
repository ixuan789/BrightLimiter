using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using WmiLight;

namespace BrightLimiter;


[SuppressMessage("Interoperability", "CA1416:验证平台兼容性")]
public static class BrightnessWmiLightHelper
{
    public static WmiEventWatcher GetWatcher() =>
        new(new WmiConnection(@"root\wmi"), "SELECT * FROM WmiMonitorBrightnessEvent");

    private static readonly WmiConnection BaseConnection = new(@"root\WMI");
    private static readonly string BrightnessQuery = "SELECT * FROM WmiMonitorBrightness";
    private static readonly string BrightnessMethodsQuery = "SELECT * FROM WmiMonitorBrightnessMethods";
    private static readonly WmiObject SetObject;
    private static readonly WmiMethod SetMethod;
    private static readonly WmiMethodParameters SetParameters;
    static BrightnessWmiLightHelper()
    {
        SetObject = BaseConnection.CreateQuery(BrightnessMethodsQuery).First();
        if (SetObject is null) throw new MissingMethodException("Can't find WmiMonitorBrightnessMethods");
        SetMethod = SetObject.GetMethod("WmiSetBrightness");
        SetParameters = SetMethod.CreateInParameters();
        SetParameters.SetPropertyValue("Brightness", 50);
        SetParameters.SetPropertyValue("Timeout", (int)int.MaxValue);
    }

    public static byte GetBrightnessLevel()
    {
        return BaseConnection.CreateQuery(BrightnessQuery).Select(wmi => (byte)wmi["CurrentBrightness"])
            .FirstOrDefault();
    }

    public static void SetBrightnessLevel(byte brightnessLevel)
    {
        if (brightnessLevel > 100) return;

        SetParameters.SetPropertyValue("Brightness", brightnessLevel);
        SetObject.ExecuteMethod<uint>(SetMethod, SetParameters, out _);
    }

}