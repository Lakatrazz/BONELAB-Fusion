using LabFusion.Utilities;

using MelonLoader;

using System.Collections;

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
        // Check if the current transaction is for this mod
        if (IsDownloading && CurrentTransaction.modFile.ModId == modId)
        {
            return CurrentTransaction;
        }

        // Look through queued transactions
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

    private static void BeginDownload(ModTransaction transaction)
    {
        ModIOFile modFile = transaction.modFile;

        if (!modFile.FileId.HasValue)
        {
            // Request the latest file id
            ModIOManager.GetMod(modFile.ModId, OnRequestedMod);

            void OnRequestedMod(ModCallbackInfo info)
            {
                if (info.result == ModResult.FAILED)
                {
                    FusionLogger.Warn($"Failed getting a mod file for mod {modFile.ModId}, cancelling download!");

                    transaction.callback?.Invoke(DownloadCallbackInfo.FailedCallback);

                    EndDownload();
                    return;
                }

                var platform = ModIOManager.GetValidPlatform(info.data);

                if (!platform.HasValue)
                {
                    FusionLogger.Warn($"Tried beginning download for mod {modFile.ModId}, but it had no valid platforms!");

                    transaction.callback?.Invoke(DownloadCallbackInfo.FailedCallback);

                    EndDownload();
                    return;
                }

                modFile = new ModIOFile(info.data.Id, platform.Value.ModfileLive);

                OnReceivedFile(modFile);
            }

            return;
        }
        
        OnReceivedFile(modFile);

        void OnReceivedFile(ModIOFile modFile)
        {
            _currentTransaction = transaction;
            _isDownloading = true;

            string url = FormatDownloadPath(modFile.ModId, modFile.FileId.Value);
            ModIOSettings.LoadToken(OnTokenLoaded);

            void OnTokenLoaded(string token)
            {
                MelonCoroutines.Start(CoDownloadWithToken(token, transaction, modFile, url));
            }
        }
    }

    private static IEnumerator CoDownloadWithToken(string token, ModTransaction transaction, ModIOFile modFile, string url)
    {
        HttpClient client = new();
        client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

        var streamTask = client.GetStreamAsync(url);

        while (!streamTask.IsCompleted)
        {
            yield return null;
        }

        ModDownloadManager.ValidateStagingDirectory();

        var zipPath = ModDownloadManager.DownloadPath + $"/m{modFile.ModId}f{modFile.FileId}.zip";

        using var fs = new FileStream(zipPath, FileMode.Create);
        var copyTask = streamTask.Result.CopyToAsync(fs);

        while (!copyTask.IsCompleted)
        {
            yield return null;
        }

        // Load the pallet
        ModDownloadManager.LoadPalletFromZip(zipPath, modFile, OnScheduledLoad, transaction.callback);

        void OnScheduledLoad()
        {
            // Delete temp zip
            File.Delete(zipPath);

            EndDownload();
        }
    }

    private static void EndDownload()
    {
        _currentTransaction = default;
        _isDownloading = false;
    }
}