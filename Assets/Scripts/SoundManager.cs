using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class Sound 
{
    public string name;
    public AudioClip clip;

    [Range(0f, 1f)]

    private AudioSource source; 

    public void SetSource(AudioSource _source)
    {   
        source = _source;
        source.clip = clip;
    }

    public void Play(float volume)
    {
        source.volume = volume; 
        source.Play();
    }

    public void PlayPan(float volume, float xRatio)
    {
        source.volume = volume; 
        source.panStereo = xRatio;
        source.Play();
    }
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [Range(0f, 1f)]
    public float volumeSFX = 0.7f;

    [Range(0f, 1f)]
    public float volumeBGM = 0.7f;

    [SerializeField] private Sound[] sounds;
    private Dictionary<string, Sound> soundsDict = new Dictionary<string, Sound>();
    [SerializeField] private Sound[] collisionSounds;

    public AudioClip clipBGM;

    public Tilemap wall;
    private float wallEdgesXMin;
    private float wallEdgesXMax;

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
        for (int i = 0; i<collisionSounds.Length; i++)
        {
            GameObject _go = new GameObject("Sound_Collide_" + i);
            _go.transform.SetParent(this.transform);
            collisionSounds[i].name = "Collide_" + i;
            collisionSounds[i].SetSource(_go.AddComponent<AudioSource>());
        }
        
        GameObject _BGMgo = new GameObject("BGM");
        _BGMgo.transform.SetParent(this.transform);
        _BGMgo.AddComponent<AudioSource>();
        AudioSource BGMas = _BGMgo.GetComponent<AudioSource>();
        BGMas.clip = clipBGM;
        BGMas.loop = true;
        BGMas.Play();

        wallEdgesXMin = wall.origin.x;
        wallEdgesXMax = wall.origin.x + wall.size.x;
    }

    public void PlaySound(string _name)
    {
        soundsDict[_name].Play(volumeSFX);
    }

    public void PlayCollision(float xPos)
    {
        int randomIndex = Random.Range(0, collisionSounds.Length);
        float xRatio = 2 * ((xPos - wallEdgesXMin) / (wallEdgesXMax - wallEdgesXMin)) - 1;
        collisionSounds[randomIndex].PlayPan(volumeSFX, xRatio);
    }

}
