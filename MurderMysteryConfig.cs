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
                } }
            };
        public Dictionary<string, List<List<float>>> taskSpawns = new Dictionary<string, List<List<float>>>
        {
            {"Home", new List<List<float>>
                {
                    new List<float> { 47, 2.7f, -42 },
                    new List<float> { 40.5f, 4.5f, -39.5f },
                    new List<float> { 21.3f, 5.5f, -23.5f },
                    new List<float> { 41.5f, 6.5f, 5.8f },
                    new List<float> { 65.8f, 3.7f, -3.8f },
                    new List<float> { 93.4f, 15, 45.75f },
                    new List<float> { 120.7f, 11.9f, 88.7f },
                    new List<float> { 132.3f, -2.5f, 29.1f },
                    new List<float> { 241.4f, -5, 35.4f },
                    new List<float> { 78, -5, -87.2f },
                    new List<float> { 133, 31, 7.8f },
                    new List<float> { 146.7f, -2.3f, -22 }
                } }
        };
    }
}
