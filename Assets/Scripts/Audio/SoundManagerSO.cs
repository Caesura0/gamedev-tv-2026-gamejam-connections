using UnityEngine;

[CreateAssetMenu(fileName = "NewSoundManager", menuName = "Audio/SoundManager")]

public class SoundManagerSO :  ScriptableObject
{
    [Header("Footstep Sounds")]

    public AudioClip[] footstepOnGrassSoundArray;
    public AudioClip[] footstepOnStoneSoundArray;
    public AudioClip[] footstepOnWoodSoundArray;
    public AudioClip[] footstepOnMetalSoundArray;
    public AudioClip[] footstepOnWaterSoundArray;
    //public AudioClip[] footstepOnSandSoundArray;
    public AudioClip[] footstepOnDirtSoundArray;
    //public AudioClip[] footstepOnSnowSoundArray;
    public AudioClip[] footstepOnLeavesSoundArray;
    public AudioClip[] twigSnaps;

    [Space]
    
    [Header("Boulder Sounds")]
    public AudioClip[] slideRock;
    public AudioClip[] rockSplash;
    
    [Space]

    [Header("Pressure Plate Sounds")]
    public AudioClip[] pressurePlatePushed;
    public AudioClip[] pressurePlateReleased;

    [Space]

    [Header("Runestone Sounds")]
    public AudioClip[] rotateRune;
    public AudioClip[] runeActivated;

    [Space]

    [Header("Door Sounds")]
    public AudioClip[] doorOpen;
    public AudioClip[] doorClose;
    public AudioClip[] wallSink;
    public AudioClip[] stoneRaised;

    [Space]

    [Header("Environmental Loops")]
    public AudioClip[] windLoop;
    public AudioClip[] waterLoop;
    public AudioClip[] thunderLoop;
    public AudioClip[] rainLoop;
    public AudioClip[] foilageLoop;
    public AudioClip[] fireLoop;
    public AudioClip[] bugsLoop;
    public AudioClip[] ambienceLoop;

    [Space]

    [Header("UI Sounds")]
    //public AudioClip hitSound;
    //public AudioClip pickupPointsSound;
    //public AudioClip pickupBigPointsSound;
    //public AudioClip pickupSpeedSound;
    //public AudioClip jumpSound;

    [Space]
    [Space]
    [Space]

    public AudioClip startGame;
    public AudioClip buttonClick;
    public AudioClip switchButtonClick;
    public AudioClip closeClick;
    public AudioClip invalidClick;
    public AudioClip pauseClick;
    public AudioClip resumeClick;
    public AudioClip changeVolumeClick;

    [Space]
    [Space]
    [Space]

    //win lose sounds
    //public AudioClip loseSound;
    //public AudioClip winSound;
    //public AudioClip lowTimeAlertSound;
    //public AudioClip addTimeSound;

    [Header("Music")]
    public AudioClip introMusic;
    public AudioClip mainMenuMusic;
    public AudioClip gameplayMusic;
    public AudioClip gameEndMusic;
    public AudioClip scoreScreenMusic;




}
