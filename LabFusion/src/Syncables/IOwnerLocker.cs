namespace LabFusion.Syncables
{
    public interface IOwnerLocker
    {
        bool CheckLock(out byte owner);
    }

    public static class IOwnerLockerExtensions
    {
        public static bool CheckLocks(this IList<IOwnerLocker> list, out byte owner)
        {
            owner = 0;

            if (list == null)
                return false;

            foreach (var locker in list)
            {
                if (locker.CheckLock(out var result))
                {
                    owner = result;
                    return true;
                }
            }

            return false;
        }
    }
}
