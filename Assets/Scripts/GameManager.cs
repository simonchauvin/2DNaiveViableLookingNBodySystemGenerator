using System.Collections;
using System.Collections.Generic;
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

        private NBodySystemGenerator generator;
        private Body[] planets;

        private bool generating;
        private bool simulating;


        void Awake()
        {
            generator = GameObject.FindObjectOfType<NBodySystemGenerator>();

            generating = false;
            simulating = false;
        }

        void Update()
        {
            if (simulating)
            {
                for (int i = 0; i < planets.Length; i++)
                {
                    planets[i].updateFixedOrbitPosition();
                }
            }
            else if (generating)
            {
                if (planets != null)
                {
                    for (int i = 0; i < planets.Length; i++)
                    {
                        if (planets[i] != null)
                        {
                            Destroy(planets[i].gameObject);
                        }
                    }
                }

                planets = generator.generate();
                generating = false;
            }
        }

        public void generate ()
        {
            generating = true;
            simulating = false;
        }

        public void simulate()
        {
            if (planets != null)
            {
                simulating = true;
            }
        }
    }
}