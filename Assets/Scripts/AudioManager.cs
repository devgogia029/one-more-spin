using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Sound Effects")]

    public AudioClip leverClip; //sound played when the lever is pulled
    public AudioClip reelStopClip; //sound played each time a reel stops
    public AudioClip winClip; //sound played when the player wins
    public AudioClip loseClip; //sound played when the player loses a spin
    public AudioClip gameOverClip; //sound played when the player runs out of coins

    [Header("Background Music")]

    public AudioClip musicClip; //looping background music that plays during the game

    [Header("Volume")]

    [Range(0f, 1f)] public float sfxVolume = 0.9f; //volume of the sound effects
    [Range(0f, 1f)] public float musicVolume = 0.1f; //volume of the background music; kept low so the sound effects stay clear

    private AudioSource sfxSource; //Audio Source that plays the short sound effects
    private AudioSource musicSource; //Audio Source that plays the background music

    void Awake()
    {
        sfxSource = gameObject.AddComponent<AudioSource>(); //creates an Audio Source for the sound effects so nothing has to be added by hand
        sfxSource.playOnAwake = false; //stops the effects source from playing anything by itself

        musicSource = gameObject.AddComponent<AudioSource>(); //creates a second Audio Source so the music and the effects never cut each other off
        musicSource.clip = musicClip; //gives the music source the background music clip
        musicSource.loop = true; //makes the music start again when it reaches the end
        musicSource.playOnAwake = false; //stops the music from starting by itself, so the Game Manager decides when it begins
        musicSource.volume = musicVolume; //sets the music volume
    }
    void OnValidate()
    {
        if (musicSource != null) //checks whether the music source has been created yet, because this method also runs in the editor before the game starts
            musicSource.volume = musicVolume; //updates the music volume straight away whenever the slider is moved
    }

    public void PlayMusic()
    {
        if (musicClip == null || musicSource.isPlaying) //checks whether there is no music clip or the music is already playing
            return; //stops the method so the music is never restarted or doubled

        musicSource.Play(); //starts the background music
    }

    public void PlayLever()
    {
        Play(leverClip); //plays the lever sound
    }

    public void PlayReelStop()
    {
        Play(reelStopClip); //plays the reel stop sound
    }

    public void PlayWin()
    {
        Play(winClip); //plays the win sound
    }

    public void PlayLose()
    {
        Play(loseClip); //plays the lose sound
    }

    public void PlayGameOver()
    {
        Play(gameOverClip); //plays the game over sound
    }

    void Play(AudioClip clip)
    {
        if (clip == null) //checks whether a clip has been assigned
            return; //does nothing so a missing sound never causes an error

        sfxSource.PlayOneShot(clip, sfxVolume); //plays the clip once without cutting off any sound that is already playing
    }
}