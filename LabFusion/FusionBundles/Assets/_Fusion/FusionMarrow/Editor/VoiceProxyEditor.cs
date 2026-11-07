using UnityEditor;

using UltEvents;

using UnityEditor.UIElements;
using UnityEngine.UIElements;

using System;

using UnityEngine;

using System.Reflection;

namespace LabFusion.Marrow.Integration
{
    [CustomEditor(typeof(VoiceProxy))]
    public class VoiceProxyEditor : Editor
    {
        private SerializedProperty _defaultChannelProperty;
        private SerializedProperty _defaultConnectedProxyProperty;
        private SerializedProperty _canHearSelfProperty;

        private SerializedProperty _distanceFalloffProperty;
        private SerializedProperty _falloffMinDistanceProperty;
        private SerializedProperty _falloffMaxDistanceProperty;

        public void OnEnable()
        {
            _defaultChannelProperty = serializedObject.FindProperty(nameof(VoiceProxy.DefaultChannel));
            _defaultConnectedProxyProperty = serializedObject.FindProperty(nameof(VoiceProxy.DefaultConnectedProxy));
            _canHearSelfProperty = serializedObject.FindProperty(nameof(VoiceProxy.CanHearSelf));

            _distanceFalloffProperty = serializedObject.FindProperty(nameof(VoiceProxy.DistanceFalloff));
            _falloffMinDistanceProperty = serializedObject.FindProperty(nameof(VoiceProxy.FalloffMinDistance));
            _falloffMaxDistanceProperty = serializedObject.FindProperty(nameof(VoiceProxy.FalloffMaxDistance));
        }

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            var proxy = target as VoiceProxy;

            if (!proxy.TryGetComponent<LifeCycleEvents>(out var lifeCycleEvent))
            {
                var warnBox = new HelpBox("If you want to change the default voice settings, please add" +
                    " a LifeCycleEvents to this GameObject!", HelpBoxMessageType.Warning);
                root.Add(warnBox);

                var addLifeCycleEventsButton = new Button(() =>
                {
                    Undo.AddComponent<LifeCycleEvents>(proxy.gameObject);
                })
                {
                    text = "Add LifeCycleEvents"
                };
                root.Add(addLifeCycleEventsButton);

                return root;
            }

            var defaultChannel = new PropertyField(_defaultChannelProperty);
            root.Add(defaultChannel);

            var defaultConnectedProxy = new PropertyField(_defaultConnectedProxyProperty);
            root.Add(defaultConnectedProxy);

            var canHearSelf = new PropertyField(_canHearSelfProperty);
            root.Add(canHearSelf);

            var distanceFalloff = new PropertyField(_distanceFalloffProperty);
            root.Add(distanceFalloff);

            var minDistance = new PropertyField(_falloffMinDistanceProperty);
            root.Add(minDistance);

            var maxDistance = new PropertyField(_falloffMaxDistanceProperty);
            root.Add(maxDistance);

            if (proxy.TryGetComponent<AudioSource>(out _))
            {
                var audioInfoBox = new HelpBox("The AudioSource on this GameObject will be used to play player voices!", HelpBoxMessageType.Info);
                root.Add(audioInfoBox);
            }
            else
            {
                var audioInfoBox = new HelpBox("There is no AudioSource on this GameObject. One will automatically be created at runtime to play player voices!", HelpBoxMessageType.Info);
                root.Add(audioInfoBox);
            }

            var infoBox = new HelpBox("The LifeCycleEvents on this GameObject is used to inject variables for these settings." +
    " Make sure nothing else is using the LifeCycleEvents on this same GameObject.", HelpBoxMessageType.Info);
            root.Add(infoBox);

            root.RegisterCallback<SerializedPropertyChangeEvent>(evt =>
            {
                OverrideLifeCycleEvent(proxy, lifeCycleEvent);
            }, TrickleDown.TrickleDown);

            return root;
        }

        private void OverrideLifeCycleEvent(VoiceProxy proxy, LifeCycleEvents lifeCycleEvent)
        {
            var ultEvent = new UltEvent();

            ApplyToUltEvent(proxy, ultEvent);

            lifeCycleEvent.AwakeEvent = ultEvent;
        }

        private void ApplyToUltEvent(VoiceProxy proxy, UltEvent ultEvent)
        {
            ultEvent.Clear();

            Action<string> setChannelAction = proxy.SetChannelString;
            Action<UnityEngine.Object> setConnectedProxyAction = proxy.SetConnectedProxy;
            Action<bool> setCanHearSelfAction = proxy.SetCanHearSelf;
            Action<bool, float, float> setDistanceFalloffAction = proxy.SetDistanceFalloff;

            if (!string.IsNullOrWhiteSpace(proxy.DefaultChannel))
            {
                var setChannelCall = ultEvent.AddPersistentCall(setChannelAction);
                setChannelCall.PersistentArguments[0].String = proxy.DefaultChannel;
            }

            if (proxy.DefaultConnectedProxy != null)
            {
                var setConnectedProxyCall = ultEvent.AddPersistentCall(setConnectedProxyAction);
                var connectedProxyArgument = setConnectedProxyCall.PersistentArguments[0];

                connectedProxyArgument.Object = proxy.DefaultConnectedProxy;

                typeof(PersistentArgument).GetField("_String", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(connectedProxyArgument, typeof(UnityEngine.Object).AssemblyQualifiedName);
            }

            if (proxy.CanHearSelf)
            {
                var setCanHearSelfCall = ultEvent.AddPersistentCall(setCanHearSelfAction);
                setCanHearSelfCall.PersistentArguments[0].Bool = proxy.CanHearSelf;
            }

            if (proxy.DistanceFalloff)
            {
                var setDistanceFalloffCall = ultEvent.AddPersistentCall(setDistanceFalloffAction);
                setDistanceFalloffCall.PersistentArguments[0].Bool = proxy.DistanceFalloff;
                setDistanceFalloffCall.PersistentArguments[1].Float = proxy.FalloffMinDistance;
                setDistanceFalloffCall.PersistentArguments[2].Float = proxy.FalloffMaxDistance;
            }
        }
    }
}