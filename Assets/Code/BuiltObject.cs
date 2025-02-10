using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class BuiltObject : MonoBehaviour
{
    public static BuiltObject instance;

    public Transform BuiltObj;
    public List<GameObject> assembledObjects = new();

    private XRGrabInteractable grabInteractable;

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }

        grabInteractable = GetComponent<XRGrabInteractable>();
        grabInteractable.hoverEntered.AddListener(OnHoverEnter);
        grabInteractable.hoverExited.AddListener(OnHoverExit);
        grabInteractable.selectEntered.AddListener(OnTriggerGrab);
        grabInteractable.selectExited.AddListener(OnTriggerRelease);
    }

    private void OnTriggerRelease(SelectExitEventArgs arg0)
    {
        
    }

    private void OnTriggerGrab(SelectEnterEventArgs arg0)
    {
        foreach (GameObject go in assembledObjects)
        {
            if(go.GetComponent<BoxCollider>() != null)
            {
                go.GetComponent<BoxCollider>().isTrigger = true;
            }
            else
            {
                if(go.GetComponent<MeshCollider>() != null)
                {
                    go.GetComponent<MeshCollider>().isTrigger = true;
                }
            }
        }
    }

    private void OnHoverExit(HoverExitEventArgs arg0)
    {
        
    }

    private void OnHoverEnter(HoverEnterEventArgs arg0)
    {
        
    }

    // Update is called once per frame
    public void AttachToObject(GameObject newObject)
    {
        newObject.transform.parent = BuiltObj;
        assembledObjects.Add(newObject);
    }
}
