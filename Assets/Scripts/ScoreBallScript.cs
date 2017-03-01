using Assets.Scripts;
using UnityEngine;
using System.Collections;

public class ScoreBallScript : MonoBehaviour
{
    public GameObject RelatedBall;
    public int HitPower = 1500;

	// Use this for initialization
    void Touched(Vector3 clickPoint) 
    {
        ApplyRandomForceToRelatedBall();
	}

    void MouseTouched(Vector3 clickPoint)
    {
        ApplyRandomForceToRelatedBall();
    }

    void ApplyRandomForceToRelatedBall()
    {

        var randomForce = new Vector3(Utility.GetRandomFloat(0.0f, 1.0f), 0, Utility.GetRandomFloat(0.0f, 1.0f));
        randomForce.Normalize();
        RelatedBall.rigidbody.velocity = new Vector3(0, 0, 0);

        RelatedBall.rigidbody.AddForce(new Vector3(randomForce.x, 0, randomForce.z) * HitPower);
    }
}
