using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Leaderboard.LeaderboardRepository;
using LobbyRelaySample.UI;
using LobbyRelaySample;

public class LeaderboardUI : UIPanelBase
{
    public TMPro.TMP_Text m_LeaderboardText;
    public GameObject itemPrefab;

    public JoinCreateLobbyUI m_JoinCreateLobbyUI;


    private void Start()
    {
        m_JoinCreateLobbyUI.m_OnTabChanged.AddListener(OnTabChanged);
    }

    void OnTabChanged(JoinCreateTabs tabState)
    {
        if (tabState == JoinCreateTabs.Leaderboard)
        {
            Show();

            LeaderBoardManager.Instance.GetLeadboardData().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError("Error getting leaderboard data");
                }
                else
                {
                    var leaderboard = task;
                    ShowLeaderboard(leaderboard.Result);
                }
            });
        }
        else
        {
            Hide();
        }
    }

    public void ShowLeaderboard(List<LeaderboardPlayer> players)
    {
        m_LeaderboardText.text = "";

        int i = 1;
        foreach (var player in players)
        {
            Debug.Log($"{i}. {player.name} - {player.maxScore} pontos");
            m_LeaderboardText.text += $"{i}. {player.name} - {player.maxScore} pontos\n";
            i++;
        }
    }
}