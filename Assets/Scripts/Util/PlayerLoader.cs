using System.IO;
using Trivia;
using UnityEngine;

public class PlayerLoader
{
    public static void SavePlayerConfig(PlayersModel players)
    {
        string saveFile = GetFilePath();
        string json = JsonUtility.ToJson(players);
        File.WriteAllText(saveFile, json);
    }

    public static PlayersModel LoadPlayerConfig()
    {
        string path = GetFilePath();

        if (File.Exists(path))
        {
            var jsonString = File.ReadAllText(path);
            var config = JsonUtility.FromJson<PlayersModel>(jsonString);

            return config;
        }
        else
            return new PlayersModel();
    }

    static string GetFilePath() => $"{Application.streamingAssetsPath}/Players.json";

}
