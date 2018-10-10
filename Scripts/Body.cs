using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace NaiveViableLooking2DPlanetarySystemGenerator
{
    public class Body : MonoBehaviour
    {
        #region PROPERTIES
        private float mass;
        private float orbitalSpeed;
        private float orbitTilt;
        private float eccentricity;
        private Vector2 ellipseCenter;
        public Vector2 centerOfMass;
        #endregion

        #region VARIABLES
        private Vector2 worldSize;
        public float radius { get; protected set; }
        private Vector2[] foci;
        private Vector2 apsidesLine;
        public float semiMajorAxis { get; private set; } // Max distance possible from the ellipse center
        private float semiMinorAxis;
        private float angle;
        private Color gizmosColor;
        private Material gizmosMat;
        #endregion


        public virtual void Awake()
        {
            radius = GetComponent<CircleCollider2D>().radius;
        }

        public virtual void Init(BodyData bodyData)
        {
            // Init
            gameObject.name = bodyData.name;
            Vector2 position = bodyData.position;
            transform.position = position;
            mass = bodyData.mass;
            orbitalSpeed = bodyData.orbitalSpeed;
            eccentricity = bodyData.eccentricity;
            orbitTilt = bodyData.orbitTilt;
            ellipseCenter = bodyData.ellipseCenter;
            centerOfMass = bodyData.centerOfMass;
            angle = 0;
            semiMajorAxis = (ellipseCenter - position).magnitude;
            worldSize = GameManager.instance.worldSize;

            // Find relative foci points
            foci = new Vector2[] { Vector2.zero, Vector2.zero };
            foci[0] = ellipseCenter - (position - ellipseCenter).normalized * (eccentricity * semiMajorAxis);
            foci[1] = ellipseCenter + (position - ellipseCenter).normalized * (eccentricity * semiMajorAxis);

            apsidesLine = (ellipseCenter - position).normalized;
            semiMinorAxis = Mathf.Sqrt(Mathf.Pow(semiMajorAxis, 2) - Mathf.Pow((ellipseCenter - foci[0]).magnitude, 2));

            // UI
            this.gizmosColor = Random.ColorHSV(0, 1, 0, 1, 0.5f, 1);

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
            if (!Application.isEditor)
            {
                GL.PushMatrix();
                gizmosMat.SetPass(0);
                GL.LoadOrtho();
                GL.Begin(GL.LINES);
                GL.Color(new Color(gizmosColor.r, gizmosColor.g, gizmosColor.b, 0.5f));

                if (GameManager.instance.showOrbits)
                {
                    // Take zoom into account
                    Vector2 scaledWorldSize = new Vector2(worldSize.x + (GameManager.instance.scale * GameManager.instance.aspect) * 2f, worldSize.y + (GameManager.instance.scale * 2f));

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
                }

                GL.End();
                GL.PopMatrix();
            }
        }

        // Render UI when in the editor
        void OnDrawGizmos()
        {
            if (Application.isEditor)
            {
                Gizmos.color = new Color(gizmosColor.r, gizmosColor.g, gizmosColor.b, 0.5f);

                // Draw debug
                if (GameManager.instance.showDebug)
                {
                    // Draw point around which the body rotates
                    Gizmos.DrawSphere(ellipseCenter, 0.25f);

                    // Draw foci
                    Gizmos.DrawSphere(foci[0], 0.1f);
                    Gizmos.DrawSphere(foci[1], 0.1f);

                    Gizmos.DrawLine(ellipseCenter, ellipseCenter + semiMajorAxis * apsidesLine);
                    Gizmos.DrawLine(ellipseCenter, (Vector3)ellipseCenter + semiMajorAxis * (Quaternion.Euler(0, 0, 180) * apsidesLine));
                    Gizmos.DrawLine(ellipseCenter, (Vector3)ellipseCenter + semiMinorAxis * (Quaternion.Euler(0, 0, 90) * apsidesLine));
                    Gizmos.DrawLine(ellipseCenter, (Vector3)ellipseCenter + semiMinorAxis * (Quaternion.Euler(0, 0, -90) * apsidesLine));
                }

                if (GameManager.instance.showOrbits)
                {
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
        }

        public Vector2 computeFixedOrbitPosition(float angle)
        {
            return (Vector3)ellipseCenter + (Quaternion.Euler(0, 0, orbitTilt) * new Vector2(semiMajorAxis * Mathf.Cos(angle), semiMinorAxis * Mathf.Sin(angle)));
        }

        public void updateFixedOrbitPosition()
        {
            angle += orbitalSpeed * Time.deltaTime; // Update angle
            if (angle > Mathf.PI * 2f)
            {
                angle = 0;
            }
            transform.position = computeFixedOrbitPosition(angle); // Update position
        }
    }
}