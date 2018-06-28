public class FitnessSimple1v1WithBall : IFitness
{
    public float Fitness(GameBehaviourScript game, OptimizationData optimizationData)
    {
        float fitness = 0;
        
        fitness -= optimizationData.FinalBallDistanceToHomeGoal;

        if (optimizationData.AwayCorner)
            fitness += 5.0f;

        if (game.Score.Goals[Team.Away] > 0)
            fitness = 100;

        fitness -= game.Clock.TotalSeconds / (Args.Minutes * 60);
        
        return fitness;
    }
}
