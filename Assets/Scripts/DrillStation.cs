using System.Collections.Generic;
using UnityEngine;

public class DrillStation : MonoBehaviour
{
    public Color[] colors;
    // 0 = blue
    // 1 = red
    // 2 = yellow
    // 3 = purple
    
    public int seqLength = 3;
    public List<int> sequence;
    private int seqIndex;
    public MeshRenderer[] indicationLights;
    public MeshRenderer[] leverLights;
    public Material lightMat;
    public float emissionLevel;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        seqIndex = 1;
        InitializeSeq();   
        InitializePuzzleLights();
        InitializeLeverLights();
    }

    private void InitializeSeq()
    {
        sequence = new List<int>();
        for (int i = 0; i < seqLength; i++)
        {
            sequence.Add(Random.Range(0, 4));
        }
    }

    private void InitializePuzzleLights()
    {
        for (int  i = 0;  i < sequence.Count;  i++)
        {
            var mesh = indicationLights[i];

            Color col;

            switch (sequence[i])
            {
                case 0:
                    col = colors[0];
                    break;
                case 1:
                    col = colors[1];
                    break;
                case 2 :
                    col = colors[2];
                    break;
                case 3 :
                    col = colors[3];
                    break;
                default:
                    col = colors[0];
                    break;
            }
            
            lightMat = new Material(lightMat);
            lightMat.EnableKeyword("_EMISSION");
            lightMat.SetColor("_EmissionColor", col * emissionLevel);
            lightMat.color = col;
            mesh.material = lightMat;
        }   
    }


    private void InitializeLeverLights()
    {
        for (int i = 0; i < leverLights.Length; i++) 
        {
            lightMat = new Material(lightMat);
            lightMat.EnableKeyword("_EMISSION");
            lightMat.SetColor("_EmissionColor", colors[i] * emissionLevel);

            leverLights[i].material = lightMat;
        }
    }
    
    public void PullLever(int value)
    {
        if (sequence[seqIndex] == value - 1)
        {
            // Correct
            leverLights[value - 1].material.color = Color.green;
            leverLights[value - 1].material.SetColor("_EmissionColor", Color.green * emissionLevel);
            seqIndex++;
            Debug.Log("Correct puzzle step");
        }
        else
        {
            seqIndex = 1;
            InitializeSeq();
            InitializePuzzleLights();
            Debug.Log("Failed puzzle - Resetting");
        }
    }
}
