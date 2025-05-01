using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

[Serializable]
public class Score {
    public int total;
    public string username;
    [FormerlySerializedAs("id")] public int userId;

    public Score(string username, int total, int userId) {
        this.username = username;
        this.total = total;
        this.userId = userId;
    }
}

[Serializable]
public class RankingWrapper
{
    public List<Score> rankings;
}