using UnityEngine;

public class SkyPoll : MonoBehaviour
{
    void Awake()
    {
        Application.runInBackground = true;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (Time.timeScale == 1f) return;
        SkyLog.Error("Time.timeScale value is incorrect: {0}f", Time.timeScale);
        Time.timeScale = 1f;
        SkyLog.Error("Time.timeScale has been set to 1.0f by SkyNet");
    }

    void FixedUpdate()
    {
        //SkyManager.PollEvents();
        SkyManager.SimulatePhysics();
    }
}
