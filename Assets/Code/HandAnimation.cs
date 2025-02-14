using UnityEngine;

public class HandAnimation : MonoBehaviour
{
    Animator animator;

    private float grabCurrent;
    private float grabTarget;
    private float triggerCurrent;
    private float triggerTarget;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        AnimateHand();
    }
    internal void SetGrab(float v)
    {
        grabTarget = v;
    }

    internal void SetTrigger(float v)
    {
        triggerTarget = v;
    }

    void AnimateHand()
    {
        if(grabCurrent != grabTarget)
        {
            grabCurrent = Mathf.MoveTowards(grabCurrent, grabTarget, 1f * Time.deltaTime);
            animator.SetFloat("Grab", grabCurrent);
        }
        if (triggerCurrent != triggerTarget)
        {
            triggerCurrent = Mathf.MoveTowards(triggerCurrent, triggerTarget, 1f * Time.deltaTime);
            animator.SetFloat("Trigger", triggerTarget);
        }
    }
}
