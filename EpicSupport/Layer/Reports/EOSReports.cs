using Epic.OnlineServices;
using Epic.OnlineServices.Reports;

namespace MarrowFusion.Epic;

internal class EOSReports : EOSInterface
{
    internal EOSRuntime Runtime;
    internal ReportsInterface ReportsInterface;
    internal ProductUserId LocalUserId;
    
    internal EOSReports(EOSRuntime eosRuntime, ReportsInterface reportsInterface, ProductUserId localUserId)
    {
        Runtime = eosRuntime;
        ReportsInterface = reportsInterface;
        LocalUserId = localUserId;
    }
    
    internal Task<bool> SubmitReportAsync(ProductUserId targetUserId, PlayerReportsCategory category, string message)
    {
        var tcs = new TaskCompletionSource<bool>();

        if (targetUserId == null || !targetUserId.IsValid())
        {
            EpicModule.Logger.Error("Failed to submit player report: Invalid target user ID");
            return Task.FromResult(false);
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            EpicModule.Logger.Error("Failed to submit player report: Invalid message");
            return Task.FromResult(false);
        }
        
        var options = new SendPlayerBehaviorReportOptions
        {
            ReporterUserId = LocalUserId,
            ReportedUserId = targetUserId,
            Category = category,
            Message = message
        };

        ReportsInterface.SendPlayerBehaviorReport(ref options, null, (ref SendPlayerBehaviorReportCompleteCallbackInfo data) => tcs.TrySetResult(data.ResultCode == Result.Success));

        return tcs.Task;
    }
}