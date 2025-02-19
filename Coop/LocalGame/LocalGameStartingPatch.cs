﻿//using CoopTarkovGameServer;
using BepInEx.Configuration;
using Comfort.Common;
using EFT;
using SIT.Coop.Core.Matchmaker;
using SIT.Core.Configuration;
using SIT.Core.Coop;
using SIT.Core.Coop.FreeCamera;
using SIT.Core.Core;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

namespace SIT.Coop.Core.LocalGame
{
    /// <summary>
    /// Target that smethod_3 like
    /// </summary>
    public class LocalGameStartingPatch : ModulePatch
    {
        //public static EchoGameServer gameServer;
        private static ConfigFile _config;

        //private static LocalGameSpawnAICoroutinePatch gameSpawnAICoroutinePatch;

        public LocalGameStartingPatch(ConfigFile config)
        {
            _config = config;
            //gameSpawnAICoroutinePatch = new SIT.Coop.Core.LocalGame.LocalGameSpawnAICoroutinePatch(_config);
        }

        public static TimeAndWeatherSettings TimeAndWeather { get; internal set; }



        protected override MethodBase GetTargetMethod()
        {
            //foreach(var ty in SIT.Tarkov.Core.PatchConstants.EftTypes.Where(x => x.Name.StartsWith("BaseLocalGame")))
            //{
            //    Logger.LogInfo($"LocalGameStartingPatch:{ty}");
            //}
            //_ = typeof(EFT.BaseLocalGame<GamePlayerOwner>);

            //var t = SIT.Tarkov.Core.PatchConstants.EftTypes.FirstOrDefault(x => x.FullName.StartsWith("EFT.LocalGame"));
            //var t = typeof(EFT.LocalGame);
            var t = typeof(EFT.BaseLocalGame<GamePlayerOwner>);
            if (t == null)
                Logger.LogInfo($"LocalGameStartingPatch:Type is NULL");

            var method = ReflectionHelpers.GetAllMethodsForType(t, false)
                .FirstOrDefault(x => x.GetParameters().Length >= 3
                && x.GetParameters().Any(x => x.Name.Contains("botsSettings"))
                && x.GetParameters().Any(x => x.Name.Contains("backendUrl"))
                && x.GetParameters().Any(x => x.Name.Contains("runCallback"))
                );

            Logger.LogInfo($"LocalGameStartingPatch:{t.Name}:{method.Name}");
            return method;
        }

        [PatchPostfix]
        public static async void PatchPostfix(
            AbstractGame __instance
            , Task __result
            )
        {
            await __result;

            if (__instance is HideoutGame)
            {
                return;
            }

            if (__instance is CoopGame coopGame)
            {
                coopGame.CreateCoopGameComponent();
            }

            //LocalGamePatches.LocalGameInstance = __instance;
            var gameWorld = Singleton<GameWorld>.Instance;
            if (gameWorld == null)
            {
                Logger.LogError("GameWorld is NULL");
                return;
            }

            if (!MatchmakerAcceptPatches.IsClient)
            {
                Dictionary<string, object> packet = new Dictionary<string, object>
                {
                    { "m", "timeAndWeather" },
                    { "t", DateTime.Now.Ticks.ToString("G") },
                    { "ct", TimeAndWeather.CloudinessType },
                    { "ft", TimeAndWeather.FogType },
                    { "hod", TimeAndWeather.HourOfDay },
                    { "rt", TimeAndWeather.RainType },
                    { "tft", TimeAndWeather.TimeFlowType },
                    { "wt", TimeAndWeather.WindType },
                    { "serverId", CoopGameComponent.GetServerId() }
                };
                AkiBackendCommunication.Instance.PostJson("/coop/server/update", packet.ToJson(), timeout: 9999, debug: true);
            }

            SendOrReceiveSpawnPoint(Singleton<GameWorld>.Instance.MainPlayer);

            CoopPatches.EnableDisablePatches();


            // Add FreeCamController to GameWorld GameObject
            gameWorld.gameObject.GetOrAddComponent<FreeCameraController>();


        }

        public static void SendOrReceiveSpawnPoint(EFT.Player player)
        {
            Logger.LogDebug(player.ProfileId + " " + player.Profile.Nickname);
            if (!player.ProfileId.StartsWith("pmc"))
                return;

            var position = player.Transform.position;
            if (!Matchmaker.MatchmakerAcceptPatches.IsClient)
            {
                Dictionary<string, object> packet = new Dictionary<string, object>
                {
                    {
                        "m",
                        "SpawnPointForCoop"
                    },
                    {
                        "serverId",
                        CoopGameComponent.GetServerId()
                    },
                    {
                        "x",
                        position.x
                    },
                    {
                        "y",
                        position.y
                    },
                    {
                        "z",
                        position.z
                    }
                };
                Logger.LogInfo("Setting Spawn Point to " + position);
                AkiBackendCommunication.Instance.PostJson("/coop/server/update", packet.ToJson());
                //var json = Request.Instance.GetJson($"/coop/server/spawnPoint/{CoopGameComponent.GetServerId()}");
                //Logger.LogInfo("Retreived Spawn Point " + json);
            }
            else if (Matchmaker.MatchmakerAcceptPatches.IsClient)
            {
                if (PluginConfigSettings.Instance.CoopSettings.AllPlayersSpawnTogether)
                {
                    var json = AkiBackendCommunication.Instance.GetJson($"/coop/server/spawnPoint/{CoopGameComponent.GetServerId()}");
                    Logger.LogInfo("Retreived Spawn Point " + json);
                    var retrievedPacket = json.ParseJsonTo<Dictionary<string, string>>();
                    var x = float.Parse(retrievedPacket["x"].ToString());
                    var y = float.Parse(retrievedPacket["y"].ToString());
                    var z = float.Parse(retrievedPacket["z"].ToString());
                    var teleportPosition = new Vector3(x, y, z);
                    player.Teleport(teleportPosition, true);
                }
            }
            //}
        }

    }
}
