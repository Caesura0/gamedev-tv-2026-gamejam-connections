using UnityEngine;

public class StartAnimator : MonoBehaviour
{

    Animator animator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        animator.SetTrigger("Start");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
