using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using OneNightWebolution.DAL;
using OneNightWebolution.Models;
using OneNightWebolution.Repositories;
using System.Data.Entity;

namespace OneNightWebolution.Repositories
{
    public class PlayerRepository
    {
        
        private WebolutionContext context { get; set; }
        public PlayerRepository(WebolutionContext db)
        {
            context = db;
        }
        public IEnumerable<Player> Get()
        {
            return context.Players.ToList();
        }

        public Player Get(int id)
        {
            return context.Players.Find(id);
        }

        public void Add(Player entity)
        {
            context.Players.Add(entity);
            context.SaveChanges();
        }

        public void Remove(Player entity)
        {
            var obj = context.Players.Find(entity.ID);
            context.Players.Remove(obj);
            context.SaveChanges();
        }
        public void SetRole(Player entity, string role)
        {
            entity.Role = role;
            context.Entry(entity).State = EntityState.Modified;
            context.SaveChanges();
        }
        public void SetSpecialist(Player entity, string specialist)
        {
            entity.Specialist = specialist;
            context.Entry(entity).State = EntityState.Modified;
            context.SaveChanges();
        }
        public void SavePlayerChanges(Player entity)
        {
            context.Entry(entity).State = EntityState.Modified;
            context.SaveChanges();
        }
    }
}