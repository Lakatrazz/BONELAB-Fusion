using LabFusion.Network;
using LabFusion.Senders;
using LabFusion.Utilities;

using SLZ.Bonelab;
using SLZ.UI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Data {
    public struct DescentIntroEvent
    {
        public int selectionNumber;
        public DescentIntroType type;

        public DescentIntroEvent(int selectionNumber, DescentIntroType type) {
            this.selectionNumber = selectionNumber;
            this.type = type;
        }
    }

    public struct DescentElevatorEvent
    {
        public DescentElevatorType type;

        public DescentElevatorEvent(DescentElevatorType type)
        {
            this.type = type;
        }
    }

    public struct DescentNooseEvent
    {
        public byte smallId;
        public DescentNooseType type;

        public DescentNooseEvent(byte smallId, DescentNooseType type)
        {
            this.smallId = smallId;
            this.type = type;
        }
    }

    public class DescentData : LevelDataHandler
    {
        public static DescentData Instance { get; private set; }

        public static NooseBonelabIntro Noose;
        public static TutorialElevator Elevator;
        public static GameControl_Descent GameController;
        public static Control_UI_BodyMeasurements BodyMeasurementsUI;

        private static List<DescentIntroEvent> _introEvents = new List<DescentIntroEvent>();
        private static List<DescentNooseEvent> _nooseEvents = new List<DescentNooseEvent>();
        private static List<DescentElevatorEvent> _elevatorEvents = new List<DescentElevatorEvent>();

        public static DescentIntroEvent CreateIntroEvent(int selectionNumber, DescentIntroType type) {
            var value = new DescentIntroEvent(selectionNumber, type);

            if (NetworkInfo.IsServer)
                _introEvents.Add(value);

            return value;
        }

        public static DescentNooseEvent CreateNooseEvent(byte smallId, DescentNooseType type)
        {
            var value = new DescentNooseEvent(smallId, type);

            if (NetworkInfo.IsServer)
                _nooseEvents.Add(value);

            return value;
        }

        public static DescentElevatorEvent CreateElevatorEvent(DescentElevatorType type)
        {
            var value = new DescentElevatorEvent(type);

            if (NetworkInfo.IsServer)
                _elevatorEvents.Add(value);

            return value;
        }

        protected override void SceneAwake() {
            Instance = this;
            _introEvents.Clear();
        }

        protected override void MainSceneInitialized() {
            Noose = GameObject.FindObjectOfType<NooseBonelabIntro>(true);
            Elevator = GameObject.FindObjectOfType<TutorialElevator>(true);
            GameController = GameObject.FindObjectOfType<GameControl_Descent>(true);
            BodyMeasurementsUI = GameObject.FindObjectOfType<Control_UI_BodyMeasurements>(true);
        }

        public void CacheValues() => MainSceneInitialized();

        protected override void PlayerCatchup(ulong longId) {
            // Send all intro events
            foreach (var intro in _introEvents) {
                CampaignSender.SendDescentIntro(intro, longId);
            }

            // Send all noose events
            foreach (var noose in _nooseEvents) {
                CampaignSender.SendDescentNoose(noose, longId);
            }

            // Send all elevator events
            foreach (var elevator in _elevatorEvents) {
                CampaignSender.SendDescentElevator(elevator, longId);
            }
        }
    }
}
