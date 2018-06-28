using UnityEngine;
using System.IO;
using System;

public class OptimizationBehaviourScript : MonoBehaviour {
    
    private static GameBehaviourScript game;
    
    private IFitness fitnessCounter;

    private OptimizationData optimizationData = new OptimizationData();

    public void Initialize()
    {
        game = GameObject.FindObjectOfType(typeof(GameBehaviourScript)) as GameBehaviourScript;
        
        switch (Args.GameMode)
        {
            case GameMode.Simple1v1WithoutBall:
                {
                    new Optimizer1v1WithoutBall().Initialize();
                    fitnessCounter = new Fitness1v1WithoutBall();
                }
                break;
            case GameMode.Simple1v1WithBall:
                {
                    new Optimizer1v1WithBall().Initialize();
                    fitnessCounter = new FitnessSimple1v1WithBall();
                }
                break;
            case GameMode.Simple1v1Play:
                {
                    new Optimizer1v1WithoutBall().Initialize();
                    fitnessCounter = new FitnessSimple1v1Play();
                }
                break;
            case GameMode.Simple2v2WithoutBall:
                {
                    new Optimizer2v2WithoutBall().Initialize();
                    fitnessCounter = new FitnessSimple2v2Play();
                }break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Args.GameMode == GameMode.Play)
            return;

        if (game.Clock.Minutes >= Args.Minutes)
        {
            Timeout();
        }
    }

    public void Corner()
    {
        if (Args.GameMode == GameMode.Simple1v1WithBall && !optimizationData.GameWasStopped)
        {
            optimizationData.FinalBallDistanceToHomeGoal = Vector3.Distance(new Vector3(-4, 0), game.Ball.PivotPoint);

            optimizationData.AwayCorner = game.Ball.transform.position.x < 0.0f;

            Summarize();
            Application.Quit();
        }
    }

    public void Goal()
    {
        if (Args.GameMode == GameMode.Simple1v1WithBall && !optimizationData.GameWasStopped)
        {
            optimizationData.FinalBallDistanceToHomeGoal = Vector3.Distance(new Vector3(-4, 0), game.Ball.PivotPoint);

            Summarize();
            Application.Quit();
        }
    }

    public void BallHitByPlayer()
    {
        if (Args.GameMode == GameMode.Simple1v1WithoutBall && optimizationData.FirstAwayContactWithBallTime == float.MaxValue && game.Ball.controllingPlayer.Team == Team.Away)
        {
            optimizationData.FirstAwayContactWithBallTime = game.Clock.TotalSeconds;

            Summarize();
            Application.Quit();
        }
    }

    private void Timeout()
    {
        if(Args.GameMode == GameMode.Simple1v1WithBall || Args.GameMode == GameMode.Simple1v1Play)
            optimizationData.FinalBallDistanceToHomeGoal = Vector3.Distance(new Vector3(-4, 0), game.Ball.PivotPoint);
        
        Summarize();
        Application.Quit();
    }

    private void Summarize()
    {
        using (var sw = new StreamWriter(Args.AwayTeamPath + "\\Fitness", false))
        {
            sw.WriteLine(fitnessCounter.Fitness(game, optimizationData));
        }
    }
}
