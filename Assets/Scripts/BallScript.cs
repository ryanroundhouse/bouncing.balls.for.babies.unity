using System;
using System.Collections.Generic;
using Assets.Scripts;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class BallScript : MonoBehaviour
{
    public int Score;
    public GameObject BounceSoundManager;
    public int HitPower = 1500;
    public int BallIndex = 0;

    public float TapVsDragThreshold = 0.5f;
    public float DragRefDistance = 5f;
    public float MaxForceMultiplier = 1.25f;

    public float OutOfPlayDropThreshold = 3f;
    public float OutOfPlayRespawnDelay = 15f;

    private BallGroupScript BallGroupScript;
    private Vector3 tapStart;
    private Vector3 dragCurrent;
    private bool isDragging;

    private LineRenderer launchLine;
    private LineRenderer tipRing;

    private Vector3 spawnPosition;
    private Quaternion spawnRotation;
    private bool outOfPlay;
    private float outOfPlaySince;

    void Start()
    {
        Score = 0;
        BallGroupScript = BounceSoundManager.GetComponent<BallGroupScript>();
        BuildLaunchIndicator();
        spawnPosition = transform.position;
        spawnRotation = transform.rotation;
    }

    void Update()
    {
        CheckOutOfPlay();
    }

    void CheckOutOfPlay()
    {
        var fallenOut = transform.position.y < spawnPosition.y - OutOfPlayDropThreshold;
        if (fallenOut && !outOfPlay)
        {
            outOfPlay = true;
            outOfPlaySince = Time.time;
        }
        else if (!fallenOut && outOfPlay)
        {
            outOfPlay = false;
        }

        if (outOfPlay && Time.time - outOfPlaySince >= OutOfPlayRespawnDelay)
        {
            RespawnAtStart();
        }
    }

    void RespawnAtStart()
    {
        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        transform.position = spawnPosition;
        transform.rotation = spawnRotation;
        outOfPlay = false;
        isDragging = false;
        HideLaunchIndicator();
    }

    public void squishSomething()
    {
        Score++;
        BumpMusicPitchForLeader();
        var target = WinningInfoScript.SquishTarget;
        if (target > 0 && Score >= target)
        {
            WinningInfoScript.WinningPlanet = name;
            WinningInfoScript.WinningPlanetIndex = BallIndex;
            SceneManager.LoadScene("VictoryScene");
        }
    }

    static void BumpMusicPitchForLeader()
    {
        var leader = 0;
        foreach (var b in GameObject.FindGameObjectsWithTag("Ball"))
        {
            var s = b.GetComponent<BallScript>();
            if (s != null && s.Score > leader) leader = s.Score;
        }
        var manager = GameObject.Find("MusicManager");
        if (manager == null) return;
        var music = manager.GetComponent<MusicManagerScript>();
        if (music == null) return;
        // Ramp 1.0x → 1.3x as the leader approaches the squish target.
        // Infinite mode has no finish line, so cap the ramp scale at 10 to keep the pitch climbing modestly.
        var rampMax = WinningInfoScript.IsInfiniteMode ? 10f : Mathf.Max(1f, WinningInfoScript.SquishTarget);
        music.SetPitchScale(1f + Mathf.Clamp01(leader / rampMax) * 0.3f);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.tag.Equals("Floor"))
        {
            BallGroupScript.PlayBounceSound(collision.gameObject.tag.Equals("Ball"));
        }
    }

    // SendMessage from CameraScript on touch/mouse-down: the initial tap anchor.
    void MouseTouched(Vector3 worldPoint)
    {
        tapStart = worldPoint;
        dragCurrent = worldPoint;
        isDragging = true;
        RefreshLaunchIndicator();
    }

    // SendMessage from CameraScript every frame while held: current finger/cursor world position,
    // projected onto this ball's horizontal plane, even when the input has moved off the ball.
    void DragUpdate(Vector3 worldPoint)
    {
        if (!isDragging) return;
        dragCurrent = worldPoint;
        RefreshLaunchIndicator();
    }

    // SendMessage from CameraScript on release. `releasedOnBall` is no longer load-bearing — the
    // tap-vs-drag decision is made from the drag distance instead.
    void TouchEnded(bool releasedOnBall)
    {
        if (!isDragging) return;

        ComputeLaunch(out Vector3 direction, out float magnitude);

        var rb = GetComponent<Rigidbody>();
        rb.linearVelocity = Vector3.zero;
        rb.AddForce(new Vector3(direction.x, 0, direction.z) * magnitude);

        isDragging = false;
        tapStart = Vector3.zero;
        dragCurrent = Vector3.zero;
        HideLaunchIndicator();
    }

    void ComputeLaunch(out Vector3 direction, out float magnitude)
    {
        var dragVector = dragCurrent - tapStart;
        dragVector.y = 0;
        var dragDistance = dragVector.magnitude;

        if (dragDistance < TapVsDragThreshold)
        {
            // Tap: push the ball away from the tap point at base power.
            var away = transform.position - tapStart;
            away.y = 0;
            if (away.sqrMagnitude < 0.0001f) away = Vector3.forward;
            direction = away.normalized;
            magnitude = HitPower;
        }
        else
        {
            // Drag: pull in the drag direction, force lerped from 1x at the threshold to MaxForceMultiplier at DragRefDistance.
            direction = dragVector / dragDistance;
            var t = Mathf.Clamp01((dragDistance - TapVsDragThreshold) / Mathf.Max(0.001f, DragRefDistance - TapVsDragThreshold));
            magnitude = Mathf.Lerp(HitPower, HitPower * MaxForceMultiplier, t);
        }
    }

    void BuildLaunchIndicator()
    {
        var lineGo = new GameObject("LaunchLine");
        lineGo.transform.SetParent(transform, false);
        launchLine = lineGo.AddComponent<LineRenderer>();
        launchLine.useWorldSpace = true;
        launchLine.positionCount = 2;
        launchLine.widthCurve = AnimationCurve.Linear(0f, 0.35f, 1f, 0.08f);
        launchLine.numCornerVertices = 4;
        launchLine.numCapVertices = 4;
        launchLine.material = MakeOverlayMaterial();
        launchLine.enabled = false;

        var ringGo = new GameObject("LaunchTipRing");
        ringGo.transform.SetParent(transform, false);
        tipRing = ringGo.AddComponent<LineRenderer>();
        tipRing.useWorldSpace = true;
        tipRing.loop = true;
        const int ringSegments = 28;
        tipRing.positionCount = ringSegments;
        tipRing.startWidth = 0.09f;
        tipRing.endWidth = 0.09f;
        tipRing.material = MakeOverlayMaterial();
        tipRing.enabled = false;
    }

    static Material MakeOverlayMaterial()
    {
        var shader = Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Color") ?? Shader.Find("Standard");
        return new Material(shader);
    }

    void RefreshLaunchIndicator()
    {
        if (launchLine == null) return;

        var dragVector = dragCurrent - tapStart;
        dragVector.y = 0;
        var dragDistance = dragVector.magnitude;
        if (dragDistance < TapVsDragThreshold)
        {
            HideLaunchIndicator();
            return;
        }

        ComputeLaunch(out Vector3 direction, out float magnitude);
        var visualLength = Mathf.Min(dragDistance, DragRefDistance * MaxForceMultiplier);

        var lift = new Vector3(0f, 0.6f, 0f);
        var start = transform.position + lift;
        var end = start + direction * visualLength;

        launchLine.enabled = true;
        launchLine.SetPosition(0, start);
        launchLine.SetPosition(1, end);

        var intensity = Mathf.InverseLerp(HitPower, HitPower * MaxForceMultiplier, magnitude);
        var cool = new Color(0.4f, 0.9f, 1f, 1f);
        var hot = new Color(1f, 0.25f, 0.1f, 1f);
        var tipColor = Color.Lerp(cool, hot, intensity);

        var gradient = new Gradient();
        gradient.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(tipColor, 1f) },
            new[] { new GradientAlphaKey(0.95f, 0f), new GradientAlphaKey(0.95f, 1f) }
        );
        launchLine.colorGradient = gradient;

        tipRing.enabled = true;
        var segments = tipRing.positionCount;
        var ringRadius = 0.35f + 0.25f * intensity;
        for (var i = 0; i < segments; i++)
        {
            var a = (i / (float)segments) * Mathf.PI * 2f;
            tipRing.SetPosition(i, end + new Vector3(Mathf.Cos(a) * ringRadius, 0f, Mathf.Sin(a) * ringRadius));
        }
        tipRing.startColor = tipColor;
        tipRing.endColor = tipColor;
    }

    void HideLaunchIndicator()
    {
        if (launchLine != null) launchLine.enabled = false;
        if (tipRing != null) tipRing.enabled = false;
    }
}
