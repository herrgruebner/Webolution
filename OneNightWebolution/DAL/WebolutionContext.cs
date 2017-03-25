using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using OneNightWebolution.Models;

namespace OneNightWebolution.DAL
{
    public class WebolutionContext : DbContext
    {
        public WebolutionContext() : base("WebolutionContext")
        {

        }
        public DbSet<Game> Games { get; set; }
        public DbSet<Player> Players { get; set; }
    }
}
