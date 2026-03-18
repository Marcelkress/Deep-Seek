using System;
using System.Collections.Generic;
using UnityEngine;

public class DRILLINGSTATION : MonoBehaviour
{
    // Fuh' yeh' 
    public enum Color
    {
        Red,
        Green,
        Blue,
        Yellow
    }

    public List<Color> colorSequence;
    
    public Light[] indicationLights;

    public bool[] levers;
    public Light[] leverLights;
    
    void Start()
    {
        Initialize();
    }

    void Initialize()
    {
        for (int  i = 0;  i < colorSequence.Count;  i++)
        {
            switch (colorSequence[i])
            {
                case Color.Blue :
                    indicationLights[i].color = UnityEngine.Color.cyan;
                    break;
                case Color.Green :
                    indicationLights[i].color = UnityEngine.Color.green;
                    break;
                case Color.Red :
                    indicationLights[i].color = UnityEngine.Color.red;
                    break;
                case Color.Yellow :
                    indicationLights[i].color = UnityEngine.Color.yellow;
                    break;
            }
            
        }
    }
    
    
}
