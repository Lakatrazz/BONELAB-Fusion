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

            transaction.Callback?.Invoke(DownloadCallbackInfo.FailedCallback);
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

    public static ModTransaction GetTransaction(int modId)
    {
        // Check if the current transaction is for this mod
        if (IsDownloading && CurrentTransaction.ModFile.ModId == modId)
        {
            return CurrentTransaction;
        }

        // Look through queued transactions
        return QueuedTransactions.FirstOrDefault((transaction) => transaction.ModFile.ModId == modId);
    }

    public static void EnqueueDownload(ModTransaction transaction)
    {
        var existingTransaction = GetTransaction(transaction.ModFile.ModId);

        // If this mod is already being downloaded, just forward the download to the existing transaction
        if (existingTransaction != null)
        {
            existingTransaction.HookDownload(transaction.Callback);
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
        ModIOFile modFile = transaction.ModFile;

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
            string url = ModIOSettings.FormatDownloadPath(modFile.ModId, modFile.FileId.Value);
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
            transaction.Callback?.Invoke(DownloadCallbackInfo.FailedCallback);

            EndDownload();
        }
    }

    private static IEnumerator CoDownloadWithToken(string token, ModTransaction transaction, ModIOFile modFile, string url)
    {
        // Before doing anything, make sure all mod directories are valid
        ModDownloadManager.ValidateDirectories();

        // Initialize the transaction progress at 0%
        transaction.Report(0f);

        // Send a request to mod.io for the headers
        // We don't want to read the whole content yet
        var handler = new HttpClientHandler()
        {
            ClientCertificateOptions = ClientCertificateOption.Manual,
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        };

        using HttpClient client = new(handler);
        client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

        var responseTask = client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

        while (!responseTask.IsCompleted)
        {
            yield return null;
        }

        // Make sure the response was successful
        if (!responseTask.IsCompletedSuccessfully)
        {
            FusionLogger.LogException("getting response from mod.io", responseTask.Exception);

            FailDownload();

            yield break;
        }

        // Get the resulting content
        var content = responseTask.Result.Content;
        var contentLength = content.Headers.ContentLength.Value;

        // Check if the file size is too large to download
        var maxBytes = transaction.MaxBytes;

        if (maxBytes.HasValue && contentLength > maxBytes.Value)
        {
            FusionLogger.Warn($"Skipped download of mod {modFile.ModId} due to the file size being too large.");

            FailDownload();

            yield break;
        }

        // Download the content into a MemoryStream
        using var downloadStream = new MemoryStream();
        var downloadTask = content.CopyToAsync(downloadStream);

        while (!downloadTask.IsCompleted)
        {
            transaction.Report((float)downloadStream.Length / contentLength);

            yield return null;
        }

        // Set progress to 100%
        transaction.Report(1f);

        // Make sure the download was successful
        if (!downloadTask.IsCompletedSuccessfully)
        {
            FusionLogger.LogException("copying download to stream", downloadTask.Exception);

            FailDownload();

            yield break;
        }

        // Install the stream into a zip file
        var zipPath = ModDownloadManager.DownloadPath + $"/m{modFile.ModId}f{modFile.FileId}.zip";

        // Copy the download to the zip file
        // Make sure this using statement ends before we load the pallet, so that the file is not in use
        using (var copyStream = new FileStream(zipPath, FileMode.Create))
        {
            downloadStream.Position = 0;
            var copyTask = downloadStream.CopyToAsync(copyStream);

            while (!copyTask.IsCompleted)
            {
                yield return null;
            }

            if (!copyTask.IsCompletedSuccessfully)
            {
                FusionLogger.LogException("copying downloaded zip", copyTask.Exception);

                FailDownload();

                yield break;
            }
        }

        // Load the pallet
        ModDownloadManager.LoadPalletFromZip(zipPath, modFile, transaction.Temporary, OnScheduledLoad, transaction.Callback);

        void OnScheduledLoad()
        {
            // Delete temp zip
            File.Delete(zipPath);

            EndDownload();
        }

        void FailDownload()
        {
            transaction.Callback?.Invoke(DownloadCallbackInfo.FailedCallback);

            EndDownload();
        }
    }

    private static void EndDownload()
    {
        _currentTransaction = default;
        _isDownloading = false;
    }
}