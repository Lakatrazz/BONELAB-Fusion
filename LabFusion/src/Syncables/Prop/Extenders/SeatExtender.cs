using LabFusion.Utilities;

using Il2CppSLZ.Vehicle;

namespace LabFusion.Syncables
{
    public class SeatExtender : PropComponentArrayExtender<Seat>
    {
        public static FusionComponentCache<Seat, PropSyncable> Cache = new FusionComponentCache<Seat, PropSyncable>();

        protected override void AddToCache(Seat seat, PropSyncable syncable)
        {
            Cache.Add(seat, syncable);

            syncable.IsVehicle = true;
        }

        protected override void RemoveFromCache(Seat seat)
        {
            Cache.Remove(seat);
        }
    }
}
