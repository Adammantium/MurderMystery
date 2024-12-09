using AMP.DedicatedServer.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MurderMystery
{
    internal class MurderMysteryConfig : PluginConfig
    {
        public int requiredPlayerCount = 3;
        public float matchTime = 300.0f;
        public float intermissionTime = 30.0f;
        public Dictionary<string, List<List<float>>> playerSpawns = new Dictionary<string, List<List<float>>> {
                {"Home", new List<List<float>> {
                    new List<float> { 37.61f, 2, -46.31f },
                    new List<float> { 75.2f, -4.8f, -86.1f },
                    new List<float> { 241f, -5.1f, 39f },
                    new List<float> { 132.4f, 11.1f, 96.4f },
                    new List<float> { 129.3f, 31.1f, -9.9f },
                    new List<float> { 93, 11, 37.3f },
                    new List<float> { 50.5f, 6, -17 },
                    new List<float> { 87.3f, 1.8f, -33.6f },
                    new List<float> { 165, 0, -33.6f },
                } },
                {"Arena", new List<List<float>>
                {
                    new List<float> { 7.18f, 0.2f, 0.03f }
                } },
                {"Canyon", new List<List<float>>
                {
                    new List<float> { 27.62f, -7, 6.54f}
                } },
                {"Citadel", new List<List<float>>
                {
                    new List<float> { 15.5f, 92.3f, 1 }
                } },
                {"Sanctuary", new List<List<float>>
                {
                    new List<float> { 0.06f, 2, 19.96f}
                } },
                {"Market", new List<List<float>>
                {
                    new List<float> { 17.19f, 0.5f, 18.52f }
                } }
            };
        public Dictionary<string, Dictionary<string, float>> taskSpawns;
    }
}
