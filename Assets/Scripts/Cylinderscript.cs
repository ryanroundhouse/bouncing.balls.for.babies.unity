using System.Collections.Generic;
using System.Linq;
using Assets.Scripts;
using UnityEngine;
using System.Collections;

public class Cylinderscript : MonoBehaviour 
{
    public List<AudioClip> SquishSounds;
    public SquishCylinderScript ParentScript;

    private bool IsSquished;
    private float expireTime;
    private Animation animator;
    private readonly List<string> slimeTypes = new List<string>{"green slime", "blue slime", "orange slime", "yellow slime"};

    void Start()
    {
        IsSquished = false;
        expireTime = -1;
        animator = GetComponent<Animation>();

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
            if (animator != null) animator.Stop();
            if (Time.time > expireTime)
            {
                ParentScript.RemoveCylinder(gameObject);
            }
        }
        else
        {
            if (animator != null && animator.clip != null && !animator.isPlaying)
            {
                animator.Play();
            }
        }
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
            var material = slimeTypes[Utility.GetRandomInt(0, slimeTypes.Count)];
            foreach (var renderer in kidMeshRenderers)
            {
                renderer.material = Resources.Load<Material>(material);
            }
        }
    }
}
