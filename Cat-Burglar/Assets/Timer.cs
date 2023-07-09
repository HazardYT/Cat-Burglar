using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{

    public float startTime;

    public float curTime;

    public float countdownSpeed;

    string displayTime;

    int mins;
    int secs;

    [SerializeField] TMP_Text display;

    // Start is called before the first frame update
    void Start()
    {
        curTime = startTime;
    }

    // Update is called once per frame
    void Update()
    {
        curTime -= countdownSpeed * Time.deltaTime;

        mins = Mathf.RoundToInt(Mathf.Floor(curTime / 60));
        secs = Mathf.RoundToInt(curTime%60);

        if(secs == 60){secs = 59;}

        displayTime = mins.ToString("00:") + secs.ToString("00");

        display.text = displayTime;
    }
}
