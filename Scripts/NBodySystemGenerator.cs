using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace NaiveViableLooking2DPlanetarySystemGenerator
{
    public class NBodySystemGenerator : MonoBehaviour
    {
        #region GENERATION_SETTINGS
        public float minMass = 2;
        public float maxMass = 40;
        public float minDistanceBetweenPlanetsSurfaces = 3;
        public float orbitalSpeed = 0.5f;
        public float maxEccentricity = 0.75f;
        #endregion

        #region VARIABLES
        public Vector2 centerOfMass { get; private set; }
        #endregion


        void Awake()
        {
            
        }

        public BodyData[] generate(float[] bodiesRadii, Vector2 worldSize, int bodyCount)
        {
            // Init arrays
            BodyData[] bodies = new BodyData[bodyCount];
            Vector2[] positions = new Vector2[bodyCount];
            float[] masses = new float[bodyCount];
            float[] orbitTilts = new float[bodyCount];
            float[] weights = new float[bodyCount];
            float closestDistanceToCenterOfMass = Mathf.Max(worldSize.x, worldSize.y);
            int closestBodyToCenterOfMassIndex = 0;
            
            // Find masses and center of mass
            float totalWeights = 0;
            for (int i = 0; i < bodyCount; i++)
            {
                masses[i] = Random.Range(minMass, maxMass);
                weights[i] = masses[i];
                totalWeights += weights[i];
            }

            // Find valid positions
            bool apart = true;
            Vector2 barycenter = new Vector2(0, 0); // Different than the center of mass because it does not account for the bodies masses
            do
            {
                // Randomize positions and compute center of mass
                centerOfMass = new Vector3(0, 0, 0);
                barycenter = new Vector2(0, 0);
                for (int i = 0; i < bodyCount; i++)
                {
                    positions[i] = new Vector2(Random.insideUnitCircle.x * ((worldSize.x * 0.5f) - bodiesRadii[i]), Random.insideUnitCircle.y * ((worldSize.y * 0.5f) - bodiesRadii[i]));
                    
                    barycenter += positions[i]; // Non weighted barycenter
                    centerOfMass += positions[i] * weights[i]; // Mass weight affects the center of mass
                }
                barycenter /= bodyCount;
                centerOfMass /= totalWeights;

                // Check that planets are not too close to each other
                apart = true;
                for (int i = 0; i < bodyCount; i++)
                {
                    for (int j = i + 1; j < bodyCount; j++)
                    {
                        if ((positions[i] - positions[j]).magnitude - (bodiesRadii[i] + bodiesRadii[i]) < minDistanceBetweenPlanetsSurfaces)
                        {
                            apart = false;
                        }
                    }
                }
            } while (!apart);

            // Rotate orbit to match the apsides line
            for (int i = 0; i < bodyCount; i++)
            {
                if (positions[i].y < centerOfMass.y)
                {
                    orbitTilts[i] = -Vector2.Angle(Vector2.right, (positions[i] - centerOfMass).normalized);
                }
                else
                {
                    orbitTilts[i] = Vector2.Angle(Vector2.right, (positions[i] - centerOfMass).normalized);
                }
            }

            // Find closest body to center of mass
            for (int i = 0; i < bodyCount; i++)
            {
                if ((positions[i] - centerOfMass).magnitude < closestDistanceToCenterOfMass)
                {
                    closestDistanceToCenterOfMass = (positions[i] - centerOfMass).magnitude;
                    closestBodyToCenterOfMassIndex = i;
                }
            }

            // Shift the center of mass to center the planets in the middle of the screen
            centerOfMass -= barycenter;

            // Final configurations
            Vector2 apsidesLine = Vector2.zero, center = Vector2.zero;
            float eccentricity = 0, newMaxEccentricity = 0;
            for (int i = 0; i < bodyCount; i++)
            {
                // Center planets on the screen
                positions[i] -= barycenter;

                apsidesLine = positions[i] - centerOfMass;

                // Cap eccentricity to prevent bodies from crashing into each other
                newMaxEccentricity = maxEccentricity;
                if (i != closestBodyToCenterOfMassIndex)
                {
                    float semiMajorAxis = apsidesLine.magnitude,
                        minPeriapsis = bodiesRadii[i] + bodiesRadii[closestBodyToCenterOfMassIndex],
                        max = 1f - (1f - (minPeriapsis / semiMajorAxis));
                    if (max < maxEccentricity)
                    {
                        newMaxEccentricity = max;
                    }
                }
                eccentricity = Random.Range(0, newMaxEccentricity);

                // Shift the ellipse center so that the foci are shared at the center of mass of the system
                center = positions[i] - apsidesLine.normalized * (apsidesLine.magnitude / (1 + eccentricity));

                // Init body
                bodies[i] = new BodyData(positions[i], masses[i], orbitalSpeed, eccentricity, orbitTilts[i], center);
            }

            return bodies;
        }

        void Update()
        {

        }
    }
}