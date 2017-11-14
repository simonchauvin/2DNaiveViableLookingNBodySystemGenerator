using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace NaiveViableLooking2DPlanetarySystemGenerator
{
    public class Body : MonoBehaviour
    {
        #region VARIABLES
        private float mass;
        private float orbitalSpeed;
        private float eccentricity;
        public float radius { get; protected set; }
        private Vector2 ellipseCenter;
        private Vector2[] foci;
        public float apsidesRadius { get; private set; } // Max distance possible from the ellipse center
        private float orbitTilt;
        private float angle;
        private Color gizmosColor;
        #endregion


        public virtual void Awake()
        {
            radius = GetComponent<SpriteRenderer>().bounds.extents.x;
        }

        public virtual void init(Vector2 position, float mass, float orbitalSpeed, float eccentricity, float orbitTilt, Vector2 ellipseCenter)
        {
            // Init
            transform.position = position;
            this.mass = mass;
            this.orbitalSpeed = orbitalSpeed;
            this.eccentricity = eccentricity;
            this.orbitTilt = orbitTilt;
            this.ellipseCenter = ellipseCenter;
            angle = 0;
            apsidesRadius = (ellipseCenter - position).magnitude;

            // Find relative foci points
            foci = new Vector2[] { Vector2.zero, Vector2.zero };
            foci[0] = Quaternion.Euler(0, 0, orbitTilt) * -new Vector2(eccentricity * apsidesRadius, 0);
            foci[1] = Quaternion.Euler(0, 0, orbitTilt) * new Vector2(eccentricity * apsidesRadius, 0);

            // UI
            this.gizmosColor = Random.ColorHSV();
        }

        public virtual void Update()
        {

        }

        public virtual void FixedUpdate()
        {

        }

        void OnDrawGizmos()
        {
            Gizmos.color = new Color(gizmosColor.r, gizmosColor.g, gizmosColor.b, 0.5f);

            // Draw point around which the body rotates
            Gizmos.DrawSphere(ellipseCenter, 0.25f);

            // Draw foci
            Gizmos.DrawSphere(ellipseCenter + foci[0], 0.1f);
            Gizmos.DrawSphere(ellipseCenter + foci[1], 0.1f);

            // Draw fixed orbit
            float lastAngle = 0;
            Vector2 prev = computeFixedOrbitPosition(lastAngle), next;
            for (int i = 0; i < 100; i++)
            {
                lastAngle += (Mathf.PI * 2f) / 100f;

                next = computeFixedOrbitPosition(lastAngle);

                Gizmos.DrawLine(prev, next);

                prev = next;
            }
        }

        public Vector2 computeFixedOrbitVelocity(float angle)
        {
            Vector2 velocity = new Vector2(Mathf.Cos(angle), (1f - eccentricity) * Mathf.Sin(angle));
            return velocity;
        }

        public Vector2 computeFixedOrbitPosition(float angle)
        {
            return (Vector3)ellipseCenter + apsidesRadius * (Quaternion.Euler(0, 0, orbitTilt) * computeFixedOrbitVelocity(angle));
        }

        public void updateFixedOrbitPosition()
        {
            // Update angle
            angle += orbitalSpeed * Time.deltaTime;
            if (angle > Mathf.PI * 2f)
            {
                angle = 0;
            }

            // Update position
            transform.position = computeFixedOrbitPosition(angle);
        }
    }
}