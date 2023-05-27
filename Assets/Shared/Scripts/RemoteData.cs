using System;
using System.Collections.Generic;
using static Enums;

[Serializable]
public class RemoteQuestionData
{

    public GameStatus Status;
    public string PlayerName;
    public string PlayerAvatar;
    public float QuestionTime;

    public string Category;
    public string Question;
    public List<string> Answers;
    public int Correct;
    public string FullAnswer;
}