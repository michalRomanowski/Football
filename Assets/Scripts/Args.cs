using System;
using UnityEngine;

public enum GameMode
{
    Play,
    Simple1v1WithoutBall,
    Simple1v1WithBall,
    Simple1v1Play,
    Simple2v2WithoutBall,
}

public static class Args {
    private const string AWAY_TEAM_PATH_ARG = "-awayTeamPath";
    private const string HOME_TEAM_PATH_ARG = "-homeTeamPath";
    private const string MODE_ARG = "-mode";
    private const string POSITIONS_PATH_ARG = "-positionsPath";
    private const string MINUTES_ARG = "-minutes";
    private const string IMMOBILIZE_ARG = "-immobilize";

    private const string GAME_MODE_1v1_WITHOUT_BALL = "Simple1v1WithoutBall";
    private const string GAME_MODE_1v1_WITH_BALL = "Simple1v1MovingWithBall";
    private const string GAME_MODE_1v1_PLAY = "Simple1v1Play";
    private const string GAME_MODE_2v2_WithoutBall = "Simple2v2WithoutBall";

    public static GameMode GameMode { get; private set; }
    public static string AwayTeamPath { get; private set; }
    public static string HomeTeamPath { get; private set; }
    public static string PositionsPath { get; private set; }
    public static int Minutes { get; private set; }
    public static bool Immobilize { get; private set; }

    static Args()
    {
        var args = Environment.GetCommandLineArgs();

        Minutes = 2;

        for (int i = 0; i < args.Length; i++)
        {
            if (AWAY_TEAM_PATH_ARG.Equals(args[i], StringComparison.InvariantCultureIgnoreCase))
                AwayTeamPath = args[i + 1];

            if (HOME_TEAM_PATH_ARG.Equals(args[i], StringComparison.InvariantCultureIgnoreCase))
                HomeTeamPath = args[i + 1];

            else if (MODE_ARG.Equals(args[i], StringComparison.InvariantCultureIgnoreCase))
            {
                Debug.Log("GameModeArg: " + args[i + 1]);

                if (GAME_MODE_1v1_WITHOUT_BALL.Equals(args[i + 1], StringComparison.InvariantCultureIgnoreCase))
                    GameMode = GameMode.Simple1v1WithoutBall;
                else if (GAME_MODE_1v1_WITH_BALL.Equals(args[i + 1], StringComparison.InvariantCultureIgnoreCase))
                    GameMode = GameMode.Simple1v1WithBall;
                else if (GAME_MODE_1v1_PLAY.Equals(args[i + 1], StringComparison.InvariantCultureIgnoreCase))
                    GameMode = GameMode.Simple1v1Play;
                else if (GAME_MODE_2v2_WithoutBall.Equals(args[i + 1], StringComparison.InvariantCultureIgnoreCase))
                    GameMode = GameMode.Simple2v2WithoutBall;
                else GameMode = GameMode.Play;
            }

            else if (POSITIONS_PATH_ARG.Equals(args[i], StringComparison.InvariantCultureIgnoreCase))
                PositionsPath = args[i + 1];

            else if (MINUTES_ARG.Equals(args[i], StringComparison.InvariantCultureIgnoreCase))
                Minutes = Convert.ToInt32(args[i + 1]);

            else if (IMMOBILIZE_ARG.Equals(args[i], StringComparison.InvariantCultureIgnoreCase))
                Immobilize = true;
        }

        Debug.Log("GameMode: " + GameMode);
        Debug.Log("AwayTeamPath: " + AwayTeamPath);
        Debug.Log("HomeTeamPath: " + HomeTeamPath);
        Debug.Log("PositionsPath: " + PositionsPath);
        Debug.Log("Minutes: " + Minutes.ToString());
        Debug.Log("Immobilize" + Immobilize.ToString());
    }


}
