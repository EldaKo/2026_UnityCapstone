using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS.ModularFirearms
{
    [HelpURL("https://docs.neofps.com/manual/weaponsref-mb-physicsbulletcasing.html")]
	[RequireComponent (typeof (Rigidbody))]
	[RequireComponent (typeof (MeshFilter))]
	[RequireComponent (typeof (PooledObject))]
	public class PhysicsBulletCasing : MonoBehaviour, IBulletCasing
	{
		[SerializeField, RequiredObjectProperty, Tooltip("The detail mesh to show while the bullet is in the first person view.")]
		private Mesh m_DetailMesh = null;

        [SerializeField, RequiredObjectProperty, Tooltip("The low poly mesh to switch to when not in the first person view.")]
		private Mesh m_LowPolyMesh = null;

        [SerializeField, Tooltip("How long should the casing remain before being returned to the pool.")]
        private float m_Lifespan = 30f;

		private Rigidbody m_RigidBody = null;
        private MeshFilter m_MeshFilter = null;
        private MeshRenderer m_MeshRenderer = null;
        private PooledObject m_PooledObject = null;
        private IEnumerator m_Coroutine = null;
        private WaitForFixedUpdate m_Delay = null;
        private Vector3 m_Position = Vector3.zero;
        private Vector3 m_Velocity = Vector3.zero;
        private Vector3 m_Angular = Vector3.zero;
        private float m_Timer = 0f;
        private float m_Scale = 1f;
        bool visible = true;

#if UNITY_EDITOR
        protected void OnValidate()
        {
            if (m_Lifespan < 10f)
                m_Lifespan = 10f;
        }
#endif

        protected void Awake ()
		{
			m_RigidBody = GetComponent<Rigidbody> ();
			m_MeshFilter = GetComponent<MeshFilter> ();
            m_MeshRenderer = GetComponent<MeshRenderer> ();
            m_PooledObject = GetComponent<PooledObject> ();

            if (gameObject.layer != PhysicsFilter.LayerIndex.Effects)
            {
                Debug.LogWarning("Changing layer on object to Effects: " + name);
                gameObject.layer = PhysicsFilter.LayerIndex.Effects;
            }
		}

        protected void OnDisable()
        {
			m_Timer = 0f;
            // Set to animated
            m_MeshFilter.mesh = m_DetailMesh;
            m_RigidBody.collisionDetectionMode = CollisionDetectionMode.Discrete;
            m_RigidBody.interpolation = RigidbodyInterpolation.Interpolate;
            //m_RigidBody.isKinematic = true;
            m_RigidBody.detectCollisions = false;
            m_Coroutine = null;
        }

        public void Eject (Vector3 velocity, Vector3 angular, float scale, bool player)
		{
            m_Velocity = velocity;
            m_Angular = angular;
            m_Scale = scale;
            m_Timer = 0f;

            // Stop existing lifespan coroutine (in case pool empty, so oldest living grabbed)
            if (m_Coroutine != null)
				StopCoroutine (m_Coroutine);

            if (player)
            {
                // Set to animated
                m_MeshFilter.mesh = m_DetailMesh;
#if UNITY_2021_2_OR_NEWER
                m_MeshRenderer.ResetLocalBounds();
#endif
                m_RigidBody.collisionDetectionMode = CollisionDetectionMode.Discrete;
                m_RigidBody.interpolation = RigidbodyInterpolation.Interpolate;
                m_RigidBody.isKinematic = true;
                m_RigidBody.detectCollisions = false;

                m_Position = m_RigidBody.position + velocity * Time.fixedDeltaTime;
                m_RigidBody.MovePosition(m_Position);

                if (m_Delay == null)
                    m_Delay = new WaitForFixedUpdate();

                // Start animated coroutine
                m_Coroutine = AnimatedCoroutine();
                StartCoroutine(m_Coroutine);

                visible = true;
            }
            else
            {
                // Set to physics
                m_MeshFilter.mesh = m_LowPolyMesh;
#if UNITY_2021_2_OR_NEWER
                m_MeshRenderer.ResetLocalBounds();
#endif
                m_RigidBody.isKinematic = false;
                m_RigidBody.detectCollisions = true;
                m_RigidBody.collisionDetectionMode = CollisionDetectionMode.Continuous;
                m_RigidBody.interpolation = RigidbodyInterpolation.None;
                m_RigidBody.angularVelocity = m_Angular;
#if UNITY_6000_0_OR_NEWER
				m_RigidBody.linearVelocity = velocity;
#else
				m_RigidBody.velocity = velocity;
#endif

                visible = false;
            }
		}

        IEnumerator AnimatedCoroutine ()
        {
            while (visible && m_Timer < 2f)
            {
                yield return m_Delay;

                if (m_Timer > 0.25f && Physics.Linecast(m_Position, m_Position + m_Velocity * Time.deltaTime, PhysicsFilter.Masks.BulletBlockers))
                {
                    visible = false;

                    float y = m_Velocity.y;
                    m_Velocity *= 0.5f;
                    m_Velocity.y = y;

                    break;
                }
                else
                {
                    // Move the object
                    m_Position += m_Velocity * Time.deltaTime;
                    m_Velocity += Physics.gravity * Time.deltaTime;
                    m_RigidBody.MovePosition(m_Position);

                    // Rotate the objet
                    m_RigidBody.MoveRotation(Quaternion.Euler(m_Angular * Time.deltaTime) * m_RigidBody.rotation);
                }
            }

            // Set to physics
            transform.localScale = Vector3.one * m_Scale;
            m_MeshFilter.mesh = m_LowPolyMesh;
#if UNITY_2021_2_OR_NEWER
            m_MeshRenderer.ResetLocalBounds();
#endif
            m_RigidBody.isKinematic = false;
            m_RigidBody.detectCollisions = true;
            m_RigidBody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            m_RigidBody.interpolation = RigidbodyInterpolation.None;
            m_RigidBody.angularVelocity = m_Angular;

#if UNITY_6000_0_OR_NEWER
            m_RigidBody.linearVelocity = m_Velocity;
#else
            m_RigidBody.velocity = m_Velocity;
#endif

            // release coroutine
            m_Coroutine = null;
        }

        protected void Update ()
        {
            m_Timer += Time.deltaTime;

            if (m_Timer > m_Lifespan)
            {
                m_PooledObject.ReturnToPool();
                m_Coroutine = null;
            }
        }
	}
}
