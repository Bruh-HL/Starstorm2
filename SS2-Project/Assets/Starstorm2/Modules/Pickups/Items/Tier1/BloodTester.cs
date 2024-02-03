﻿using RoR2;
using RoR2.Items;
using UnityEngine;
using UnityEngine.Networking;

namespace Moonstorm.Starstorm2.Items
{
    public sealed class BloodTester : ItemBase
    {
        private const string token = "SS2_ITEM_BLOODTESTER_DESC";
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("BloodTester", SS2Bundle.Items);

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Amount of health regeneration granted per 25 gold, per stack. (1 = 1 hp/s)")]
        [TokenModifier(token, StatTypes.Default, 0)]
        public static float healthRegen = 0.4f;

        public override void Initialize()
        {
            R2API.RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, R2API.RecalculateStatsAPI.StatHookEventArgs args)
        {
            int stack = sender.inventory?.GetItemCount(SS2Content.Items.BloodTester) ?? 0;
            if (stack > 0)
                args.baseRegenAdd += (healthRegen * stack) * sender.master.money / Run.instance.GetDifficultyScaledCost(25);
        }
    }
}
