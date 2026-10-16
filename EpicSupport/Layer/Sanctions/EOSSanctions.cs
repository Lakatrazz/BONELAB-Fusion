using Epic.OnlineServices;
using Epic.OnlineServices.Sanctions;

namespace MarrowFusion.Epic;

internal class EOSSanctions : EOSInterface
{
    internal EOSRuntime Runtime;
    internal SanctionsInterface SanctionsInterface;
    internal ProductUserId LocalUserId;
    
    internal EOSSanctions(EOSRuntime eosRuntime, SanctionsInterface sanctionsInterface, ProductUserId localUserId)
    {
        Runtime = eosRuntime;
        SanctionsInterface = sanctionsInterface;
        LocalUserId = localUserId;
    }

    internal Task<List<PlayerSanction>> GetActiveSanctionsAsync(ProductUserId targetUserId)
    {
        var tcs = new TaskCompletionSource<List<PlayerSanction>>();
        
        var sanctions = new List<PlayerSanction>();
        
        var queryOptions = new QueryActivePlayerSanctionsOptions
        {
            LocalUserId = LocalUserId,
            TargetUserId = targetUserId
        };

        SanctionsInterface.QueryActivePlayerSanctions(ref queryOptions, null, (ref QueryActivePlayerSanctionsCallbackInfo data) =>
        {
            if (data.ResultCode != Result.Success)
            {
                EpicModule.Logger.Error($"Failed to query sanctions for user {targetUserId}: {data.ResultCode}");
                tcs.TrySetResult(sanctions);
                return;
            }
            
            var countOptions = new GetPlayerSanctionCountOptions
            {
                TargetUserId = targetUserId
            };

            uint count = SanctionsInterface.GetPlayerSanctionCount(ref countOptions);

            for (uint i = 0; i < count; i++)
            {
                var indexOptions = new CopyPlayerSanctionByIndexOptions
                {
                    TargetUserId = targetUserId,
                    SanctionIndex = i
                };
                
                var result = SanctionsInterface.CopyPlayerSanctionByIndex(ref indexOptions, out var sanction);

                if (result == Result.Success && sanction.HasValue)
                    sanctions.Add(sanction.Value);
            }

            tcs.TrySetResult(sanctions);
        });

        return tcs.Task;
    }
}