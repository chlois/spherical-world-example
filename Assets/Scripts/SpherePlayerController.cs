using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;


namespace UnityStandardAssets.Characters.ThirdPerson {
    [RequireComponent(typeof (SphereCharacter))]
    public class SpherePlayerController : MonoBehaviour {
        public float moveSpeed = 15;
        private Vector3 moveDir;
        private SphereCharacter m_Character;
        private Transform m_Cam;     
        private Vector3 m_Move;
        private bool m_Jump;
        
        private void Start() {
            // get the transform of the main camera
            if (Camera.main != null) {
                m_Cam = Camera.main.transform;
            }
            else {
                Debug.LogError(
                    "Error: no main camera found. Third person character needs a Camera tagged \"MainCamera\", for camera-relative controls.", gameObject);
            }
            m_Character = GetComponent<SphereCharacter>();
        }
        
        void Update() {
            if (!m_Jump) {
                m_Jump = CrossPlatformInputManager.GetButtonDown("Jump");
            }

            if (Input.GetMouseButtonDown(0)) {
                var myMousePosition = Input.mousePosition;
            }
        }
        
        void FixedUpdate() {
            // read inputs
            float h = CrossPlatformInputManager.GetAxis("Horizontal");
            float v = CrossPlatformInputManager.GetAxis("Vertical");
            bool crouch = Input.GetKey(KeyCode.C);

            // calculate move direction to pass to character
            if (m_Cam != null) {
                // calculate relative direction to move
                Vector3 gravityUp = m_Character.m_SphereObject.GetGravityUp();
                // define local north and east according to current camera pose
                Vector3 north = Vector3.ProjectOnPlane(m_Cam.up, gravityUp).normalized;
                Vector3 east = Vector3.Cross(gravityUp, north);
                Vector3 m_Forward = Vector3.ProjectOnPlane(transform.forward, gravityUp).normalized;
                Vector3 m_Right = Vector3.ProjectOnPlane(transform.right, gravityUp).normalized;
                float localV = Vector3.Dot(v*north, m_Forward) + Vector3.Dot(h*east, m_Forward);
                float localH = Vector3.Dot(v*north, m_Right) + Vector3.Dot(h*east, m_Right);
                m_Move = new Vector3(localH, 0, localV);
            }
            else {
                Debug.LogError(
                    "Error: no main camera found. Third person character needs a Camera tagged \"MainCamera\", for camera-relative controls.", gameObject);
            }
#if !MOBILE_INPUT
			// walk speed multiplier
	        if (Input.GetKey(KeyCode.LeftShift)) m_Move *= 0.5f;
#endif
            m_Character.Move(m_Move, crouch, m_Jump);
            m_Jump = false;
        }
    }
}
