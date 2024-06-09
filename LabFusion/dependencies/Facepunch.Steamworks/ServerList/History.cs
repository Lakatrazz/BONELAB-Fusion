namespace Steamworks.ServerList
{
    public class History : Base
    {
        internal override void LaunchQuery()
        {
            var filters = GetFilters();
            request = Internal.RequestHistoryServerList(AppId.Value, ref filters, (uint)filters.Length, IntPtr.Zero);
        }
    }
}