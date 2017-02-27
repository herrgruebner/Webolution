using System;
using System.Web;
using Microsoft.AspNet.SignalR;
using OneNightWebolution.Models;
using OneNightWebolution.DAL;
using System.Linq;

namespace OneNightWebolution
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

        public async void AddPlayerToParty(string playerName, string partyName)
        {
            Player player = new Player(playerName, Context.ConnectionId);
            await Groups.Add(Context.ConnectionId, partyName);
            db.Players.Add(player);

            Game game = db.Games.Where(s => s.PartyName == partyName).FirstOrDefault();
            if (game == null)
            {
                game = new Game() { PartyName = partyName };
                db.Games.Add(game);
                Clients.Caller.ShowStartButton();
            }
            SetPartyAndPlayerNameAndID(partyName, playerName, game.ID, player.ID);
            game.AddPlayer(player);
            Clients.Group(partyName).ShowOtherPlayer(playerName);
            db.SaveChanges();
        }

        public void SetPartyAndPlayerNameAndID(string partyName, string playerName, int partyID, int gameID)
        {
            Clients.Caller.ShowPlayerName(playerName);
            Clients.Caller.ShowPartyName(partyName);
            Clients.Caller.SetPlayerID(gameID);
            Clients.Caller.SetPartyID(partyID);
        }

        public async void BeginGame(int gameID)
        {
            Game game = db.Games.First(s => s.ID == gameID);
            int numberTraitors = 3;
            int numberRebels = game.NumberPlayers;
            var group = Clients.Group(game.PartyName);

            Random r = new Random();
            string[] roles = new string[7] {"investigator","signaller","thief","reassigner","analyst","confirmer","revealer"};
            int[] roleAmounts = new int[7] { 2, 2, 2, 2, 2, 2, 2 };
            int totalRoleCards = 14;

            foreach (Player player in game.Players)
            {
                // Call functions on specific clients using Clients.Client(player.ConnectionID).functionname();
                if(r.Next(1, numberTraitors + numberRebels) <= numberTraitors)
                { // Player is traitor
                    player.Role = "traitor";
                    numberTraitors--;
                    ShowRole(player.ConnectionID, true);
                }
                else
                {
                    player.Role = "rebel";
                    numberRebels--;
                    ShowRole(player.ConnectionID, false);
                }

                int rand  = r.Next(1, totalRoleCards);
                int i = totalRoleCards;

                foreach (int currentRole in roleAmounts)
                {
                    i -= roleAmounts[currentRole];

                    if(i <= 0)
                    {
                        player.Specialist = roles[currentRole];
                        roleAmounts[currentRole]--;
                        ShowSpecialist(player.ConnectionID, player.Specialist);
                    }
                }
            }

            game.NumberTraitors = numberTraitors;
        }
        /// <summary>
        /// Shows the role (traitor/rebel) on the client page
        /// </summary>
        /// <param name="connectionID"></param>
        /// <param name="IsTraitor"></param>
        public void ShowRole(string connectionID, bool IsTraitor)
        {
            Clients.Client(connectionID).ShowRole(IsTraitor);
        }
        /// <summary>
        /// Shows the assigned specialist on the client page
        /// </summary>
        /// <param name="connectionID"></param>
        /// <param name="specialist"></param>
        public void ShowSpecialist(string connectionID, string specialist)
        {
            Clients.Client(connectionID).ShowSpecialist(specialist);
        }

    }

}