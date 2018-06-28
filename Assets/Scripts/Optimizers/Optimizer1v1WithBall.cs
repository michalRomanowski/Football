using System;
using UnityEngine;
using System.IO;

public class Optimizer1v1WithBall{

    public Optimizer1v1WithBall(){ }

    public void Initialize()
    {
        var game = GameObject.FindObjectOfType(typeof(GameBehaviourScript)) as GameBehaviourScript;

        foreach (var p in game.Teams[Team.Away])
        {
            p.LoadFromDisc(Args.AwayTeamPath);
        }

        if(!String.IsNullOrEmpty(Args.HomeTeamPath))
        {
            foreach (var p in game.Teams[Team.Home])
            {
                p.Controller = Controller.Bot;
                p.LoadFromDisc(Args.AwayTeamPath);
            }
        }

        if (!String.IsNullOrEmpty(Args.PositionsPath))
        {
            float x;
            float y;
            int angle;

            using (var sr = new StreamReader(Args.PositionsPath))
            {
                for (int i = 0; i < game.Players.Count; i++)
                {
                    x = (float)Convert.ToDecimal(sr.ReadLine().Replace(',', '.'));
                    y = (float)Convert.ToDecimal(sr.ReadLine().Replace(',', '.'));
                    angle = Convert.ToInt32(sr.ReadLine());

                    game.Players[i].MoveTo(x, y, angle);
                }

                x = game.Teams[Team.Away][0].transform.position.x;
                y = game.Teams[Team.Away][0].transform.position.y;
                
                game.Ball.MoveTo(x, y);
            }
        }
    }


}
