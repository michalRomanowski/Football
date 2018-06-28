using UnityEngine;
using System.Collections.Generic;

public class GameBehaviourScript : MonoBehaviour {
    public List<PlayerBehaviourScript> Players { get; private set; }
    public Dictionary<Team, List<PlayerBehaviourScript>> Teams { get; private set; }

    public ScoreBehaviourScript Score { get; private set; }
    public BallBehaviourScript Ball { get; private set; }
    public ClockBehaviourScript Clock { get; private set; }

    public OptimizationBehaviourScript Optimization { get; private set; }

    // Use this for initialization
    void Start () {

        Score = FindObjectOfType(typeof(ScoreBehaviourScript)) as ScoreBehaviourScript;

        //Ball = (BallBehaviourScript)GameObject.Find("Ball").GetComponent("BallBehaviourScript");

        Ball = FindObjectOfType(typeof(BallBehaviourScript)) as BallBehaviourScript;

        Clock = FindObjectOfType(typeof(ClockBehaviourScript)) as ClockBehaviourScript;

        if(Args.GameMode != GameMode.Play)
            Optimization = FindObjectOfType(typeof(OptimizationBehaviourScript)) as OptimizationBehaviourScript;

        Players = new List<PlayerBehaviourScript>();

        var playerObjects = GameObject.FindObjectsOfType(typeof(PlayerBehaviourScript));
        
        foreach (var p in playerObjects)
        {
            Players.Add(p as PlayerBehaviourScript);
        }

        Teams = new Dictionary<Team, List<PlayerBehaviourScript>>();
        Teams.Add(Team.Home, new List<PlayerBehaviourScript>());
        Teams.Add(Team.Away, new List<PlayerBehaviourScript>());

        foreach (var p in Players)
        {
            Teams[p.Team].Add(p);
            p.ToStartingPosition();
        }

        if(Args.GameMode != GameMode.Play)
        {
            (GameObject.FindObjectOfType(typeof(OptimizationBehaviourScript)) as OptimizationBehaviourScript).Initialize();
        }
    }
	
	// Update is called once per frame
	void Update () {
	}

    public void Goal(Team scoredTeam)
    {
        Score.Goals[scoredTeam]++;

        if (Args.GameMode != GameMode.Play)
        {
            Optimization.Goal();
        }

        foreach (var p in Players)
        {
            p.Goal();
            p.ToStartingPosition();
        }

        if(scoredTeam == Team.Home)
            Teams[Team.Away][0].MoveTo(0, 0, 90);
        else
            Teams[Team.Home][0].MoveTo(0, 0, 270);

        Score.UpdateScoreBoard();
    }

    public void Restart()
    {
        foreach (var p in Players)
        {
            p.ToStartingPosition();
        }

        Ball.ToStartingPosition();

        Score.Goals[Team.Home] = 0;
        Score.Goals[Team.Away] = 0;
    }

    public void Corner(CornerType corner)
    {
        if (Args.GameMode != GameMode.Play)
            Optimization.Corner();


        if (Ball.lastControllingPlayer == null)
            return;

        if ((corner == CornerType.UpperLeft || corner == CornerType.LowerLeft))
        {
            if (Ball.lastControllingPlayer.Team == Team.Home)
                Corner(corner, Team.Away, Team.Home);
            else
                GoalKeeperFreekick(Team.Home, Team.Away);
        }
        else if ((corner == CornerType.UpperRight || corner == CornerType.LowerRight))
        {
            if (Ball.lastControllingPlayer.Team == Team.Away)
                Corner(corner, Team.Home, Team.Away);
            else
                GoalKeeperFreekick(Team.Away, Team.Home);
        }
    }

    private void Corner(CornerType corner, Team attackingTeam, Team defendingTeam)
    {
        Score.Corners[attackingTeam]++;

        Ball.MoveToCorner(corner);
        
        foreach (var p in Teams[attackingTeam])
            p.MoveToAttackingCornerPosition(corner);

        foreach (var p in Teams[defendingTeam])
            p.MoveToDefendingCornerPosition(corner);
    }

    private void GoalKeeperFreekick(Team withBallTeam, Team withoutBallTeam)
    {
        Score.ShotsOffTarget[withoutBallTeam]++;

        if(withBallTeam == Team.Home)
            Ball.MoveTo(-3.7f, 0f);
        else Ball.MoveTo(3.7f, 0f);

        foreach (var p in Teams[withBallTeam])
            p.MoveToWithBallGoalKeeperFreekickPosition();

        foreach (var p in Teams[withoutBallTeam])
            p.MoveToWithoutBallGoalKeeperFreekickPosition();
    }
}
