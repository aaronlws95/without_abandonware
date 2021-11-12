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

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [Range(0f, 1f)]
    public static float volumeSFX = 1.0f;

    [Range(0f, 1f)]
    public static float volumeBGM = 1.0f;

    [SerializeField] private Sound[] sounds;
    private Dictionary<string, Sound> soundsDict = new Dictionary<string, Sound>();

    public AudioClip clipBGM;

    static GameObject _BGMgo;
    static AudioSource bgmAS;

    static string sceneName = "";

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

        string curSceneName = SceneManager.GetActiveScene().name;
        if (sceneName != curSceneName)
        {
            bgmAS.clip = clipBGM;
            bgmAS.loop = true;
            bgmAS.volume = volumeBGM;
            bgmAS.Play();
            sceneName = curSceneName;
        }
    }

    void Update()
    {
        bgmAS.volume = volumeBGM;
    }

    public void PlaySound(string _name)
    {
        soundsDict[_name].Play(volumeSFX);
    }
}
