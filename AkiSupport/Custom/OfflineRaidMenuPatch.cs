//using Aki.Custom.Models;
//using EFT;
//using EFT.UI;
//using EFT.UI.Matchmaker;
//using SIT.Tarkov.Core;
//using System.Reflection;
//using UnityEngine;

//namespace Aki.Custom.Patches
//{
//    public class OfflineRaidMenuPatch : ModulePatch
//    {
//        protected override MethodBase GetTargetMethod()
//        {
//            var desiredType = typeof(MatchmakerOfflineRaidScreen);
//            var desiredMethod = desiredType.GetMethod(nameof(MatchmakerOfflineRaidScreen.Show));

//            Logger.LogDebug($"{this.GetType().Name} Type: {desiredType?.Name}");
//            Logger.LogDebug($"{this.GetType().Name} Method: {desiredMethod?.Name}");

//            return desiredMethod;
//        }

//        [PatchPrefix]
//        private static void PatchPrefix(MatchmakerOfflineRaidScreen.GClass2772 controller, UpdatableToggle ____offlineModeToggle)
//        {
//            var raidSettings = controller.RaidSettings;

//            raidSettings.RaidMode = ERaidMode.Local;
//            raidSettings.BotSettings.IsEnabled = true;

//            // Default checkbox to be ticked
//            ____offlineModeToggle.isOn = true;

//            // get settings from server
//            var json = Request.Instance.GetJson("/singleplayer/settings/raid/menu");
//            var settings = Json.Deserialize<DefaultRaidSettings>(json);
//            Logger.LogDebug(json);

//            // TODO: Not all settings are used and they also don't cover all the new settings that are available client-side
//            if (settings != null)
//            {
//                raidSettings.BotSettings.BotAmount = settings.AiAmount;
//                raidSettings.WavesSettings.BotAmount = settings.AiAmount;

//                raidSettings.WavesSettings.BotDifficulty = settings.AiDifficulty;

//                raidSettings.WavesSettings.IsBosses = settings.BossEnabled;

//                raidSettings.BotSettings.IsScavWars = false;

//                raidSettings.WavesSettings.IsTaggedAndCursed = settings.TaggedAndCursed;
//            }
//        }

//        [PatchPostfix]
//        private static void PatchPostfix()
//        {
//            // disable "no progression save" panel
//            var offlineRaidScreenContent = GameObject.Find("Matchmaker Offline Raid Screen").transform.Find("Content").transform;
//            var warningPanel = offlineRaidScreenContent.Find("WarningPanelHorLayout");
//            warningPanel.gameObject.SetActive(false);
//            var spacer = offlineRaidScreenContent.Find("Space (1)");
//            spacer.gameObject.SetActive(false);
//        }
//    }
//}
