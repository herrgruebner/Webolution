using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;


namespace OneNightWebolution.Models
{
    public class Game
    {
        public Game()
        {
            Players = new List<Player>();
            NumberPlayers = 0;
        }
        public int ID { get; set; }
        public int LeaderID { get; set; }
        public string PartyName { get; set; }
        public int NumberPlayers { get; set; }
        public int NumberTraitors { get; set; }
        public virtual List<Player> Players { get; set; }
        public void AddPlayer(Player player)
        {
            Players.Add(player);
            NumberPlayers += 1;
        }
    }
    public class Player
    {
        public int ID { get; set; }
        public int PositionInGame { get; set; }
        public string ConnectionID { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public string Specialist { get; set; }
        public int GameID { get; set; }
        public int votingForID { get; set; }
        public virtual Game Game { get; set; }
        public Player(string playerName, string connectionID)
        {
            Name = playerName;
            ConnectionID = connectionID;
        }
        public Player()
        {

        }
        
    }
}
