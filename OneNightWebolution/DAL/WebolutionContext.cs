using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using OneNightWebolution.Models;

namespace OneNightWebolution.DAL
{
    class WebolutionContext : DbContext
    {
        public WebolutionContext() : base("WebolutionContext")
        {

        }
        public DbSet<GameModels> Games { get; set; }
        public DbSet<PlayerModels> Players { get; set; }
    }
}
