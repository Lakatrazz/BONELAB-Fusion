using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Rig;

using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

    private RigReferenceCollection _references = null;

    public void SwapAvatar(SerializedAvatarStats stats, string barcode)
    {
        _stats = stats;
        _avatarBarcode = barcode;
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

    public void Resolve(RigReferenceCollection references)
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
            _vitals?.CopyTo(references.RigManager.GetComponentInChildren<BodyVitals>());

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
