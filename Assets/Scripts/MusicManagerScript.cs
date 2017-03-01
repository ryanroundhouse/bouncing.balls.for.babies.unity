using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;

public class MusicManagerScript : MonoBehaviour
{
    private List<AudioSource> sources;

    void Start()
    {
        sources = GetComponents<AudioSource>().ToList();
    }

    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
    }

    public void PlayMusic(Music music)
    {
        for(var source = 0; source < sources.Count; source++)
        {
            if(source == (int)music)
            {
                if(!sources[source].isPlaying)
                {
                    if (source == (int)Music.Gameplay)
                    {
                        sources[(int) Music.Gameplay].timeSamples = 350000;
                    }
                    sources[source].Play();
                }
            }
            else
            {
                sources[source].Stop();
            }
        }
    }
}

public enum Music
{
    Gameplay = 0,
    Victory = 1
}