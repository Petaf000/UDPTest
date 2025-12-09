using UnityEngine;

public class UITest : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnClick()
    {
        Debug.Log("Button Clicked!");
    }

    public void OnSliderChange(float val)
    {
        Debug.Log("Slider Value: " + val);
    }
}
