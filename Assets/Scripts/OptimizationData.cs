using UnityEngine;

public class OptimizationData
{
    public bool CornerHappened = false;
    public bool GoalHappened = false;
    public bool GameWasStopped { get { return CornerHappened || GoalHappened; } }

    public float FinalBallDistanceToHomeGoal = float.MaxValue;

    public float FirstAwayContactWithBallTime = float.MaxValue;

    public bool AwayCorner = false;
}
