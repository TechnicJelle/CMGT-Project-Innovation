using UnityEngine;
using UnityEngine.UI;

public class TutorialSlides : MonoBehaviour
{
    [SerializeField] private Image[] slides;
    [SerializeField] private Slider progressBar;
    [SerializeField] private float slideDuration;

    private Image currentSlide;
    private float slideTime;
    private int progress;

    private void OnEnable()
    {
        progress = 0;
        currentSlide = slides[progress];
        slideTime = Time.time + slideDuration;
        foreach (Image slide in slides)
        {
            if (slide != currentSlide)
            {
                slide.color = Color.clear;
            }
        }
    }

    void Update()
    {
        if (NewSlide())
        {
            ChangeSlide();
        }
        
        UpdateProgress();
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

    private void UpdateProgress()
    {
        float t = 1-(slideTime - Time.time) / slideDuration;
        progressBar.value = t;
    }
}
