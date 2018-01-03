using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

namespace NaiveViableLooking2DPlanetarySystemGenerator
{
    public class GameManager : MonoBehaviour
    {
        private static GameManager _instance;
        public static GameManager instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = GameObject.FindObjectOfType<GameManager>();
                }
                return _instance;
            }
        }

        public Transform bodyPrefab;
        public string planetsFolderName = "Planets";

        private Transform planetsFolder;
        private NBodySystemGenerator generator;
        private Body[] bodies;
        private BodyData[] bodiesData;
        private Camera mainCam;
        private Canvas canvas;

        private int bodyCount;
        private bool generating;
        private bool simulating;
        public Vector2 worldSize { get; private set; }
        public float aspect { get; private set; }
        public float scale { get; private set; }
        public bool showOrbits { get; private set; }


        void Awake()
        {
            generator = GameObject.FindObjectOfType<NBodySystemGenerator>();
            mainCam = Camera.main;
            canvas = GameObject.FindObjectOfType<Canvas>();

            bodyCount = (int)canvas.GetComponentInChildren<Slider>().value;
            generating = false;
            simulating = false;

            Vector2[] worldBounds = new Vector2[2];
            worldBounds[0] = mainCam.ScreenToWorldPoint(new Vector2(0, 0));
            worldBounds[1] = mainCam.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
            worldSize = new Vector2(worldBounds[1].x - worldBounds[0].x, worldBounds[1].y - worldBounds[0].y);
            aspect = mainCam.aspect;
            scale = 0;
            showOrbits = true;
        }

        void Update()
        {
            if (simulating)
            {
                for (int i = 0; i < bodies.Length; i++)
                {
                    bodies[i].updateFixedOrbitPosition();
                }
            }
            else if (generating)
            {
                if (bodies != null)
                {
                    for (int i = 0; i < bodies.Length; i++)
                    {
                        if (bodies[i] != null)
                        {
                            Destroy(bodies[i].gameObject);
                        }
                    }
                }

                bodies = new Body[bodyCount];
                float[] bodiesRadii = new float[bodyCount];
                for (int i = 0; i < bodyCount; i++)
                {
                    bodies[i] = Instantiate(bodyPrefab, Vector3.zero, Quaternion.identity, planetsFolder).GetComponentInChildren<Body>();
                    bodiesRadii[i] = bodies[i].radius;
                }

                bodiesData = generator.generate(bodiesRadii, worldSize, bodyCount);
                for (int i = 0; i < bodyCount; i++)
                {
                    
                    bodies[i].init(bodiesData[i].position, bodiesData[i].mass, bodiesData[i].orbitalSpeed, bodiesData[i].eccentricity, bodiesData[i].orbitTilt, bodiesData[i].ellipseCenter);
                }
                generating = false;
            }

            if (Input.GetKeyDown(KeyCode.Return))
            {
                generate();
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                simulate();
            }

            if (Input.GetKey(KeyCode.UpArrow))
            {
                mainCam.orthographicSize -= 4f * Time.deltaTime;
                scale -= 4f * Time.deltaTime;
            }

            if (Input.GetKey(KeyCode.DownArrow))
            {
                mainCam.orthographicSize += 4f * Time.deltaTime;
                scale += 4f * Time.deltaTime;
            }

            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                canvas.gameObject.SetActive(!canvas.gameObject.activeSelf);
            }

            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                showOrbits = !showOrbits;
            }

            if (Input.GetKey(KeyCode.P))
            {
                if (Time.timeScale + 0.1f < 100)
                {
                    Time.timeScale += 0.1f;
                }
            }

            if (Input.GetKey(KeyCode.O))
            {
                if (Time.timeScale - 0.1f > 0)
                {
                    Time.timeScale -= 0.1f;
                }
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }
        }

        public void generate ()
        {
            generating = true;
            simulating = false;
        }

        public void simulate()
        {
            if (bodies != null)
            {
                simulating = !simulating;
            }
        }

        public void setBodyCount(float count)
        {
            bodyCount = (int)count;
        }
    }
}