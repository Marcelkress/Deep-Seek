using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class StationTracker : MonoBehaviour
{
    public DecentSystem decentSystem;
    public bool[] stations;
    private int index;
    public static StationTracker instance;

    public TMP_Text UITrackerText;
    public string textAfterNumber = " of 4 stations";
    public TMP_Text completedAllStationsText;
    public float showTextTime = 5;

    [Header("Debug")] public bool enableTest;
    public InputActionReference testButton;
    public AK.Wwise.Event extractionSound;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        index = 0;
        stations = new[] { false, false, false, false };

        UITrackerText = GetComponentInChildren<TMP_Text>();
        
        UITrackerText.text = "0" + textAfterNumber;

        if(enableTest)
            testButton.action.started += CompletedTest;
    }

    void CompletedTest(InputAction.CallbackContext context)
    {
        Completed();
    }

    public void Completed()
    {
        stations[index] = true;
        index++;

        int total = 0;
        for (int i = 0; i < index; i++)
        {
            if (stations[i] == true)
            {
                total++;
            }
        }

        UITrackerText.text = total.ToString() + textAfterNumber;

        if (total == 4)
        {
            completedAllStationsText.DOFade(1,0.3f).OnComplete(() =>
            {
                Invoke(nameof(HideText), showTextTime);
                StartCoroutine(decentSystem.Sequence(true));
                extractionSound.Post(gameObject);
            });
        }
    }

    private void HideText()
    {
        completedAllStationsText.DOFade(0, 0.3f);
    }
    
}