using UnityEngine;
 
public class FPSDisplay : MonoBehaviour
{
    private float deltaTime = 0.0f;
    private int frames = 0;
    private float timeLeft = 0.0f;
 
    void Update()
    {
        timeLeft -= Time.deltaTime;
        deltaTime += Time.deltaTime;
        frames++;
 
        if (timeLeft <= 0.0)
        {
            float fps = frames / deltaTime;
            Debug.Log(string.Format("Frames per second: {0}", fps));
            timeLeft = 1.0f; // Reset the timer to measure again in 1 second
            frames = 0; // Reset the frame counter
            deltaTime = 0.0f; // Reset the delta time counter
        }
    }
}