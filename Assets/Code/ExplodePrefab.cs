using Oculus.Interaction;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;

public class ExplodePrefab : MonoBehaviour
{
    public Vector3 startPosition;
    public Quaternion startRotation;
    public Vector3 explodePosition;

    public bool correctlyPlaced = false;
    public bool isGrabbed = false;

    private XRGrabInteractable grabInteractable;
    public GameObject XRRig;

    public Renderer[] allRenderers;
    public Renderer[] allGhostRenderers;

    public List<Material> defaultMaterials = new();
    public Material hoverMaterial;
    public Material grabbedMaterial;
    public Material hoverTargetMaterial;
    public Material ghostMaterial;

    private bool hasMeshCollider = false;
    private BoxCollider boxCollider;
    private MeshCollider meshCollider;

    private int objIndex;
    public Transform targetGhost;

    public InputActionReference leftJoystick;
    public InputActionReference rightJoystick;

    public InputActionReference leftGrip;
    public InputActionReference leftTrigger;
    public InputActionReference rightGrip;
    public InputActionReference rightTrigger;

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

        objIndex = BuiltObject.instance.interactableObjects.IndexOf(this.transform);
        targetGhost = BuiltObject.instance.ghostObjects[objIndex];

        allRenderers = GetComponentsInChildren<Renderer>();
        allGhostRenderers = targetGhost.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in allRenderers)
        {
            List<Material> materialsToAdd = r.GetComponent<Renderer>().materials.ToList();
            foreach (Material m in materialsToAdd)
            {
                defaultMaterials.Add(m);
            }  
        } 
    }

    private void Update()
    {
        if (isGrabbed)
        {
            Vector2 _leftJoystick = leftJoystick.action.ReadValue<Vector2>();
            Vector2 _rightJoystick = rightJoystick.action.ReadValue<Vector2>();

            if(_leftJoystick.x > 0.1f || _leftJoystick.y < -0.1f)
            {
                grabInteractable.trackPosition = false;
            }
            else
            {
                grabInteractable.trackPosition = true;
            }
            if (_rightJoystick.x > 0.1f || _rightJoystick.y < -0.1f)
            {
                grabInteractable.trackRotation = false;
            }
            else
            {
                grabInteractable.trackRotation = true;
            }

            Vector3 movementVector = 0.5f * Time.deltaTime * new Vector3(0f, 0f, -_leftJoystick.y);
            Vector3 rotationVector = 60f * Time.deltaTime * new Vector3(_rightJoystick.y, _rightJoystick.x, 0f);

            transform.Translate(movementVector, Space.Self);
            transform.Rotate(rotationVector, Space.Self);
        }
        else
        {
            grabInteractable.trackPosition = true;
            grabInteractable.trackRotation = true;
        }

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
        isGrabbed = false;
        
        XRRig.GetComponent<ContinuousMoveProvider>().enabled = true;
        XRRig.GetComponent<ContinuousTurnProvider>().enabled = true;

        if (!correctlyPlaced)
        {
            Debug.LogWarning(Quaternion.Angle(transform.rotation, startRotation));
            if (Vector3.Distance(transform.position, startPosition) < 0.1f && Quaternion.Angle(transform.rotation, startRotation) < 45f)
            {
                BuiltObject.instance.AttachToObject(gameObject);
                correctlyPlaced = true;
                transform.SetPositionAndRotation(startPosition, startRotation);

                Destroy(GetComponent<XRGrabInteractable>());
                Destroy(GetComponent<Rigidbody>());
                Destroy(GetComponent<MeshCollider>());
                Destroy(GetComponent<BoxCollider>());
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
                tempM[j] = defaultMaterials[j];
            }
            tempR.materials = tempM;
        }

        for (int i = 0; i < allGhostRenderers.Length; i++)
        {
            Renderer tempR = allGhostRenderers[i];
            Material[] tempM = tempR.materials;
            for (int j = 0; j < tempM.Length; j++)
            {
                tempM[j] = ghostMaterial;
            }
            tempR.materials = tempM;
        }
    }

    private void OnTriggerGrab(BaseInteractionEventArgs arg)
    {
        isGrabbed = true;

        XRRig.GetComponent<ContinuousMoveProvider>().enabled = false;
        XRRig.GetComponent<ContinuousTurnProvider>().enabled = false;

        if (!hasMeshCollider)
        {
            boxCollider.isTrigger = true;
        }
        else
        {
            meshCollider.isTrigger = true;
        }

        for (int i = 0; i < allGhostRenderers.Length; i++)
        {
            Renderer tempR = allGhostRenderers[i];
            Material[] tempM = tempR.materials;
            for (int j = 0; j < tempM.Length; j++)
            {
                tempM[j] = hoverTargetMaterial;
            }
            tempR.materials = tempM;
        }
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
