using UnityEngine;

namespace amaz.gameplay.pong
{
    public class Football : MonoBehaviour
    {
        private Rigidbody _rigidbody = null;
        public float _moveSpeed = 10.0f;
        
        void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();
            
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        private void FixedUpdate()
        {
            _rigidbody.linearVelocity = _rigidbody.linearVelocity.normalized * _moveSpeed;
        }
        
        private void OnTriggerEnter(Collider other)
        {
            Debug.Log("football.OnCollisionEnter:" + other.gameObject.name);
            
            // 获取触发器和进入物体的边界
            Bounds triggerBounds = GetComponent<Collider>().bounds;
            Bounds otherBounds = other.bounds;
    
            // 计算两个边界的最近点（近似碰撞点）
            Vector3 closestPoint = triggerBounds.ClosestPoint(otherBounds.center);
    
            // 调试用：绘制碰撞点
            Debug.DrawRay(closestPoint, Vector3.up * 2f, Color.red, 2f);
        }

        private void OnCollisionEnter(Collision collision)
        {
            Debug.Log("football.OnCollisionEnter:" + collision.gameObject.name);
            // 获取第一个接触点的法线
            ContactPoint contact = collision.contacts[0];
            Vector3 normal = contact.normal;
            
            Vector3 input = _rigidbody.linearVelocity.normalized;
            Vector3 reflectDirection = Vector3.Reflect(_rigidbody.linearVelocity.normalized, normal);
            Vector3 output = reflectDirection = reflectDirection.normalized;

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

        public void Dash()
        {
            _rigidbody.linearVelocity = new Vector3(1.0f, 1.0f, 0.0f);
        }
    }

}

