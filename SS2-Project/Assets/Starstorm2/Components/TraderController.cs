﻿using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using RoR2;
using EntityStates.Trader.Bag;
using RoR2.UI;

namespace Moonstorm.Starstorm2.Components
{
    public class TraderController : NetworkBehaviour
    {
        public ItemIndex lastTradedItemIndex { get; private set; }
        public EntityStateMachine esm;
        public ModelLocator modelLocator;
        public ChildLocator childLocator;
        public PickupPickerController pickupPickerController;
        private Interactor interactor;

        public float commonValue = 0.12f;
        public float uncommonValue = 0.3f;
        public float rareValue = 0.62f;
        public float lunarValue = 0.45f;

        private ItemDef favoriteItem;
        private LanguageTextMeshController ltmcPrice;

        public Dictionary<ItemDef, float> itemValues = new Dictionary<ItemDef, float>();
        //private Dictionary<ItemDef, Sprite> itemSprites = new Dictionary<ItemDef, Sprite>(); //this is the stupidest thing ever.
        private Dictionary<Sprite, ItemDef> reversedItemSprites = new Dictionary<Sprite, ItemDef>();

       // private static GameObject menu = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Scrapper/ScrapperPickerPanel.prefab").WaitForCompletion();
        void Start()
        {
            modelLocator = GetComponent<ModelLocator>();
            childLocator = modelLocator.modelTransform.GetComponent<ChildLocator>();

            //Modify pickup controller; deprecate once something custom is made in project.
            if (pickupPickerController)
            {
                pickupPickerController.panelPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Scrapper/ScrapperPickerPanel.prefab").WaitForCompletion();
                ModifyMenu(pickupPickerController.panelPrefab);
            }

            //Assign a favorite item.
            favoriteItem = FindFavorite();

            //Give every item a value.
            if (NetworkServer.active)
            {
                foreach (ItemDef item in ItemCatalog.allItemDefs)
                {
                    if (Run.instance.IsItemAvailable(item.itemIndex))
                    {
                        float value = CalcValue(item);
                        itemValues[item] = value;
                        Debug.Log("Value for " + item.nameToken + " is " + itemValues[item]);
                        if (item == favoriteItem)
                            Debug.Log("I ABSOLUTELY FUCKING LOVE " + item.nameToken + " BTW");
                        reversedItemSprites[item.pickupIconSprite] = item; //jesus.
                        //Debug.Log("item: " + reversedItemSprites[item.pickupIconSprite]);
                    }
                }
            }
        }

        public ItemDef GetItemThroughSprite(Sprite sprite)
        {
            return reversedItemSprites[sprite];
        }

        private GameObject ModifyMenu(GameObject menuPrefab)
        {
            Transform panel = menuPrefab.transform.Find("MainPanel");
            Transform juice = panel.Find("Juice");
            Transform label = juice.Find("Label");
            LanguageTextMeshController ltmc = label.GetComponent<LanguageTextMeshController>();

            if (ltmc != null)
                ltmc.token = "SS2_TRADER_POPUP_TEXT";
            else
                Debug.Log("ltmc null");

            GameObject priceLabel = Instantiate(label.gameObject);
            priceLabel.transform.SetParent(label);

            RectTransform priceLabelRect = priceLabel.GetComponent<RectTransform>();
            priceLabelRect.SetPositionAndRotation(new Vector3(priceLabelRect.localPosition.x, priceLabelRect.localPosition.y - 45, priceLabelRect.localPosition.z), priceLabel.transform.rotation);
            ltmcPrice = priceLabel.GetComponent<LanguageTextMeshController>();

            if (ltmcPrice != null)
                ltmcPrice.token = "PRICE:";
            else
                Debug.Log("ltmc2 null");

            Transform iconContainer = juice.Find("IconContainer");
            Transform pickupTemplate = iconContainer.Find("PickupButtonTemplate");
            TooltipProvider tooltipProvider = pickupTemplate.gameObject.AddComponent<TooltipProvider>();
            PriceTooltipManager priceTooltipManager = pickupTemplate.gameObject.AddComponent<PriceTooltipManager>();

            //tooltipProvider.titleColor = pickupTemplate.GetComponent<pickup>
            //tooltipProvider.titleToken = ItemCatalog.get

            return menuPrefab;
        }

        private ItemDef FindFavorite()
        {
            int itemCount = ItemCatalog.allItemDefs.Length;
            int randomindex = UnityEngine.Random.Range(0, itemCount); //dual wielding randoms
            ItemDef favoriteItem = ItemCatalog.allItemDefs[randomindex];

            //if an item can't be found, then find an item.
            if (!Run.instance.IsItemAvailable(favoriteItem.itemIndex))
                favoriteItem = RoR2Content.Items.BarrierOnOverHeal;

            //replace trash with trash
            if (favoriteItem == RoR2Content.Items.ScrapGreen || favoriteItem == RoR2Content.Items.ScrapWhite || favoriteItem == RoR2Content.Items.ScrapYellow || favoriteItem == RoR2Content.Items.ScrapRed)
                favoriteItem = RoR2Content.Items.BarrierOnOverHeal;

            return favoriteItem;
        }

        private float CalcValue(ItemDef itemDef)
        {
            float value = 0;

            switch (itemDef.tier)
            {
                //rare, boss, and void equivalents:
                case ItemTier.Tier3:
                case ItemTier.Boss:
                case ItemTier.VoidTier3:
                case ItemTier.VoidBoss:
                    value = UnityEngine.Random.Range(rareValue * 0.7f, rareValue * 1.3f);
                    break;

                //lunar:
                case ItemTier.Lunar:
                    value = UnityEngine.Random.Range(lunarValue * 0.7f, lunarValue * 1.3f);
                    break;

                //uncommon and void equivalent:
                case ItemTier.Tier2:
                case ItemTier.VoidTier2:
                    value = UnityEngine.Random.Range(uncommonValue * 0.7f, uncommonValue * 1.3f);
                    break;

                //common and void equivalent:
                default:
                    value = UnityEngine.Random.Range(commonValue * 0.7f, commonValue * 1.3f);
                    if ((itemDef.tier != ItemTier.Tier1) && (itemDef.tier != ItemTier.VoidTier1)) //bonus for mystery items
                        value += 0.65f; //now how'd you manage that...?
                    break;
            }

            //small bonus for desired item; lets items go over 100% but that's fun and will be rare so ... why not?
            if (itemDef == favoriteItem)
                value += 0.3f;

            //zanzan does not care much for garbage
            if (itemDef == RoR2Content.Items.ScrapGreen || itemDef == RoR2Content.Items.ScrapWhite || itemDef == RoR2Content.Items.ScrapYellow || itemDef == RoR2Content.Items.ScrapRed)
                value *= 0.2f;

            //we stay silly
            if (itemDef == RoR2Content.Items.BarrierOnOverHeal && favoriteItem != RoR2Content.Items.BarrierOnOverHeal)
                value -= 0.2f;

            return value;
        }

        void Update()
        { }

        [Server]
        public void AssignPotentialInteractor(Interactor potentialInteractor)
        {
            if (!NetworkServer.active)
            {
                Debug.Log("THIS MF IS A CLIENT LOL @ TraderController AssignPotentialInteractor");
                return;
            }
            else
            {
                Debug.Log("hello!");
            }
            interactor = potentialInteractor;
        }

        [Server]
        public void BeginTrade(int intPickupIndex)
        {
            if (!NetworkServer.active)
            {
                return;
            }
            PickupDef pickupDef = PickupCatalog.GetPickupDef(new PickupIndex(intPickupIndex));
            if (pickupDef != null && interactor)
            {
                lastTradedItemIndex = pickupDef.itemIndex;
                CharacterBody interactorBody = interactor.GetComponent<CharacterBody>();
                if (interactorBody && interactorBody.inventory)
                {
                    interactorBody.inventory.RemoveItem(pickupDef.itemIndex, 1);
                    CreateItemTakenOrb(interactorBody.corePosition, gameObject, pickupDef.itemIndex);
                }
            }
            if (esm)
            {
                esm.SetNextState(new WaitToBeginTrade());
            }
        }

        public float GetValue(ItemDef itemDef)
        {
            return itemValues[itemDef];
        }

        public void ReduceValue()
        {
            itemValues[ItemCatalog.GetItemDef(lastTradedItemIndex)] *= 0.8f;
        }

        [Server]
        public static void CreateItemTakenOrb(Vector3 effectOrigin, GameObject targetObject, ItemIndex itemIndex)
        {
            if (!NetworkServer.active)
            {
                Debug.Log("client client client @ TraderController CreateItemTakenOrb");
                return;
            }
            GameObject effectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/ItemTakenOrbEffect.prefab").WaitForCompletion();
            EffectData effectData = new EffectData
            {
                origin = effectOrigin,
                genericFloat = 1.5f,
                genericUInt = (uint)(itemIndex + 1)
            };
            effectData.SetNetworkedObjectReference(targetObject);
            EffectManager.SpawnEffect(effectPrefab, effectData, true);
        }

        private void UNetVersion()
        { }

        public override void OnDeserialize(NetworkReader reader, bool initialState)
        { }

        public override void PreStartClient()
        { }
    }
}