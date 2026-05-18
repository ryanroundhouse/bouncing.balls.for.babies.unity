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
        foreach (var s in sources)
        {
            s.spatialBlend = 0f;
        }
        Debug.Log($"[MusicManager] Start: found {sources.Count} AudioSource(s)");
        for (var i = 0; i < sources.Count; i++)
        {
            var s = sources[i];
            Debug.Log($"[MusicManager]   [{i}] clip={(s.clip != null ? s.clip.name : "<null>")} volume={s.volume} mute={s.mute} enabled={s.enabled} playOnAwake={s.playOnAwake} spatialBlend={s.spatialBlend}");
        }
    }

    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
    }

    public void SetPitchScale(float scale)
    {
        if (sources == null) return;
        var clamped = Mathf.Clamp(scale, 0.5f, 2.0f);
        foreach (var s in sources) s.pitch = clamped;
    }

    public void PlayMusic(Music music)
    {
        Debug.Log($"[MusicManager] PlayMusic({music}) called; sources={(sources == null ? -1 : sources.Count)}");
        if (sources == null) return;
        // Victory should always play at the natural pitch — the gameplay pitch ramp shouldn't leak into the win sting.
        if (music == Music.Victory) SetPitchScale(1f);
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
                    Debug.Log($"[MusicManager]   Play() on [{source}] clip={(sources[source].clip != null ? sources[source].clip.name : "<null>")} isPlaying={sources[source].isPlaying}");
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