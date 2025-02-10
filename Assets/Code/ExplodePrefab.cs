using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ExplodePrefab : MonoBehaviour
{
    public Vector3 startPosition;
    public Quaternion startRotation;
    public Vector3 explodePosition;

    public bool correctlyPlaced = false;

    private XRGrabInteractable grabInteractable;

    public List<GameObject> gameObjects = new();
    public List<Material> defaultMaterials = new();
    public Material hoverMaterial;
    public Material grabbedMaterial;
    public Material hoverTargetMaterial;

    private bool hasMeshCollider = false;
    private BoxCollider boxCollider;
    private MeshCollider meshCollider;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        grabInteractable.hoverEntered.AddListener(OnHoverEnter);
        grabInteractable.hoverExited.AddListener(OnHoverExit);
        grabInteractable.selectEntered.AddListener(OnTriggerGrab);
        grabInteractable.selectExited.AddListener(OnTriggerRelease);
    }

    private void Start()
    {
        boxCollider = transform.GetComponent<BoxCollider>();

        if (boxCollider == null)
        {
            meshCollider = transform.GetComponent<MeshCollider>();
            hasMeshCollider = true;
        }

        startPosition = transform.position;
        startRotation = transform.rotation;
        explodePosition = startPosition + Random.onUnitSphere * 0.2f;

        defaultMaterials = GetComponentInChildren<Renderer>().materials.ToList();
        foreach (Transform tr in transform.GetComponentsInChildren<Transform>())
        {
            gameObjects.Add(tr.gameObject);
        }

        StartCoroutine(Explode());
    }

    private void OnHoverEnter(BaseInteractionEventArgs arg)
    {
        for (int i = 0; i < gameObjects.Count; i++)
        {
            gameObjects[i].GetComponent<Renderer>().material = hoverMaterial;
        }
    }

    private void OnHoverExit(BaseInteractionEventArgs arg)
    {
        for(int i = 0; i < gameObjects.Count; i++)
        {
            gameObjects[i].GetComponent<Renderer>().material = defaultMaterials[i];
        }
    }

    private void OnTriggerRelease(BaseInteractionEventArgs arg)
    {
        if(Vector3.Distance(transform.position, startPosition) < 0.1f)
        {
            transform.SetPositionAndRotation(startPosition, startRotation);
            if(!correctlyPlaced)
            {
                BuiltObject.instance.AttachToObject(gameObject);
                correctlyPlaced = true;
                grabInteractable.selectEntered.RemoveAllListeners();
                grabInteractable.selectExited.RemoveAllListeners();
            }
        }
        if (!hasMeshCollider)
        {
            boxCollider.isTrigger = false;
        }
        else
        {
            meshCollider.isTrigger = false;
        }
        for (int i = 0; i < gameObjects.Count; i++)
        {
            gameObjects[i].GetComponent<Renderer>().material = hoverMaterial;
        }
    }

    private void OnTriggerGrab(BaseInteractionEventArgs arg)
    {
        if (!hasMeshCollider)
        {
            boxCollider.isTrigger = true;
        }
        else
        {
            meshCollider.isTrigger = true;
        }
        for (int i = 0; i < gameObjects.Count; i++)
        {
            gameObjects[i].GetComponent<Renderer>().material = grabbedMaterial;
        }
    }

    IEnumerator Explode()
    {
        while (Vector3.Distance(transform.position, explodePosition) > Mathf.Epsilon)
        {
            float distanceToMove = Time.deltaTime * 1f;

            transform.position = Vector3.MoveTowards(transform.position, explodePosition, distanceToMove);

            yield return null;
        }
    }
}
