using LabFusion.Utilities;
using SLZ.Bonelab;

namespace LabFusion.Syncables
{
    public class BoardGeneratorExtender : PropComponentExtender<BoardGenerator>
    {
        public static FusionComponentCache<BoardGenerator, PropSyncable> Cache = new FusionComponentCache<BoardGenerator, PropSyncable>();

        protected override void AddToCache(BoardGenerator generator, PropSyncable syncable)
        {
            Cache.Add(generator, syncable);
        }

        protected override void RemoveFromCache(BoardGenerator generator)
        {
            Cache.Remove(generator);
        }
    }
}
