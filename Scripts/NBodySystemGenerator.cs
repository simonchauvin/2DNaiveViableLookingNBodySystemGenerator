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
        public float defaultOrbitalSpeed = 0.5f;
        public float minEccentricity = 0f;
        public float maxEccentricity = 0.75f;
        #endregion

        private bool verbose;


        void Awake()
        {
            
        }
        
        // Set mass of the body based on the position to the center of mass
        /// <summary>
        /// Generate the positions, masses and orbits of the bodies specified count.
        /// </summary>
        /// <param name="bodyCount">The total number of bodies wanted.</param>
        /// <param name="staticBodyCount">The number of bodies that should not move.</param>
        /// <param name="bodiesRadii">The radius of each body wanted.</param>
        /// <param name="worldSize">The full size of the world/screen.</param>
        /// <param name="verbose">Whether debug logs should be displayed.</param>
        /// <returns>An array of BodyData if the generation succeeded, null otherwise.</returns>
        public BodyData[] Generate(int bodyCount, StaticBodyCount staticBodyCount, float[] bodiesRadii, Vector2 worldSize, bool verbose)
        {
            bool hasConverged = true;
            BodyData[] bodies = new BodyData[bodyCount];
            Vector2 worldExtent = worldSize * 0.5f;
            this.verbose = verbose;

            string[] names = new string[bodyCount];

            Vector2[] positions = null;
            while (positions == null)
            {
                positions = GenerateBodiesPositions(bodyCount, worldExtent, bodiesRadii);
            }

            float[] masses = GenerateBodiesMasses(bodyCount);
            Vector2 centerOfMass = FindCenterOfMass(bodyCount, positions, masses);
            float[] eccentricities = new float[bodyCount];
            Vector2[] centers = new Vector2[bodyCount];
            float[] orbitalSpeeds = new float[bodyCount];
            
            for (int i = 0; i < bodyCount; i++)
            {
                names[i] = "Body" + i;
                orbitalSpeeds[i] = defaultOrbitalSpeed;

                // Find max semi major axis length
                Vector2 maxOppositePosition = new Vector3(centerOfMass.x, centerOfMass.y) + Quaternion.Euler(0, 0, 180) * (positions[i] - centerOfMass);
                if (maxOppositePosition.x < centerOfMass.x)
                {
                    maxOppositePosition.x -= bodiesRadii[i];
                }
                else
                {
                    maxOppositePosition.x += bodiesRadii[i];
                }
                float proportionOutside = GetMaxProportionOutsideScreen(maxOppositePosition, worldExtent);
                float maxSemiMajorAxis = (maxOppositePosition - centerOfMass).magnitude;
                if (proportionOutside > 0)
                {
                    maxSemiMajorAxis -= maxSemiMajorAxis * proportionOutside;
                }

                float actualMinEccentricity = ((centerOfMass - positions[i]).magnitude - maxSemiMajorAxis) / maxSemiMajorAxis; // minFocalDistance is the difference between the theoretical and the actual maxSemiMajorAxis
                if (actualMinEccentricity > maxEccentricity)
                {
                    if (verbose)
                    {
                        Debug.LogWarning("It is impossible for the current generated system configuration to find an eccentricity for " + names[i] + " that is below the max specified");

                        Debug.Log("semi major axis proportion outside: " + positions[i] + " -> " + proportionOutside);
                        Debug.Log("max semi major axis: " + maxSemiMajorAxis);
                    }

                    hasConverged = false;
                }
                else
                {
                    if (actualMinEccentricity < minEccentricity)
                    {
                        actualMinEccentricity = minEccentricity;
                    }

                    // Find the fitting eccentricity and check if the ellipse fits in the screen
                    Vector2 maxB1, maxB2;
                    float semiMajorAxis, semiMinorAxis, focalDistance, maxB1Outside, maxB2Outside, maxBOutside;
                    int safeWhileCount = 500;
                    bool notEccentricEnough, noSolutionPossible;
                    do
                    {
                        notEccentricEnough = false;
                        noSolutionPossible = false;

                        // Choose an eccentricity and find the relative parameters
                        eccentricities[i] = Random.Range(actualMinEccentricity, maxEccentricity);
                        semiMajorAxis = (centerOfMass - positions[i]).magnitude / (eccentricities[i] + 1f);
                        focalDistance = semiMajorAxis * eccentricities[i];
                        semiMinorAxis = Mathf.Sqrt(Mathf.Pow(semiMajorAxis, 2) - Mathf.Pow(focalDistance, 2));
                        centers[i] = positions[i] + (centerOfMass - positions[i]).normalized * semiMajorAxis;

                        // Compute vertical bounds
                        maxB1 = new Vector3(centers[i].x, centers[i].y) + Quaternion.Euler(0, 0, 90) * Vector2.right * semiMinorAxis;
                        maxB2 = new Vector3(centers[i].x, centers[i].y) + Quaternion.Euler(0, 0, -90) * Vector2.right * semiMinorAxis;
                        maxB1.y += bodiesRadii[i];
                        maxB2.y -= bodiesRadii[i];

                        // Check vertical bounds and reduce eccentricity if needed
                        maxB1Outside = GetMaxProportionOutsideScreen(maxB1, worldExtent);
                        maxB2Outside = GetMaxProportionOutsideScreen(maxB2, worldExtent);
                        maxBOutside = Mathf.Max(maxB1Outside, maxB2Outside);
                        if (maxBOutside > 0)
                        {
                            if (actualMinEccentricity < maxEccentricity)
                            {
                                actualMinEccentricity = eccentricities[i]; // Converge toward the value by resetting the min value
                                notEccentricEnough = true;
                            }
                            else
                            {
                                noSolutionPossible = true;
                            }
                        }

                        safeWhileCount--;
                    } while (notEccentricEnough && !noSolutionPossible && safeWhileCount > 0);

                    if (safeWhileCount <= 0 || noSolutionPossible)
                    {
                        if (verbose)
                        {
                            Debug.LogWarning("In the current generated system it is not possible to find an eccentricity for " + names[i] + " that is below the max eccentricity");

                            Debug.Log("maxB1outside: " + positions[i] + " -> " + maxB1Outside);
                            Debug.Log("maxB2outside: " + positions[i] + " -> " + maxB2Outside);

                            Debug.Log("eccentricity: " + eccentricities[i]);
                            Debug.Log("center: " + centers[i]);
                            Debug.Log("semiMajorAxis: " + semiMajorAxis);
                            Debug.Log("semiMinorAxis: " + semiMinorAxis);

                            Debug.Log("maxVerticalPos" + maxB1);
                            Debug.Log("maxVerticalPos" + maxB2);
                        }

                        hasConverged = false;
                    }
                }
            }

            // Set static bodies
            if (staticBodyCount != StaticBodyCount.NONE)
            {
                int closestBodyToCenterOfMassIndex = GetClosestBodyToCenterOfMassIndex(bodyCount, positions, centerOfMass, Mathf.Max(worldSize.x, worldSize.y));
                if (staticBodyCount == StaticBodyCount.ONE)
                {
                    positions[closestBodyToCenterOfMassIndex] = centers[closestBodyToCenterOfMassIndex];
                    orbitalSpeeds[closestBodyToCenterOfMassIndex] = 0;
                }
                else
                {
                    for (int i = 0; i < bodyCount; i++)
                    {
                        orbitalSpeeds[i] = 0;
                    }
                }
            }

            if (hasConverged)
            {
                for (int i = 0; i < bodyCount; i++)
                {
                    bodies[i].name = names[i];
                    bodies[i].position = positions[i];
                    bodies[i].mass = masses[i];
                    bodies[i].orbitalSpeed = orbitalSpeeds[i];

                    // Rotate orbit to match the apsides line
                    if (positions[i].y < centerOfMass.y)
                    {
                        bodies[i].orbitTilt = -Vector2.Angle(Vector2.right, (positions[i] - centerOfMass).normalized);
                    }
                    else
                    {
                        bodies[i].orbitTilt = Vector2.Angle(Vector2.right, (positions[i] - centerOfMass).normalized);
                    }

                    bodies[i].eccentricity = eccentricities[i];
                    bodies[i].ellipseCenter = centers[i];
                    bodies[i].centerOfMass = centerOfMass;
                }
                return bodies;
            }
            else
            {
                return null;
            }
        }

        private Vector2[] GenerateBodiesPositions(int bodyCount, Vector2 worldExtent, float[] bodiesRadii)
        {
            bool hasConverged = true;
            Vector2[] positions = new Vector2[bodyCount];

            bool apart = true;
            Vector2 barycenter = Vector2.zero;
            Vector2 currentBarycenter = barycenter;
            for (int i = 0; i < bodyCount; i++)
            {
                int safeWhileCount = 1000;
                do
                {
                    apart = true;
                    positions[i] = currentBarycenter + new Vector2(Random.Range(-worldExtent.x + bodiesRadii[i], worldExtent.x - bodiesRadii[i]), Random.Range(-worldExtent.y + bodiesRadii[i], worldExtent.y - bodiesRadii[i]));
                    for (int j = 0; j < i; j++)
                    {
                        if ((positions[i] - positions[j]).magnitude - (bodiesRadii[i] + bodiesRadii[j]) < minDistanceBetweenPlanetsSurfaces)
                        {
                            apart = false;
                        }
                    }
                    safeWhileCount--;
                } while (!apart && safeWhileCount > 0);
                if (safeWhileCount <= 0)
                {
                    if (verbose)
                    {
                        Debug.LogWarning("The generation of bodies positions failed.");
                    }

                    hasConverged = false;
                }

                barycenter += positions[i];
                currentBarycenter = barycenter / (i + 1);
            }
            barycenter /= bodyCount;
            
            if (hasConverged)
            {
                // Shift bodies positions so that barycenter is at 0,0
                for (int i = 0; i < bodyCount; i++)
                {
                    positions[i] -= barycenter;
                }
                barycenter = Vector2.zero;

                return positions;
            }
            else
            {
                return null;
            }
        }

        private float[] GenerateBodiesMasses(int bodyCount)
        {
            float[] masses = new float[bodyCount];
            for (int i = 0; i < bodyCount; i++)
            {
                masses[i] = Random.Range(minMass, maxMass);
            }
            return masses;
        }

        private Vector2 FindCenterOfMass(int bodyCount, Vector2[] positions, float[] masses)
        {
            float totalMass = 0;
            Vector2 centerOfMass = Vector3.zero;
            for (int i = 0; i < bodyCount; i++)
            {
                centerOfMass += positions[i] * masses[i];
                totalMass += masses[i];
            }
            centerOfMass /= totalMass;
            return centerOfMass;
        }

        private float GetMaxProportionOutsideScreen (Vector2 point, Vector2 worldExtent)
        {
            float xValue = 0, yValue = 0;
            if (point.x > 0)
            {
                if (point.x > worldExtent.x)
                {
                    xValue = 1f - (worldExtent.x / point.x);
                }
            }
            else
            {
                if (point.x < -worldExtent.x)
                {
                    xValue = 1f - (worldExtent.x / -point.x);
                }
            }
            if (point.y > 0)
            {
                if (point.y > worldExtent.y)
                {
                    yValue = 1f - (worldExtent.y / point.y);
                }
            }
            else
            {
                if (point.y < -worldExtent.y)
                {
                    yValue = 1f - (worldExtent.y / -point.y);
                }
            }

            return Mathf.Max(xValue, yValue);
        }

        private int GetClosestBodyToCenterOfMassIndex(int bodyCount, Vector2[] positions, Vector2 centerOfMass, float maxDistance)
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