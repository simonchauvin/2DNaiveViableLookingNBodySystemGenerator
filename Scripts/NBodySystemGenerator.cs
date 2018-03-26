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
        public Vector2 barycenter { get; private set; }
        public Vector2 centerOfMass { get; private set; }
        #endregion


        void Awake()
        {
            
        }

        public BodyData[] generate(float[] bodiesRadii, bool oneIsStatic, Vector2 worldSize, int bodyCount)
        {
            // Init
            BodyData[] bodies = new BodyData[bodyCount];
            Vector2[] positions = new Vector2[bodyCount];
            float[] eccentricities = new float[bodyCount];
            Vector2[] centers = new Vector2[bodyCount];
            float[] masses = new float[bodyCount];
            float[] orbitTilts = new float[bodyCount];
            float[] minEccentricities = new float[bodyCount];
            float[] maxEccentricities = new float[bodyCount];
            Vector2 distToBounds = worldSize * 0.5f;
            int closestBodyToCenterOfMassIndex = 0;

            // Generate a mass for each body
            for (int i = 0; i < bodyCount; i++)
            {
                masses[i] = Random.Range(minMass, maxMass);
            }

            // Find valid positions
            Vector2 currentCenterOfMass = Vector2.zero, 
                maxRadius = distToBounds, 
                barycenterToCenterOfMass;
            float maxRadiusApsidesRatio = 0,
                minEccentricity = 0,
                newMaxEccentricity = maxEccentricity,
                totalMass = 0;
            bool apart = true,
                validEccentricity = true;
            do
            {
                // Randomize positions and compute center of mass
                barycenter = new Vector2(0, 0);
                centerOfMass = new Vector3(0, 0, 0);
                totalMass = 0;
                for (int i = 0; i < bodyCount; i++)
                {
                    do
                    {
                        positions[i] = currentCenterOfMass + new Vector2(Random.Range(-distToBounds.x + bodiesRadii[i], distToBounds.x - bodiesRadii[i]), Random.Range(-distToBounds.y + bodiesRadii[i], distToBounds.y - bodiesRadii[i]));

                        currentCenterOfMass = (centerOfMass + positions[i] * masses[i]) / (totalMass + masses[i]);
                        barycenterToCenterOfMass = (currentCenterOfMass - ((barycenter + positions[i]) / (i + 1)));

                        closestBodyToCenterOfMassIndex = getClosestBodyToCenterOfMassIndex(positions, i + 1, currentCenterOfMass, Mathf.Max(worldSize.x, worldSize.y));

                        // Generate a valid eccentricity based on the new center of mass created by the new body
                        // TODO Fix Go slighlty outside when n > 3
                        validEccentricity = true;
                        for (int j = 0; j <= i; j++)
                        {
                            // TODO Should check orbitTilt of bodies compared to their eccentricity (similar orbittilts coupled with high eccentricity is likely to collide)
                            // Cap eccentricity to prevent bodies from crashing into each other
                            newMaxEccentricity = maxEccentricity;
                            if (j != closestBodyToCenterOfMassIndex)
                            {
                                float semiMajorAxis = (positions[j] - currentCenterOfMass).magnitude,
                                    minPeriapsis = bodiesRadii[j] + bodiesRadii[closestBodyToCenterOfMassIndex],
                                    max = 1f - (1f - (minPeriapsis / semiMajorAxis));
                                if (max < maxEccentricity)
                                {
                                    newMaxEccentricity = max;
                                }
                            }
                            maxEccentricities[j] = newMaxEccentricity;

                            maxRadius = new Vector2(distToBounds.x - bodiesRadii[j] - Mathf.Abs(barycenterToCenterOfMass.x), distToBounds.y - bodiesRadii[j] - Mathf.Abs(barycenterToCenterOfMass.y));
                            if (Vector2.Distance(positions[j], currentCenterOfMass) > 0)
                            {
                                maxRadiusApsidesRatio = (Mathf.Min(maxRadius.x, maxRadius.y)) / (positions[j] - currentCenterOfMass).magnitude;
                                if  (maxRadiusApsidesRatio < 1f - newMaxEccentricity) // Too eccentric
                                {
                                    validEccentricity = false;
                                }
                                else
                                {
                                    minEccentricity = 1f - Mathf.Clamp01(maxRadiusApsidesRatio);
                                }
                            }
                            else
                            {
                                minEccentricity = 0;
                            }
                            minEccentricities[j] = minEccentricity;
                        }
                    } while (!validEccentricity);

                    barycenter += positions[i]; // Non weighted barycenter
                    centerOfMass += positions[i] * masses[i]; // Mass weight affects the center of mass
                    totalMass += masses[i];
                }
                barycenter /= bodyCount;
                centerOfMass /= totalMass;

                // Check that planets are not too close to each other
                apart = true;
                for (int i = 0; i < bodyCount; i++)
                {
                    for (int j = i + 1; j < bodyCount; j++)
                    {
                        if ((positions[i] - positions[j]).magnitude - (bodiesRadii[i] + bodiesRadii[j]) < minDistanceBetweenPlanetsSurfaces)
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

            closestBodyToCenterOfMassIndex = getClosestBodyToCenterOfMassIndex(positions, bodyCount, centerOfMass, Mathf.Max(worldSize.x, worldSize.y));

            centerOfMass -= barycenter; // Shift the center of mass to account for the fact that the barycenter will be on 0,0,0

            // Final configurations
            Vector2 apsidesLine = Vector2.zero;
            for (int i = 0; i < bodyCount; i++)
            {
                positions[i] -= barycenter; // Center planets on the screen

                apsidesLine = positions[i] - centerOfMass;

                eccentricities[i] = Random.Range(minEccentricities[i], maxEccentricities[i]);

                // Shift the ellipse center so that the foci are shared at the center of mass of the system
                centers[i] = positions[i] - apsidesLine.normalized * (apsidesLine.magnitude / (1 + eccentricities[i]));
            }

            // If a static body exists it is the closest to the center of mass
            for (int i = 0; i < bodyCount; i++)
            {
                if (oneIsStatic)
                {
                    if (i == closestBodyToCenterOfMassIndex)
                    {
                        positions[i] = centers[i];
                    }
                }

                // Init body
                bodies[i] = new BodyData(positions[i], masses[i], orbitalSpeed, eccentricities[i], orbitTilts[i], centers[i]);
            }

            barycenter = Vector3.zero;

            return bodies;
        }

        private int getClosestBodyToCenterOfMassIndex(Vector2[] positions, int bodyCount, Vector2 centerOfMass, float maxDistance)
        {
            int index = 0;
            float closestDistanceToCenterOfMass = maxDistance;
            for (int i = 0; i < bodyCount; i++)
            {
                if ((positions[i] - centerOfMass).magnitude < closestDistanceToCenterOfMass)
                {
                    closestDistanceToCenterOfMass = (positions[i] - centerOfMass).magnitude;
                    index = i;
                }
            }
            return index;
        }

        void Update()
        {

        }
    }
}