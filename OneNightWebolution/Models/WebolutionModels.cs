using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneNightWebolution.Models
{
    class Game
    {
        public Game()
        {
            Players = new List<Player>();
            NumberPlayers = 0;
        }
        public int ID { get; set; }
        public string PartyName { get; set; }
        public int NumberPlayers { get; set; }
        public virtual List<Player> Players { get; set; }
        public void AddPlayer(Player player)
        {
            Players.Add(player);
            NumberPlayers += 1;
        }
    }
    class Player
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public string Specialist { get; set; }
        public int GameID { get; set; }
        public virtual Game Game { get; set; }
        public Player(string playerName)
        {
            Name = playerName;
        }
    }
}
