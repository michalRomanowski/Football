using UnityEngine;

public class Fitness1v1WithoutBall : IFitness
{
    public float Fitness(GameBehaviourScript game, OptimizationData optimizationData)
    {
        float fitness = 0;
        
        if(optimizationData.FirstAwayContactWithBallTime < float.MaxValue)
        {
            fitness += 150 - optimizationData.FirstAwayContactWithBallTime;
        }
        else
        {
            foreach (var p in game.Players)
            {
                if (p.Team == Team.Away)
                {
                    fitness -= Vector3.Distance(p.PivotPoint, game.Ball.PivotPoint);
                }
            }
        }

        if (game.Ball.controllingPlayer == null || game.Ball.controllingPlayer.Team == Team.Home)
            fitness -= 50;

        return fitness;
    }
}
