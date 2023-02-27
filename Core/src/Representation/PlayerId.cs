using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.SDK.Points;
using LabFusion.Senders;
using LabFusion.Utilities;

using SLZ.Rig;

namespace LabFusion.Representation
{
    public class PlayerId : IFusionSerializable, IDisposable, IEquatable<PlayerId> {
        public bool IsSelf => LongId == PlayerIdManager.LocalLongId;
        public bool IsValid => PlayerIdManager.PlayerIds.Contains(this);

        public ulong LongId { get; private set; }
        public byte SmallId { get; private set; }

        /// <summary>
        /// When the metadata of the ID is changed.
        /// </summary>
        public event Action<PlayerId> OnMetadataChanged;

        /// <summary>
        /// When a specific key and value pair of metadata is changed.
        /// <para>The second argument is the key, and the third argument is the value.</para>
        /// </summary>
        public event Action<PlayerId, string, string> OnMetadataPairChanged;

        private readonly Dictionary<string, string> _internalMetadata = new Dictionary<string, string>();
        public Dictionary<string, string> Metadata => _internalMetadata;

        private List<string> _internalEquippedItems = new List<string>();
        public List<string> EquippedItems => _internalEquippedItems;

        public PlayerId() { }

        public PlayerId(ulong longId, byte smallId, Dictionary<string, string> metadata, List<string> equippedItems) {
            LongId = longId;
            SmallId = smallId;
            _internalMetadata = metadata;
            _internalEquippedItems = equippedItems;
        }

        public bool Equals(PlayerId other) {
            if (other == null)
                return false;

            return SmallId == other.SmallId;
        }

        public static implicit operator byte(PlayerId id) => id.SmallId;

        public static implicit operator ulong(PlayerId id) => id.LongId;

        public static bool IsNullOrInvalid(PlayerId id) {
            return id == null || !id.IsValid;
        }

        public bool TrySetMetadata(string key, string value) {
            // If we are the server, we just accept the request
            // Otherwise, we make sure this is our PlayerId
            if (NetworkInfo.IsServer || SmallId == PlayerIdManager.LocalSmallId) {
                PlayerSender.SendPlayerMetadataRequest(SmallId, key, value);
                return true;
            }

            return false;
        }

        public bool TryGetMetadata(string key, out string value) {
            return _internalMetadata.TryGetValue(key, out value);
        }

        public string GetMetadata(string key) {
            if (_internalMetadata.TryGetValue(key, out string value))
                return value;

            return null;
        }

        internal void Internal_ForceSetMetadata(string key, string value) {
            if (_internalMetadata.ContainsKey(key))
                _internalMetadata[key] = value;
            else
                _internalMetadata.Add(key, value);

            OnMetadataPairChanged.InvokeSafe(this, key, value, $"invoking OnMetadataPairChanged for player {SmallId}");
            OnMetadataChanged.InvokeSafe(this, $"invoking OnMetadataChanged hook for player {SmallId}");
        }

        internal void Internal_ForceSetEquipped(string barcode, bool value) {
            // Remove/add to the list
            if (value && !_internalEquippedItems.Contains(barcode))
                _internalEquippedItems.Add(barcode);
            else if (!value && _internalEquippedItems.Contains(barcode))
                _internalEquippedItems.Remove(barcode);

            // Invoke the events on the item
            PointItemManager.Internal_OnEquipChange(this, barcode, value);
        }

        public void Insert() {
            if (PlayerIdManager.PlayerIds.Any((id) => id.SmallId == SmallId)) {
                var list = PlayerIdManager.PlayerIds.FindAll((id) => id.SmallId == SmallId);
                for (var i = 0; i < list.Count; i++)
                    list[i].Dispose();
            }

            PlayerIdManager.PlayerIds.Add(this);
        }

        public void Dispose() {
            PlayerIdManager.PlayerIds.RemoveInstance(this);
            if (PlayerIdManager.LocalId == this)
                PlayerIdManager.RemoveLocalId();

            GC.SuppressFinalize(this);
        }

        public void Serialize(FusionWriter writer) {
            writer.Write(LongId);
            writer.Write(SmallId);

            // Write the player metadata
            writer.Write(_internalMetadata);

            writer.Write(_internalEquippedItems);
        }
        
        public void Deserialize(FusionReader reader) {
            LongId = reader.ReadUInt64();
            SmallId = reader.ReadByte();

            // Read the player metadata
            var metaData = reader.ReadStringDictionary();
            foreach (var pair in metaData) {
                Internal_ForceSetMetadata(pair.Key, pair.Value);
            }

            var equippedItems = reader.ReadStrings();

            foreach (var item in equippedItems) {
                Internal_ForceSetEquipped(item, true);
            }
        }
    }
}
