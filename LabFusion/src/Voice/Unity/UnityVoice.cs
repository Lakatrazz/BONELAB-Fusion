using UnityEngine;

namespace LabFusion.Voice.Unity
{
    public static class UnityVoice
    {
        public const int SampleRate = 41000;

        public const int ClipLength = 1;

        private static bool _hasMicrophones = false;
        private static bool _checkedForMic = false;

        private static bool CheckForMicrophone()
        {
            try
            {
                // As of now, using the Microphone with LemonLoader on a Quest 3 appears to cause infinite loading
                // This also includes checking if the devices array exists at all
                // However, checking if an error is thrown when trying to access the first element appears to prevent the infinite load
                // So, this works I guess?
                string firstMicrophone = Microphone.devices[0];

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsSupported()
        {
            if (!_checkedForMic)
            {
                _hasMicrophones = CheckForMicrophone();

                _checkedForMic = true;
            }

            return _hasMicrophones;
        }
    }
}
