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
    private Dictionary<GameObject, int> dragInputId;
    private const int MouseInputId = -1;

    void Start()
    {
        shakeDetectionThreshold *= shakeDetectionThreshold;
        lowPassValue = Input.acceleration;
        touchList = new List<GameObject>();
        dragInputId = new Dictionary<GameObject, int>();

        GameObject.Find("MusicManager").GetComponent<MusicManagerScript>().PlayMusic(Music.Gameplay);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        var releasedOnBall = new Dictionary<int, bool>();
        var hit = new RaycastHit();
        if (Input.touches.Any())
        {
            foreach (var touch in Input.touches)
            {
                var ray = GetComponent<Camera>().ScreenPointToRay(touch.position);
                var rayHitBall = Physics.Raycast(ray, out hit) && hit.transform.tag.Equals("Ball");

                if (touch.phase == TouchPhase.Began && rayHitBall)
                {
                    var ball = hit.transform.gameObject;
                    if (!touchList.Contains(ball))
                    {
                        touchList.Add(ball);
                    }
                    dragInputId[ball] = touch.fingerId;
                    // Seed the tap anchor on the ball. Previously only the mouse path did this,
                    // which left the touch path's tap-push using a zero anchor.
                    ball.SendMessage("MouseTouched", ProjectToBallPlane(touch.position, ball), SendMessageOptions.DontRequireReceiver);
                }
                else if ((touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary) && rayHitBall)
                {
                    hit.transform.gameObject.SendMessage("Touched", hit.point, SendMessageOptions.DontRequireReceiver);
                }
                else if (touch.phase == TouchPhase.Canceled || touch.phase == TouchPhase.Ended)
                {
                    releasedOnBall[touch.fingerId] = rayHitBall;
                }
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            var ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            var rayHitBall = Physics.Raycast(ray, out hit) && hit.transform.tag.Equals("Ball");
            releasedOnBall[MouseInputId] = rayHitBall;
        }
        else if (Input.GetMouseButtonDown(0))
        {
            var ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit) && hit.transform.tag.Equals("Ball"))
            {
                var ball = hit.transform.gameObject;
                if (!touchList.Contains(ball))
                {
                    touchList.Add(ball);
                }
                dragInputId[ball] = MouseInputId;
                ball.SendMessage("MouseTouched", ProjectToBallPlane(Input.mousePosition, ball), SendMessageOptions.DontRequireReceiver);
            }
        }
        else if (Input.GetMouseButton(0))
        {
            var ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit) && hit.transform.tag.Equals("Ball"))
            {
                hit.transform.gameObject.SendMessage("Touched", hit.point, SendMessageOptions.DontRequireReceiver);
            }
        }

        // Stream the live input position (projected onto the ball's horizontal plane) to each ball
        // being dragged, so drag tracking and the launch indicator keep working when the finger or
        // cursor has moved off the ball collider.
        foreach (var ball in touchList)
        {
            if (!dragInputId.TryGetValue(ball, out int inputId)) continue;
            if (!TryGetInputScreenPosition(inputId, out Vector2 screenPos)) continue;
            ball.SendMessage("DragUpdate", ProjectToBallPlane(screenPos, ball), SendMessageOptions.DontRequireReceiver);
        }

        // End a drag only when the input that started it actually ends — not when the input moves
        // off the ball. The old "lost balls" check fired TouchEnded the moment the cursor/finger
        // crossed the collider boundary, which made any meaningful drag impossible to see.
        var endedBalls = new List<GameObject>();
        foreach (var ball in touchList)
        {
            if (!dragInputId.TryGetValue(ball, out int inputId)) continue;
            if (!IsInputStillActive(inputId)) endedBalls.Add(ball);
        }
        foreach (var ball in endedBalls)
        {
            var inputId = dragInputId[ball];
            var endedOnBall = releasedOnBall.TryGetValue(inputId, out bool b) && b;
            ((ScoreCameraScript)GameObject.Find("GuiWriter").GetComponent("ScoreCameraScript")).AbortInstructions = true;
            ball.SendMessage("TouchEnded", endedOnBall, SendMessageOptions.DontRequireReceiver);
            touchList.Remove(ball);
            dragInputId.Remove(ball);
        }

        acceleration = Input.acceleration;
        lowPassValue = Vector3.Lerp(lowPassValue, acceleration, lowPassFilterFactor);
        deltaAcceleration = acceleration - lowPassValue;
        if (deltaAcceleration.sqrMagnitude >= shakeDetectionThreshold)
        {
            ((ScoreCameraScript)GameObject.Find("GuiWriter").GetComponent("ScoreCameraScript")).AbortInstructions = true;
            foreach (var ball in GameObject.FindGameObjectsWithTag("Ball"))
            {
                var randomForce = new Vector3(Utility.GetRandomFloat(0.0f, 1.0f), 0, Utility.GetRandomFloat(0.0f, 1.0f));
                randomForce.Normalize();
                ball.GetComponent<Rigidbody>().linearVelocity = new Vector3(0, 0, 0);

                ball.GetComponent<Rigidbody>().AddForce(new Vector3(randomForce.x, 0, randomForce.z) * HitPower);
            }
        }
    }

    Vector3 ProjectToBallPlane(Vector2 screenPos, GameObject ball)
    {
        var plane = new Plane(Vector3.up, ball.transform.position);
        var ray = GetComponent<Camera>().ScreenPointToRay(screenPos);
        return plane.Raycast(ray, out float dist) ? ray.GetPoint(dist) : ball.transform.position;
    }

    bool TryGetInputScreenPosition(int inputId, out Vector2 screenPos)
    {
        if (inputId == MouseInputId)
        {
            if (Input.GetMouseButton(0))
            {
                screenPos = Input.mousePosition;
                return true;
            }
            screenPos = default;
            return false;
        }
        foreach (var t in Input.touches)
        {
            if (t.fingerId == inputId)
            {
                screenPos = t.position;
                return true;
            }
        }
        screenPos = default;
        return false;
    }

    bool IsInputStillActive(int inputId)
    {
        if (inputId == MouseInputId) return Input.GetMouseButton(0);
        foreach (var t in Input.touches)
        {
            if (t.fingerId == inputId)
            {
                return t.phase != TouchPhase.Ended && t.phase != TouchPhase.Canceled;
            }
        }
        return false;
    }
}
