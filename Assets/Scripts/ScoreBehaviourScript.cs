using UnityEngine;
using System.Collections.Generic;

public class ScoreBehaviourScript : MonoBehaviour {

    private TextMesh textMesh;

    public Dictionary<Team, int> Goals = new Dictionary<Team, int>();
    public Dictionary<Team, float> Possession = new Dictionary<Team, float>();
    public Dictionary<Team, int> Corners = new Dictionary<Team, int>();
    public Dictionary<Team, int> ShotsOffTarget = new Dictionary<Team, int>();

    

    public int HomeShotOffTarget;
    public int AwayShotOffTarget;

    private BallBehaviourScript ball;

    // Use this for initialization
    void Start ()
    {
        textMesh = GetComponent<TextMesh>();

        ball = (BallBehaviourScript)GameObject.Find("Ball").GetComponent("BallBehaviourScript");

        Goals.Add(Team.Home, 0);
        Goals.Add(Team.Away, 0);

        Possession.Add(Team.Home, 0);
        Possession.Add(Team.Away, 0);

        Corners.Add(Team.Home, 0);
        Corners.Add(Team.Away, 0);

        ShotsOffTarget.Add(Team.Home, 0);
        ShotsOffTarget.Add(Team.Away, 0);
    }
	
	// Update is called once per frame
	void Update () {
        if (ball.controllingPlayer != null)
        {
            if (ball.controllingPlayer.Team == Team.Home)
                Possession[Team.Home] += Time.deltaTime;
            else if (ball.controllingPlayer.Team == Team.Away)
                Possession[Team.Away] += Time.deltaTime;
        }
    }

    public void UpdateScoreBoard()
    {
        textMesh.text = Goals[Team.Home] + ":" + Goals[Team.Away];
    }
}
