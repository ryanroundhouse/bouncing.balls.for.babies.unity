using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class VictoryBallScript : MonoBehaviour
{
    public List<Material> PotentialMaterials;

    private const float ScaleRate = 0.01f;

    // Use this for initialization
	void Start ()
	{
	    GetComponent<MeshRenderer>().material = PotentialMaterials[WinningInfoScript.WinningPlanetIndex];
	}
	
	// Update is called once per frame
	void Update ()
	{
	    RotateVictoryBall();
        transform.position = new Vector3(Mathf.PingPong(Time.time * 5, 16), Mathf.PingPong(Time.time * 3, 6.5f), transform.position.z);
	}

    private void ApplyScaleRate()
    {
        transform.localScale += Vector3.one * ScaleRate;
    }

    private void RotateVictoryBall()
    {
        transform.Rotate(Vector3.down * Time.deltaTime * 100);
        transform.Rotate(Vector3.forward * Time.deltaTime * 100);
    }
}
