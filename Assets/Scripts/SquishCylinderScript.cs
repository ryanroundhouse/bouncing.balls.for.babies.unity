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
	            squishCylinder.GetComponent<Cylinderscript>().ParentScript = this;
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

    private float GetTimeSinceLastRestart()
    {
        return Time.time - TimeSinceLastRestart;
    }
}
