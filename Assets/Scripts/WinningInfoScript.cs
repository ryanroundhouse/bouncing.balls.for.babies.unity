using UnityEngine;
using System.Collections;

public class WinningInfoScript : MonoBehaviour
{
    public static string WinningPlanet = string.Empty;
    public static int WinningPlanetIndex = 0;

    public const int InfiniteSquishTarget = int.MaxValue;
    private const string SquishTargetPrefKey = "SquishTarget";
    private const int DefaultSquishTarget = 5;

    private static int squishTarget;
    private static bool squishTargetLoaded;

    // Unity throws if PlayerPrefs is touched from a MonoBehaviour type initializer, so the read is
    // deferred until first access — by then the engine is fully up and the call is allowed.
    public static int SquishTarget
    {
        get
        {
            if (!squishTargetLoaded)
            {
                squishTarget = PlayerPrefs.GetInt(SquishTargetPrefKey, DefaultSquishTarget);
                squishTargetLoaded = true;
            }
            return squishTarget;
        }
        set
        {
            squishTarget = value;
            squishTargetLoaded = true;
            PlayerPrefs.SetInt(SquishTargetPrefKey, value);
            PlayerPrefs.Save();
        }
    }

    public static bool IsInfiniteMode { get { return SquishTarget >= InfiniteSquishTarget; } }
}
