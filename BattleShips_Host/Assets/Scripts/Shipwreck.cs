using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shipwreck : MonoBehaviour
{
    private int treasure;

    public int Treasure => treasure;

    public void Initialize(int pTreasure)
    {
        treasure = pTreasure;
        this.transform.Rotate(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360));
    }
}
