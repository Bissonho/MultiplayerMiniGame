using LanguageExt;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using LobbyRelaySample.RestClient;

namespace Leaderboard{ 

public class LeaderboardRepository
    {

    [Serializable]
    public class Session
    {
        public Session(
            string name,
            List<Player> players
        )
        {
            this.name = name;
            this.players = players;
        }

        public string name;
        public List<Player> players;
    }

    [Serializable]
    public class Leaderboard
        {
        public Leaderboard(
            string message,
            List<LeaderboardPlayer> players
        )
        {
            this.message = message;
            this.players = players;
        }

        public string message;
        public List<LeaderboardPlayer> players;
    }

    
    [Serializable]
    public class LeaderboardPlayer
    {
        public LeaderboardPlayer(
            int maxScore,
            string name
        )
        {
            this.name = name;
            this.maxScore = maxScore;
        }
        public string name;
        public int maxScore;
    }

    [Serializable]
    public class Player
    {
        public Player(
            int score,
            string name
        )
        {


            this.name = name;
            this.score = score;
        }
        public string name;
        public int score;
    }

    public async Task<Either<ServerFailure, Session>> CreateSession(Session session)
    {
            RestClient client = new RestClient(
            serializationOption: new JsonSerializationOption(),
            Headers: Headers()
        );

        return await client.Post<Session, Session>("/session", session);
    }
  
    private Dictionary<string, string> Headers()
    {
        return new Dictionary<string, string> {};
    }
    
    public async Task<Either<ServerFailure, Leaderboard>> GetLeaderboard()
    {

            RestClient client = new RestClient(
            serializationOption: new JsonSerializationOption(),
            Headers: Headers()
            );

        return await client.Get<Leaderboard>("/session/leaderboard");
    }
}

}