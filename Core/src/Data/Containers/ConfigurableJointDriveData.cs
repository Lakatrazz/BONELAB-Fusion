using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Data {
    public struct ConfigurableJointDriveData {
        public JointDrive xDrive;
        public JointDrive yDrive;
        public JointDrive zDrive;

        public JointDrive angularXDrive;
        public JointDrive angularYZDrive;
        public JointDrive slerpDrive;

        public ConfigurableJointDriveData(ConfigurableJoint joint) { 
            xDrive = joint.xDrive;
            yDrive = joint.yDrive;
            zDrive = joint.zDrive;

            angularXDrive = joint.angularXDrive;
            angularYZDrive = joint.angularYZDrive;
            slerpDrive = joint.slerpDrive;
        }

        public void CopyTo(ConfigurableJoint joint) {
            joint.xDrive = xDrive;
            joint.yDrive = yDrive;
            joint.zDrive = zDrive;

            joint.angularXDrive = angularXDrive;
            joint.angularYZDrive = angularYZDrive;
            joint.slerpDrive = slerpDrive;
        }
    }
}
