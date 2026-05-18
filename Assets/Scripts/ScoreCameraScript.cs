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

    // Hidden parent menu: long-press the top-right icon for 5 seconds.
    private const float MenuHoldDuration = 5f;
    private bool menuOpen;
    private bool pressOnIcon;
    private float pressStartUnscaledTime;
    private static bool inputCaptured;

    // True while the menu is open or the icon is being long-pressed; CameraScript reads this to
    // suppress ball input so the menu interaction doesn't double as a flick.
    public static bool IsInputCaptured { get { return inputCaptured; } }

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

    void Update()
    {
        UpdateMenuIconHold();
    }

    void UpdateMenuIconHold()
    {
        if (menuOpen)
        {
            inputCaptured = true;
            pressOnIcon = false;
            return;
        }

        var iconRect = GetIconRect();
        var hasInput = TryGetPrimaryInputPositionGui(out Vector2 guiPos);
        var nowOverIcon = hasInput && iconRect.Contains(guiPos);

        if (nowOverIcon && !pressOnIcon)
        {
            pressOnIcon = true;
            pressStartUnscaledTime = Time.unscaledTime;
        }
        else if (!nowOverIcon && pressOnIcon)
        {
            pressOnIcon = false;
        }

        inputCaptured = pressOnIcon;

        if (pressOnIcon && Time.unscaledTime - pressStartUnscaledTime >= MenuHoldDuration)
        {
            menuOpen = true;
            pressOnIcon = false;
        }
    }

    bool TryGetPrimaryInputPositionGui(out Vector2 guiPos)
    {
        if (Input.touchCount > 0)
        {
            for (var i = 0; i < Input.touches.Length; i++)
            {
                var t = Input.touches[i];
                if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled) continue;
                guiPos = new Vector2(t.position.x, Screen.height - t.position.y);
                return true;
            }
        }
        else if (Input.GetMouseButton(0))
        {
            guiPos = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
            return true;
        }
        guiPos = default;
        return false;
    }

    Rect GetIconRect()
    {
        var size = Mathf.Min(Screen.width, Screen.height) * 0.06f;
        var pad = size * 0.4f;
        return new Rect(Screen.width - size - pad, pad, size, size);
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

        DrawMenuIcon();
        if (menuOpen) DrawMenu();
	}

    void DrawMenuIcon()
    {
        var rect = GetIconRect();
        var prevColor = GUI.color;

        GUI.color = new Color(0f, 0f, 0f, 0.45f);
        GUI.DrawTexture(rect, Texture2D.whiteTexture);

        if (pressOnIcon)
        {
            var t = Mathf.Clamp01((Time.unscaledTime - pressStartUnscaledTime) / MenuHoldDuration);
            var fillHeight = rect.height * t;
            GUI.color = new Color(1f, 0.75f, 0.1f, 0.55f);
            GUI.DrawTexture(new Rect(rect.x, rect.y + (rect.height - fillHeight), rect.width, fillHeight), Texture2D.whiteTexture);
        }

        GUI.color = new Color(1f, 1f, 1f, 0.9f);
        var lineThickness = Mathf.Max(2f, rect.height * 0.09f);
        var lineWidth = rect.width * 0.55f;
        var lineX = rect.x + (rect.width - lineWidth) * 0.5f;
        var firstLineY = rect.y + rect.height * 0.28f;
        var lineGap = rect.height * 0.20f;
        for (var i = 0; i < 3; i++)
        {
            GUI.DrawTexture(new Rect(lineX, firstLineY + i * lineGap, lineWidth, lineThickness), Texture2D.whiteTexture);
        }

        GUI.color = prevColor;
    }

    void DrawMenu()
    {
        var screenW = Screen.width;
        var screenH = Screen.height;
        var prevColor = GUI.color;

        GUI.color = new Color(0f, 0f, 0f, 0.7f);
        GUI.DrawTexture(new Rect(0, 0, screenW, screenH), Texture2D.whiteTexture);

        var panelW = Mathf.Min(screenW * 0.75f, 700f);
        var panelH = Mathf.Min(screenH * 0.75f, 600f);
        var panelRect = new Rect((screenW - panelW) * 0.5f, (screenH - panelH) * 0.5f, panelW, panelH);

        GUI.color = new Color(0.15f, 0.16f, 0.22f, 0.97f);
        GUI.DrawTexture(panelRect, Texture2D.whiteTexture);
        GUI.color = Color.white;

        var menuFont = Resources.Load("AlegreyaBold") as Font;

        var titleStyle = new GUIStyle();
        titleStyle.font = menuFont;
        titleStyle.fontSize = Mathf.RoundToInt(panelH * 0.11f);
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.normal.textColor = Color.white;
        GUI.Label(new Rect(panelRect.x, panelRect.y + panelH * 0.04f, panelW, panelH * 0.16f), "Squish to win", titleStyle);

        var current = WinningInfoScript.SquishTarget;
        var infinite = WinningInfoScript.IsInfiniteMode;

        var btnW = panelW * 0.75f;
        var btnH = panelH * 0.14f;
        var btnX = panelRect.x + (panelW - btnW) * 0.5f;
        var firstBtnY = panelRect.y + panelH * 0.24f;
        var btnGap = panelH * 0.035f;
        var btnFontSize = Mathf.RoundToInt(panelH * 0.085f);

        if (DrawMenuButton(new Rect(btnX, firstBtnY + 0 * (btnH + btnGap), btnW, btnH), "5 squishes", !infinite && current == 5, menuFont, btnFontSize))
        {
            Apply(5);
        }
        if (DrawMenuButton(new Rect(btnX, firstBtnY + 1 * (btnH + btnGap), btnW, btnH), "10 squishes", !infinite && current == 10, menuFont, btnFontSize))
        {
            Apply(10);
        }
        if (DrawMenuButton(new Rect(btnX, firstBtnY + 2 * (btnH + btnGap), btnW, btnH), "Infinite", infinite, menuFont, btnFontSize))
        {
            Apply(WinningInfoScript.InfiniteSquishTarget);
        }

        var cancelW = panelW * 0.35f;
        var cancelH = panelH * 0.1f;
        var cancelRect = new Rect(panelRect.x + (panelW - cancelW) * 0.5f, panelRect.y + panelH - cancelH - panelH * 0.04f, cancelW, cancelH);
        if (DrawMenuButton(cancelRect, "Cancel", false, menuFont, Mathf.RoundToInt(cancelH * 0.55f), cancel: true))
        {
            menuOpen = false;
        }

        GUI.color = prevColor;
    }

    bool DrawMenuButton(Rect rect, string text, bool selected, Font font, int fontSize, bool cancel = false)
    {
        var prevColor = GUI.color;

        var baseColor = cancel
            ? new Color(0.45f, 0.18f, 0.20f, 1f)
            : (selected ? new Color(0.95f, 0.65f, 0.15f, 1f) : new Color(0.30f, 0.36f, 0.55f, 1f));

        GUI.color = baseColor;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);

        var border = Mathf.Max(2f, rect.height * 0.06f);
        GUI.color = new Color(1f, 1f, 1f, 0.18f);
        GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, border), Texture2D.whiteTexture);
        GUI.color = new Color(0f, 0f, 0f, 0.25f);
        GUI.DrawTexture(new Rect(rect.x, rect.y + rect.height - border, rect.width, border), Texture2D.whiteTexture);

        GUI.color = Color.white;
        var labelStyle = new GUIStyle();
        labelStyle.font = font;
        labelStyle.fontSize = fontSize;
        labelStyle.alignment = TextAnchor.MiddleCenter;
        labelStyle.normal.textColor = Color.white;
        var label = selected ? text + "  (current)" : text;
        GUI.Label(rect, label, labelStyle);

        GUI.color = prevColor;

        return GUI.Button(rect, GUIContent.none, GUIStyle.none);
    }

    void Apply(int target)
    {
        WinningInfoScript.SquishTarget = target;
        menuOpen = false;
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
