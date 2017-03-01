using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Assets.Scripts;
using UnityEngine;
using System.Collections;

public class CameraScript : MonoBehaviour
{
    public int HitPower = 1000;

    private const float accelerometerUpdateInterval = 1.0f/60.0f;
    // The greater the value of LowPassKernelWidthInSeconds, the slower the filtered value will converge towards current input sample (and vice versa).
    private const float lowPassKernelWidthInSeconds = 1.0f;
    // This next parameter is initialized to 2.0 per Apple's recommendation, or at least according to Brady! ;)
    private float shakeDetectionThreshold = 2.0f;
    private readonly float lowPassFilterFactor = accelerometerUpdateInterval / lowPassKernelWidthInSeconds;
    private Vector3 lowPassValue = Vector3.zero;
    private Vector3 acceleration;
    private Vector3 deltaAcceleration;

    private List<GameObject> touchList;

    void Start()
    {
        shakeDetectionThreshold *= shakeDetectionThreshold;
        lowPassValue = Input.acceleration;
        touchList = new List<GameObject>();

        GameObject.Find("MusicManager").GetComponent<MusicManagerScript>().PlayMusic(Music.Gameplay);
    }

	// Update is called once per frame
	void Update ()
	{
	    if (Input.GetKeyDown(KeyCode.Escape))
	    {
	        Application.Quit();
	    }

        var touchedBallsThisRound = new List<GameObject>();
	    var inputEndedOnBall = false;
        var hit = new RaycastHit();
	    if (Input.touches.Any())
	    {
	        foreach (var touch in Input.touches)
            {
                var ray = camera.ScreenPointToRay(touch.position);
                if(Physics.Raycast(ray, out hit))
                {
                    if(hit.transform.tag.Equals("Ball"))
                    {
                        if(touch.phase == TouchPhase.Began)
                        {
                            if(!touchList.Contains(hit.transform.gameObject))
                            {
                                touchList.Add(hit.transform.gameObject);
                            }
                            touchedBallsThisRound.Add(hit.transform.gameObject);
                            hit.transform.gameObject.SendMessage("Touched", hit.point, SendMessageOptions.DontRequireReceiver);
                        }
                        else if(touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                        {
                            touchedBallsThisRound.Add(hit.transform.gameObject);
                            hit.transform.gameObject.SendMessage("Touched", hit.point, SendMessageOptions.DontRequireReceiver);
                        }
                        else if (touch.phase == TouchPhase.Canceled || touch.phase == TouchPhase.Ended)
                        {
                            inputEndedOnBall = true;
                        }
                    }
                }
	        }
        }
        else if(Input.GetMouseButtonUp(0))
        {
            var ray = camera.ScreenPointToRay(Input.mousePosition);
            if(Physics.Raycast(ray, out hit))
            {
                if(hit.transform.tag.Equals("Ball"))
                {
                    inputEndedOnBall = true;
                }
            }
        }
        else if (Input.GetMouseButtonDown(0))
        {
            var ray = camera.ScreenPointToRay(Input.mousePosition);
            if(Physics.Raycast(ray, out hit))
            {
                if(hit.transform.tag.Equals("Ball"))
                {
                    if(!touchList.Contains(hit.transform.gameObject))
                    {
                        touchList.Add(hit.transform.gameObject);
                    }
                    touchedBallsThisRound.Add(hit.transform.gameObject);
                    hit.transform.gameObject.SendMessage("MouseTouched", hit.point, SendMessageOptions.DontRequireReceiver);
                }
            }
        }
        else if (Input.GetMouseButton(0))
        {
            var ray = camera.ScreenPointToRay(Input.mousePosition);
            if(Physics.Raycast(ray, out hit))
            {
                if (hit.transform.tag.Equals("Ball"))
                {
                    touchedBallsThisRound.Add(hit.transform.gameObject);
                    hit.transform.gameObject.SendMessage("Touched", hit.point, SendMessageOptions.DontRequireReceiver);
                }
            }
        }

	    var lostBalls = touchList.Except(touchedBallsThisRound).ToArray();
        foreach (var ball in lostBalls)
        {
            ((ScoreCameraScript) GameObject.Find("GuiWriter").GetComponent("ScoreCameraScript")).AbortInstructions = true;
            ball.SendMessage("TouchEnded", inputEndedOnBall, SendMessageOptions.DontRequireReceiver);
	        touchList.Remove(ball);
	    }

        acceleration = Input.acceleration;
        lowPassValue = Vector3.Lerp(lowPassValue, acceleration, lowPassFilterFactor);
        deltaAcceleration = acceleration - lowPassValue;
        if(deltaAcceleration.sqrMagnitude >= shakeDetectionThreshold)
        {
            ((ScoreCameraScript)GameObject.Find("GuiWriter").GetComponent("ScoreCameraScript")).AbortInstructions = true;
            foreach(var ball in GameObject.FindGameObjectsWithTag("Ball"))
            {
                var randomForce = new Vector3(Utility.GetRandomFloat(0.0f, 1.0f), 0, Utility.GetRandomFloat(0.0f, 1.0f));
                randomForce.Normalize();
                ball.rigidbody.velocity = new Vector3(0, 0, 0);

                ball.rigidbody.AddForce(new Vector3(randomForce.x, 0, randomForce.z) * HitPower);
            }
        }
	}
}
