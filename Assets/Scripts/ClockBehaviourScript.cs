using UnityEngine;
using System.Collections;
using System.Diagnostics;

public class ClockBehaviourScript : MonoBehaviour {
    public float Seconds { get; private set; }
    public float Minutes { get; private set; }

    public float TotalSeconds { get { return Minutes * 60 + Seconds; } }

    private TextMesh textMesh;

    private const float TIME_MULTIPLIER = 20;

    // Use this for initialization
    void Start () {
        textMesh = GetComponent<TextMesh>();
    }
	
	// Update is called once per frame 
	void Update ()
    {
        if (Seconds >= 60)
        {
            Minutes++;
            Seconds = 0.0f;
        }
        textMesh.text = ((int)Minutes).ToString("D2") + ":" + ((int)Seconds).ToString("D2");

        Seconds += Time.deltaTime * TIME_MULTIPLIER;
    }

    public void Restart()
    {
        Seconds = 0.0f;
        Minutes = 0.0f;
    }
}
