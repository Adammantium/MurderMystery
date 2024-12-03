using AMP;
using AMP.DedicatedServer;
using AMP.DedicatedServer.Plugins;
using AMP.Events;
using AMP.Network.Data;
using AMP.Network.Packets.Implementation;
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
        public override string VERSION => "0.2";

        private bool gameRunning = false;

        private MurderMysteryConfig config;

        private ClientData murderer;
        private ClientData detective;

        private List<ClientData> players = new List<ClientData>();
        private List<ClientData> citizens = new List<ClientData>();
        private List<ClientData> deadPlayers = new List<ClientData>();

        private CancellationTokenSource cancellationTokenSource;
        private CancellationToken cancellationToken;


        internal class MurderMysteryConfig : PluginConfig
        {
            public int requiredPlayerCount = 3;
            public float matchTime = 300.0f;
            public float intermissionTime = 10.0f;
        }


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

        private void matchLoop()
        {

        }

        private void intermissionLoop()
        {

        }

        public override void OnStart()
        {
            cancellationTokenSource = new CancellationTokenSource();
            cancellationToken = cancellationTokenSource.Token;
            ServerEvents.onPlayerJoin += OnPlayerJoin;
            ServerEvents.onPlayerQuit += OnPlayerQuit;
            config = (MurderMysteryConfig)GetConfig();
        }

        public void OnPlayerJoin(ClientData client)
        {
            players.Add(client);
            if (gameRunning)
            {
                deadPlayers.Add(client);
                client.SetDamageMultiplicator(0);
                ModManager.serverInstance.netamiteServer.SendTo(
                    client,
                    new DisplayTextPacket("deadNotify", "You have joined during a match and have been automatically murdered.", Color.white, new Vector3(0,0,2), true, true, 5)
                );
            }
            else
            {
                ModManager.serverInstance.netamiteServer.SendToAll(
                    new DisplayTextPacket("playerJoinNotify", $"{client.ClientName} has joined the server.\n{players.Count}/{config.requiredPlayerCount}", Color.white, new Vector3(0,0,2), true, true, 1)
                );

                Thread.Sleep(1000);

                if (players.Count >= config.requiredPlayerCount)
                {
                    ModManager.serverInstance.netamiteServer.SendToAll(
                        new DisplayTextPacket("matchStart", $"Enough players have joined. Starting game.", Color.white, new Vector3(0, 0, 2), true, true, 1)
                    );
                }
            }
        }

        public void OnPlayerQuit(ClientData client)
        {
            players.Remove(players.Where(i => i.ClientId == client.ClientId).First());

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

        }
    }
}
