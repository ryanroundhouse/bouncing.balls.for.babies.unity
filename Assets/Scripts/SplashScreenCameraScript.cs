using System.Linq;
using UnityEngine;
using System.Collections;

public class SplashScreenCameraScript : MonoBehaviour
{
    private Texture splashTexture;
    private float expireTime;

	// Use this for initialization
	void Start ()
	{
	    splashTexture = Resources.Load<Texture>("Splash Screen");
        expireTime = Time.time + 3f;
	}
	
	// Update is called once per frame
    void OnGUI()
    {
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), splashTexture);
    }
    
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        if(Input.GetMouseButtonDown(0) || Input.touches.Any() || Time.time > expireTime)
        {
            Application.LoadLevel("MainScene");
        }
    }
}
