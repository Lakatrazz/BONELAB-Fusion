using LabFusion.Network;
using LabFusion.Representation;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.SDK.Gamemodes {
    public abstract class Gamemode {
        public virtual bool CanLateJoin => true;

        public virtual void ResetDeathCounts() {
            foreach (var id in PlayerIdManager.PlayerIds) {
                id.TrySetMetadata(MetadataHelper.GamemodeDeathCountKey, "0");
            }
        }

        public virtual void IncrementDeath(PlayerId id) { 
            if (NetworkInfo.IsServer) {
                string value = id.GetMetadata(MetadataHelper.GamemodeDeathCountKey);

                if (value != null && int.TryParse(value, out int count))
                    value = $"{count++}";
                else
                    value = $"{1}";

                id.TrySetMetadata(MetadataHelper.GamemodeDeathCountKey, value);
            }
        }

        public virtual void DecrementDeath(PlayerId id) {
            if (NetworkInfo.IsServer) {
                string value = id.GetMetadata(MetadataHelper.GamemodeDeathCountKey);

                if (value != null && int.TryParse(value, out int count))
                    value = $"{count--}";
                else
                    value = $"{0}";

                id.TrySetMetadata(MetadataHelper.GamemodeDeathCountKey, value);
            }
        }
    }
}
