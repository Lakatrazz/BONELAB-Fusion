using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Extensions {
    public static class RigidbodyExtensions {
        /// <summary>
        /// Returns if this rigidbody is already sleeping or is below the sleep threshold.
        /// </summary>
        /// <param name="rigidbody"></param>
        /// <returns></returns>
        public static bool ShouldSleep(this Rigidbody rigidbody) {
            return (rigidbody.velocity.sqrMagnitude < rigidbody.sleepVelocity * rigidbody.sleepVelocity) 
                || (rigidbody.angularVelocity.sqrMagnitude < rigidbody.sleepAngularVelocity * rigidbody.sleepAngularVelocity);
        }
    }
}
