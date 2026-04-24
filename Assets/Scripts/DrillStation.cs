using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class DrillStation : MonoBehaviour
{
    public PlayerOxygen playerOxygen;
    
    [Header("Colors")]
    public Color[] colors;
    // 0 = blue
    // 1 = red
    // 2 = yellow
    // 3 = purple
    
    [Header("Sequence")]
    public int seqLength = 3;
    public List<int> sequence;
    private int seqIndex;
    public MeshRenderer[] indicationLights;
    public bool active;

    [Header("Screen")] 
    public MeshRenderer screenRenderer;
    
    
    [Header("Levers and lights")]
    public MeshRenderer[] leverLights;
    public float leverLightIntensity = 0.15f;
    public Light evaluationLight;
    public float targetIntensity;
    public float fadeDownTime;
    
    [Header("Materials")]    
    public Material lightMat;
    public float emissionLevel;

    private bool completed;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        seqIndex = 0;
        completed = false;
        active = false;
        //InitializeSeq();   
        InitializePuzzleLights(false);
        InitializeLeverLights(false);
    }

    public void ActivatePuzzle()
    {
        active = true;
        InitializeSeq();   
        InitializePuzzleLights(true);
        InitializeLeverLights(true);
    }

    private void InitializeSeq()
    {
        sequence = new List<int>();
        for (int i = 0; i < seqLength; i++)
        {
            sequence.Add(Random.Range(0, 4));
        }
    }

    private void InitializePuzzleLights(bool activate)
    {
        
        for (int  i = 0;  i < sequence.Count;  i++)
        {
            var mesh = indicationLights[i];
            Color col;
            lightMat = new Material(lightMat);

            if (activate)
            {
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
                
                lightMat.EnableKeyword("_EMISSION");
                lightMat.SetColor("_EmissionColor", col * emissionLevel);
            }
            else
            {
                col = Color.black;
                lightMat.DisableKeyword("_EMISSION");
            }
            lightMat.color = col;
            mesh.material = lightMat;
        }   
    }
    
    private void InitializeLeverLights(bool activate)
    {
        for (int i = 0; i < leverLights.Length; i++) 
        {
            lightMat = new Material(lightMat);
            Light light = leverLights[i].GetComponentInChildren<Light>();
            
            if (activate)
            {
                lightMat.EnableKeyword("_EMISSION");
                lightMat.SetColor("_EmissionColor", colors[i] * emissionLevel);
                leverLights[i].material = lightMat;
                light.color = colors[i];
                light.intensity = leverLightIntensity;
            }
            else
            {
                lightMat.DisableKeyword("_EMISSION");
                lightMat.SetColor("_EmissionColor", colors[i] * 0);
                leverLights[i].material = lightMat;
                light.color = colors[i];
                light.DOIntensity(0, 0.1f);
            }

        }
    }

    private bool correctStep;
    
    public void PullLever(int value)
    {
        if (value - 1 == sequence[seqIndex])
        {
            // Correct
            correctStep = true;
            seqIndex++;
            CheckCompletion();
            Debug.Log("Correct puzzle step");
        }
        else
        {
            seqIndex = 0;
            active = false;
            InitializePuzzleLights(false);
            correctStep = false;
            Debug.Log("Failed puzzle - Resetting");
        }
    }
    
    private void CheckCompletion()
    {
        if (seqIndex >= sequence.Count && !completed)
        {
            Debug.Log("Puzzle completed!!");
            completed = true;
            StationTracker.instance.Completed();
            playerOxygen.AddOxygen(playerOxygen.maxOxygen);
        }
    }

    public void SetEvalLight()
    {
        if (correctStep)
        {
            evaluationLight.color = Color.green;

            if (completed)
            {
                evaluationLight.DOIntensity(targetIntensity, 0.1f);
                InitializeLeverLights(false);
                active = false;
            }
            else
            {
                evaluationLight.DOIntensity(targetIntensity, 0.1f).OnComplete(() =>
                {
                    evaluationLight.DOIntensity(0, fadeDownTime);
                });
            }
        }
        else
        {
            evaluationLight.color = Color.red;
            evaluationLight.DOIntensity(targetIntensity, 0.1f).OnComplete(() =>
            {
                evaluationLight.DOIntensity(0, fadeDownTime);
            });
            InitializePuzzleLights(false);
            InitializeLeverLights(false);
        }
        
    }
}
