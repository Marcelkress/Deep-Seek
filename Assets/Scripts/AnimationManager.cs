using System;
using StarterAssets;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    private StarterAssetsInputs _input;
    private Animator anim;
    public AK.Wwise.Event Footsteps;
    
    
    private bool walking, sprinting;
    private void Start()
    {
        _input = GetComponentInParent<StarterAssetsInputs>();
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        walking = _input.move != Vector2.zero;
        sprinting = _input.sprint;
        
        anim.SetBool("Walking", walking);
        anim.SetBool("Running", sprinting);
    }
    
    public void FootStepSound()
    {
        Footsteps.Post(gameObject);
			
    }

   
    
}
