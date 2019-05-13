using System;

[Serializable]
public class MyScores
{
    public string Date;
    public int Times;
    public int Score;

    public MyScores(string date, int times, int score)
    {
        Date = date;
        Times = times;
        Score = score;
    }
}

