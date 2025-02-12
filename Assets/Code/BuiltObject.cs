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
    public Transform referenceGhostObject;

    public Material ghostMaterial;

    public List<Transform> interactableObjects = new();
    public List<GameObject> assembledObjects = new();
    public List<Transform> ghostObjects = new();

    private XRGrabInteractable grabInteractable;

    public Vector3 startPosition;
    public Quaternion startRotation;

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }

        CreateReferenceList(referenceObj, false);

        referenceGhostObject = Instantiate(referenceObj, referenceObj.transform.position, referenceObj.transform.rotation);

        CreateReferenceList(referenceGhostObject, true);

        grabInteractable = GetComponent<XRGrabInteractable>();

        grabInteractable.hoverEntered.AddListener(OnHoverEnter);
        grabInteractable.hoverExited.AddListener(OnHoverExit);
        grabInteractable.selectEntered.AddListener(OnTriggerGrab);
        grabInteractable.selectExited.AddListener(OnTriggerRelease);
    }

    private void Start()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    private void CreateReferenceList(Transform obj, bool isGhost)
    {
        foreach (Transform child in obj)
        {
            if (child.GetComponent<XRGrabInteractable>() != null)
            {
                if(!isGhost)
                {
                    interactableObjects.Add(child);
                }
                else
                {
                    ghostObjects.Add(child);

                    foreach (Renderer r in child.GetComponentsInChildren<Renderer>())
                    {
                        Material[] tempM = r.materials;
                        for (int j = 0; j < tempM.Length; j++)
                        {
                            tempM[j] = ghostMaterial;
                        }
                        r.materials = tempM;
                    }

                    Destroy(child.GetComponent<XRGrabInteractable>());
                    Destroy(child.GetComponent<ExplodePrefab>());
                    Destroy(child.GetComponent<Rigidbody>());
                    Destroy(child.GetComponent<MeshCollider>());
                    Destroy(child.GetComponent<BoxCollider>());
                }
            }
        }
    }

    private void OnTriggerRelease(SelectExitEventArgs arg0)
    {
        transform.SetPositionAndRotation(startPosition, startRotation);
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

    public void AttachToObject(GameObject newObject)
    {
        grabInteractable.colliders.Add(newObject.GetComponent<Collider>());
        newObject.transform.parent = BuiltObj;
        assembledObjects.Add(newObject);

        if (assembledObjects.Count == interactableObjects.Count)
        {
            Debug.Log("Assembled!");
        }
    }
}
