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
            new ItemDrop() { ItemCrateReference = new("c1534c5a-282b-4430-b009-58954b617461"), Probability = 0.9f, }, // Katana
            new ItemDrop() { ItemCrateReference = new("c1534c5a-f6f9-4c96-b88e-91d74c656164"), Probability = 53f, }, // Lead Pipe
            new ItemDrop() { ItemCrateReference = new("c1534c5a-4774-460f-a814-149541786546"), Probability = 30f, }, // Axe Firefighter
            new ItemDrop() { ItemCrateReference = new("SLZ.BONELAB.Content.Spawnable.ElectricGuitar"), Probability = 41f, }, // Electric Guitar

            // Guns
            new ItemDrop() { ItemCrateReference = new("c1534c5a-ec8e-418a-a545-cf955269666c"), Probability = 11f, }, // MK18 Laser Foregrip
            new ItemDrop() { ItemCrateReference = new("SLZ.BONELAB.CORE.Spawnable.GunEHG"), Probability = 15f, }, // e-HG Blaster
            new ItemDrop() { ItemCrateReference = new("c1534c5a-2a4f-481f-8542-cc9545646572"), Probability = 22f, }, // Eder22

            // Gadgets
            new ItemDrop() { ItemCrateReference = new("c1534c5a-e963-4a7c-8c7e-1195546f7942"), Probability = 50f, }, // Toy Balloon Gun
            new ItemDrop() { ItemCrateReference = new("c1534c5a-c6a8-45d0-aaa2-2c954465764d"), Probability = 3f, }, // Dev Manipulator
            new ItemDrop() { ItemCrateReference = new("c1534c5a-e777-4d15-b0c1-3195426f6172"), Probability = 10f, }, // Boardgun
            new ItemDrop() { ItemCrateReference = new("SLZ.BONELAB.Content.Spawnable.PropHealthPickup"), Probability = 20f, }, // Prop_Health Pickup

            // Props
            new ItemDrop() { ItemCrateReference = new("SLZ.BONELAB.Content.Spawnable.CardboardBoxHeadsetA"), Probability = 60f, }, // Cardboard Box Headset A
            new ItemDrop() { ItemCrateReference = new("SLZ.BONELAB.Content.Spawnable.CardboardBoxHeadsetB"), Probability = 55f, }, // Cardboard Box Headset B
            new ItemDrop() { ItemCrateReference = new("c1534c5a-5be2-49d6-884e-d35c576f6f64"), Probability = 83f, }, // Crate 1m Wooden
            new ItemDrop() { ItemCrateReference = new("c1534c5a-837c-43ca-b4b5-33d842617365"), Probability = 63f, }, // Baseball
            new ItemDrop() { ItemCrateReference = new("c1534c5a-f938-40cb-8be5-23db41706f6c"), Probability = 70f, }, // Apollo
            new ItemDrop() { ItemCrateReference = new("SLZ.BONELAB.Content.Spawnable.ApolloGold"), Probability = 5f, }, // Apollo Gold
            new ItemDrop() { ItemCrateReference = new("SLZ.BONELAB.Content.Spawnable.BlueApollo"), Probability = 35f, }, // Apollo Blue
            new ItemDrop() { ItemCrateReference = new("SLZ.BONELAB.Content.Spawnable.Propbroom"), Probability = 66f, }, // Broom
            new ItemDrop() { ItemCrateReference = new("SLZ.BONELAB.Content.Spawnable.BowlingBall"), Probability = 67f, }, // Bowling Ball
            new ItemDrop() { ItemCrateReference = new("c1534c5a-1e43-4d94-a504-31d457617465"), Probability = 78f, }, // Watermelon
            new ItemDrop() { ItemCrateReference = new("c1534c5a-6f93-4d58-b9a9-ca1c50726f70"), Probability = 68f, }, // Brick
            new ItemDrop() { ItemCrateReference = new("SLZ.BONELAB.Content.Spawnable.Died20"), Probability = 70f, }, // Die D20
            new ItemDrop() { ItemCrateReference = new("SLZ.BONELAB.Content.Spawnable.ClayPotA"), Probability = 69f, }, // Clay Pot A
            new ItemDrop() { ItemCrateReference = new("SLZ.BONELAB.Content.Spawnable.ClayVase"), Probability = 51f, }, // Clay Vase

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

            float sum = 0f;

            foreach (var drop in itemDrops)
            {
                sum += drop.Probability;

                if (randomValue <= sum)
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