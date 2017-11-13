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
            Vector2[] positions = new Vector2[bodies.Length];
            float[] masses = new float[bodyCount];
            float[] orbitalSpeeds = new float[bodyCount];
            float[] orbitTilts = new float[bodyCount];
            float[] eccentricities = new float[bodyCount];
            float[] weights = new float[bodies.Length];
            float[] maxOrbitRadii = new float[bodies.Length];
            
            // Find masses and center of mass
            float totalWeights = 0;
            for (int i = 0; i < bodies.Length; i++)
            {
                Transform tsfm = Instantiate(planetPrefab, Vector3.zero, Quaternion.identity, bodiesFolder);
                bodies[i] = tsfm.GetComponentInChildren<Body>();

                masses[i] = Random.Range(minMass, maxMass);
                weights[i] = masses[i];
                totalWeights += weights[i];
                
                maxOrbitRadii[i] = Mathf.Min(worldSize.x, worldSize.y) * 0.5f - bodies[i].radius;
            }

            // Find positions
            bool apart = true;
            Vector2 barycenter = new Vector2(0, 0);
            do
            {
                // Find positions and center of mass
                centerOfMass = new Vector3(0, 0, 0);
                barycenter = new Vector2(0, 0);
                for (int i = 0; i < bodies.Length; i++)
                {
                    positions[i] = new Vector2(Random.insideUnitCircle.x * ((worldSize.x * 0.5f) - bodies[i].radius), Random.insideUnitCircle.y * ((worldSize.y * 0.5f) - bodies[i].radius));
                    
                    barycenter += positions[i];
                    centerOfMass += positions[i] * weights[i]; // Mass weight affects the barycenter position and increase the effect of mass on the barycenter position
                }
                barycenter /= bodies.Length;
                centerOfMass /= totalWeights;

                // Rotate orbit to match the apsides line
                for (int i = 0; i < bodies.Length; i++)
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

                // Check that planets are apart from each other
                apart = true;
                for (int i = 0; i < bodies.Length; i++)
                {
                    for (int j = i + 1; j < bodies.Length; j++)
                    {
                        if ((positions[i] - positions[j]).magnitude - (bodies[i].radius + bodies[j].radius) < minDistanceBetweenPlanetsSurfaces)
                        {
                            apart = false;
                        }
                    }
                }
            } while (!apart);
            centerOfMass -= barycenter; // Center barycenter to the non weighted one

            // Init bodies
            for (int i = 0; i < bodies.Length; i++)
            {
                positions[i] -= barycenter; // Center planets on the barycenter to have them on screen
                orbitalSpeeds[i] = orbitalSpeed;
                eccentricities[i] = Random.Range(0, maxEccentricity);
                centerOfMass = positions[i] - (positions[i] - centerOfMass).normalized * ((positions[i] - centerOfMass).magnitude / (1 + eccentricities[i]));

                bodies[i].init(positions[i], masses[i], orbitalSpeeds[i], eccentricities[i], orbitTilts[i], centerOfMass);
            }

            return bodies;
        }

        void Update()
        {

        }
    }
}