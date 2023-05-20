using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shipwreck : MonoBehaviour
{
    public int Treasure { get; private set; }

    public void Initialize(int pTreasure)
    {
        Treasure = pTreasure;
        this.transform.Rotate(0, Random.Range(0, 360), 0);
    }
}
