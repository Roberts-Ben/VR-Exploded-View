using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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

    public Renderer[] allRenderers;

    public List<Material> defaultMaterials = new();
    public Material hoverMaterial;
    public Material grabbedMaterial;
    public Material hoverTargetMaterial;
    public Material ghostMaterial;

    private bool hasMeshCollider = false;
    private BoxCollider boxCollider;
    private MeshCollider meshCollider;

    private int objIndex;
    public Renderer targetGhostRenderer;

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
        boxCollider = transform.GetComponent<BoxCollider>(); // Try GetCollider?

        if (boxCollider == null)
        {
            meshCollider = transform.GetComponent<MeshCollider>();
            hasMeshCollider = true;
        }

        startPosition = transform.position;
        startRotation = transform.rotation;

        defaultMaterials = GetComponentInChildren<Renderer>().materials.ToList();

        allRenderers = GetComponentsInChildren<Renderer>();

        objIndex = BuiltObject.instance.interactableObjects.IndexOf(this.transform);
        targetGhostRenderer = BuiltObject.instance.ghostObjects[objIndex].GetComponent<Renderer>();
    }

    private void Update()
    {
        Vector2 input = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
        Vector3 rotationVector = 1f * Time.deltaTime * new Vector3(0, input.x, input.y);
        transform.Rotate(rotationVector, Space.Self);
    }

    private void OnHoverEnter(BaseInteractionEventArgs arg)
    {
        for(int i = 0; i < allRenderers.Length; i++)
        {
            Renderer tempR = allRenderers[i];
            Material[] tempM = tempR.materials;
            for(int j = 0; j < tempM.Length; j++)
            {
                tempM[j] = hoverMaterial;
            }
            tempR.materials = tempM;
        }
    }

    private void OnHoverExit(BaseInteractionEventArgs arg)
    {
        for (int i = 0; i < allRenderers.Length; i++)
        {
            Renderer tempR = allRenderers[i];
            Material[] tempM = tempR.materials;
            for (int j = 0; j < tempM.Length; j++)
            {
                tempM[j] = defaultMaterials[j];
            }
            tempR.materials = tempM;
        }
    }

    private void OnTriggerRelease(BaseInteractionEventArgs arg)
    {
        if (Vector3.Distance(transform.position, startPosition) < 0.1f && Quaternion.Angle(transform.rotation, startRotation) < 25f)
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
        for (int i = 0; i < allRenderers.Length; i++)
        {
            Renderer tempR = allRenderers[i];
            Material[] tempM = tempR.materials;
            for (int j = 0; j < tempM.Length; j++)
            {
                tempM[j] = hoverMaterial;
            }
            tempR.materials = tempM;
        }

        targetGhostRenderer.material = ghostMaterial;
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

        targetGhostRenderer.material = hoverTargetMaterial; // Doesn't work on children
        StartCoroutine(SelectMaterialUpdate());
    }
    IEnumerator SelectMaterialUpdate()
    {
        yield return new WaitForSeconds(0.01f);
        for (int i = 0; i < allRenderers.Length; i++)
        {
            Renderer tempR = allRenderers[i];
            Material[] tempM = tempR.materials;
            for (int j = 0; j < tempM.Length; j++)
            {
                tempM[j] = grabbedMaterial;
            }
            tempR.materials = tempM;
        }
    }

    public void Explode()
    {
        explodePosition = startPosition + new Vector3(Random.Range(-0.7f, 0.7f), Random.Range(-0.1f, 0.5f), 0);
        StartCoroutine(ExplodeMove());
    }

    public void Reset()
    {
        transform.SetPositionAndRotation(startPosition, startRotation);
    }

    IEnumerator ExplodeMove()
    {
        while (Vector3.Distance(transform.position, explodePosition) > Mathf.Epsilon)
        {
            float distanceToMove = Time.deltaTime * 0.5f;

            transform.position = Vector3.MoveTowards(transform.position, explodePosition, distanceToMove);

            yield return null;
        }
    }
}
