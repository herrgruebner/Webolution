using System;
using System.Web;
using Microsoft.AspNet.SignalR;
using OneNightWebolution.Models;
using OneNightWebolution.DAL;
using System.Linq;

namespace SignalRChat
{
    public class OneNightWebolutionHub : Hub
    {
        private WebolutionContext db;
        public OneNightWebolutionHub()
        {
            this.db = new WebolutionContext();
        }
        
        public void Send(string name, string message)
        {
            // Call the addNewMessageToPage method to update clients.
            Clients.All.addNewMessageToPage(name, message);
        }
        public void AddPlayerToParty(string playerName, string partyName)
        {
            Player player = new Player(playerName);
            db.Players.Add(player);

            Game game = db.Games.Where(s => s.PartyName == partyName).FirstOrDefault();
            if (game == null)
            {
                game = new Game() { PartyName = partyName };
                Clients.Caller.ShowStartButton();
            }
            game.AddPlayer(player);
            Clients.All.ShowOtherPlayer(playerName);
        }
    }
}