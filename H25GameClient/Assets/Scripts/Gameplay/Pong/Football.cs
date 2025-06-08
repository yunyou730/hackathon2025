using System;
using UnityEngine;
using Random = UnityEngine.Random;

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
            _dispatcher.AddListener(EventDefine.GAMEPLAY_MISSION_COMPLETE,OnMissionComplete);
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void OnDestroy()
        {
            _dispatcher.RemoveListener(EventDefine.GAMEPLAY_MISSION_COMPLETE,OnMissionComplete);
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
            
            // handle football velocity and direction
            ContactPoint contact = collision.contacts[0];
            ReflectFootball(contact,collision.gameObject);
            
            // destroy collision target
            if (IsGameObjectAsHitTarget(collision.gameObject))
            {
                var hitTarget = collision.gameObject.transform.parent.GetComponent<HitTarget>();
                hitTarget.HitByOther(1);   
            }
        }

        private void ReflectFootball(ContactPoint contact,GameObject target)
        {
            Vector3 normal = contact.normal;
            
            Vector3 input = _rigidbody.linearVelocity.normalized;
            Vector3 reflectDirection = Vector3.Reflect(_rigidbody.linearVelocity.normalized, normal);
            Vector3 output = reflectDirection.normalized;

            if (!IsGameObjectAsHitTarget(target))
            {
                output = CorrectRflection(input,output);
            }
            _rigidbody.linearVelocity = output * _rigidbody.linearVelocity.magnitude;
        }

        private Vector3 CorrectRflection(Vector3 input,Vector3 output)
        {
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
            return output;
        }

        private bool IsGameObjectAsHitTarget(GameObject target)
        {
            if (target.transform.parent != null)
            {
                var hitTarget = target.transform.parent.GetComponent<HitTarget>();
                if (hitTarget != null)
                {
                    return true;
                }
            }
            return false;
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

        private void OnMissionComplete()
        {
            Reset();
        }
    }

}

