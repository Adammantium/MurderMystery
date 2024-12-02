using AMP;
using AMP.DedicatedServer;
using AMP.DedicatedServer.Plugins;
using AMP.Events;
using AMP.Network.Data;
using AMP.Network.Packets.Implementation;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MurderMystery
{
    public class MurderMystery : AMP_Plugin
    {
        public override string NAME => "MurderMystery";
        public override string AUTHOR => "LetsJustPlay";
        public override string VERSION => "0.1";

        private bool gameRunning = false;

        private MurderMysteryConfig config;

        private ClientData murderer;
        private ClientData detective;

        private List<ClientData> players = new List<ClientData>();
        private List<ClientData> citizens = new List<ClientData>();
        private List<ClientData> dead = new List<ClientData>();


        internal class MurderMysteryConfig : PluginConfig
        {
            public int requiredPlayerCount = 3;
            public float matchTime = 300.0f;
            public float intermissionTime = 10.0f;
        }


        public override void OnStart()
        {
            ServerEvents.onPlayerJoin += OnPlayerJoin;
            ServerEvents.onPlayerQuit += OnPlayerQuit;
            config = (MurderMysteryConfig)GetConfig();
        }

        public void OnPlayerJoin(ClientData client)
        {
            players.Add(client);
            if (gameRunning)
            {
                dead.Add(client);
                ModManager.serverInstance.netamiteServer.SendTo(
                    client,
                    new DisplayTextPacket("deadNotify", "You have joined during a match and have been automatically murdered.", Color.white, new Vector3(0,0,2), true, true, 5)
                );
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
                    dead.Remove(dead.Where(i => i.ClientId == client.ClientId).First());
                }
                catch { }
            }

        }
    }
}
