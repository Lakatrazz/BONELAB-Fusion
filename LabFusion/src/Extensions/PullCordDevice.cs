using Il2CppSLZ.Bonelab;

namespace LabFusion.Extensions
{
    public static class PullCordDeviceExtensions
    {
        public static void TryEnableBall(this PullCordDevice device)
        {
            if (!device.ballShown)
            {
                device.EnableBall();
                device.ballShown = true;
                device.ballHost.EnableInteraction();
            }
        }

        public static void TryDisableBall(this PullCordDevice device)
        {
            if (device.ballShown)
            {
                device.DisableBall();
                device.ballShown = false;
                device.ballHost.DisableInteraction();
            }
        }
    }
}
