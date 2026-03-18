using System;
using System.Collections.Generic;
using UnityEngine;

public class DRILLINGSTATION : MonoBehaviour
{
    // Fuh' yeh' 
    /*
    public enum Color
    {
        Red,
        Green,
        Blue,
        Yellow
    }
    */

    //public List<UnityEngine.Color> colors;

    public List<Color> colorSequence;
    
    public MeshRenderer[] indicationLights;
    public Material lightMat;

    public bool[] levers;
    public Light[] leverLights;
    public float emmissionLevel = 2;
    
    void Start()
    {
        Initialize();
    }

    void Initialize()
    {
        for (int  i = 0;  i < colorSequence.Count;  i++)
        {
            var mesh = indicationLights[i];
            
            lightMat = new Material(lightMat);
            lightMat.EnableKeyword("_EMISSION");
            lightMat.SetColor("_EmissionColor", colorSequence[i] * emmissionLevel);
            lightMat.color = colorSequence[i];
            mesh.material = lightMat;
            
            
            /*
            switch (colorSequence[i])
            {
                case Color.Blue :
                    lightMat = new Material(lightMat);
                    lightMat.EnableKeyword("_EMISSION");
                    lightMat.SetColor("_EmissionColor", UnityEngine.Color.cyan);
                    lightMat.color = UnityEngine.Color.cyan;
                    mesh.material = lightMat;
                    break;
                case Color.Green :
                    lightMat = new Material(lightMat);
                    lightMat.EnableKeyword("_EMISSION");
                    lightMat.SetColor("_EmissionColor", UnityEngine.Color.green);
                    lightMat.color = UnityEngine.Color.green;
                    mesh.material = lightMat;
                    break;
                case Color.Red :
                    lightMat = new Material(lightMat);
                    lightMat.EnableKeyword("_EMISSION");
                    lightMat.SetColor("_EmissionColor", UnityEngine.Color.red);
                    lightMat.color = UnityEngine.Color.red;
                    mesh.material = lightMat;
                    break;
                case Color.Yellow :
                    lightMat = new Material(lightMat);
                    lightMat.EnableKeyword("_EMISSION");
                    lightMat.SetColor("_EmissionColor", UnityEngine.Color.yellow);
                    lightMat.color = UnityEngine.Color.yellow;
                    mesh.material = lightMat;
                    break;
            }
            */

        }
    }
    
    
}
