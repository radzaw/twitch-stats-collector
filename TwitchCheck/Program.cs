namespace TwitchCheck
{
    using Newtonsoft.Json.Linq;
    using System;
    using System.Linq;
    using System.Net;

    class Program
    {
        static void Main(string[] args)
        {
            WebClient webClient = new WebClient();

            webClient.Headers[HttpRequestHeader.UserAgent] = "TwitchStatsCollector/1.0";
            string content = webClient.DownloadString("https://api.twitch.tv/kraken/games/top");

            dynamic contentParsed = JObject.Parse(content);

            // for now it works only for first top 10

            dynamic games = contentParsed.top;

            TwitchStatsEntities database = new TwitchStatsEntities();

            foreach (dynamic gameData in games)
            {
                int viewers = gameData.viewers;
                int channels = gameData.channels;

                dynamic gameInfo = gameData.game;
                int gameId = gameInfo._id;

                // get game from database
                Game gameInDb = (from g in database.Game where g.Id == gameId select g).FirstOrDefault();

                if (gameInDb == null)
                {
                    gameInDb = new Game();
                    gameInDb.Id = gameId;
                    gameInDb.Name = gameInfo.name;
                    gameInDb.ImageUrl = gameInfo.box.large;
                    database.Game.Add(gameInDb);
                }

                // add to stats

                GameStat stat = new GameStat();
                stat.Channels = channels;
                stat.Viewers = viewers;
                stat.CheckDate = DateTime.Now;

                gameInDb.GameStat.Add(stat);

                // save everything

                database.SaveChanges();
            }
        }
    }
}
