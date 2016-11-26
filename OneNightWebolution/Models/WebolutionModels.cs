using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneNightWebolution.Models
{
    class GameModels
    {
        public string PartyName { get; set; }
        public int NumberPlayers { get; set; }
        public IEnumerable<PlayerModels> Players { get; set; }
    }
    class PlayerModels
    {
        public string Name { get; set; }
        public string Role { get; set; }
        public string Specialist { get; set; }
    }
}
