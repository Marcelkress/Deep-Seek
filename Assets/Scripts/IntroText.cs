using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class IntroText : MonoBehaviour
{
    public TMP_Text[] textFields;
    public float textShowTime = 1f, timeBetweenCharacters = .5f;
    private int index;

    public float panelFadeDuration = 1f;
    public Image backgroundPanel;
    
    public UnityEvent characterShowEvent;
    public MenuMusicScript menuMusic;
    public BreathingSoundScript breathingSound;

    private void Start()
    {
        menuMusic.MenuMusicPlay();
        backgroundPanel.DOFade(0, 0);
        foreach (var text in textFields)
        {
            text.DOFade(0, 0);
        }
    }

    public void StartSeq()
    {
        StartCoroutine(StartSequence());
    }

    private IEnumerator StartSequence()
    {
        Debug.Log("Starting text seq");
        yield return backgroundPanel.DOFade(1, panelFadeDuration).WaitForCompletion();

        foreach (var text in textFields)
        {
            text.DOFade(1, .1f);
            
            string fullText = text.text;
            text.text = "";
            
            foreach (var character in fullText.ToCharArray())
            {
                text.text += character;
                characterShowEvent.Invoke();
                yield return new WaitForSeconds(timeBetweenCharacters);
            }

            yield return new WaitForSeconds(textShowTime);
            text.gameObject.SetActive(false);
        }
        menuMusic.MenuMusicStop();
        yield return new WaitForSeconds(2f);
        menuMusic.GetUnderWater();
        breathingSound.StartBreathingSoundWalk();

        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene("Level");
    }
}
