using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneNightWebolution.Models
{
    class Game
    {
        public int ID { get; set; }
        public string PartyName { get; set; }
        public int NumberPlayers { get; set; }
        public virtual ICollection<Player> Players { get; set; }
        public void AddPlayer(Player player)
        {
            this.Players.Add(player);
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
            this.Name = playerName;
        }
    }
}
