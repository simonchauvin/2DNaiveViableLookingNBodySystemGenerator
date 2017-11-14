using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace NaiveViableLooking2DPlanetarySystemGenerator
{
    public class NBodySystemGenerator : MonoBehaviour
    {
        #region GENERATION_SETTINGS
        public Transform planetPrefab;
        [Range(1, 8)]
        public int bodyCount = 2;
        public float minMass = 2;
        public float maxMass = 40;
        public float minDistanceBetweenPlanetsSurfaces = 3;
        public float orbitalSpeed = 1f;
        public float maxEccentricity = 0.75f;
        #endregion

        #region GENERAL_SETTINGS
        public string planetarySystemFolderName = "PlanetarySystem";
        public string bodiesFolderName = "Bodies";
        #endregion

        #region OBJECTS
        private Camera mainCam;
        private Transform planetarySystemFolder;
        private Transform bodiesFolder;
        #endregion

        #region VARIABLES
        private Vector2 worldSize;
        private Vector2[] worldBounds;
        private Vector2 centerOfMass;
        #endregion


        void Awake()
        {
            mainCam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
            planetarySystemFolder = GameObject.Find(planetarySystemFolderName).transform;
            bodiesFolder = GameObject.Find(bodiesFolderName).transform;

            worldBounds = new Vector2[2];
            worldBounds[0] = mainCam.ScreenToWorldPoint(new Vector2(0, 0));
            worldBounds[1] = mainCam.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
            worldSize = new Vector2(worldBounds[1].x - worldBounds[0].x, worldBounds[1].y - worldBounds[0].y);
        }

        public Body[] generate()
        {
            // Init arrays
            Body[] bodies = new Body[bodyCount];
            Vector2[] positions = new Vector2[bodyCount];
            float[] masses = new float[bodyCount];
            float[] orbitTilts = new float[bodyCount];
            float[] weights = new float[bodies.Length];
            float greatestMass = 0;
            int greatestMassBodyIndex = 0;
            
            // Find masses and center of mass
            float totalWeights = 0;
            for (int i = 0; i < bodyCount; i++)
            {
                Transform tsfm = Instantiate(planetPrefab, Vector3.zero, Quaternion.identity, bodiesFolder);
                bodies[i] = tsfm.GetComponentInChildren<Body>();

                masses[i] = Random.Range(minMass, maxMass);
                weights[i] = masses[i];
                totalWeights += weights[i];

                if (masses[i] > greatestMass)
                {
                    greatestMass = masses[i];
                    greatestMassBodyIndex = i;
                }
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
                    positions[i] = new Vector2(Random.insideUnitCircle.x * ((worldSize.x * 0.5f) - bodies[i].radius), Random.insideUnitCircle.y * ((worldSize.y * 0.5f) - bodies[i].radius));
                    
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
                        if ((positions[i] - positions[j]).magnitude - (bodies[i].radius + bodies[j].radius) < minDistanceBetweenPlanetsSurfaces)
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
                if (i != greatestMassBodyIndex)
                {
                    float semiMajorAxis = apsidesLine.magnitude,
                        minPeriapsis = bodies[i].radius + bodies[greatestMassBodyIndex].radius,
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
                bodies[i].init(positions[i], masses[i], orbitalSpeed, eccentricity, orbitTilts[i], center);
            }

            return bodies;
        }

        void Update()
        {

        }
    }
}