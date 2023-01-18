using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PhysicsEventTrigger : MonoBehaviour
{
    /// <summary>
    /// Events which can invoke this trigger.
    /// </summary>
    public enum PhysicsEvent
    {
        Undefined,
        OnTriggerEnter,
        OnTriggerStay,
        OnTriggerExit,
    }

    /// <summary>
    /// The event which invokes this trigger.
    /// </summary>
    [SerializeField]
    private PhysicsEvent physicsEvent = PhysicsEvent.Undefined;

    [Space]

    /// <summary>
    /// The methods invoked when this trigger is invoked.
    /// </summary>
    [SerializeField]
    private UnityEvent unityEvent = new UnityEvent();

    /// <summary>
    /// Unity event.
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        if (physicsEvent == PhysicsEvent.OnTriggerEnter)
        {
            unityEvent.Invoke();
        }
    }

    /// <summary>
    /// Unity event.
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerStay(Collider other)
    {
        if (physicsEvent == PhysicsEvent.OnTriggerStay)
        {
            unityEvent.Invoke();
        }
    }

    /// <summary>
    /// Unity event.
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerExit(Collider other)
    {
        if (physicsEvent == PhysicsEvent.OnTriggerExit)
        {
            unityEvent.Invoke();
        }
    }
}
