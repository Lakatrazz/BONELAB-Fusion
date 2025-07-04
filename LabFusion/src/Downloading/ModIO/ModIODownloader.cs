﻿using LabFusion.Data;
using LabFusion.Preferences.Client;
using LabFusion.Safety;
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
#if DEBUG
        FusionLogger.Warn("Cancelling queued mod transactions.");
#endif

        var count = QueuedTransactions.Count;

        for (var i = 0; i < count; i++)
        {
            var transaction = QueuedTransactions.Dequeue();

            transaction.Callback?.Invoke(DownloadCallbackInfo.CanceledCallback);
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
        if (IsDownloading && CurrentTransaction.ModFile.ModID == modId)
        {
            return CurrentTransaction;
        }

        // Look through queued transactions
        return QueuedTransactions.FirstOrDefault((transaction) => transaction.ModFile.ModID == modId);
    }

    public static void EnqueueDownload(ModTransaction transaction)
    {
        var existingTransaction = GetTransaction(transaction.ModFile.ModID);

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

        ModIOFile modFile = transaction.ModFile;

        // Request the latest mod data from mod.io
        ModIOManager.GetMod(modFile.ModID, OnRequestedMod);

        void OnRequestedMod(ModCallbackInfo info)
        {
            // Check if the mod request failed
            if (info.Result != ModResult.SUCCEEDED)
            {
                // If it failed, but we have a FileID anyways, try downloading that
                // That way hidden mods download properly
                if (modFile.FileID.HasValue)
                {
                    OnReceivedFile(modFile);
                    return;
                }

                FusionLogger.Warn($"Failed getting a mod file for mod {modFile.ModID}, cancelling download!");

                FailDownload();
                return;
            }

            // Check for maturity
            if (info.Data.Mature && !ClientSettings.Downloading.DownloadMatureContent.Value)
            {
                FusionLogger.Warn($"Skipped download of mod {info.Data.NameID} due to it containing mature content.");

                FailDownload();
                return;
            }

            // Check for blacklist
            if (ModBlacklist.IsBlacklisted(info.Data.NameID) || GlobalModBlacklistManager.IsNameIDBlacklisted(info.Data.NameID))
            {
                FusionLogger.Warn($"Skipped download of mod {info.Data.NameID} due to it being blacklisted!");

                FailDownload();
                return;
            }

            var platform = ModIOManager.GetValidPlatform(info.Data);

            if (!platform.HasValue)
            {
                FusionLogger.Warn($"Tried beginning download for mod {modFile.ModID}, but it had no valid platforms!");

                FailDownload();
                return;
            }

            modFile = new ModIOFile(info.Data.ID, platform.Value.ModFileLive);

            OnReceivedFile(modFile);
        }

        void OnReceivedFile(ModIOFile modFile)
        {
            int modID = modFile.ModID;

            // Check for blacklist
            if (ModBlacklist.IsBlacklisted(modID.ToString()) || GlobalModBlacklistManager.IsModIDBlacklisted(modFile.ModID))
            {
                FusionLogger.Warn($"Skipped download of mod {modID} due to it being blacklisted!");

                FailDownload();
                return;
            }

            string url = ModIOSettings.FormatDownloadPath(modFile.ModID, modFile.FileID.Value);
            ModIOSettings.LoadToken(OnTokenLoaded);

            void OnTokenLoaded(string token)
            {
                // If the token is null, it likely didn't load
                if (string.IsNullOrWhiteSpace(token))
                {
#if DEBUG
                    FusionLogger.Warn("Token is null, cancelling mod download.");
#endif

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
            FusionLogger.Warn($"Skipped download of mod {modFile.ModID} due to the file size being too large.");

            FailDownload();

            yield break;
        }

        // Install the content into a zip file
        var zipPath = ModDownloadManager.DownloadPath + $"/m{modFile.ModID}f{modFile.FileID}.zip";

        // Make sure this using statement ends before we load the pallet, so that the file is not in use
        using (var copyStream = new FileStream(zipPath, FileMode.Create))
        {
            var copyTask = content.CopyToAsync(copyStream);

            while (!copyTask.IsCompleted)
            {
                transaction.Report((float)copyStream.Length / contentLength);

                yield return null;
            }

            if (!copyTask.IsCompletedSuccessfully)
            {
                FusionLogger.LogException("copying downloaded zip", copyTask.Exception);

                FailDownload();

                yield break;
            }
        }

        // Set progress to 100%
        transaction.Report(1f);

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