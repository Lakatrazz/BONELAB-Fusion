using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Downloading.ModIO;

public static class ModIODownloader
{
    private static ModTransaction _currentTransaction = default;
    private static bool _isDownloading = false;

    public static ModTransaction CurrentTransaction => _currentTransaction;
    
    public static bool IsDownloading => _isDownloading;

    private static readonly Queue<ModTransaction> _queuedTransactions = new();

    public static Queue<ModTransaction> QueuedTransactions => _queuedTransactions;

    public static void UpdateQueue()
    {
        if (!IsDownloading && QueuedTransactions.Count > 0)
        {
            BeginDownload(QueuedTransactions.Dequeue());
        }

        ModForklift.UpdateForklift();
    }

    public static string FormatDownloadPath(int modId, int fileId)
    {
        return $"{ModIOSettings.GameApiPath}{modId}/files/{fileId}/download";
    }

    public static ModTransaction GetTransaction(int modId)
    {
        return QueuedTransactions.FirstOrDefault((transaction) => transaction.modFile.ModId == modId);
    }

    public static void EnqueueDownload(ModTransaction transaction)
    {
        var existingTransaction = GetTransaction(transaction.modFile.ModId);

        // If this mod is already being downloaded, just forward the download to the existing transaction
        if (existingTransaction != null)
        {
            existingTransaction.HookDownload(transaction.callback);
            return;
        }

        // Enqueue the download
        QueuedTransactions.Enqueue(transaction);
    }

    private static async void BeginDownload(ModTransaction transaction)
    {
        ModIOFile modFile = transaction.modFile;

        if (!modFile.FileId.HasValue)
        {
            // Request the latest file id
            var modData = await ModIOManager.GetMod(modFile.ModId);

            var platform = ModIOManager.GetValidPlatform(modData);

            if (!platform.HasValue)
            {
                FusionLogger.Warn($"Tried beginning download for mod {modFile.ModId}, but it had no valid platforms!");

                transaction.callback?.Invoke(DownloadCallbackInfo.FailedCallback);

                EndDownload();
                return;
            }

            modFile = new ModIOFile(modData.Id, platform.Value.ModfileLive);
        }

        _currentTransaction = transaction;
        _isDownloading = true;

        string url = FormatDownloadPath(modFile.ModId, modFile.FileId.Value);
        string token = await ModIOSettings.LoadTokenAsync();
        
        HttpClient client = new();
        client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
        
        var stream = await client.GetStreamAsync(url);

        ModDownloadManager.ValidateStagingDirectory();

        var zipPath = ModDownloadManager.DownloadPath + $"/m{modFile.ModId}f{modFile.FileId}.zip";

        try
        {
            using var fs = new FileStream(zipPath, FileMode.Create);
            await stream.CopyToAsync(fs);
        }
        catch (Exception e)
        {
            FusionLogger.LogException($"downloading zip from mod {modFile.ModId}", e);

            transaction.callback?.Invoke(DownloadCallbackInfo.FailedCallback);

            EndDownload();
            return;
        }

        // Load the pallet
        await ModDownloadManager.LoadPalletFromZip(zipPath, modFile.ModId, modFile.FileId.Value, transaction.callback);

        // Delete temp zip
        File.Delete(zipPath);

        EndDownload();
    }

    private static void EndDownload()
    {
        _currentTransaction = default;
        _isDownloading = false;
    }
}