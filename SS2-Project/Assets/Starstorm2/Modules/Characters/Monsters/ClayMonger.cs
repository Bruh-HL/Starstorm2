using RoR2;
using RoR2.ContentManagement;
using SS2.Components;
using System.Collections;

namespace SS2.Monsters
{
    public class ClayMonger : SS2Monster
    {
        public override SS2AssetRequest<MonsterAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<MonsterAssetCollection>("acClayMonger", SS2Bundle.Indev);

        public override void Initialize()
        {
            if(SS2Main.ChileanIndependenceWeek)
            {
                SpecialEventPickup pickup = SpecialEventPickup.AddAndSetupComponent(CharacterPrefab.GetComponent<CharacterBody>());
                pickup.contextToken = "SS2_INTERACTABLE_LUCKYPUP_CONTEXT";
                SS2Main.Instance.StartCoroutine(AwaitForLoad(pickup));
            }

            R2API.RecalculateStatsAPI.GetStatCoefficients += HandleTar;
        }

        private void HandleTar(CharacterBody sender, R2API.RecalculateStatsAPI.StatHookEventArgs args)
        {
            if(!sender.HasBuff(SS2Content.Buffs.bdMongerTar))
            {
                return;
            }

            args.moveSpeedReductionMultAdd += 0.1f;
            args.attackSpeedReductionMultAdd += 0.5f; 
        }

        private IEnumerator AwaitForLoad(SpecialEventPickup pickup)
        {
            while(!SS2Content.loadStaticContentFinished)
            {
                yield return null;
            }

            pickup.itemDef = SS2Content.Items.LuckyPup;
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }
    }
}