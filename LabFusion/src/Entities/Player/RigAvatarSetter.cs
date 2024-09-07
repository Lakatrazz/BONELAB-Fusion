using Il2CppSLZ.Marrow.Warehouse;

using LabFusion.Data;
using LabFusion.Downloading;
using LabFusion.Extensions;
using LabFusion.Marrow;
using LabFusion.Network;
using LabFusion.Preferences.Client;
using LabFusion.RPC;
using LabFusion.Utilities;

using UnityEngine;

namespace LabFusion.Entities;

public class RigAvatarSetter
{
    public event Action OnAvatarChanged;

    private bool _isAvatarDirty = false;
    private SerializedAvatarStats _stats = null;
    private string _avatarBarcode = CommonBarcodes.INVALID_AVATAR_BARCODE;

    public SerializedAvatarStats AvatarStats => _stats;

    public string AvatarBarcode => _avatarBarcode;

    private bool _isVitalsDirty = false;
    private SerializedBodyVitals _vitals = null;

    private RigRefs _references = null;

    private NetworkEntity _entity = null;

    private readonly RigProgressBar _progressBar = new();
    public RigProgressBar ProgressBar => _progressBar;

    public RigAvatarSetter(NetworkEntity entity)
    {
        _entity = entity;

        ProgressBar.Visible = false;
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
        // Check if we need to install the avatar
        bool hasCrate = CrateFilterer.HasCrate<AvatarCrate>(new(barcode));

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

        var owner = _entity.OwnerId.SmallId;

        NetworkModRequester.RequestAndInstallMod(new NetworkModRequester.ModInstallInfo()
        {
            target = owner,
            barcode = barcode,
            beginDownloadCallback = OnAvatarBeginDownload,
            finishDownloadCallback = OnAvatarDownloaded,
            maxBytes = maxBytes,
            reporter = ProgressBar,
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

        if (info.result == ModResult.FAILED)
        {
            FusionLogger.Warn($"Failed downloading avatar for rig {_entity.Id}!");
            return;
        }

        // We just set the avatar dirty, so that if it's changed to another avatar by this point we aren't overriding it
        SetAvatarDirty();
    }

    public void SetVitals(SerializedBodyVitals vitals)
    {
        this._vitals = vitals;
        SetVitalsDirty();
    }

    public void SetAvatarDirty()
    {
        _isAvatarDirty = true;
    }

    public void SetVitalsDirty()
    {
        _isVitalsDirty = true;
    }

    public void SetDirty()
    {
        SetAvatarDirty();
        SetVitalsDirty();
    }

    public void Resolve(RigRefs references)
    {
        _references = references;

        if (_isAvatarDirty)
        {
            references.SwapAvatarCrate(AvatarBarcode, OnSwapAvatar, OnPrepareAvatar);

            PlayerAdditionsHelper.OnAvatarChanged(references.RigManager);

            _isAvatarDirty = false;
        }

        if (_isVitalsDirty)
        {
            //_vitals?.CopyTo(references.RigManager.GetComponentInChildren<BodyVitals>());

            _isVitalsDirty = false;
        }
    }


    private void OnSwapAvatar(bool success)
    {
        var rm = _references.RigManager;

        if (!success)
        {
            _references.SwapAvatarCrate(FusionAvatar.POLY_BLANK_BARCODE, OnSwapFallback, OnPrepareAvatar);
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

    private void OnPrepareAvatar(string barcode, GameObject avatar)
    {
        // If we have synced avatar stats, set the scale properly
        if (_stats != null)
        {
            Transform transform = avatar.transform;

            // Polyblank should just scale based on the custom avatar height
            if (barcode == FusionAvatar.POLY_BLANK_BARCODE)
            {
                float newHeight = _stats.height;
                transform.localScale = Vector3Extensions.one * (newHeight / 1.76f);
            }
            // Otherwise, apply the synced scale
            else
            {
                transform.localScale = _stats.localScale;
            }
        }
    }
}
