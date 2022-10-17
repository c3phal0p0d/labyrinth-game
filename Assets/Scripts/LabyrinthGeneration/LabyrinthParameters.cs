using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class LabyrinthParameters : MonoBehaviour, ICloneable
{
    public int numSections;
    public bool isFinalLevel;
    public int enemyDensity;
    public int pickupDensity;
    public GameObject origin;
    public System.Random random;
    public Material brickMaterial;
    public GameObject wallTorchPrefab;

    public object Clone()
    {
        return this.MemberwiseClone();
    }
}
