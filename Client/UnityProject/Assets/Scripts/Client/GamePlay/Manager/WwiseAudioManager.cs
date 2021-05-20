using AK.Wwise;
using BiangLibrary.Singleton;
using UnityEngine;
using Event = AK.Wwise.Event;

public class WwiseAudioManager : MonoSingleton<WwiseAudioManager>
{
    public WwiseBGMConfiguration WwiseBGMConfiguration;

    public void ShutDown()
    {
        WwiseBGMConfiguration.BGM_Stop();
    }

    public RTPC InMenu_LowPass;
    public RTPC Master_Volume;
    public RTPC Music_Volume;
    public RTPC UI_Volume;
    public RTPC World_Volume;

    void Awake()
    {
    }

    public Trigger Trigger_PlayerDeath;
    public Trigger Trigger_QuestComplete;
    public Trigger Trigger_Teleport;
    public Trigger Trigger_Victory;

    public Event UI_ButtonClick;
    public Event UI_ButtonHover;
    public Event KeyBoxInLock;
    public Event LockBoxUnlocked;
    public Event BoxTypeChange;
    public Event RewardBoxShow;
    public Event TransportBoxShow;

    public enum CommonAudioEvent
    {
        UI_START = 0,

        UI_ButtonClick,
        UI_ButtonHover,

        BOX_START = 1000,

        KeyBoxInLock,
        LockBoxUnlocked,
        BoxTypeChange,
        RewardBoxShow,
        TransportBoxShow,

        ACTOR_START = 2000,
    }

    public void PlayCommonAudioSound(CommonAudioEvent commonAudioEvent, GameObject sourceGameObject)
    {
        switch (commonAudioEvent)
        {
            case CommonAudioEvent.UI_ButtonClick:
            {
                UI_ButtonClick?.Post(sourceGameObject);
                break;
            }
            case CommonAudioEvent.UI_ButtonHover:
            {
                UI_ButtonHover?.Post(sourceGameObject);
                break;
            }
            case CommonAudioEvent.KeyBoxInLock:
            {
                KeyBoxInLock?.Post(sourceGameObject);
                break;
            }
            case CommonAudioEvent.LockBoxUnlocked:
            {
                LockBoxUnlocked?.Post(sourceGameObject);
                break;
            }
            case CommonAudioEvent.BoxTypeChange:
            {
                BoxTypeChange?.Post(sourceGameObject);
                break;
            }
            case CommonAudioEvent.RewardBoxShow:
            {
                RewardBoxShow?.Post(sourceGameObject);
                break;
            }
            case CommonAudioEvent.TransportBoxShow:
            {
                TransportBoxShow?.Post(sourceGameObject);
                break;
            }
        }
    }
}