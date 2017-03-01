using System.Collections.Generic;
using Assets.Scripts;
using UnityEngine;
using System.Collections;

public class BallGroupScript : MonoBehaviour
{
    public List<AudioClip> BounceSounds;
    private bool BallHitWasLast;

    public void PlayBounceSound(bool isBallAgainstBallBounce)
    {
        if(BounceSounds != null)
        {
            if (isBallAgainstBallBounce && !BallHitWasLast)
            {
                audio.PlayOneShot(BounceSounds[Utility.GetRandomInt(0, BounceSounds.Count)]);
                BallHitWasLast = true;
            }
            else if (!isBallAgainstBallBounce)
            {
                audio.PlayOneShot(BounceSounds[Utility.GetRandomInt(0, BounceSounds.Count)]);
            }
            else
            {
                BallHitWasLast = false;
            }
        }
    }
}
