using System.Linq;
using UnityEngine;

public class VictoryCameraScript : MonoBehaviour
{
    private string WinnerText;
    private GUIStyle style;
    private GUIStyle smallStyle;

    private float CanRestartTimer;
    private const string CanContinueText = "Tap the screen to play again";

	// Use this for initialization
	void Start ()
	{
	    CanRestartTimer = Time.time + 3;
        WinnerText = string.Format("{0} Wins!", WinningInfoScript.WinningPlanet);

        GameObject.Find("MusicManager").GetComponent<MusicManagerScript>().PlayMusic(Music.Victory);
	}

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        if (Time.time > CanRestartTimer && (Input.GetMouseButtonDown(0) || Input.touches.Any()))
        {
            Application.LoadLevel("MainScene");
        }
    }
	
	// Update is called once per frame
    void OnGUI()
    {
        var screenHeight = Screen.height;
        var screenWidth = Screen.width;

        if (style == null)
        {
            style = GUI.skin.GetStyle("label");
            style.font = Resources.Load("AlegreyaBoldVictory") as Font;
            style.fontSize = 32;
            style.wordWrap = false;
        }
        if(smallStyle == null)
        {
            smallStyle = GUI.skin.GetStyle("label");
            smallStyle.font = Resources.Load("AlegreyaBold") as Font;
            smallStyle.fontSize = 18;
            smallStyle.wordWrap = false;
        }
        var textSize = style.CalcSize(new GUIContent(WinnerText));
        var smalltextSize = smallStyle.CalcSize(new GUIContent(CanContinueText));

        GUI.Label(new Rect((screenWidth - textSize.x) / 2f, (screenHeight - textSize.y) / 2f, textSize.x, textSize.y), WinnerText, style);
        if (Time.time > CanRestartTimer)
        {
            GUI.Label(
                new Rect((screenWidth - smalltextSize.x)/2f, (screenHeight - smalltextSize.y)*0.8f, smalltextSize.x,
                    smalltextSize.y), CanContinueText, smallStyle);
        }
    }
}
