using AMP;
using AMP.DedicatedServer;
using AMP.DedicatedServer.Plugins;
using AMP.Events;
using AMP.Logging;
using AMP.Network.Data;
using AMP.Network.Data.Sync;
using AMP.Network.Packets.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace MurderMystery
{
    public class MurderMystery : AMP_Plugin
    {
        public override string NAME => "MurderMystery";
        public override string AUTHOR => "LetsJustPlay";
        public override string VERSION => "0.8";

        private bool gameRunning = false;

        private enum MatchResult {
            STALEMATE,
            CITIZEN_VICTORY,
            MURDERER_VICTORY_KILL,
            DETECTIVE_WRONG_KILL,
            MURDERER_LEFT,
            DETECTIVE_LEFT
        }

        private MatchResult matchResults = MatchResult.STALEMATE;

        private MurderMysteryConfig config;

        private ClientData murderer;
        private ClientData detective;

        private List<ClientData> citizens = new List<ClientData>();
        private List<ClientData> deadPlayers = new List<ClientData>();

        private Dictionary<int, int> playerScores = new Dictionary<int, int>();

        private CancellationTokenSource deadCancelTokenSource;
        private CancellationTokenSource matchCancelTokenSource;
        private CancellationToken deadCancelToken;
        private CancellationToken matchCancelToken;


        private void deadTick(CancellationToken cancellationToken)
        {
            while (gameRunning && !cancellationToken.IsCancellationRequested)
            {
                List<ClientData> localClients = deadPlayers;
                int[] deadPlayerIds = localClients.Select(i => i.ClientId).ToArray();
                foreach (ClientData client in localClients)
                {
                    Vector3 pos = client.player.Position;
                    pos.y += 1000;
                    ModManager.serverInstance.netamiteServer.SendToAllExcept(
                        new PlayerPositionPacket(
                            playerId: client.ClientId,
                            handLeftPos: new Vector3(),
                            handLeftRot: new Vector3(),
                            handRightPos: new Vector3(),
                            handRightRot: new Vector3(),
                            headPos: new Vector3(),
                            headRot: new Vector3(),
                            playerPos: pos,
                            playerRot: 0
                        ),
                        deadPlayerIds
                    );
                }
                Thread.Sleep(100);
            }
        }

        private void matchLoop(CancellationToken cancellationToken)
        {
            float timer = config.matchTime;
            while (timer > 0 && !cancellationToken.IsCancellationRequested)
            {
                ModManager.serverInstance.netamiteServer.SendToAll(
                    new DisplayTextPacket("matchTimer", $"{timer}", Color.white, new Vector3(-1, 0, 2), true, true, 2)
                );
                //foreach (var client in playerScores)
                //{
                //    ModManager.serverInstance.netamiteServer.SendTo(
                //        client.Key,
                //        new DisplayTextPacket("citizenScore", $"{client.Value}", Color.white, new Vector3(-1, 0, 4), true, true, 1)
                //    );
                //}
                Thread.Sleep(1000);
                timer--;
            }

            deadCancelTokenSource.Cancel();
            if (timer == 0) {
                matchResults = 0;
            }
            switch (matchResults) {
                case MatchResult.STALEMATE:
                    ModManager.serverInstance.netamiteServer.SendToAll(
                        new DisplayTextPacket("endGameNotify", "Stalemate\nThe Murderer failed to kill everyone and the\nDetective failed to find the Murderer.", Color.white, new Vector3(0, 0, 2), true, true, 5)
                    );
                    break;
                case MatchResult.CITIZEN_VICTORY:
                    ModManager.serverInstance.netamiteServer.SendToAll(
                        new DisplayTextPacket("endGameNotify", $"Citizens Victory!\nThe Murderer has been stopped! The town is saved!", Color.blue, new Vector3(0, 0, 2), true, true, 5)
                    );
                    break;
                case MatchResult.MURDERER_VICTORY_KILL:
                    ModManager.serverInstance.netamiteServer.SendToAll(
                        new DisplayTextPacket("endGameNotify", $"Murderer Victory!\nThe Murderer has killed everyone!", Color.red, new Vector3(0, 0, 2), true, true, 5)
                    );
                    break;
                case MatchResult.DETECTIVE_WRONG_KILL:
                    ModManager.serverInstance.netamiteServer.SendToAll(
                        new DisplayTextPacket("endGameNotify", $"Murderer Victory!\nThe Detective has slain a citizen,\nmeaning the Murderer wins! For some reason.", Color.red, new Vector3(0, 0, 2), true, true, 5)
                    );
                    break;
                case MatchResult.MURDERER_LEFT:
                    ModManager.serverInstance.netamiteServer.SendToAll(
                        new DisplayTextPacket("endGameNotify", $"Stalemate\nThe Murderer left the game.", Color.white, new Vector3(0, 0, 2), true, true, 5)
                    );
                    break;
                case MatchResult.DETECTIVE_LEFT:
                    ModManager.serverInstance.netamiteServer.SendToAll(
                        new DisplayTextPacket("endGameNotify", $"Stalemate\nThe Detective left the game.\nThe Murderer left the game.", Color.white, new Vector3(0, 0, 2), true, true, 5)
                    );
                    break;
                default: break;
            }

            Thread.Sleep(5000);
            
            if (ModManager.serverInstance.connectedClients >= config.requiredPlayerCount) {
                Thread intermission = new Thread(intermissionLoop);
                intermission.Start();
            } else {
                gameRunning = false;
            }
        }

        private void intermissionLoop() {
            
            gameRunning = true;
            
            float timer = config.intermissionTime;
            while (timer > 0) {
                ModManager.serverInstance.netamiteServer.SendToAll(
                    new DisplayTextPacket("intermissionTimer", $"The match will start in {timer}.", Color.white, new Vector3(-1, 0, 2), true, true, timer)
                );
                Thread.Sleep(1000);
                timer--;
            }

            System.Random rand = new System.Random();

            murderer = ModManager.serverInstance.Clients[rand.Next(ModManager.serverInstance.Clients.Length)];
            murderer.SetDamageMultiplicator(10);
            Log.Info($"{murderer.ClientName} is the murderer");

            ClientData tryDetective = ModManager.serverInstance.Clients[rand.Next(ModManager.serverInstance.Clients.Length)];
            while (tryDetective.ClientId == murderer.ClientId) {
                tryDetective = ModManager.serverInstance.Clients[rand.Next(ModManager.serverInstance.Clients.Length)];
            }
            detective = tryDetective;
            detective.SetDamageMultiplicator(10);
            Log.Info($"{detective.ClientName} is the detective");

            citizens.Clear();
            playerScores.Clear();
            foreach (ClientData client in ModManager.serverInstance.Clients)
            {
                if (client.ClientId != murderer.ClientId && client.ClientId != detective.ClientId)
                {
                    citizens.Add(client);
                    client.SetDamageMultiplicator(0);
                    Log.Info($"{client.ClientName} is a citizen");
                }
            }
            deadPlayers.Clear();

            foreach (ClientData client in citizens) 
            {
                playerScores.Add(client.ClientId, 0);
                client.ShowText("citizenNotify", "You are a Citizen.\nStay alive and help the detective.", 0, Color.white, 10);
                client.ShowText("role", $"Citizen", -0.8f, Color.green, config.matchTime); // Display it for the whole match time
            }
            
            murderer.ShowText("murdererNotify", "You are the Murderer.\nKill all the other players and avoid being caught by the detective.", 0, Color.red, 10);
            murderer.ShowText("role", $"Murderer", -0.8f, Color.red, config.matchTime); // Display it for the whole match time

            detective.ShowText("murdererNotify", "You are the Detective.\nProtect the Citizens and find the Murderer.", 0, Color.blue, 10);
            detective.ShowText("role", $"Detective", -0.8f, Color.blue, config.matchTime); // Display it for the whole match time

            Thread.Sleep(1000);
            deadCancelTokenSource = new CancellationTokenSource();
            deadCancelToken = deadCancelTokenSource.Token;
            matchCancelTokenSource = new CancellationTokenSource();
            matchCancelToken = matchCancelTokenSource.Token;

            if (config.playerSpawns.ContainsKey(ModManager.serverInstance.currentLevel)) {
                List<List<float>> playerSpawns = config.playerSpawns[ModManager.serverInstance.currentLevel];
                foreach (ClientData client in ModManager.serverInstance.Clients) {
                    List<float> randomSpawn = playerSpawns[rand.Next(playerSpawns.Count())];
                    Vector3 startPos = new Vector3(randomSpawn[0], randomSpawn[1], randomSpawn[2]);
                    client.Teleport(startPos);
                }
            }

            Thread match = new Thread(() => matchLoop(matchCancelToken));
            Thread deadLoop = new Thread(() => deadTick(deadCancelToken));
            match.Start();
            deadLoop.Start();
        }

        public override void OnStart()
        {
            ServerEvents.onPlayerJoin += OnPlayerJoin;
            ServerEvents.onPlayerQuit += OnPlayerQuit;
            ServerEvents.onPlayerKilled += OnPlayerKilled;

            config = (MurderMysteryConfig)GetConfig();

            ServerInit.SetGameModeOverride("Murder-Mystery");
        }

        public override void OnStop()
        {
            if(matchCancelTokenSource != null) matchCancelTokenSource.Cancel();
            if(deadCancelTokenSource != null) deadCancelTokenSource.Cancel();
        }

        public void OnPlayerKilled(ClientData killed, ClientData killer)
        {
            if (killed.ClientId == murderer.ClientId)
            {
                matchResults = MatchResult.CITIZEN_VICTORY;
                matchCancelTokenSource.Cancel();
            }
            else if (killed.ClientId == detective.ClientId)
            {
                deadPlayers.Add(killed);
                killed.SetDamageMultiplicator(0);
                System.Random rand = new System.Random();

                if (citizens.Count > 0)
                {
                    detective = citizens[rand.Next(citizens.Count)];
                    citizens.Remove(detective);
                    detective.SetDamageMultiplicator(10);
                    
                    detective.ShowText("newDetectiveNotify", "The previous Detective has been murdered\nand you are the new Detective.", 0, Color.blue, 4);
                    detective.ShowText("role", $"Detective", 0.8f, Color.blue, 4);
                }
                else
                {
                    detective = null;
                }
            }
            else if (killer != null && killer.ClientId == detective.ClientId)
            {
                matchResults = MatchResult.DETECTIVE_WRONG_KILL;
                matchCancelTokenSource.Cancel();
            }
            else if (deadPlayers.Where(i => i.ClientId == killed.ClientId).FirstOrDefault() == null)
            {
                deadPlayers.Add(killed);
                citizens.Remove(citizens.Where(i => i.ClientId == killed.ClientId).First());
            }

            if (citizens.Count == 0 && detective == null)
            {
                matchResults = MatchResult.MURDERER_VICTORY_KILL;
                matchCancelTokenSource.Cancel();
            }
        }

        public void OnPlayerJoin(ClientData client)
        {
            if (gameRunning) {
                deadPlayers.Add(client);
                client.SetDamageMultiplicator(0);
                
                client.ShowText("deadNotify", "You have joined during a match and have been automatically murdered.", 0, Color.white, 5);
            } else {
                client.ShowText("playerJoinNotify", $"{client.ClientName} has joined the server.\n\n{ModManager.serverInstance.connectedClients}/{config.requiredPlayerCount}", 0, Color.white, 5);

                Thread.Sleep(1000);

                if (ModManager.serverInstance.connectedClients >= config.requiredPlayerCount) {
                    ModManager.serverInstance.netamiteServer.SendToAll(
                        new DisplayTextPacket("matchStart", $"Enough players have joined. Starting match.", Color.white, new Vector3(0, 0, 2), true, true, 1)
                    );

                    Thread intermission = new Thread(intermissionLoop);
                    intermission.Start();
                }
            }
            
            client.SendPacket(new BookAvailabilityPacket(false, false));
        }

        public void OnPlayerQuit(ClientData client) {
            try
            {
                citizens.Remove(citizens.Where(i => i.ClientId == client.ClientId).First());
            }
            catch
            {
                try
                {
                    deadPlayers.Remove(deadPlayers.Where(i => i.ClientId == client.ClientId).First());
                }
                catch { }
            }
            
            if (client == murderer || client == detective) {
                matchResults = client == murderer ? MatchResult.MURDERER_LEFT : MatchResult.DETECTIVE_LEFT;
                matchCancelTokenSource.Cancel();
            }
        }
    }
}
