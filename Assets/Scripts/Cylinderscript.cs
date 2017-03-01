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
    }

    void Update()
    {
        if (IsSquished)
        {
            animator.Stop("idle");
            if (Time.time > expireTime)
            {
                ParentScript.RemoveCylinder(gameObject);
            }
        }
        else
        {
            if (!animator.IsPlaying("idle"))
            {
                animator.Play("idle");
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
                audio.PlayOneShot(SquishSounds[Utility.GetRandomInt(0, SquishSounds.Count)]);
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
