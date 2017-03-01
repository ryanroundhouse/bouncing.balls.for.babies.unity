using System;
using System.Globalization;
using UnityEngine;
using System.Collections;

public class ScoreCameraScript : MonoBehaviour
{
    private Texture ObjectiveOverlay;
    private Texture InstructionOverlay;
    private Texture ScoreTextOverlay;
    private float TimeSinceLastRestart;

    private const int InstructionDelayBeforeDisplaying = 0;
    private const int InstructionOverlayDelayBeforeFadeStarts = 2;
    private const int InstructionOverlayFadeDuration = 4;
    private float InstructionOverlayExpiration;
    private float InstructionOverlayPercentageTransparancy;
    public bool AbortInstructions = false;

    private const int ObjectiveDelayBeforeDisplaying = 8;
    private const int ObjectiveOverlayDelayBeforeFadeStarts = 2;
    private const int ObjectiveOverlayFadeDuration = 4;
    private float ObjectiveOverlayExpiration;
    private float ObjectiveOverlayPercentageTransparancy;

    public GameObject EarthBall;
    public GameObject JupiterBall;
    public GameObject MoonBall;

    private BallScript EarthBallScript;
    private BallScript JupiterBallScript;
    private BallScript MoonBallScript;

    private GUIStyle style;

    void Start()
    {
        EarthBallScript = EarthBall.GetComponent<BallScript>();
        JupiterBallScript = JupiterBall.GetComponent<BallScript>();
        MoonBallScript = MoonBall.GetComponent<BallScript>();
        ObjectiveOverlay = Resources.Load<Texture>("ObjectiveOverlay");
        InstructionOverlay = Resources.Load<Texture>("InstructionOverlay");
        ScoreTextOverlay = Resources.Load<Texture>("ScoreText");
        TimeSinceLastRestart = Time.time;

        InstructionOverlayExpiration = GetTimeSinceLastRestart() + InstructionOverlayFadeDuration;
        ObjectiveOverlayExpiration = GetTimeSinceLastRestart() + ObjectiveOverlayFadeDuration;
        InstructionOverlayPercentageTransparancy = 1f;
        ObjectiveOverlayPercentageTransparancy = 1f;
    }

	void OnGUI ()
	{
	    if (style == null)
	    {
	        style = GUI.skin.GetStyle("label");
	        style.fontSize = 0;
	        style.font = Resources.Load("AlegreyaBold") as Font;
	    }
	    var screenHeight = Screen.height;
	    var screenWidth = Screen.width;

        GUI.Label(new Rect(screenWidth * 0.1f, screenHeight * 0.34f, 100, 100), EarthBallScript.Score.ToString(CultureInfo.InvariantCulture), style);
        GUI.Label(new Rect(screenWidth * 0.1f, screenHeight * 0.53f, 100, 100), JupiterBallScript.Score.ToString(CultureInfo.InvariantCulture), style);
        GUI.Label(new Rect(screenWidth * 0.1f, screenHeight * 0.72f, 100, 100), MoonBallScript.Score.ToString(CultureInfo.InvariantCulture), style);

        DisplayOverlay(ObjectiveOverlay, ref ObjectiveOverlayPercentageTransparancy, ObjectiveOverlayDelayBeforeFadeStarts, ObjectiveOverlayFadeDuration, ObjectiveOverlayExpiration, ObjectiveDelayBeforeDisplaying, false);
        DisplayOverlay(InstructionOverlay, ref InstructionOverlayPercentageTransparancy, InstructionOverlayDelayBeforeFadeStarts, InstructionOverlayFadeDuration, InstructionOverlayExpiration, InstructionDelayBeforeDisplaying, AbortInstructions);
        GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, 1);
        GUI.DrawTexture(new Rect(0, 0, screenWidth, screenHeight), ScoreTextOverlay);
	}

    private void DisplayOverlay(Texture overlay, ref float overlayFadeAlphaChannel, int overlayFadeDelay, int overlayFadeDuration,
        float overlayExpiration, int delayBeforeFadeStarts, bool shouldAbortOverlay)
    {
        var screenHeight = Screen.height;
        var screenWidth = Screen.width;

        if (shouldAbortOverlay)
        {
            overlayFadeAlphaChannel = 0;
        }

        if(overlayFadeAlphaChannel > 0)
        {
            if(GetTimeSinceLastRestart() > delayBeforeFadeStarts)
            {
                if(Time.time + delayBeforeFadeStarts >= overlayFadeDelay)
                {
                    overlayFadeAlphaChannel =
                        Mathf.Clamp(((overlayFadeDuration - GetTimeSinceLastRestart() + overlayFadeDelay + delayBeforeFadeStarts) / overlayExpiration), 0f, 1f);
                }

                GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, overlayFadeAlphaChannel);
                GUI.DrawTexture(new Rect(0, 0, screenWidth, screenHeight), overlay);
            }
        }
    }

    private float GetTimeSinceLastRestart()
    {
        return Time.time - TimeSinceLastRestart;
    }
}
