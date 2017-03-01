using UnityEngine;
using System.Collections;

public class GuiGameObjectRepositionScript : MonoBehaviour
{
    private Camera MainCamera;
    public Vector3 PositionInViewpoint;

	// Use this for initialization
	void Start ()
	{
        MainCamera = GameObject.FindGameObjectWithTag("MainCamera").camera;
        var v3Pos = PositionInViewpoint;
        transform.position = MainCamera.ViewportToWorldPoint(v3Pos);
	}
}
