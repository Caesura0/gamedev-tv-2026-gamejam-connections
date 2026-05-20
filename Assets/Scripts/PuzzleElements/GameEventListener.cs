using System;
using UnityEngine;

public class GameEventListener : MonoBehaviour
{
    [SerializeField] bool isPressurePlateNeeded;
    [SerializeField] bool isRuneReceiverNeeded;


    private PressurePlateGroup pressurePlateTrigger;
    bool isPressurePlateObjectiveActive = false;

    private RuneReceiverGroup runeReceiverTrigger;
    bool isRuneReceiverObjectiveActive = false;


    private void Awake()
    {
        SubscribeToEvents();
    }

    private void SubscribeToEvents()
    {
        if (TryGetComponent(out pressurePlateTrigger))
        {
            pressurePlateTrigger.OnGroupStateChanged += PressurePlateTrigger_OnGroupStateChanged; 
        }
        if (TryGetComponent(out runeReceiverTrigger))
        {
            runeReceiverTrigger.OnGroupStateChanged += RuneReceiverTrigger_OnGroupStateChanged;
        }

    }

    private void RuneReceiverTrigger_OnGroupStateChanged(bool obj)
    {
        Debug.Log($"[GameEventListener] Rune receiver group state changed: {(obj ? "Activated" : "Deactivated")}");
    }

    private void PressurePlateTrigger_OnGroupStateChanged(bool obj)
    {
       Debug.Log($"[GameEventListener] Pressure plate group state changed: {(obj ? "Activated" : "Deactivated")}");
    }



    private void OnDestroy()
    {
        if (pressurePlateTrigger != null)
            pressurePlateTrigger.OnGroupStateChanged += PressurePlateTrigger_OnGroupStateChanged;
        if (runeReceiverTrigger != null)
            runeReceiverTrigger.OnGroupStateChanged += RuneReceiverTrigger_OnGroupStateChanged;

    }


}
