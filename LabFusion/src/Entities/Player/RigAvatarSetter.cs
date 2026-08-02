using Il2CppSLZ.Marrow.Warehouse;

using LabFusion.Data;
using LabFusion.Downloading;
using LabFusion.Marrow.Data;
using LabFusion.Marrow;
using LabFusion.Marrow.Extensions;
using LabFusion.Preferences.Client;
using LabFusion.RPC;
using LabFusion.Utilities;

using UnityEngine;

using Avatar = Il2CppSLZ.VRMK.Avatar;

namespace LabFusion.Entities;

public class RigAvatarSetter
{
    public event Action OnAvatarChanged;

    private bool _isAvatarDirty = false;
    private SerializedAvatarStats _stats = null;
    private string _avatarBarcode = MarrowBarcodes.EmptyBarcode;

    public SerializedAvatarStats AvatarStats => _stats;

    public string AvatarBarcode => _avatarBarcode;

    private RigRefs _references = null;

    private NetworkEntity _entity = null;

    public RigProgressBar ProgressBar { get; } = new();

    public RigAvatarSetter()
    {
        ProgressBar.Visible = false;
    }

    public void SetEntity(NetworkEntity entity)
    {
        _entity = entity;
    }

    public void SwapAvatar(SerializedAvatarStats stats, string barcode)
    {
        _stats = stats;
        _avatarBarcode = barcode;
        SetAvatarDirty();

        CheckForInstall(barcode);
    }

    private void CheckForInstall(string barcode)
    {
        // Hide the progress bar before checking for a new install
        ProgressBar.Visible = false;

        // Check if we need to install the avatar
        bool hasCrate = AssetWarehouseSearcher.HasCrate<AvatarCrate>(new(barcode));

        if (hasCrate)
        {
            return;
        }

        bool shouldDownload = ClientSettings.Downloading.DownloadAvatars.Value;

        // Check if we should download the mod (it's not blacklisted, mod downloading disabled, etc.)
        if (!shouldDownload)
        {
            return;
        }

        long maxBytes = DataConversions.ConvertMegabytesToBytes(ClientSettings.Downloading.MaxFileSize.Value);

        var owner = _entity.OwnerID.SmallID;

        NetworkModRequester.RequestAndInstallMod(new NetworkModRequester.ModInstallInfo()
        {
            Target = owner,
            Barcode = barcode,
            BeginDownloadCallback = OnAvatarBeginDownload,
            FinishDownloadCallback = OnAvatarDownloaded,
            MaxBytes = maxBytes,
            Reporter = ProgressBar,
        });
    }

    private void OnAvatarBeginDownload(NetworkModRequester.ModCallbackInfo info)
    {
        // Now that we know the download has been queued, we can show the progress bar
        ProgressBar.Report(0f);
        ProgressBar.Visible = true;
    }

    private void OnAvatarDownloaded(DownloadCallbackInfo info)
    {
        ProgressBar.Visible = false;

        if (info.Result != ModResult.SUCCEEDED)
        {
            FusionLogger.Warn($"Failed downloading avatar for rig {_entity.ID}!");
            return;
        }

        // We just set the avatar dirty, so that if it's changed to another avatar by this point we aren't overriding it
        SetAvatarDirty();
    }

    public void SetAvatarDirty()
    {
        _isAvatarDirty = true;
    }

    public void SetDirty()
    {
        SetAvatarDirty();
    }

    public void Resolve(RigRefs references)
    {
        _references = references;

        if (_isAvatarDirty)
        {
            var rigManager = references.RigManager;

            rigManager.SwitchAvatarWithCallbacks(new RigManagerExtensions.AvatarSwitchInfo()
            {
                Barcode = AvatarBarcode,
                BeforeSwapAvatarCallback = OnBeforeAvatarSwap,
                CompletedCallback = OnSwapAvatar,
            });

            _isAvatarDirty = false;
        }
    }

    public void OnRefreshBodyMeasurements(Avatar avatar)
    {
        if (_stats == null)
        {
            return;
        }

        _stats.CopyTo(avatar);
    }

    private void OnSwapAvatar(bool success)
    {
        var rigManager = _references.RigManager;

        if (!success)
        {
            var calibrationAvatarBarcode = MarrowGameReferences.CalibrationAvatarReference.Barcode.ID;

            rigManager.SwitchAvatarWithCallbacks(new RigManagerExtensions.AvatarSwitchInfo()
            {
                Barcode = calibrationAvatarBarcode,
                BeforeSwapAvatarCallback = OnBeforeAvatarSwap,
                CompletedCallback = OnSwapFallback,
            });
        }
        else
        {
            OnAvatarChanged?.Invoke();
        }
    }

    private void OnSwapFallback(bool success)
    {
        OnAvatarChanged?.Invoke();
    }

    private void OnBeforeAvatarSwap(string barcode, Avatar avatar)
    {
        if (_stats == null)
        {
            return;
        }

        var avatarTransform = avatar.transform;

        avatar.PreComputed = false;

        avatar.PrecomputeAvatar();
        avatar.RefreshBodyMeasurements();

        float baseHeight = avatar.height;
        var baseScale = avatarTransform.localScale;

        float newHeight = _stats.Height;

        if (!Mathf.Approximately(baseHeight, newHeight))
        {
            avatar.PreComputed = false;

            var newScale = baseScale * (newHeight / baseHeight);

            avatarTransform.localScale = newScale;
        }
    }
}
