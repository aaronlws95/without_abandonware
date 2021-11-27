using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class Sound 
{
    public string name;
    public AudioClip clip;
    private AudioSource source; 
    [Range(0f, 1f)]
    public float volume = 1.0f;

    public void SetSource(AudioSource _source)
    {   
        source = _source;
        source.clip = clip;
    }

    public void Play(float _volume)
    {
        source.volume = _volume*volume; 
        source.Play();
    }
}


[System.Serializable]
public class BGM
{
    public AudioClip clip;
    public int startLevel;
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    public static float volumeSFX = 1.0f;

    public static float volumeBGM = 1.0f;

    [Range(0f, 1f)]
    public float baseVolumeBGM = 1.0f;

    [SerializeField] private Sound[] sounds;
    private Dictionary<string, Sound> soundsDict = new Dictionary<string, Sound>();

    public List<BGM> bgmClips;

    static GameObject _BGMgo;
    static AudioSource bgmAS;
    public static int curBGMidx = 0;
    public static int nextLevelBGM;

    void Awake()
    {
        if (instance != null)
        {
            Debug.LogError ("More than one SoundManager in the scene.");
        }
        else 
        {
            instance = this;
        }
    }
    
    void Start()
    {
        for (int i = 0; i<sounds.Length; i++)
        {
            GameObject _go = new GameObject("Sound_" + i + "_" + sounds[i].name);
            _go.transform.SetParent(this.transform);
            sounds[i].SetSource(_go.AddComponent<AudioSource>());
            soundsDict[sounds[i].name] = sounds[i];
        }

        if (GameObject.Find("BGM") == null)
        {
            _BGMgo = new GameObject("BGM");
            bgmAS = _BGMgo.AddComponent<AudioSource>();
            DontDestroyOnLoad(_BGMgo);
        }
    }

    public void nextBGM()
    {
        int nextBGMidx = curBGMidx + 1;
        if (nextBGMidx < bgmClips.Count)
        {
            nextLevelBGM = bgmClips[nextBGMidx].startLevel;
            curBGMidx += 1;
        } 
        else 
        {
            nextLevelBGM = 1000; // hack
        }
    }

    public void playBGM(int idx)
    {
        bgmAS.clip = bgmClips[idx].clip;
        bgmAS.loop = true;
        bgmAS.volume = baseVolumeBGM*volumeBGM;
        bgmAS.Play();
    }

    void Update()
    {
        bgmAS.volume = baseVolumeBGM*volumeBGM;
    }

    public void PlaySound(string _name)
    {
        if (soundsDict.ContainsKey(_name))
        {
            soundsDict[_name].Play(volumeSFX);
        }
    }
}
