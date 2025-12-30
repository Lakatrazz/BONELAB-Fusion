using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Marrow;

using MarrowFusion.Bonelab.Data;
using LabFusion.Entities;

namespace MarrowFusion.Bonelab.Extenders;

public class BonelabRigVitals
{
    public SerializedBodyVitals Vitals { get; private set; } = null;

    public bool IsVitalsDirty { get; private set; } = false;

    public void SetVitals(SerializedBodyVitals vitals)
    {
        Vitals = vitals;
        SetVitalsDirty();
    }

    public void SetVitalsDirty() => IsVitalsDirty = true;

    public void Resolve(RigRefs references)
    {
        if (IsVitalsDirty)
        {
            ApplyVitals(references);

            IsVitalsDirty = false;
        }
    }

    private void ApplyVitals(RigRefs references)
    {
        if (Vitals == null)
        {
            return;
        }

        CalibratePlayerBodyScale(references);

        ApplyPullCord(references);

        ApplyControls(references);
    }

    private void CalibratePlayerBodyScale(RigRefs references)
    {
        var controllerRig = references.ControllerRig;

        var realHeptaAvatar = controllerRig.avatar.TryCast<RealHeptaAvatar>();

        if (realHeptaAvatar == null)
        {
            return;
        }

        realHeptaAvatar.realWorldHeight = Vitals.Height;

        realHeptaAvatar.SetTorsoFromCircumferences(Vitals.Chest, Vitals.Underbust, Vitals.Waist, Vitals.Hips);
        realHeptaAvatar.SetPlayerWingspan(Vitals.Wingspan, Vitals.Height);
        realHeptaAvatar.SetPlayerInseam(Vitals.Inseam, Vitals.Height);

        var calibrationAvatar = references.RigManager.CalibrationAvatar;

        if (calibrationAvatar != null)
        {
            realHeptaAvatar.RefreshBodyMeasurements(calibrationAvatar);
        }
    }

    private void ApplyPullCord(RigRefs references)
    {
        var pullCord = references.RigManager.GetComponentInChildren<PullCordDevice>(true);

        if (pullCord == null)
        {
            return;
        }

        var rigManager = references.RigManager;
        var avatar = rigManager.avatar;

        var slotContainer = pullCord._slotContainer;
        var bodyRegion = Vitals.BodyLogFlipped ? SlotContainer.BodyRegion.ArmLowerLf : SlotContainer.BodyRegion.ArmLowerRt;
        slotContainer.bodyRegion = bodyRegion;

        if (avatar != null)
        {
            rigManager.physicsRig.SetBodySlot(avatar);
        }

        pullCord.gameObject.SetActive(Vitals.HasBodyLog);

        pullCord.bodyLogEnabled = Vitals.BodyLogEnabled;
        pullCord.leftHanded = !Vitals.BodyLogFlipped;

        pullCord.BodyVitalsUpdateEvent();
    }

    private void ApplyControls(RigRefs references)
    {
        var openControllerRig = references.ControllerRig.TryCast<OpenControllerRig>();

        if (openControllerRig == null)
        {
            return;
        }

        openControllerRig.degreesPerSnap = Vitals.Loco_DegreesPerSnap;
        openControllerRig.snapDegreesPerFrame = Vitals.Loco_SnapDegreesPerFrame;

        openControllerRig.isRightHanded = Vitals.IsRightHanded;

        openControllerRig.curveMode = (OpenControllerRig.CurveMode)Vitals.Loco_CurveMode;
        openControllerRig.directionMode = (OpenControllerRig.DirectionMode)Vitals.Loco_Direction;
    }
}
