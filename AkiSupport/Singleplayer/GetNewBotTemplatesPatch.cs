using EFT;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SIT.Core.AkiSupport.Singleplayer
{
    public class GetNewBotTemplatesPatch : ModulePatch
    {
        private static MethodInfo _getNewProfileMethod;

        public GetNewBotTemplatesPatch()
        {
            _getNewProfileMethod = ReflectionHelpers.GetMethodForType(typeof(LocalGameBotCreator), nameof(LocalGameBotCreator.GetNewProfile));
        }

        protected override MethodBase GetTargetMethod()
        {
            return ReflectionHelpers.GetMethodForType(typeof(LocalGameBotCreator), nameof(LocalGameBotCreator.CreateProfile));
        }

        [PatchPrefix]
        private static bool PatchPrefix(ref Task<Profile> __result, BotsPresets __instance, CreationData data, bool withDelete)
        {
            if (PatchConstants.BackEndSession == null)
            {
                Logger.LogDebug("GetNewBotTemplatesPatch BackEndSession is NULL?");
                return true;
            }

            GetLogger(typeof(GetNewBotTemplatesPatch)).LogDebug("GetNewBotTemplatesPatch.PatchPrefix");

            /*
                in short when client wants new bot and GetNewProfile() return null (if not more available templates or they don't satisfy by Role and Difficulty condition)
                then client gets new piece of WaveInfo collection (with Limit = 30 by default) and make request to server
                but use only first value in response (this creates a lot of garbage and cause freezes)
                after patch we request only 1 template from server

                along with other patches this one causes to call data.PrepareToLoadBackend(1) gets the result with required role and difficulty:
                new[] { new WaveInfo() { Limit = 1, Role = role, Difficulty = difficulty } }
                then perform request to server and get only first value of resulting single element collection
            */

            var taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            var taskAwaiter = (Task<Profile>)null;

            try
            {
                var profile = _getNewProfileMethod.Invoke(__instance, new object[] { data, true });
            }
            catch (Exception e)
            {
                Logger.LogDebug($"getnewbot failed: {e.Message} {e.InnerException}");
                throw;
            }

            // load from server
            var source = data.PrepareToLoadBackend(1).ToList();
            taskAwaiter = PatchConstants.BackEndSession.LoadBots(source).ContinueWith(GetFirstResult, taskScheduler);

            // load bundles for bot profile
            var continuation = new Aki.Custom.Models.BundleLoader(taskScheduler);
            __result = taskAwaiter.ContinueWith(continuation.LoadBundles, taskScheduler).Unwrap();
            return false;
        }

        private static Profile GetFirstResult(Task<Profile[]> task)
        {
            var result = task.Result[0];
            Logger.LogDebug($"Loading bot profile from server. role: {result.Info.Settings.Role} side: {result.Side}");
            return result;
        }
    }
}
