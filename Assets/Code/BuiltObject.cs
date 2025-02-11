using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using static Unity.VisualScripting.Metadata;

public class BuiltObject : MonoBehaviour
{
    public static BuiltObject instance;

    public Transform BuiltObj;
    public Transform referenceObj;
    public List<Transform> interactableObjects;

    public List<GameObject> assembledObjects = new();

    private XRGrabInteractable grabInteractable;

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }

        foreach (Transform child in referenceObj)
        {
            if (child.GetComponent<XRGrabInteractable>() != null)
            {
                interactableObjects.Add(child);
            }
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

    public void ExplodeButton()
    {
        ResetButton();
        foreach (Transform t in interactableObjects)
        {
            t.GetComponent<ExplodePrefab>().Explode();
        }
    }

    public void ResetButton()
    {
        foreach (Transform t in interactableObjects)
        {
            t.GetComponent<ExplodePrefab>().Reset();
            t.SetParent(referenceObj);
        }
    }

    // Update is called once per frame
    public void AttachToObject(GameObject newObject)
    {
        grabInteractable.colliders.Add(newObject.GetComponent<Collider>());
        newObject.transform.parent = BuiltObj;
        assembledObjects.Add(newObject);

        if (assembledObjects.Count == interactableObjects.Count)
        {
            // Assembly complete
        }
    }
}
