using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NaiveViableLooking2DPlanetarySystemGenerator
{
    /// <summary>
    /// Data structure output by the generator for each body generated.
    /// </summary>
    public struct BodyData
    {
        public string name;
        public Vector2 position;
        public float mass;
        public float orbitalSpeed;
        public float eccentricity;
        public float orbitTilt;
        public Vector2 ellipseCenter;
        public Vector2 centerOfMass;
    }
}