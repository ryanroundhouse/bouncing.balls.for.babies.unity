using System;
using System.Collections.Generic;
using Assets.Scripts;
using UnityEngine;
using System.Collections;

public class BallScript : MonoBehaviour
{
    public int Score;
    public GameObject BounceSoundManager;
    public int HitPower = 1500;
    public int BallIndex = 0;

    private BallGroupScript BallGroupScript;
    private Vector3 lastTouchPoint;
    private Vector3 lastClickPoint;

    void Start()
    {
        Score = 0;
        BallGroupScript = BounceSoundManager.GetComponent<BallGroupScript>();
    }

    public void squishSomething()
    {
        Score++;
        if (Score >= 5)
        {
            WinningInfoScript.WinningPlanet = name;
            WinningInfoScript.WinningPlanetIndex = BallIndex;
            Application.LoadLevel("VictoryScene");
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.tag.Equals("Floor"))
        {
            BallGroupScript.PlayBounceSound(collision.gameObject.tag.Equals("Ball"));
        }
    }

    void Touched(Vector3 touchPoint)
    {
        lastTouchPoint = touchPoint;
    }

    void MouseTouched(Vector3 clickPoint)
    {
        lastClickPoint = clickPoint;
    }

    void TouchEnded(bool push)
    {
        Vector3 forceDirection;
        if(!push)
        {
            forceDirection = lastTouchPoint - transform.position;
        }
        else
        {
            forceDirection = transform.position - lastClickPoint;
        }
        forceDirection.Normalize();

        rigidbody.velocity = Vector3.zero;
        rigidbody.AddForce(new Vector3(forceDirection.x, 0, forceDirection.z) * HitPower);
        lastClickPoint = Vector3.zero;
        lastTouchPoint = Vector3.zero;
    }
}
