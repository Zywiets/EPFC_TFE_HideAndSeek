using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class RankingEntryModifier : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI rank;
    [SerializeField] private TextMeshProUGUI username;
    [SerializeField] private TextMeshProUGUI score;

    public void AddValues(string ra, string na, string sc)
    {
        rank.text = ra;
        username.text = na;
        score.text = sc;
    }

    public void AddValues(string na)
    {
        username.text = na;
    }
}
