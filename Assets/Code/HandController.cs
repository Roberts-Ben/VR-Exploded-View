using UnityEngine;
using UnityEngine.InputSystem;

public class HandController : MonoBehaviour
{
    public InputActionReference thumbstickInput;
    public InputActionReference triggerInput;
    public InputActionReference grabInput;

    public HandAnimation handAnimation;

    void Update()
    {
        handAnimation.SetTrigger(triggerInput.action.ReadValue<float>());
        handAnimation.SetGrab(grabInput.action.ReadValue<float>());
    }
}
