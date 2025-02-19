﻿using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIT.Core.Coop
{
    internal class CoopInventoryController : PlayerInventoryController
    {
        ManualLogSource BepInLogger { get; set; }

        public CoopInventoryController(EFT.Player player, Profile profile, bool examined) : base(player, profile, examined)
        {
            BepInLogger = BepInEx.Logging.Logger.CreateLogSource(nameof(CoopInventoryController));
        }

        public override Task<IResult> LoadMagazine(BulletClass sourceAmmo, MagazineClass magazine, int loadCount, bool ignoreRestrictions)
        {
            BepInLogger.LogInfo("LoadMagazine");
            return base.LoadMagazine(sourceAmmo, magazine, loadCount, ignoreRestrictions);
        }

        public override Task<IResult> UnloadMagazine(MagazineClass magazine)
        {
            BepInLogger.LogInfo("UnloadMagazine");
            return base.UnloadMagazine(magazine);
        }

        public override void ThrowItem(Item item, IEnumerable<ItemsCount> destroyedItems, Callback callback = null, bool downDirection = false)
        {
            BepInLogger.LogInfo("ThrowItem");
            destroyedItems = new List<ItemsCount>();
            base.ThrowItem(item, destroyedItems, callback, downDirection);
        }
    }
}
