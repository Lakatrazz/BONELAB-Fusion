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

    /// <summary>
    /// Cancels all queued downloads. Does not cancel the currently active download.
    /// </summary>
    public static void CancelQueue()
    {
        var count = QueuedTransactions.Count;

        for (var i = 0; i < count; i++)
        {
            var transaction = QueuedTransactions.Dequeue();

            transaction.callback?.Invoke(DownloadCallbackInfo.FailedCallback);
        }
    }

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
        // Set the active transaction to this one
        _currentTransaction = transaction;
        _isDownloading = true;

        // Validate the mod file
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

                    FailDownload();
                    return;
                }

                var platform = ModIOManager.GetValidPlatform(info.data);

                if (!platform.HasValue)
                {
                    FusionLogger.Warn($"Tried beginning download for mod {modFile.ModId}, but it had no valid platforms!");

                    FailDownload();
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
            string url = FormatDownloadPath(modFile.ModId, modFile.FileId.Value);
            ModIOSettings.LoadToken(OnTokenLoaded);

            void OnTokenLoaded(string token)
            {
                // If the token is null, it likely didn't load
                if (string.IsNullOrWhiteSpace(token))
                {
                    FailDownload();

                    return;
                }

                MelonCoroutines.Start(CoDownloadWithToken(token, transaction, modFile, url));
            }
        }

        void FailDownload()
        {
            transaction.callback?.Invoke(DownloadCallbackInfo.FailedCallback);

            EndDownload();
        }
    }

    private static IEnumerator CoDownloadWithToken(string token, ModTransaction transaction, ModIOFile modFile, string url)
    {
        // Before doing anything, make sure all mod directories are valid
        ModDownloadManager.ValidateDirectories();

        // Initialize the transaction progress at 0%
        // TODO: Update this value
        transaction.Progress = 0f;

        // Send a request to mod.io for the files
        using HttpClient client = new();
        client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

        var streamTask = client.GetStreamAsync(url);

        while (!streamTask.IsCompleted)
        {
            yield return null;
        }

        // Install the stream into a zip file
        var zipPath = ModDownloadManager.DownloadPath + $"/m{modFile.ModId}f{modFile.FileId}.zip";

        FileStream copyStream = null;
        Task copyTask = null;

        copyStream = new FileStream(zipPath, FileMode.Create);
        copyTask = streamTask.Result.CopyToAsync(copyStream);

        while (!copyTask.IsCompleted)
        {
            yield return null;
        }

        copyStream?.Dispose();

        if (!copyTask.IsCompletedSuccessfully)
        {
            FusionLogger.LogException("copying downloaded zip", copyTask.Exception);

            FailDownload();

            yield break;
        }

        // Load the pallet
        ModDownloadManager.LoadPalletFromZip(zipPath, modFile, transaction.temporary, OnScheduledLoad, transaction.callback);

        void OnScheduledLoad()
        {
            // Delete temp zip
            File.Delete(zipPath);

            EndDownload();
        }

        void FailDownload()
        {
            transaction.callback?.Invoke(DownloadCallbackInfo.FailedCallback);

            EndDownload();
        }
    }

    private static void EndDownload()
    {
        _currentTransaction = default;
        _isDownloading = false;
    }
}