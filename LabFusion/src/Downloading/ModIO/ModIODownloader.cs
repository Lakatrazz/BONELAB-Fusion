using LabFusion.Data;
using LabFusion.Preferences;
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
            if (info.Data.Mature && !CommonPreferences.ShowMatureMods)
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
            EndDownload();

            transaction.Callback?.Invoke(DownloadCallbackInfo.FailedCallback);
        }
    }

    private static IEnumerator CoDownloadWithToken(string token, ModTransaction transaction, ModIOFile modFile, string url)
    {
        var zipPath = ModDownloadManager.DownloadPath + $"/m{modFile.ModID}f{modFile.FileID}.zip";
        using var routine = DownloadWithToken(token, transaction, modFile, url, zipPath);
        bool handedOff = false;

        try
        {
            while (true)
            {
                bool moved;
                try
                {
                    moved = routine.MoveNext();
                }
                catch (Exception e)
                {
                    FusionLogger.LogException("downloading mod", e);
                    break;
                }

                if (!moved)
                {
                    handedOff = true;
                    break;
                }

                yield return routine.Current;
            }
        }
        finally
        {
            if (!handedOff)
            {
                EndDownload();
                DeleteDownload(zipPath);
                transaction.Callback?.Invoke(DownloadCallbackInfo.FailedCallback);
            }
        }
    }

    private static IEnumerator<object> DownloadWithToken(string token, ModTransaction transaction, ModIOFile modFile, string url, string zipPath)
    {
        ModDownloadManager.ValidateDirectories();
        transaction.Report(0f);

        using HttpClient client = new();
        client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

        var responseTask = client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        while (!responseTask.IsCompleted)
        {
            yield return null;
        }

        using var response = responseTask.GetAwaiter().GetResult();
        response.EnsureSuccessStatusCode();
        var content = response.Content;
        var contentLength = content.Headers.ContentLength;
        var maxBytes = transaction.MaxBytes;

        if (maxBytes.HasValue && contentLength.HasValue && contentLength.Value > maxBytes.Value)
        {
            throw new InvalidDataException("The mod exceeds the download size limit.");
        }

        using (var copyStream = new FileStream(zipPath, FileMode.Create, FileAccess.Write))
        {
            long copied = 0;
            var copyTask = CopyDownload(content, copyStream, maxBytes, bytes => System.Threading.Interlocked.Exchange(ref copied, bytes));

            while (!copyTask.IsCompleted)
            {
                if (contentLength > 0)
                {
                    transaction.Report((float)System.Threading.Interlocked.Read(ref copied) / contentLength.Value);
                }

                yield return null;
            }

            copyTask.GetAwaiter().GetResult();
        }

        transaction.Report(1f);
        ModDownloadManager.LoadPalletFromZip(zipPath, modFile, transaction.Temporary, OnScheduledLoad, transaction.Callback);

        void OnScheduledLoad()
        {
            EndDownload();
            DeleteDownload(zipPath);
        }
    }

    internal static async Task CopyDownload(HttpContent content, Stream destination, long? maxBytes, Action<long> progress)
    {
        using var source = await content.ReadAsStreamAsync().ConfigureAwait(false);
        var buffer = new byte[81920];
        long total = 0;
        int read;

        while ((read = await source.ReadAsync(buffer.AsMemory()).ConfigureAwait(false)) != 0)
        {
            if (maxBytes.HasValue && read > maxBytes.Value - total)
            {
                throw new InvalidDataException("The mod exceeds the download size limit.");
            }

            await destination.WriteAsync(buffer.AsMemory(0, read)).ConfigureAwait(false);
            total += read;
            progress?.Invoke(total);
        }

        if (content.Headers.ContentLength is long expected && total != expected)
        {
            throw new InvalidDataException("The mod download is incomplete.");
        }
    }

    private static void DeleteDownload(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch (Exception e)
        {
            FusionLogger.LogException("deleting downloaded zip", e);
        }
    }

    private static void EndDownload()
    {
        _currentTransaction = default;
        _isDownloading = false;
    }
}
