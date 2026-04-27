using DG.Tweening;
using StarterAssets;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.VFX;

public class DecentSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private VisualEffect visualEffect;
    [SerializeField] private VisualEffect playerVfx;
    [SerializeField] private GameObject playerLight;
    [SerializeField] private Transform elevatorRoot;
    [SerializeField] private Transform groundTarget;
    [SerializeField] private Animator door;
    [SerializeField] private Image fadeImage;
    [SerializeField] private CameraShake cameraShake;
    [SerializeField] private GameObject monsterDirector;
    [SerializeField] private FirstPersonController controller;

    [Header("Sequence Settings")]
    [SerializeField] private Vector3 doorOpenOffset = new Vector3(-1f, 0f, 0f);
    [SerializeField] private float doorTweenTime = 1f;
    [SerializeField] private float waitBeforeFade = 3f;
    [SerializeField] private float fadeTime = 1f;
    [SerializeField] private float buildupTime = 10f; 
    [SerializeField] private float moveDownSpeed = 5f; 
    [SerializeField] private float impactPause = 1f;
    [SerializeField] private float ascendSpeed = 4f;
    [SerializeField] private float ascendDistance = 80f;

    [SerializeField] private float maxVfxSpeed = 25f;
    [SerializeField] private float maxShake = 1.2f;
    [SerializeField] private float impactShake = 2f;

    [Header("Elevator interior")]
    [SerializeField] private Light redBlinkingLight;

    [Header("Events")]
    [SerializeField] private UnityEvent onSequenceStart, onRumbleStart, onImpact, onDoorOpen, onDoorClose, onAscendStart;

    private Vector3 startPos;
    private Vector3 doorClosedPos;
    private bool playerEntered;
    private bool playerInTrigger;

    private void Awake()
    {
        controller.canMove = false; // disable player controller at start
        
        if (monsterDirector != null)
        {
             monsterDirector.SetActive(false);
        }
       
        if (playerLight != null)
        {
            playerLight.gameObject.SetActive(false);     
        }
        if (playerVfx != null)
        { 
            playerVfx.gameObject.SetActive(false);
        }
        
        elevatorRoot = elevatorRoot ? elevatorRoot : transform;
        
        startPos = elevatorRoot.position + new Vector3(0, ascendDistance, 0);
        
        //if (door) doorClosedPos = door.localPosition;
        if (fadeImage) fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, 1f);
        
        StartCoroutine(Sequence());
    }

    private IEnumerator Sequence()
    {
        onSequenceStart?.Invoke();
        yield return new WaitForSeconds(waitBeforeFade);
        
        fadeImage?.DOFade(0f, fadeTime);
        yield return new WaitForSeconds(fadeTime);

        onRumbleStart?.Invoke();
        
        DOTween.To(() => 0.1f, x => visualEffect?.SetFloat("MovementSpeed", x), maxVfxSpeed, buildupTime).SetEase(Ease.InQuad);
        DOTween.To(() => 0f, x => cameraShake?.StartShake(x), maxShake, buildupTime).SetEase(Ease.InQuad);
        yield return new WaitForSeconds(buildupTime);

        if (groundTarget) yield return elevatorRoot.DOMove(groundTarget.position, Vector3.Distance(elevatorRoot.position, groundTarget.position) / moveDownSpeed).SetEase(Ease.InQuad).WaitForCompletion();

        cameraShake?.StartShake(impactShake);
        onImpact?.Invoke();
        
        DOTween.To(() => impactShake, x => cameraShake?.StartShake(x), 0f, impactPause);
        yield return new WaitForSeconds(impactPause);
        visualEffect.gameObject.SetActive(false); // stop vfx
        if (playerVfx != null)
        {
            playerVfx.gameObject.SetActive(true); // start player vfx

        }

        controller.canMove = true; 
        
        onDoorOpen?.Invoke();

        //if (door) yield return door.DOLocalMove(doorClosedPos + doorOpenOffset, doorTweenTime).SetEase(Ease.OutQuad).WaitForCompletion();

        door.SetTrigger("Open");
        
        yield return new WaitUntil(() => playerEntered && !playerInTrigger);

        onDoorClose?.Invoke();
        //if (door) yield return door.DOLocalMove(doorClosedPos, doorTweenTime).SetEase(Ease.InQuad).WaitForCompletion();

        door.SetTrigger("Close");
        
        onAscendStart?.Invoke();

        
        yield return elevatorRoot.DOMove(startPos, Vector3.Distance(elevatorRoot.position, startPos) / ascendSpeed).SetEase(Ease.OutQuad).WaitForCompletion();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerEntered = true;
            playerInTrigger = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.transform.SetParent(null);

            if (playerLight != null)
            {
                playerLight.gameObject.SetActive(true);     
            }
                
                
            if (monsterDirector != null)
            {
                monsterDirector.SetActive(true);
            }
            playerInTrigger = false;
        }
    }

}
