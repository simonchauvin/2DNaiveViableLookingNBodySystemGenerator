using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NaiveViableLooking2DPlanetarySystemGenerator
{
    /// <summary>
    /// Data structure output by the generator for each body generated.
    /// </summary>
    public class BodyData
    {
        public Vector2 position { get; private set; }
        public float mass { get; private set; }
        public float orbitalSpeed { get; private set; }
        public float eccentricity { get; private set; }
        public float orbitTilt { get; private set; }
        public Vector2 ellipseCenter { get; private set; }


        public BodyData(Vector2 position, float mass, float orbitalSpeed, float eccentricity, float orbitTilt, Vector2 ellipseCenter)
        {
            this.position = position;
            this.mass = mass;
            this.orbitalSpeed = orbitalSpeed;
            this.eccentricity = eccentricity;
            this.orbitTilt = orbitTilt;
            this.ellipseCenter = ellipseCenter;
        }
    }
}