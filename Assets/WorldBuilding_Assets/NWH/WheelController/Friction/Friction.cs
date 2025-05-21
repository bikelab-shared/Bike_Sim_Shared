using System;
using UnityEngine;

namespace NWH.WheelController3D
{
    /// <summary>
    ///     All info related to longitudinal force calculation.
    /// </summary>
    [Serializable]
    public class Friction
    {
        /// <summary>
        ///     Current force in friction direction.
        /// </summary>
        [Tooltip("    Current force in friction direction.")]
        public float force;

        /// <summary>
        ///     Modifies force value.
        /// </summary>
        [Tooltip("    Modifies force value.")]
        [Range(0.1f, 32f)]
        public float forceCoefficient = 1f;

        /// <summary>
        ///     Current slip in friction direction.
        /// </summary>
        [Tooltip("    Current slip in friction direction.")]
        public float slip;

        /// <summary>
        ///     Modifies slip value.
        /// </summary>
        [Tooltip("    Modifies slip value.")]
        [Range(0.1f, 16f)]
        public float slipCoefficient = 1;

        /// <summary>
        ///     Speed at the point of contact with the surface.
        /// </summary>
        [Tooltip("    Speed at the point of contact with the surface.")]
        public float speed;
    }
}