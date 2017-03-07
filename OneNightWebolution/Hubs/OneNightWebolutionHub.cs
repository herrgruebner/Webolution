using System;
using System.Web;
using Microsoft.AspNet.SignalR;
using OneNightWebolution.Models;
using OneNightWebolution.DAL;
using System.Linq;
using OneNightWebolution.Repositories;
using System.Collections.Generic;

namespace OneNightWebolution
{
    public class OneNightWebolutionHub : Hub
    {
        private WebolutionContext db;
        private PlayerRepository pRepo;
        public OneNightWebolutionHub()
        {
            this.db = new WebolutionContext();
        }
        
        public void Send(string name, string message)
        {
            // Call the addNewMessageToPage method to update clients.
            Clients.All.addNewMessageToPage(name, message);
        }

        /// <summary>
        /// Associates a player with a game/partyname, creates if non existing.
        /// </summary>
        /// <param name="playerName"></param>
        /// <param name="partyName"></param>
        public async void AddPlayerToParty(string playerName, string partyName)
        {
            Player player = new Player(playerName, Context.ConnectionId);
            await Groups.Add(Context.ConnectionId, partyName);
            pRepo.Add(player);

            Game game = db.Games.Where(s => s.PartyName == partyName).FirstOrDefault();
            if (game == null)
            {
                game = new Game() { PartyName = partyName };
                db.Games.Add(game);
                Clients.Caller.ShowStartButton();
            }
            ShowPartyAndPlayerNameAndID(partyName, playerName, game.ID, player.ID);
            game.AddPlayer(player);
            Clients.Group(partyName).ShowOtherPlayer(playerName, player.ID);
            db.SaveChanges();
        }

        /// <summary>
        /// Shows the party name and player name and sets the IDs on the client page
        /// </summary>
        /// <param name="partyName"></param>
        /// <param name="playerName"></param>
        /// <param name="partyID"></param>
        /// <param name="gameID"></param>
        public void ShowPartyAndPlayerNameAndID(string partyName, string playerName, int partyID, int gameID)
        {
            Clients.Caller.ShowPlayerName(playerName);
            Clients.Caller.ShowPartyName(partyName);
            Clients.Caller.SetPlayerID(gameID);
            Clients.Caller.SetPartyID(partyID);
        }
        /// <summary>
        /// Called from party leader client side, locks adding players and begins the game.
        /// </summary>
        /// <param name="gameID"></param>
        public async void BeginGame(int gameID)
        {
            // Todo: add a game started lock.
            Game game = db.Games.First(s => s.ID == gameID);
            AssignRolesAndSpecialists(game);
            var group = Clients.Group(game.PartyName);
            Clients.Group(game.PartyName).ShowGameBegun();
            Clients.Client(game.Players.FirstOrDefault().ConnectionID).TakeTurn();
            ShowTraitorsToTraitors(game);
            
        }
        /// <summary>
        /// At the start of a round, reveals all traitors to the other traitors.
        /// </summary>
        /// <param name="game"></param>
        public void ShowTraitorsToTraitors(Game game)
        {
            Dictionary<int, string> idConnectionDict = new Dictionary<int, string>();
            List<string> connections = new List<string>();
            List<int> ids = new List<int>();
            List<Player> traitors = new List<Player>();
            foreach (Player player in game.Players)
            {
                if (player.Role == "traitor")
                {
                    traitors.Add(player);
                }
            }
            foreach (Player playerToReveal in traitors)
            {
                foreach (Player playerToUpdate in traitors)
                {
                    ShowAsTraitor(playerToReveal.ID, playerToUpdate.ConnectionID);
                }
            }
        }
        /// <summary>
        /// Assigns roles and specialists randomly to each player, updates database.
        /// </summary>
        /// <param name="game"></param>
        private void AssignRolesAndSpecialists(Game game)
        {
            int numberTraitors = 3;
            int numberRebels = game.NumberPlayers;
            var group = Clients.Group(game.PartyName);

            Random r = new Random();
            string[] specialisations = new string[7] { "investigator", "signaller", "thief", "reassigner", "analyst", "confirmer", "revealer" };
            Dictionary<string, int> specialisationDict = new Dictionary<string, int>() {
                { "investigator", 2 }, { "signaller", 2 },
                {"thief", 2 }, { "reassigner", 2 },
                {"analyst",2 }, {"confirmer", 2},
                {"revealer", 2 }
            };
            //int totalSpecialisationCards = 14;
            //int[] specialisationAmounts = new int[7] { 2, 2, 2, 2, 2, 2, 2 };

            foreach (Player player in game.Players)
            {
                // Call functions on specific clients using Clients.Client(player.ConnectionID).functionname();
                if (r.Next(1, numberTraitors + numberRebels) <= numberTraitors)
                { // Player is traitor
                    player.Role = "traitor";
                    numberTraitors--;
                    ShowRole(player.ConnectionID, true);
                    pRepo.SavePlayerChanges(player);
                }
                else
                {
                    player.Role = "rebel";
                    numberRebels--;
                    ShowRole(player.ConnectionID, false);
                    pRepo.SavePlayerChanges(player);
                }

                /*int rand  = r.Next(1, totalSpecialisationCards);
                foreach (int current in specialisationAmounts)
                {
                    rand -= specialisationAmounts[current];
                    if(rand <= 0)
                    {
                        player.Specialist = specialisations[current];
                        specialisationAmounts[current]--;
                        ShowSpecialist(player.ConnectionID, player.Specialist);
                        pRepo.SavePlayerChanges(player);
                    }
                }*/
                string randSpecialisation = specialisations[r.Next(0, specialisations.Length)];
                if (specialisationDict[randSpecialisation] > 0)
                {
                    player.Specialist = randSpecialisation;
                    specialisationDict[randSpecialisation] -= 1;
                    ShowSpecialist(player.ConnectionID, player.Specialist);
                    pRepo.SavePlayerChanges(player);
                }

                //totalSpecialisationCards--;
            }

            game.NumberTraitors = numberTraitors;
        }
        public void TakeSingleAction(int playerID, string specialist, int selectedID)
        {
            Player actingPlayer = pRepo.Get(playerID);
            if (actingPlayer.Specialist != specialist)
            {
                return;
            }
            /*switch (specialist)
            {
                "investigator";
                     , 
                    case "signaller", 
                    case: "thief", 
                    case: "analyst", 
                    case: "confirmer", 
                    case: "revealer"
                default:
            }*/
        }

        /// <summary>
        /// For specified connection, reveals player role identified by playerID as a traitor.
        /// Used for showing fellow traitors.
        /// </summary>
        /// <param name="playerID"></param>
        /// <param name="connectionID"></param>
        public void ShowAsTraitor(int playerID, string connectionID)
        {
            Clients.Client(connectionID).ShowOtherRole(playerID, "traitor");
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
