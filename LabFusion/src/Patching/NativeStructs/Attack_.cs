using System;
using System.Runtime.InteropServices;
using SLZ.Marrow.Data;
using UnityEngine;

namespace LabFusion.NativeStructs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Attack_
    {
        public float damage;

        public Vector3 normal;

        public Vector3 origin;

        public Vector3 direction;

        public bool backFacing;

        public int OrderInPool;

        public IntPtr collider;

        public AttackType attackType;

        public IntPtr proxy;
    }
}
