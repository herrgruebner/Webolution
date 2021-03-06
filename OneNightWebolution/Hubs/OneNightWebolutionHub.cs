﻿using System;
using System.Web;
using Microsoft.AspNet.SignalR;
using OneNightWebolution.Models;
using OneNightWebolution.DAL;
using System.Linq;
using OneNightWebolution.Repositories;
using System.Collections.Generic;
using System.Diagnostics;

namespace OneNightWebolution
{
    public class OneNightWebolutionHub : Hub
    {
        private WebolutionContext db;
        private PlayerRepository pRepo;

        private string rebel = "rebel";
        private string traitor = "traitor";

        private const string investigator = "investigator";
        private const string thief = "thief";
        private const string analyst = "analyst";
        private const string confirmer = "confirmer";
        private const string revealer = "revealer";
        private const string reassigner = "reassigner";
        private const string signaller = "signaller";
        public OneNightWebolutionHub()
        {
            this.db = new WebolutionContext();
            this.pRepo = new PlayerRepository(db);
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
            bool isLeader = false;
            Game game = db.Games.FirstOrDefault(s => s.PartyName == partyName);
            if (game == null)
            {
                game = new Game() { PartyName = partyName };
                db.Games.Add(game);
                Clients.Caller.ShowStartButton();
                isLeader = true;
                
            }
            else
            {
                ShowCurrentlyConnectedPlayersOnJoin(partyName); 
            }
            player.Game = game;
            player.GameID = game.ID;
            player.PositionInGame = game.NumberPlayers;
            
            pRepo.Add(player);

            ShowPartyAndPlayerNameAndID(partyName, playerName, game.ID, player.ID);
            if (isLeader)
            {
                game.LeaderID = player.ID;
            }
            game.AddPlayer(player);
            Clients.Group(partyName).ShowOtherPlayer(playerName, player.ID);
            db.SaveChanges();
        }
        /// <summary>
        /// When a player joins, show them all the players who joined before
        /// </summary>
        private void ShowCurrentlyConnectedPlayersOnJoin(string partyName)
        {
            Game game = db.Games.FirstOrDefault(s => s.PartyName == partyName);
            foreach (Player player in game.Players)
            {
                Clients.Caller.ShowOtherPlayer(player.Name, player.ID);
            }
        }
        /// <summary>
        /// Shows the party name and player name and sets the IDs on the client page
        /// </summary>
        /// <param name="partyName"></param>
        /// <param name="playerName"></param>
        /// <param name="partyID"></param>
        /// <param name="gameID"></param>
        private void ShowPartyAndPlayerNameAndID(string partyName, string playerName, int partyID, int gameID)
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
        public void BeginGame(int gameID)
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
        private void ShowTraitorsToTraitors(Game game)
        {
            Dictionary<int, string> idConnectionDict = new Dictionary<int, string>();
            List<string> connections = new List<string>();
            List<int> ids = new List<int>();
            List<Player> traitors = new List<Player>();
            foreach (Player player in game.Players)
            {
                if (player.Role == traitor)
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
            foreach (Player player in game.Players)
            {
                if (r.Next(1, numberTraitors + numberRebels) <= numberTraitors)
                { // Player is traitor
                    player.Role = traitor;
                    numberTraitors--;
                    ShowRole(player.ConnectionID, true);
                    pRepo.SavePlayerChanges(player);
                }
                else
                {
                    player.Role = rebel;
                    numberRebels--;
                    ShowRole(player.ConnectionID, false);
                    pRepo.SavePlayerChanges(player);
                }
                bool notAssigned = true;

                while (notAssigned) // loop until we find an available specialisation.
                {
                    string randSpecialisation = specialisations[r.Next(0, specialisations.Length)];
                    if (specialisationDict[randSpecialisation] > 0)
                    {
                        player.Specialist = randSpecialisation;
                        specialisationDict[randSpecialisation] -= 1;
                        ShowSpecialist(player.ConnectionID, player.Specialist);
                        pRepo.SavePlayerChanges(player);
                        notAssigned = false;
                    }

                }
            }

            game.NumberTraitors = numberTraitors;
        }

        public void TakeReassignRebelAction(string partyName, int playerID, string specialist, int toSwap1, int toSwap2)
        {
            if (specialist != reassigner)
            {
                return;
            }
            Player actingPlayer = pRepo.Get(playerID);
            if (specialist != actingPlayer.Specialist)
            {
                return;
            }
            Player playerToSwap1 = pRepo.Get(toSwap1);
            Player playerToSwap2 = pRepo.Get(toSwap2);

            SwapPlayerRolesInDB(playerToSwap1, playerToSwap2);
            TakeNextTurn(db.Games.FirstOrDefault(s => s.PartyName == partyName), playerID);

        }
        public void TakeReassignTraitorAction(string partyName, int playerID, string specialist, int toSwap1)
        {
            Game game = db.Games.FirstOrDefault(s => s.PartyName == partyName);
            if (game.NumberTraitors == 0)
            {
                Clients.Client(Context.ConnectionId).showFullTraitorsMessage();
            }
            else
            {
                Player toTraitor = pRepo.Get(toSwap1);
                toTraitor.Role = traitor;
                pRepo.SavePlayerChanges(toTraitor);
                ShowAsTraitor(toSwap1, Context.ConnectionId);
            }
            TakeNextTurn(game, playerID);
        }
        public void TakeSingleAction(string partyName, int playerID, string specialist, int selectedID)
        {
            Debug.WriteLine("single action");
            Debug.WriteLine("specialist " + specialist);
            Debug.WriteLine("playerid " + playerID );
            Debug.WriteLine("selectedid " + selectedID);
            Player actingPlayer = pRepo.Get(playerID);
            Player selectedPlayer = pRepo.Get(selectedID);
            Game game = db.Games.FirstOrDefault(s => s.PartyName == partyName);
            if (actingPlayer.Specialist != specialist)
            {
                return;
            }
            switch (specialist)
            {
                case investigator:
                    if (selectedPlayer.Role == traitor)
                    {
                        ShowAsTraitor(selectedID, Context.ConnectionId);
                    }
                    else
                    {
                        ShowAsRebel(selectedID, Context.ConnectionId);
                    }
                    break;
                case thief:
                    if (actingPlayer.Role == rebel)
                    {
                        SwapPlayerRolesInDB(actingPlayer, selectedPlayer);
                        Clients.Caller.ShowRole(actingPlayer.Role);
                    }
                    break;
                case analyst:
                    ShowSpecialist(Context.ConnectionId, selectedPlayer.Specialist, selectedPlayer.ID);
                    break;
                case confirmer:
                    if (actingPlayer.Role == traitor)
                    {
                        ShowRole(Context.ConnectionId, true);
                    }
                    else
                    {
                        ShowRole(Context.ConnectionId, false);
                    }
                    break;
                case revealer:
                    if (actingPlayer.Role == rebel)
                    {
                        if (selectedPlayer.Role == rebel)
                        {
                            ShowRoleToAll(game.PartyName, selectedPlayer.ID, selectedPlayer.Role);
                        }
                    }
                    break;
                case signaller:
                    Clients.Client(selectedPlayer.ConnectionID).ShowTapMessage(actingPlayer.Name);
                    break;
                default:
                    break;
            }
            TakeNextTurn(game, playerID);
        }

        
        /// <summary>
        /// Swaps two player's roles and updates the database
        /// </summary>
        /// <param name="actingPlayer"></param>
        /// <param name="selectedPlayer"></param>
        private void SwapPlayerRolesInDB(Player actingPlayer, Player selectedPlayer)
        {
            string roleToSwap = actingPlayer.Role;
            actingPlayer.Role = selectedPlayer.Role;
            selectedPlayer.Role = roleToSwap;
            pRepo.SavePlayerChanges(actingPlayer);
            pRepo.SavePlayerChanges(selectedPlayer);
        }
        private void TakeNextTurn(Game game, int playerID)
        {
            Player nextPlayer = GetNextPlayer(game, pRepo.Get(playerID));
            if (nextPlayer != null)
            {
                Debug.WriteLine(nextPlayer.Name);
                Clients.Client(nextPlayer.ConnectionID).TakeTurn();
            }
            else
            {
                Debug.WriteLine("Accusation phase reached");
                Player leader = pRepo.Get(game.LeaderID);
                Clients.Group(game.PartyName).AddClickToVoteHandlers();
                Clients.Client(leader.ConnectionID).AddEndGameButton();
                Clients.Group(game.PartyName).SetGameStateFromServer("Accusation phase");
            }
        }

        public void AddVote(int playerID, int selectedID)
        {
            Player actingPlayer = pRepo.Get(playerID);
            actingPlayer.votingForID = selectedID;
            pRepo.SavePlayerChanges(actingPlayer);
        }

        public void EndGame(int gameID)
        {
            Game game = db.Games.FirstOrDefault(s => s.ID == gameID);
            Dictionary<int, int> voteDict = new Dictionary<int, int>();

            //Check if everyone has voted
            foreach(Player player in game.Players)
            {
                if(player.votingForID == 0)
                {
                    return;
                }

                if(voteDict.Keys.Contains(player.ID))
                {
                    voteDict[player.ID]++;
                }
                else
                {
                    voteDict[player.ID] = 1;
                }
            }

            var toKills = voteDict.Where(x => x.Value == voteDict.Values.Max());
            Boolean traitorsWin = true;
            foreach (var toKill in toKills)
            {
                Player player = pRepo.Get(toKill.Key);

                if(player.Role == "traitor")
                {
                    traitorsWin = false;
                }
            }

            if(traitorsWin)
            {
                Clients.Group(game.PartyName).setGameStateFromServer("Traitors Win");
            }
            else
            {
                Clients.Group(game.PartyName).setGameStateFromServer("Rebels Win");
            }
        }

        /// <summary>
        /// For specified connection, reveals player role identified by playerID as a traitor.
        /// Used for showing fellow traitors.
        /// </summary>
        /// <param name="playerID"></param>
        /// <param name="connectionID"></param>
        private void ShowAsTraitor(int playerID, string connectionID)
        {
            Clients.Client(connectionID).ShowOtherRole(playerID, traitor);
        }
        /// <summary>
        /// For specified connection, reveals player's (identified by playerID) role as a traitor. 
        /// </summary>
        /// <param name="playerID"></param>
        /// <param name="connectionID"></param>
        private void ShowAsRebel(int playerID, string connectionID)
        {
            Clients.Client(connectionID).ShowOtherRole(playerID, rebel);
        }
        private void ShowRoleToAll(string partyName, int playerID, string role)
        {
            Clients.Group(partyName).ShowOtherRole(playerID, role);
        }
        /// <summary>
        /// Shows the role (traitor/rebel) on the client page
        /// </summary>
        /// <param name="connectionID"></param>
        /// <param name="IsTraitor"></param>
        private void ShowRole(string connectionID, bool IsTraitor)
        {
            Clients.Client(connectionID).ShowRole(IsTraitor);
        }

        /// <summary>
        /// Shows the clients assigned specialist on the client page
        /// </summary>
        /// <param name="connectionID"></param>
        /// <param name="specialist"></param>
        private void ShowSpecialist(string connectionID, string specialist)
        {
            Clients.Client(connectionID).ShowSpecialist(specialist);
        }
        /// <summary>
        /// Shows the assigned specialist to the defined connected client for the selected other player
        /// </summary>
        /// <param name="connectionID"></param>
        /// <param name="specialist"></param>
        /// <param name="playerID"></param>
        private void ShowSpecialist(string connectionID, string specialist, int playerID)
        {
            Clients.Client(connectionID).ShowOtherSpecialist(specialist, playerID); // showOtherSpecialist not showSpecialist because js doesn't do method overloads
        }

        private Player GetNextPlayer(Game game, Player currentPlayer)
        {
            int positionNumber = currentPlayer.PositionInGame;
            if (currentPlayer.PositionInGame == game.NumberPlayers-1)
            {
                return null;
            }
            var nextPlayer = game.Players.SkipWhile(s => s.PositionInGame != positionNumber + 1).FirstOrDefault();
            return nextPlayer;
           
        }
    }
}
