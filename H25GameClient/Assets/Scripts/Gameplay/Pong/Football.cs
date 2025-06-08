using UnityEngine;

namespace amaz.gameplay.pong
{
    public class Football : MonoBehaviour
    {
        private Rigidbody _rigidbody = null;
        public float _moveSpeed = 10.0f;
        
        private EventDispatcher _dispatcher = null;
        private Vector3 _originScale;

        void Awake()
        {
            _originScale = transform.localScale;
            Debug.Log($"Football Awake,origin scale:{_originScale}");
        }

        void Start()
        {
            _dispatcher = RacingGame.Instance().GetService<EventDispatcher>(); 
            _rigidbody = GetComponent<Rigidbody>();
        }
        
        private void FixedUpdate()
        {
            _rigidbody.linearVelocity = _rigidbody.linearVelocity.normalized * _moveSpeed;
        }
        
        private void OnTriggerEnter(Collider other)
        {
            Debug.Log("football.OnCollisionEnter:" + other.gameObject.name);
            if (other.gameObject.name == "left_wall")
            {
                _dispatcher.Dispatch(EventDefine.GAMEPLAY_LOST_1P);
            }
            else if (other.gameObject.name == "right_wall")
            {
                _dispatcher.Dispatch(EventDefine.GAMEPLAY_LOST_2P);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            Debug.Log("football.OnCollisionEnter:" + collision.gameObject.name);
            // 获取第一个接触点的法线
            ContactPoint contact = collision.contacts[0];
            Vector3 normal = contact.normal;
            
            Vector3 input = _rigidbody.linearVelocity.normalized;
            Vector3 reflectDirection = Vector3.Reflect(_rigidbody.linearVelocity.normalized, normal);
            Vector3 output = reflectDirection.normalized;

            float dot = Vector3.Dot(input, output);
            
            //  两个向量 过于 平行的情况下，加一点随机
            if (dot < 0.0f && Mathf.Abs(dot) > 0.9f)   
            {
                Debug.Log("fix dir manually!");
                float rdv = Random.value;
                if (input.x > 0.0f)
                {
                    output.x = rdv;
                }
                else
                {
                    output.x = -rdv;
                }
            }
            output = output.normalized;
            _rigidbody.linearVelocity = output * _rigidbody.linearVelocity.magnitude;
        }

        public void Reset()
        {
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
        }

        public void Dash()
        {
            var root = RacingGame.Instance().GetRootTransform();
            transform.SetParent(root);
            _rigidbody.linearVelocity = new Vector3(1.0f, 1.0f, 0.0f);
        }
    }

}

