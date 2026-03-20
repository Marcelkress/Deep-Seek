using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class DRILLINGSTATION : MonoBehaviour
{
    public Color[] colors;
    //  
    
    public int length = 4;
    public List<Color> colorSequence;
    
    public MeshRenderer[] indicationLights;
    public Material lightMat;
    
    public MeshRenderer[] leverLights;
    public float emmissionLevel = 2;

    private int puzzleIndex;


    public UnityEvent failedPuzzleEvent;
    
    void Awake()
    {
        puzzleIndex = 1;
        InitializeColors();
        InitializeLights();
        
        //PullLever(1);
    }

    void InitializeColors()
    {
        colorSequence = new List<Color>(length);

        for (int i = 0; i < length; i++)
        {
            colorSequence.Add(colors[Random.Range(0, 4)]);
        }
    }

    void InitializeLights()
    {
        // Puzzle lights
        for (int  i = 0;  i < colorSequence.Count;  i++)
        {
            var mesh = indicationLights[i];
            
            lightMat = new Material(lightMat);
            lightMat.EnableKeyword("_EMISSION");
            lightMat.SetColor("_EmissionColor", colorSequence[i] * emmissionLevel);
            lightMat.color = colorSequence[i];
            mesh.material = lightMat;
        }

        // Lever lights
        for (int i = 0; i < leverLights.Length; i++) 
        {
            lightMat = new Material(lightMat);
            lightMat.EnableKeyword("_EMISSION");
            lightMat.SetColor("_EmissionColor", colors[i] * emmissionLevel);

            leverLights[i].material = lightMat;
        }
    }

    public bool PullLever(int id)
    {
        if (leverLights[id - 1].material.color.ToHexString() == colorSequence[puzzleIndex - 1].ToHexString())
        {
            leverLights[id - 1].material.color = Color.green;
            leverLights[id - 1].material.SetColor("_EmissionColor", Color.green * emmissionLevel);
            puzzleIndex++;
            CheckPuzzleComplete();
            Debug.Log("Correct puzzle step");
            return true;
        }
        else
        {
            failedPuzzleEvent.Invoke();
            puzzleIndex = 1;
            InitializeColors();
            InitializeLights();
            Debug.Log("Failed puzzle - Resetting");
            return false;
        }
    }

    private void CheckPuzzleComplete()
    {
        for (int i = 0; i < colorSequence.Count; i++)
        {
            
        }
    }
    
}
