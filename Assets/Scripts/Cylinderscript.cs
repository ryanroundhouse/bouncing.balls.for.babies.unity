using System.Collections.Generic;
using System.Linq;
using Assets.Scripts;
using UnityEngine;
using System.Collections;

public class Cylinderscript : MonoBehaviour
{
    public List<AudioClip> SquishSounds;
    public SquishCylinderScript ParentScript;

    public bool IsRunner;
    public float ScuttleTriggerDistance = 5f;
    public float ScuttleSpeed = 4f;

    private bool IsSquished;
    private float expireTime;
    private float wigglePhase;
    private Vector3 wanderDirection;
    private float wanderRedirectAt;
    private readonly List<string> slimeTypes = new List<string>{"green slime", "blue slime", "orange slime", "yellow slime"};

    void Start()
    {
        IsSquished = false;
        expireTime = -1;
        // Legacy Animation and Animator components carry over from the original 4.5-era FBX import.
        // Neither has a clip or controller assigned in this prefab so they don't drive anything visible,
        // but leaving them enabled risks them silently overriding the procedural wiggle/scuttle if a clip
        // ever sneaks back in. Disable them so Cylinderscript fully owns the transform.
        var legacyAnimation = GetComponent<Animation>();
        if (legacyAnimation != null) legacyAnimation.enabled = false;
        var animator = GetComponent<Animator>();
        if (animator != null) animator.enabled = false;
        wigglePhase = Utility.GetRandomFloat(0f, Mathf.PI * 2f);
        wanderDirection = RandomHorizontalDirection();
        wanderRedirectAt = Time.time + Utility.GetRandomFloat(1.5f, 3.5f);
        Debug.Log($"[Bug] {name} spawned IsRunner={IsRunner} pos={transform.position}");

        var filters = GetComponentsInChildren<MeshFilter>(true);
        Debug.Log($"[Cylinder] {name} at pos={transform.position} scale={transform.localScale} layer={gameObject.layer} active={gameObject.activeInHierarchy}");
        foreach (var f in filters)
        {
            var m = f.sharedMesh;
            var r = f.GetComponent<MeshRenderer>();
            var go = f.gameObject;
            var vCount = m != null ? m.vertexCount : -1;
            var subM = m != null ? m.subMeshCount : -1;
            var bounds = r != null ? r.bounds.ToString() : "(none)";
            Debug.Log($"[Cylinder] {go.name} worldPos={go.transform.position} lossyScale={go.transform.lossyScale} active={go.activeInHierarchy} vertices={vCount} submeshes={subM} rendererBounds={bounds} layer={go.layer}");
        }
        var cam = Camera.main;
        if (cam != null) Debug.Log($"[Cylinder] Camera pos={cam.transform.position} cullingMask={System.Convert.ToString(cam.cullingMask, 2)}");
    }

    void Update()
    {
        if (IsSquished)
        {
            if (Time.time > expireTime)
            {
                ParentScript.RemoveCylinder(gameObject);
            }
            return;
        }

        Wiggle();
        if (IsRunner) Scuttle();
    }

    void Wiggle()
    {
        // Runners have a more frantic wiggle to telegraph that they'll scuttle when a ball is near.
        var frequency = IsRunner ? 7f : 3.5f;
        var amplitude = IsRunner ? 22f : 12f;
        var angle = Mathf.Sin(Time.time * frequency + wigglePhase) * amplitude;
        transform.localRotation = Quaternion.Euler(0f, angle, 0f);
    }

    void Scuttle()
    {
        var nearest = FindNearestBallWithin(ScuttleTriggerDistance);

        Vector3 moveDir;
        float moveSpeed;
        if (nearest != null)
        {
            var away = transform.position - nearest.transform.position;
            away.y = 0;
            if (away.sqrMagnitude > 0.0001f)
            {
                wanderDirection = away.normalized;
            }
            moveDir = wanderDirection;
            moveSpeed = ScuttleSpeed;
        }
        else
        {
            if (Time.time > wanderRedirectAt)
            {
                wanderDirection = RandomHorizontalDirection();
                wanderRedirectAt = Time.time + Utility.GetRandomFloat(1.5f, 3.5f);
            }
            moveDir = wanderDirection;
            moveSpeed = ScuttleSpeed * 0.35f;
        }

        transform.position += moveDir * moveSpeed * Time.deltaTime;
        ClampToSpawnBounds();
    }

    GameObject FindNearestBallWithin(float maxDistance)
    {
        GameObject nearest = null;
        var nearestSqr = maxDistance * maxDistance;
        foreach (var ball in GameObject.FindGameObjectsWithTag("Ball"))
        {
            var d = ball.transform.position - transform.position;
            d.y = 0;
            var sqr = d.sqrMagnitude;
            if (sqr < nearestSqr) { nearestSqr = sqr; nearest = ball; }
        }
        return nearest;
    }

    Vector3 RandomHorizontalDirection()
    {
        var angle = Utility.GetRandomFloat(0f, Mathf.PI * 2f);
        return new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
    }

    void ClampToSpawnBounds()
    {
        if (ParentScript == null) return;
        ParentScript.GetSpawnBoundsX(out var minX, out var maxX);
        ParentScript.GetSpawnBoundsZ(out var minZ, out var maxZ);
        var pos = transform.position;
        var clampedX = Mathf.Clamp(pos.x, minX, maxX);
        var clampedZ = Mathf.Clamp(pos.z, minZ, maxZ);
        // If clamped, also nudge the wander direction so the bug doesn't pin itself against the wall.
        if (!Mathf.Approximately(clampedX, pos.x)) wanderDirection.x = -wanderDirection.x;
        if (!Mathf.Approximately(clampedZ, pos.z)) wanderDirection.z = -wanderDirection.z;
        transform.position = new Vector3(clampedX, pos.y, clampedZ);
    }

	// Update is called once per frame
    void OnTriggerEnter(Collider other)
    {
        if (!IsSquished)
        {
            if (SquishSounds.Any())
            {
                GetComponent<AudioSource>().PlayOneShot(SquishSounds[Utility.GetRandomInt(0, SquishSounds.Count)]);
            }
            transform.localScale = new Vector3(1.6f, 0.1f, 1.6f);
            transform.position = new Vector3(transform.position.x, transform.position.y*0.1f, transform.position.z);

            if (other.GetComponent<BallScript>())
            {
                other.GetComponent<BallScript>().squishSomething();
            }

            expireTime = Time.time + 5;
            IsSquished = true;

            var gooPosition = new Vector3(transform.position.x, transform.position.y - 0.2f, transform.position.z);
            var goo = Instantiate(Resources.Load("GooPrefab"), gooPosition, new Quaternion())
                        as GameObject;
            goo.transform.Rotate(0f, Utility.GetRandomFloat(0, 360), 0f);
            var kidMeshRenderers = goo.GetComponentsInChildren<MeshRenderer>();
            var materialName = slimeTypes[Utility.GetRandomInt(0, slimeTypes.Count)];
            var slimeMaterial = Resources.Load<Material>(materialName);
            foreach (var renderer in kidMeshRenderers)
            {
                renderer.material = slimeMaterial;
            }

            SpawnSquishBurst(gooPosition + Vector3.up * 0.4f, slimeMaterial);
            RequestCameraBump();
        }
    }

    static void SpawnSquishBurst(Vector3 position, Material slimeMaterial)
    {
        var go = new GameObject("SquishBurst");
        go.transform.position = position;
        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.duration = 0.4f;
        main.loop = false;
        main.startLifetime = 0.55f;
        main.startSpeed = 5f;
        main.startSize = 0.28f;
        main.gravityModifier = 1.5f;
        main.startColor = (slimeMaterial != null && slimeMaterial.HasProperty("_Color")) ? slimeMaterial.color : Color.green;
        main.stopAction = ParticleSystemStopAction.Destroy;

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 28) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Hemisphere;
        shape.radius = 0.25f;

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        var shader = Shader.Find("Sprites/Default") ?? Shader.Find("Unlit/Color") ?? Shader.Find("Standard");
        renderer.material = new Material(shader);

        ps.Play();
    }

    static void RequestCameraBump()
    {
        var mainCam = Camera.main;
        if (mainCam == null) return;
        var script = mainCam.GetComponent<CameraScript>();
        if (script != null) script.Bump(0.18f, 0.18f);
    }
}
