﻿using LabFusion.Utilities;
using SLZ.Interaction;

namespace LabFusion.Syncables
{
    public class KeyExtender : PropComponentExtender<Key>
    {
        public static FusionComponentCache<Key, PropSyncable> Cache = new FusionComponentCache<Key, PropSyncable>();

        protected override void AddToCache(Key key, PropSyncable syncable)
        {
            Cache.Add(key, syncable);
        }

        protected override void RemoveFromCache(Key key)
        {
            Cache.Remove(key);
        }
    }
}
