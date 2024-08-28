using UnityEngine;

namespace Main
{
    public class PlayerMove : MonoBehaviour
    {
        float MoveSpeed => 5f;

        SocketManager SocketManagerNode => FindObjectOfType<SocketManager>();
        Animator AnimatorNode => GetComponent<Animator>();
        public SpriteRenderer RendererNode => GetComponent<SpriteRenderer>();
        int Walk => Animator.StringToHash("Walk");

        Vector3 _position = new(0, -1.25f, -1f);
        Vector3 _move = new();
        float _positionX = 0;

        void Update()
        {
            if (name == SocketManagerNode.thisID)
            {
                SocketManagerNode.thisPlayer = this;
                Move(SocketManagerNode.moveInput);
            }
            else
            {
                _positionX = SocketManagerNode.ClientList[name];
                _position.x = _positionX >= 100 ? _positionX - 100 : _positionX;
                transform.position = _position;
                RendererNode.flipX = SocketManagerNode.ClientList[name] >= 100;
            }
        }

        void Move(float input)
        {
            AnimatorNode.SetBool(Walk, input != 0);
            RendererNode.flipX = input < 0 ? true : input > 0 ? false : RendererNode.flipX;
            _move.x = input * MoveSpeed * Time.deltaTime;
            transform.position += _move;
        }
    }
}
