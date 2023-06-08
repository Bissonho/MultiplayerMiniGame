using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using LanguageExt;
using Leaderboard;
using static Leaderboard.LeaderboardRepository;

namespace LobbyRelaySample
{

    public class LeaderBoardManager : Singleton<LeaderBoardManager>
    {
        private LeaderboardRepository repository;

        private void Start()
        {
            repository = new LeaderboardRepository();
        }

        private async Task CreateSessionExample()
        {
            var players = new List<LeaderboardRepository.Player>
        {
            new LeaderboardRepository.Player(100, "Player 1"),
            new LeaderboardRepository.Player(200, "Player 2"),
            new LeaderboardRepository.Player(300, "Player 3")
        };
            var session = new LeaderboardRepository.Session("Example Session", players);

            var result = await repository.CreateSession(session);


            result.Match(
                session => Debug.Log("Sessão criada com sucesso: " + session.name),
                failure => Debug.LogError("Falha ao criar sessão: " + failure.Error)
            );
        }

        public async Task CreateSession(List<LeaderboardRepository.Player> players)
        {
            var session = new LeaderboardRepository.Session("Example Session", players);
            var result = await repository.CreateSession(session);

            result.Match(
                session => Debug.Log("Sessão criada com sucesso: " + session.name),
                failure => Debug.LogError("Falha ao criar sessão: " + failure.Error)
            );
        }

        public async Task<List<LeaderboardPlayer>> GetLeadboardData()
        {
            var result = await repository.GetLeaderboard();
            List<LeaderboardPlayer> players = new List<LeaderboardPlayer>();

            result.Match(
                leadboard =>
                {
                    Debug.Log("Mensagem: " + leadboard.message);
                    foreach (var player in leadboard.players)
                    {
                        players.Add(player);
                    }
                },
                failure => Debug.LogError("Falha ao obter o leadboard: " + failure.Error)
            );

            return players;
        }
    }
}