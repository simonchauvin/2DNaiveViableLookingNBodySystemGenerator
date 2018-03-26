using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace NaiveViableLooking2DPlanetarySystemGenerator
{
    public class Body : MonoBehaviour
    {
        #region VARIABLES
        private Vector2 worldSize;
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
        private Material gizmosMat;
        #endregion


        public virtual void Awake()
        {
            radius = GetComponent<CircleCollider2D>().radius;
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
            worldSize = GameManager.instance.worldSize;

            // Find relative foci points
            foci = new Vector2[] { Vector2.zero, Vector2.zero };
            foci[0] = Quaternion.Euler(0, 0, orbitTilt) * -new Vector2(eccentricity * apsidesRadius, 0);
            foci[1] = Quaternion.Euler(0, 0, orbitTilt) * new Vector2(eccentricity * apsidesRadius, 0);

            // UI
            this.gizmosColor = Random.ColorHSV();

            // Unity has a built-in shader that is useful for drawing
            // simple colored things.
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            gizmosMat = new Material(shader);
            gizmosMat.hideFlags = HideFlags.HideAndDontSave;
            // Turn on alpha blending
            gizmosMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            gizmosMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            // Turn backface culling off
            gizmosMat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            // Turn off depth writes
            gizmosMat.SetInt("_ZWrite", 0);
        }

        public virtual void Update()
        {
            
        }

        public virtual void FixedUpdate()
        {

        }

        // Render UI when in-game
        void OnRenderObject ()
        {
            if (!Application.isEditor && GameManager.instance.showOrbits)
            {
                GL.PushMatrix();
                gizmosMat.SetPass(0);
                GL.LoadOrtho();
                GL.Begin(GL.LINES);
                GL.Color(new Color(gizmosColor.r, gizmosColor.g, gizmosColor.b, 0.5f));

                // Take zoom into account
                Vector2 scaledWorldSize = new Vector2(worldSize.x + (GameManager.instance.scale * GameManager.instance.aspect) * 2f, worldSize.y + (GameManager.instance.scale * 2f));

                // Draw orbit
                float lastAngle = 0;
                Vector2 prev = computeFixedOrbitPosition(lastAngle), next;
                for (int i = 0; i < 100; i++)
                {
                    lastAngle += (Mathf.PI * 2f) / 100f;

                    next = computeFixedOrbitPosition(lastAngle);

                    GL.Vertex(new Vector3((prev.x + scaledWorldSize.x * 0.5f) / scaledWorldSize.x, (prev.y + scaledWorldSize.y * 0.5f) / scaledWorldSize.y));
                    GL.Vertex(new Vector3((next.x + scaledWorldSize.x * 0.5f) / scaledWorldSize.x, (next.y + scaledWorldSize.y * 0.5f) / scaledWorldSize.y));

                    prev = next;
                }

                GL.End();
                GL.PopMatrix();
            }
        }

        // Render UI when in the editor
        void OnDrawGizmos()
        {
            if (Application.isEditor && GameManager.instance.showOrbits)
            {
                Gizmos.color = new Color(gizmosColor.r, gizmosColor.g, gizmosColor.b, 0.5f);

                // Draw point around which the body rotates
                Gizmos.DrawSphere(ellipseCenter, 0.25f);

                // Draw foci
                Gizmos.DrawSphere(ellipseCenter + foci[0], 0.1f);
                Gizmos.DrawSphere(ellipseCenter + foci[1], 0.1f);

                // Draw orbit
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
        }

        public Vector2 computeFixedOrbitVelocity(float angle)
        {
            return new Vector2(Mathf.Cos(angle), (1f - eccentricity) * Mathf.Sin(angle));
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