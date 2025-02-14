using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;

public class ExplodePrefab : MonoBehaviour
{
    private Vector3 startPosition;
    private Quaternion startRotation;
    private Vector3 explodePosition;

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

    private Rigidbody rb;

    public InputActionReference leftJoystick;
    public InputActionReference rightJoystick;

    public InputActionReference leftGrip;
    public InputActionReference leftTrigger;
    public InputActionReference rightGrip;
    public InputActionReference rightTrigger;

    public GameObject leftController;
    public GameObject rightController;
    private Vector3 targetControllerPos;
    private bool leftControllerGrabbed;
    private bool rightControllerGrabbed;

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
        rb = GetComponent<Rigidbody>();
        boxCollider = transform.GetComponent<BoxCollider>(); // Try GetCollider instead?

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

            if(_leftJoystick.x != 0 || _leftJoystick.y != 0 || _rightJoystick.x != 0 || _rightJoystick.y != 0)
            {
                grabInteractable.trackPosition = false; // Need to enable these without resetting pos/rotation if held
                grabInteractable.trackRotation = false;

                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            Vector3 movementVector = 0.5f * Time.deltaTime * new Vector3(0f, 0f, -_leftJoystick.y);
            Vector3 rotationVector = 180f * Time.deltaTime * new Vector3(-_rightJoystick.y, -_rightJoystick.x, 0f);

            if (leftControllerGrabbed)
            {
                targetControllerPos = leftController.transform.position;
            }
            else if (rightControllerGrabbed)
            {
                targetControllerPos = rightController.transform.position;
            }

            transform.position = Vector3.MoveTowards(transform.position, targetControllerPos, movementVector.z * 50f * Time.deltaTime);
            transform.Rotate(rotationVector, Space.Self);
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

        if (leftGrip.action.ReadValue<float>() < 0.1f)
        {
            leftControllerGrabbed = false;
        }
        if (rightGrip.action.ReadValue<float>() < 0.1f)
        {
            rightControllerGrabbed = false;
        }

        XRRig.GetComponent<ContinuousMoveProvider>().enabled = true;
        XRRig.GetComponent<ContinuousTurnProvider>().enabled = true;

        grabInteractable.trackPosition = true;
        grabInteractable.trackRotation = true;

        if (!correctlyPlaced)
        {
            Debug.LogWarning(Quaternion.Angle(transform.rotation, startRotation));
            if (Vector3.Distance(transform.position, startPosition) < 0.1f && Quaternion.Angle(transform.rotation, startRotation) < 45f)
            {
                BuiltObject.instance.AttachToObject(gameObject);
                correctlyPlaced = true;
                transform.SetPositionAndRotation(startPosition, startRotation);

                UpdateComponenets(false);
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

        if(leftGrip.action.ReadValue<float>() > 0.1f)
        {
            if(!rightControllerGrabbed)
            {
                leftControllerGrabbed = true;
            }
        }
        if(rightGrip.action.ReadValue<float>() > 0.1f)
        {
            if (!leftControllerGrabbed)
            {
                rightControllerGrabbed = true;
            }
        }

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
        StopAllCoroutines();
        UpdateComponenets(true);
        
        explodePosition = startPosition + new Vector3(Random.Range(-0.7f, 0.7f), Random.Range(-0.1f, 0.5f), 0);

        StartCoroutine(ExplodeMove());
    }

    public void Reset()
    {
        StopAllCoroutines();
        UpdateComponenets(false);

        transform.SetPositionAndRotation(startPosition, startRotation);
        
        correctlyPlaced = false;
        isGrabbed = false;
    }

    private void UpdateComponenets(bool enable)
    {
        GetComponent<XRGrabInteractable>().enabled = enable;
        if (boxCollider != null)
        {
            GetComponent<BoxCollider>().enabled = enable;
        }
        else
        {
            GetComponent<MeshCollider>().enabled = enable;
        }
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
