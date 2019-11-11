using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetachGameObject : MonoBehaviour
{
    public enum DetachPoint { OnAwake, OnStart }

    public DetachPoint detachPoint;
    
    void Awake()
    {
        if (detachPoint == DetachPoint.OnAwake)
            Detach();
    }

    void Start()
    {
        if (detachPoint == DetachPoint.OnStart)
            Detach();
    }

    private void Detach()
    {
        transform.parent = null;
        Destroy(this);
    }
}