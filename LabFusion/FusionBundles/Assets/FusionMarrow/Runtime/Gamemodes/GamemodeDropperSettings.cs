using UnityEngine;

using System;
using System.Collections.Generic;


#if MELONLOADER
using MelonLoader;

using Il2CppSLZ.Marrow.Warehouse;

using Il2CppInterop.Runtime.Attributes;
#else
using SLZ.Marrow.Warehouse;
#endif

namespace LabFusion.Marrow.Integration
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#else
    [DisallowMultipleComponent]
#endif
    public class GamemodeDropperSettings : MonoBehaviour
    {
        [Serializable]
        public struct ItemDrop
        {
            public SpawnableCrateReference ItemCrateReference;

#if !MELONLOADER
            [Range(0f, 100f)]
#endif
            public float Probability;
        }

        public static readonly List<ItemDrop> DefaultDrops = new()
        {
            // Melee
            new ItemDrop() { ItemCrateReference = new("c1534c5a-b59c-4790-9b09-499553776f72"), Probability = 20f }, // Sword Claymore
            new ItemDrop() { ItemCrateReference = new("c1534c5a-a1c4-4c90-ad5d-ea1a53776f72"), Probability = 60f }, // Sword Noodledog
            new ItemDrop() { ItemCrateReference = new("c1534c5a-d3fc-4987-a93d-d79544616767"), Probability = 35f }, // Dagger
            new ItemDrop() { ItemCrateReference = new("fa534c5a868247138f50c62e424c4144.Spawnable.Baton"), Probability = 45f}, // Baton
            new ItemDrop() { ItemCrateReference = new("c1534c5a-6441-40aa-a070-909542617365"), Probability = 25f, }, // Baseball Bat
            new ItemDrop() { ItemCrateReference = new("c1534c5a-1f5a-4993-bbc1-03be4d656c65"), Probability = 19f, }, // Sledgehammer
            new ItemDrop() { ItemCrateReference = new("c1534c5a-6d15-47c7-9ad4-b04156696b69"), Probability = 50f, }, // Viking Shield
            new ItemDrop() { ItemCrateReference = new("c1534c5a-282b-4430-b009-58954b617461"), Probability = 5f, }, // Katana
            new ItemDrop() { ItemCrateReference = new("c1534c5a-f6f9-4c96-b88e-91d74c656164"), Probability = 55f, }, // Lead Pipe
            new ItemDrop() { ItemCrateReference = new("c1534c5a-4774-460f-a814-149541786546"), Probability = 30f, }, // Axe Firefighter
            new ItemDrop() { ItemCrateReference = new("SLZ.BONELAB.Content.Spawnable.ElectricGuitar"), Probability = 41f, }, // Electric Guitar
            new ItemDrop() { ItemCrateReference = new("c1534c5a-02e7-43cf-bc8d-26955772656e"), Probability = 45f, }, // Wrench
            new ItemDrop() { ItemCrateReference = new("c1534c5a-5d31-488d-b5b3-aa1c53686f76"), Probability = 30f, }, // Shovel
            new ItemDrop() { ItemCrateReference = new("c1534c5a-d0e9-4d53-9218-e76446727969"), Probability = 50f, }, // Frying Pan
            new ItemDrop() { ItemCrateReference = new("c1534c5a-0c8a-4b82-9f8b-7a9543726f77"), Probability = 55f, }, // Crowbar
            new ItemDrop() { ItemCrateReference = new("c1534c5a-1fb8-477c-afbe-2a95436f6d62"), Probability = 35f, }, // Combat Knife
            new ItemDrop() { ItemCrateReference = new("c1534c5a-f0d1-40b6-9f9b-c19544616767"), Probability = 25f, }, // Kunai
            new ItemDrop() {ItemCrateReference = new("c1534c5a-a97f-4bff-b512-e44d53706561"), Probability = 20f, }, // Spear

            // Guns
            new ItemDrop() { ItemCrateReference = new("c1534c5a-ec8e-418a-a545-cf955269666c"), Probability = 11f, }, // MK18 Laser Foregrip
            new ItemDrop() { ItemCrateReference = new("SLZ.BONELAB.CORE.Spawnable.GunEHG"), Probability = 15f, }, // e-HG Blaster
            new ItemDrop() { ItemCrateReference = new("c1534c5a-2a4f-481f-8542-cc9545646572"), Probability = 22f, }, // Eder22
            new ItemDrop() { ItemCrateReference = new("c1534c5a-a6b5-4177-beb8-04d947756e41"), Probability = 5f, }, // AKM
            new ItemDrop() {ItemCrateReference = new("SLZ.BONELAB.Content.Spawnable.RifleM1Garand"), Probability = 18f, }, // M1 Garand

            // Gadgets
            new ItemDrop() { ItemCrateReference = new("c1534c5a-e963-4a7c-8c7e-1195546f7942"), Probability = 50f, }, // Toy Balloon Gun
            new ItemDrop() { ItemCrateReference = new("c1534c5a-c6a8-45d0-aaa2-2c954465764d"), Probability = 5f, }, // Dev Manipulator
            new ItemDrop() { ItemCrateReference = new("c1534c5a-e777-4d15-b0c1-3195426f6172"), Probability = 20f, }, // Boardgun
            new ItemDrop() { ItemCrateReference = new("c1534c5a-cebf-42cc-be3a-4595506f7765"), Probability = 10f }, // Power Puncher
            new ItemDrop() { ItemCrateReference = new("c1534c5a-87ce-436d-b00c-ef9547726176"), Probability = 10f, }, // Gravity Cup
            // new ItemDrop() { ItemCrateReference = new("SLZ.BONELAB.Content.Spawnable.PropHealthPickup"), Probability = 20f, }, // Prop_Health Pickup, add functionality later

            // Props
            new ItemDrop() { ItemCrateReference = new("SLZ.BONELAB.Content.Spawnable.CardboardBoxHeadsetA"), Probability = 70f, }, // Cardboard Box Headset A
            new ItemDrop() { ItemCrateReference = new("SLZ.BONELAB.Content.Spawnable.CardboardBoxHeadsetB"), Probability = 60f, }, // Cardboard Box Headset B
            new ItemDrop() { ItemCrateReference = new("c1534c5a-5be2-49d6-884e-d35c576f6f64"), Probability = 90f, }, // Crate 1m Wooden
            new ItemDrop() { ItemCrateReference = new("c1534c5a-837c-43ca-b4b5-33d842617365"), Probability = 75f, }, // Baseball
            new ItemDrop() { ItemCrateReference = new("c1534c5a-f938-40cb-8be5-23db41706f6c"), Probability = 70f, }, // Apollo
            new ItemDrop() { ItemCrateReference = new("SLZ.BONELAB.Content.Spawnable.ApolloGold"), Probability = 5f, }, // Apollo Gold
            new ItemDrop() { ItemCrateReference = new("SLZ.BONELAB.Content.Spawnable.BlueApollo"), Probability = 30f, }, // Apollo Blue
            new ItemDrop() { ItemCrateReference = new("SLZ.BONELAB.Content.Spawnable.Propbroom"), Probability = 75f, }, // Broom
            new ItemDrop() { ItemCrateReference = new("SLZ.BONELAB.Content.Spawnable.BowlingBall"), Probability = 80f, }, // Bowling Ball
            new ItemDrop() { ItemCrateReference = new("c1534c5a-1e43-4d94-a504-31d457617465"), Probability = 88f, }, // Watermelon
            new ItemDrop() { ItemCrateReference = new("c1534c5a-6f93-4d58-b9a9-ca1c50726f70"), Probability = 73f, }, // Brick
            new ItemDrop() { ItemCrateReference = new("SLZ.BONELAB.Content.Spawnable.Died20"), Probability = 60f, }, // Die D20
            new ItemDrop() { ItemCrateReference = new("SLZ.BONELAB.Content.Spawnable.ClayPotA"), Probability = 75f, }, // Clay Pot A
            new ItemDrop() { ItemCrateReference = new("SLZ.BONELAB.Content.Spawnable.ClayVase"), Probability = 60f, }, // Clay Vase
            new ItemDrop() { ItemCrateReference = new("c1534c5a-9629-4660-8439-186b50726f70"), Probability = 85f, }, // Coffee Cup
            new ItemDrop() { ItemCrateReference = new("SLZ.BONELAB.Content.Spawnable.BowlingPin"), Probability = 80f, }, // Bowling Pin
            new ItemDrop() { ItemCrateReference = new("c1534c5a-3199-4102-91e2-8ac650726f70"), Probability = 45f, }, // Gym D20
            new ItemDrop() {ItemCrateReference = new("c1534c5a-202f-43f8-9a6c-1e9450726f70"), Probability = 5f, }, // Monkey

            // NPCs
            new ItemDrop() { ItemCrateReference = new("c1534c5a-ef15-44c0-88ae-aebc4e756c6c"), Probability = 11f, }, // Null Rat
        };

        public const int DefaultMaxItems = 5;

#if MELONLOADER
        public GamemodeDropperSettings(IntPtr intPtr) : base(intPtr) { }

        public static GamemodeDropperSettings Instance { get; private set; } = null;

        [HideFromIl2Cpp]
        public List<ItemDrop> ItemDrops { get; private set; } = new();

        [HideFromIl2Cpp]
        public int MaxItems { get; private set; } = DefaultMaxItems;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void AddItemDrop(string barcode, float probability)
        {
            ItemDrops.Add(new ItemDrop()
            {
                ItemCrateReference = new SpawnableCrateReference(barcode),
                Probability = probability,
            });
        }

        public void SetMaxItems(int maxItems)
        {
            MaxItems = maxItems;
        }

        public static ItemDrop GetItemDrop()
        {
            if (Instance != null && Instance.ItemDrops.Count > 0)
            {
                return GetItemDrop(Instance.ItemDrops);
            }

            return GetItemDrop(DefaultDrops);
        }

        private static ItemDrop GetItemDrop(List<ItemDrop> itemDrops)
        {
            var totalProbability = itemDrops.Sum(d => d.Probability);
            var randomValue = UnityEngine.Random.Range(0f, totalProbability);

            foreach (var drop in itemDrops)
            {
                float probability = drop.Probability;

                if (probability <= 0f)
                {
                    continue;
                }

                randomValue -= probability;

                if (randomValue <= 0f)
                {
                    return drop;
                }
            }

            return default;
        }

        public static int GetMaxItems()
        {
            if (Instance != null)
            {
                return Instance.MaxItems;
            }

            return DefaultMaxItems;
        }
#else
        public List<ItemDrop> ItemDrops = new();

        [Min(0)]
        public int MaxItems = DefaultMaxItems;

        public void AddItemDrop(string barcode, float probability)
        {
        }

        public void SetMaxItems(int maxItems)
        {
        }
#endif
    }
}