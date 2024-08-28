using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Main
{
    public class SocketManager : MonoBehaviour
    {
        SocketIOUnity _socket;
        int Port => 3000;
        public string thisID;
        
        public List<PlayerMove> playerMoveList;
        public PlayerMove thisPlayer;
        
        public Dictionary<string, float> ClientList;
        
        bool _playerSet;
        bool _positionOn;
        
        public float moveInput = 0;
        float _payload = 0;
        
        Transform _playerManager;
        Button _disconnectButton;
        Button _connectButton;
        
        void Awake()
        {
            _disconnectButton = GameObject.Find("Disconnect").GetComponent<Button>();
            _disconnectButton.onClick.AddListener(Disconnect);
            _connectButton = GameObject.Find("Connect").GetComponent<Button>();
            _connectButton.onClick.AddListener(Connect);
            
            _playerManager = GameObject.Find("PlayerManager").transform;
            playerMoveList = _playerManager.GetComponentsInChildren<PlayerMove>().ToList();
            foreach (var player in playerMoveList)
            {
                player.gameObject.SetActive(false);
            }
        }

        void Start()
        {
            MainThreadRun(); 
            SocketThreadRun();
        }

        void Update()
        {
            moveInput = Input.GetAxis("Horizontal"); 
        }

        void MainThreadRun()
        {
            PlayerSet();
        }

        async void PlayerSet()
        {
            while (!_playerSet) await Task.Yield();
            _positionOn = false;
            
            if (ClientList != null)
            {
                PlayerInit();
                int index = 0;
                foreach (var client in ClientList)
                {
                    if (index >= playerMoveList.Count)
                    {
                        print("We need more players!");
                        index += 1;
                        continue;
                    }

                    print($"{client.Key} is positionX {client.Value}");

                    playerMoveList[index].gameObject.SetActive(true);
                    playerMoveList[index].name = client.Key;
                    playerMoveList[index].transform.position = new Vector3(client.Value, -1.25f, -1f);
                    index += 1;
                }

                thisPlayer = playerMoveList.Find(i => i.name == thisID);
                print($"{thisPlayer.name} is ready!");
            }
            else
            {
                _playerSet = false;
                PlayerSet();
            }

            _positionOn = true;
            PositionEmit();
            
            _playerSet = false;
            if (!_playerSet) PlayerSet();
        }

        void PlayerInit()
        {
            foreach (var player in playerMoveList)
            {
                player.gameObject.SetActive(false);
                player.name = "Player";
                player.transform.position = new Vector3(0f, 0f, 0f);
            }
        }

        void SocketThreadRun()
        {
            _socket = new SocketIOUnity($"http://localhost:{Port}");
            _socket.Connect();
            
            _socket.OnConnected += async (sender, eventArgs) =>
            {
                thisID = _socket.Id;
                print($"{thisID} is connected!");
            };
            
            _socket.On("playerSet", clientList =>
            {
                try 
                {
                    _playerSet = true;
                    ClientList = clientList.GetValue<Dictionary<string, float>>();
                }
                catch (Exception exception)
                {
                    Debug.LogError(exception.Message);
                    throw;
                }
            });
            
            _socket.On("updatePlayer", clientList =>
            {
                try
                {
                    ClientList = clientList.GetValue<Dictionary<string, float>>();
                }
                catch (Exception exception)
                {
                    Debug.LogError(exception);
                    throw;
                }
            });

            _socket.OnDisconnected += async (sender, eventArgs) =>
            {
                print($"{thisID} is disconnected!");
                _playerSet = false;
                PlayerInit();
            };
        }

        async void PositionEmit()
        {
            _payload = thisPlayer.RendererNode.flipX ? thisPlayer.transform.position.x + 100 : thisPlayer.transform.position.x;
            _socket.Emit("position", _payload);
            await Task.Yield();
            if (_positionOn) PositionEmit();
        }
        
        void OnApplicationQuit()
        {
            PlayerInit();
            _socket.Disconnect();
        }

        void Disconnect()
        {
            _socket?.Disconnect();
        }
        
        void Connect()
        {
            _socket?.Connect();
        }
    }
}
