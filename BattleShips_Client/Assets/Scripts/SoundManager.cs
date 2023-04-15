using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    private AudioSource _audioSource;
    [SerializeField] private AudioClip treasure;
    [SerializeField] private AudioClip docking;
    [SerializeField] private AudioClip joining;
    [SerializeField] private AudioClip wind;

    public enum Sound 
    {
        Treasure,
        Docking, 
        Joining, 
        Wind,
    }

    private static Sound _soundType;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            _audioSource = gameObject.GetComponent<AudioSource>();
        }
        else
        {
            Debug.LogError("There are more then one sound manager!");
        }
    }

    public void PlaySound(Sound type)
    {
        switch (type)
        {
            case Sound.Treasure:
                _audioSource.PlayOneShot(treasure);  
                break;
            case Sound.Docking:
                _audioSource.PlayOneShot(docking);  
                break;
            case Sound.Joining:
                _audioSource.PlayOneShot(joining);  
                break;
            case Sound.Wind:
                _audioSource.PlayOneShot(wind);  
                break;
        }
    }
}
