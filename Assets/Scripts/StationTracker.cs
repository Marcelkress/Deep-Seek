using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class StationTracker : MonoBehaviour
{
    public bool[] stations;
    private int index;
    public static StationTracker instance;

    public TMP_Text UITrackerText;
    public string textAfterNumber = " of 4 stations";
    public TMP_Text completedAllStationsText;
    public float showTextTime = 5;

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
    }

    public void Completed()
    {
        stations[index] = true;

        int total = 0;

        for (int i = 0; i < stations.Length; i++)
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
            });
        }
    }

    private void HideText()
    {
        completedAllStationsText.DOFade(0, 0.3f);
    }
    
}