using System.Collections.Generic;
using UnityEngine;

public class GameSoundManager : MonoBehaviour
{
    public static GameSoundManager Instance;

    [Header("General Settings")]
    [Range(0f, 1f)] public float randomHornChance = 0.05f;
    public float minHornDelay = 2f;

    [Header("Car Sounds")]
    public AudioClip carEngineLoop;
    public AudioClip carHorn;
    public AudioClip carHit;

    [Header("Train Sounds")]
    public AudioClip trainPass;
    public AudioClip trainWarning;
    public AudioClip trainCrush;

    [Header("River Sounds")]
    public AudioClip riverLoop;
    public AudioClip splash;

    private float lastHornTime;

    public List<AudioSource> activeLoops = new List<AudioSource>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Play3DClipAtPoint(AudioClip clip, Vector3 pos, float volume = 1f)
    {
        if (clip == null) return;

        GameObject go = new GameObject("TempAudio");
        go.transform.position = pos;

        AudioSource src = go.AddComponent<AudioSource>();
        src.clip = clip;
        src.spatialBlend = 1f;
        src.volume = volume;
        src.minDistance = 1f;
        src.maxDistance = 30f;
        src.dopplerLevel = 0f;
        src.rolloffMode = AudioRolloffMode.Linear;

        src.Play();
        Destroy(go, clip.length);
    }
    public AudioSource AddLoopToObject(GameObject target, AudioClip clip, bool playNow = true, float volume = 1f, bool isStripAmbient = false)
    {
        if (target == null || clip == null) return null;

        GameObject audioHolder = target;

        if (isStripAmbient)
        {
            audioHolder = new GameObject("AudioEmitter");
            audioHolder.transform.SetParent(target.transform);
            audioHolder.transform.localPosition = Vector3.zero;
            audioHolder.AddComponent<FollowListenerInX>();
        }

        AudioSource existing = audioHolder.GetComponent<AudioSource>();
        if (existing != null && existing.clip == clip)
            return existing;

        AudioSource source = audioHolder.AddComponent<AudioSource>();
        source.clip = clip;
        source.loop = true;
        source.volume = volume;
        source.spatialBlend = 1f;
        source.minDistance = 1f;
        source.maxDistance = 40f;
        source.dopplerLevel = 0f;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.playOnAwake = false;

        if (playNow)
            source.Play();

        if (isStripAmbient)
        {
            source.spread = 180f;
        }

        activeLoops.Add(source);
        return source;
    }

    public void RemoveLoop(AudioSource source)
    {
        if (source != null)
        {
            activeLoops.Remove(source);

            if (source.gameObject.name == "AudioEmitter")
            {
                Destroy(source.gameObject);
            }
            else
            {
                Destroy(source);
            }
        }
    }

    private void OnDestroy()
    {
        foreach (var s in activeLoops)
        {
            if (s != null)
                Destroy(s);
        }
        activeLoops.Clear();
    }


    public void PlayCarHorn(Vector3 pos)
    {
        if (Time.time - lastHornTime < minHornDelay)
            return;

        if (Random.value < randomHornChance)
        {
            lastHornTime = Time.time;
            Play3DClipAtPoint(carHorn, pos);
        }
    }



    public void PlayCarHit(Vector3 pos)
    {
        Play3DClipAtPoint(carHit, pos);
    }



    public void PlayTrainPass(Vector3 pos)
    {
        Play3DClipAtPoint(trainPass, pos);
    }

    public void PlayTrainWarning(Vector3 pos)
    {
        Play3DClipAtPoint(trainWarning, pos);
    }

    public void PlayTrainCrush(Vector3 pos)
    {
        Play3DClipAtPoint(trainCrush, pos);
    }




    public void PlaySplash(Vector3 pos)
    {
        Play3DClipAtPoint(splash, pos);
    }
}