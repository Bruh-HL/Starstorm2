﻿using SS2;
using RoR2;
using RoR2.ContentManagement;
using MSU;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SS2.Survivors
{
    public class MulT : SS2VanillaSurvivor
    {
        public override SS2AssetRequest<VanillaSurvivorAssetCollection> assetRequest => SS2Assets.LoadAssetAsync<VanillaSurvivorAssetCollection>("acToolbot", SS2Bundle.Indev);


        public override void Initialize()
        {
            GameObject toolbotBodyPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Toolbot/ToolbotBody.prefab").WaitForCompletion();

            SkillLocator skillLocator = toolbotBodyPrefab.GetComponent<SkillLocator>();
            SkillFamily primarySkillFamily = skillLocator.primary.skillFamily;
            SkillFamily secondarySkillFamily = skillLocator.primary.skillFamily;
            SkillFamily specialSkillFamily = skillLocator.special.skillFamily;
        }

        public override void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.AddContentFromAssetCollection(assetCollection);
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }
    }
}

