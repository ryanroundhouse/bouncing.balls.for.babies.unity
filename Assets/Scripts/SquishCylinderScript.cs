using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Assets.Scripts;
using UnityEngine;
using System.Collections;

public class SquishCylinderScript : MonoBehaviour
{
    public Vector3 TopLeftSpawnLocation;
    public Vector3 BottomRightSpawnLocation;
    public float RunnerSpawnChance = 0.35f;
    private List<GameObject> SquishCylinders;
    private const int DelayBeforeSpawningBugs = 2;
    private float TimeSinceLastRestart;

	// Use this for initialization
	void Start () 
    {
        SquishCylinders = new List<GameObject>();
        TimeSinceLastRestart = Time.time;
	}
	
	// Update is called once per frame
	void Update ()
	{
	    if (GetTimeSinceLastRestart() > DelayBeforeSpawningBugs)
	    {
	        if (SquishCylinders.Count < 3 || Input.GetKey(KeyCode.Space))
	        {
	            Vector3 position;
	            do
	            {
	                position = new Vector3(Utility.GetRandomFloat(TopLeftSpawnLocation.x, TopLeftSpawnLocation.z),
	                    1.6f,
	                    Utility.GetRandomFloat(BottomRightSpawnLocation.x, BottomRightSpawnLocation.z));
	            } while (Physics.OverlapSphere(position, 1.5f).Any());

	            var squishCylinder =
	                Instantiate(Resources.Load("BugPrefab"), position, new Quaternion())
	                    as GameObject;
	            var cylinderScript = squishCylinder.GetComponent<Cylinderscript>();
	            cylinderScript.ParentScript = this;
	            cylinderScript.IsRunner = Utility.GetRandomFloat(0f, 1f) < RunnerSpawnChance;
	            SquishCylinders.Add(squishCylinder);
	        }
	    }
	}

    public void RemoveCylinder(GameObject cylinder)
    {
        if (SquishCylinders.Contains(cylinder))
        {
            SquishCylinders.Remove(cylinder);
            Destroy(cylinder);
        }
    }

    // Spawn-rectangle bounds for the arena. Note the original spawn samples X from
    // TopLeftSpawnLocation.{x,z} and Z from BottomRightSpawnLocation.{x,z} — these helpers
    // expose the same min/max so runners can be clamped to where they're allowed to be.
    public void GetSpawnBoundsX(out float min, out float max)
    {
        min = Mathf.Min(TopLeftSpawnLocation.x, TopLeftSpawnLocation.z);
        max = Mathf.Max(TopLeftSpawnLocation.x, TopLeftSpawnLocation.z);
    }

    public void GetSpawnBoundsZ(out float min, out float max)
    {
        min = Mathf.Min(BottomRightSpawnLocation.x, BottomRightSpawnLocation.z);
        max = Mathf.Max(BottomRightSpawnLocation.x, BottomRightSpawnLocation.z);
    }

    private float GetTimeSinceLastRestart()
    {
        return Time.time - TimeSinceLastRestart;
    }
}
