using System;
using UnityEngine;

public class GameEventListener : MonoBehaviour
{

    [Header("These triggers are what 'success' looks like")]
    [Header("False means it must be unpowered to trigger 'succeed'")]
    [Header("True means it must be powered to trigger 'succeed'")]
    [Space]
    [Tooltip("Only set if you have PressurePlateGroupListenerTrigger on this game object and it is needed")]
    [SerializeField] bool isPressurePlateNeededToBeActive;
    [Space]
    [Tooltip("Only set if you have isRuneReceiverNeededToBeActive on this game object and it is needed")]
    [SerializeField] bool isRuneReceiverNeededToBeActive;


    private PressurePlateGroupListenerTrigger pressurePlateTrigger;
    bool isPressurePlateObjectiveActive = false;

    private RuneReceiverGroupListenerTrigger runeReceiverTrigger;
    bool isRuneReceiverObjectiveActive = false;

    public event System.Action<bool> OnFullConditionMet;

    private void Awake()
    {
        SubscribeToEvents();
        
        
    }

    private void SubscribeToEvents()
    {
        if (TryGetComponent(out pressurePlateTrigger))
        {
            isPressurePlateObjectiveActive = !isPressurePlateNeededToBeActive; // If not needed, consider it active by default
            pressurePlateTrigger.OnGroupStateChanged += PressurePlateTrigger_OnGroupStateChanged; 
        }
        if (TryGetComponent(out runeReceiverTrigger))
        {
            isRuneReceiverObjectiveActive = !isRuneReceiverNeededToBeActive; // If not needed, consider it active by default
            runeReceiverTrigger.OnGroupStateChanged += RuneReceiverTrigger_OnGroupStateChanged;
        }

    }

    private void RuneReceiverTrigger_OnGroupStateChanged(bool obj)
    {
        isRuneReceiverObjectiveActive = obj;
        CheckFullCondition();
    }



    private void PressurePlateTrigger_OnGroupStateChanged(bool obj)
    {
        isPressurePlateObjectiveActive = obj;
        CheckFullCondition();
    }

    private void CheckFullCondition()
    {
        if (isPressurePlateObjectiveActive == isPressurePlateNeededToBeActive
            && isRuneReceiverObjectiveActive == isRuneReceiverNeededToBeActive) 
        {
            OnFullConditionMet?.Invoke( true);
        }
        else
        {
            OnFullConditionMet?.Invoke(false);
        }
    }

    private void OnDestroy()
    {
        if (pressurePlateTrigger != null)
            pressurePlateTrigger.OnGroupStateChanged += PressurePlateTrigger_OnGroupStateChanged;
        if (runeReceiverTrigger != null)
            runeReceiverTrigger.OnGroupStateChanged += RuneReceiverTrigger_OnGroupStateChanged;

    }


}
