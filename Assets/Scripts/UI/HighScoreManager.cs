using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class HighScoreManager : SingletonMonobehaviour<HighScoreManager>
{
    private HighScore highScore = new HighScore();

    protected override void Awake()
    {
        base.Awake();

        LoadScores();
    }

    /// <summary>
    /// Load Scores From Disk
    /// </summary>
    private void LoadScores()
    {
        BinaryFormatter bf = new BinaryFormatter();

        if (File.Exists(Application.persistentDataPath + "/DungeonGunnerHighScores.dat"))
        {
            ClearScoreList();

            FileStream file = File.OpenRead(Application.persistentDataPath + "/DungeonGunnerHighScores.dat");

            highScore = (HighScore)bf.Deserialize(file);

            file.Close();

        }
    }

    /// <summary>
    /// Clear All Scores
    /// </summary>
    private void ClearScoreList()
    {
        highScore.scoreList.Clear();
    }

    /// <summary>
    /// Add score to high scores list
    /// </summary>
    public void AddScore(Score score, int rank)
    {
        highScore.scoreList.Insert(rank - 1, score);

        // Maintain the maximum number of scores to save
        if (highScore.scoreList.Count > Settings.numberOfHighScoresToSave)
        {
            highScore.scoreList.RemoveAt(Settings.numberOfHighScoresToSave);
        }

        SaveScores();
    }

    /// <summary>
    /// Save Scores To Disk
    /// </summary>
    private void SaveScores()
    {
        BinaryFormatter bf = new BinaryFormatter();

        FileStream file = File.Create(Application.persistentDataPath + "/DungeonGunnerHighScores.dat");

        bf.Serialize(file, highScore);

        file.Close();
    }

    /// <summary>
    /// Get highscores
    /// </summary>
    public HighScore GetHighScores()
    {
        return highScore;
    }

    /// <summary>
    /// Return the rank of the playerScore compared to the other high scores (returns 0 if the score isn't higher than any in the high scores list)
    /// </summary>
    public int GetRank(long playerScore)
    {
        // If there are no scores currently in the list - then this score must be ranked 1 - then return
        if (highScore.scoreList.Count == 0) return 1;

        int index = 0;

        // Loop through scores in list to find the rank of this score
        for (int i = 0; i < highScore.scoreList.Count; i++)
        {
            index++;

            if (playerScore >= highScore.scoreList[i].playerScore)
            {
                return index;
            }
        }

        if (highScore.scoreList.Count < Settings.numberOfHighScoresToSave)
            return (index + 1);

        return 0;
    }
}