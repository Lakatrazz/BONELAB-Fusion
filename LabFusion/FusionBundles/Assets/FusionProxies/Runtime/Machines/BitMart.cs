using UnityEngine;

#if MELONLOADER
using MelonLoader;

using LabFusion.Menu;
using LabFusion.Scene;
using LabFusion.SDK.Points;
using LabFusion.Extensions;
using LabFusion.Bonelab;
using LabFusion.Utilities;
using LabFusion.Math;
using LabFusion.Marrow.Pool;

using Il2CppInterop.Runtime.Attributes;

using Il2CppSLZ.Marrow.Audio;
using Il2CppSLZ.Marrow.Data;
using Il2CppSLZ.Marrow.Warehouse;
using Il2CppSLZ.Props;
using Il2CppSLZ.Marrow.Pool;
using Il2CppSLZ.Marrow;
#endif

namespace LabFusion.Marrow.Proxies
{
#if MELONLOADER
    [RegisterTypeInIl2Cpp]
#endif
    public class BitMart : MonoBehaviour
    {
#if MELONLOADER
        public BitMart(IntPtr intPtr) : base(intPtr) { }

        public const float PowerTransitionLength = 0.5f;

        public BitMartElement BitMartElement { get; set; } = null;

        public Rigidbody DoorRigidbody { get; set; } = null;

        public Transform ItemSpawnPoint { get; set; } = null;

        public AudioSource MusicSource { get; set; } = null;

        private bool _hasElements = false;

        private bool _turnedOn = false;

        private float _startMusicPitch = 0f;
        private bool _powerTransitioning = false;
        private float _powerTransitionElapsed = 0f;

        private void Awake()
        {
            GetElements();

            AddInteraction();

            FusionSceneManager.HookOnLevelLoad(OnLevelLoad);

            MusicSource.Pause();
            MusicSource.pitch = 0f;

            TurnOff();
        }

        private void AddInteraction()
        {
            PersistentAssetCreator.HookOnSoftGrabLoaded((pose) =>
            {
                var gripRoot = transform.Find("Art");

                foreach (var collider in gripRoot.GetComponentsInChildren<Collider>())
                {
                    if (collider.attachedRigidbody && !collider.attachedRigidbody.isKinematic)
                    {
                        continue;
                    }

                    if (Grip.Cache.Get(collider.gameObject))
                    {
                        continue;
                    }

                    var genericGrip = collider.gameObject.AddComponent<GenericGrip>();
                    genericGrip.isThrowable = true;
                    genericGrip.ignoreGripTargetOnAttach = false;
                    genericGrip.additionalGripColliders = new Collider[0];
                    genericGrip.handleAmplifyCurve = AnimationCurve.Linear(0f, 1f, 0f, 1f);
                    genericGrip.gripOptions = InteractionOptions.MultipleHands;
                    genericGrip.priority = 1f;
                    genericGrip.handPose = pose;
                    genericGrip.minBreakForce = float.PositiveInfinity;
                    genericGrip.maxBreakForce = float.PositiveInfinity;
                    genericGrip.defaultGripDistance = float.PositiveInfinity;
                    genericGrip.radius = 0.24f;
                }
            });
        }

        private void OnLevelLoad()
        {
            MenuButtonHelper.PopulateTexts(gameObject);
            MenuButtonHelper.PopulateButtons(gameObject);

            MusicSource.outputAudioMixerGroup = Audio3dManager.diegeticMusic;
        }

        private void Update()
        {
            if (_powerTransitioning)
            {
                UpdatePowerTransition();
            }
        }

        private void UpdatePowerTransition()
        {
            float endPitch = _turnedOn ? 1f : 0f;

            _powerTransitionElapsed += Time.deltaTime;

            if (_powerTransitionElapsed >= PowerTransitionLength)
            {
                _powerTransitioning = false;
                MusicSource.pitch = endPitch;

                if (!_turnedOn)
                {
                    MusicSource.Pause();
                }
                return;
            }

            float percent = _powerTransitionElapsed / PowerTransitionLength;

            MusicSource.pitch = ManagedMathf.Lerp(_startMusicPitch, endPitch, percent);
        }

        public void GetElements()
        {
            if (_hasElements)
            {
                return;
            }

            BitMartElement = GetComponentInChildren<BitMartElement>();
            BitMartElement.GetElements();

            BitMartElement.CatalogElement.OnItemPurchased += OnItemPurchased;
            BitMartElement.CatalogElement.OnItemEquipped += OnItemEquipped;
            BitMartElement.CatalogElement.OnItemUnequipped += OnItemUnequipped;
            BitMartElement.CatalogElement.OnAllItemsUnequipped += OnAllItemsUnequipped;

            DoorRigidbody = transform.Find("Art/Offset/VendorAtlas/Door Pivot").GetComponent<Rigidbody>();

            MusicSource = transform.Find("Logic/Music").GetComponent<AudioSource>();

            ItemSpawnPoint = transform.Find("Logic/Item Spawn Point");

            _hasElements = true;
        }

        public void TurnOn()
        {
            BitMartElement.gameObject.SetActive(true);

            if (!_turnedOn)
            {
                LocalAudioPlayer.PlayAtPoint(new AudioReference(FusionMonoDiscReferences.UITurnOnReference), BitMartElement.transform.position, LocalAudioPlayer.SFXSettings);

                _turnedOn = true;
                _powerTransitioning = true;

                MusicSource.UnPause();
            }

            _startMusicPitch = MusicSource.pitch;
            _powerTransitionElapsed = 0f;
        }

        public void TurnOff()
        {
            BitMartElement.gameObject.SetActive(false);

            if (_turnedOn)
            {
                LocalAudioPlayer.PlayAtPoint(new AudioReference(FusionMonoDiscReferences.UITurnOffReference), BitMartElement.transform.position, LocalAudioPlayer.SFXSettings);

                _turnedOn = false;
                _powerTransitioning = true;
            }

            _startMusicPitch = MusicSource.pitch;
            _powerTransitionElapsed = 0f;
        }

        [HideFromIl2Cpp]
        private void OnItemPurchased(PointItem item)
        {
            PushDoor(100f);

            SpawnGacha(item.Barcode);
        }

        [HideFromIl2Cpp]
        private void OnItemEquipped(PointItem item)
        {
            PushDoor();
        }

        [HideFromIl2Cpp]
        private void OnItemUnequipped(PointItem item)
        {
            PushDoor();
        }

        [HideFromIl2Cpp]
        private void OnAllItemsUnequipped()
        {
            PushDoor();
        }

        private void PushDoor(float force = 10f)
        {
            DoorRigidbody.AddRelativeTorque(Vector3Extensions.left * force, ForceMode.Impulse);
        }

        private void SpawnGacha(string barcode)
        {
            if (ItemSpawnPoint == null)
            {
                return;
            }

            var gachaSpawnable = LocalAssetSpawner.CreateSpawnable(BonelabSpawnableReferences.GachaCapsuleReference);

            LocalAssetSpawner.Register(gachaSpawnable);

            LocalAssetSpawner.Spawn(gachaSpawnable, ItemSpawnPoint.position, ItemSpawnPoint.rotation, (poolee) =>
            {
                var gachaCapsule = poolee.GetComponent<GachaCapsule>();

                if (gachaCapsule == null)
                {
                    return;
                }

                gachaCapsule.selectedCrate = new GenericCrateReference(barcode);
                gachaCapsule.SetPreviewMesh();

                // Add a bunch of torque to get the ball out
                var rigidbody = poolee.GetComponentInChildren<Rigidbody>();

                if (rigidbody == null)
                {
                    return;
                }

                rigidbody.velocity = ItemSpawnPoint.forward * 0.1f;
                rigidbody.angularVelocity = ItemSpawnPoint.right * 150f;
            });
        }
#else
        public void TurnOn()
        {
        }

        public void TurnOff()
        {
        }
#endif
    }
}