using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialSlides : MonoBehaviour
{
    [SerializeField] private Image[] slides;
    [SerializeField] private float slideDuration;

    private Image currentSlide;
    private float slideTime;
    private int progress = 0;

    private void Start()
    {
        slideTime = Time.time + slideDuration;
        currentSlide = slides[progress];
        foreach (Image slide in slides)
        {
            if (slide != currentSlide)
            {
                slide.color = Color.clear;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (NewSlide())
        {
            ChangeSlide();
        }

        Debug.Log(Time.time);
    }
    
    private bool NewSlide()
    {
        if (Time.time < slideTime) return false;

        slideTime += slideDuration;
        return true;
    }

    private void ChangeSlide()
    {
        progress++;
        if (progress >= slides.Length)
            progress = 0;
        currentSlide.color = Color.clear;
        currentSlide = slides[progress];
        currentSlide.color = Color.white;
    }
}
