public class FitnessSimple2v2Play : IFitness
{
    public float Fitness(GameBehaviourScript game, OptimizationData optimizationData)
    {
        float fitness = 0;

        fitness -= optimizationData.FinalBallDistanceToHomeGoal;
        fitness += (game.Score.Goals[Team.Away] - game.Score.Goals[Team.Home]) * 100.0f;

        return fitness;
    }
}
