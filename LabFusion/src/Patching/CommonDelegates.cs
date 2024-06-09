namespace LabFusion.Patching
{
    public delegate void ReceiveAttackPatchDelegate(IntPtr instance, IntPtr attack, IntPtr method);

    public delegate void EmptyPatchDelegate(IntPtr instance, IntPtr method);
}
