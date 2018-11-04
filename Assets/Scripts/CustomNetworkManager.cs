using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CustomNetworkManager : NetworkManager
{

    //Public variables
    public Text connectionText, clientCount;
    public short messageID = 777;
    public GameObject iPadPlayer, lensPlayer;
    Vector3 startPosition = new Vector3(16.5f, 0.5f, 57);

    //Private and Protected Variables
    protected int clientID;
    private int clients;
    protected GameObject lens_player, ipad_player;
    static Dictionary<string, string> clientAddressDictionary = new Dictionary<string, string>();
    static Dictionary<string, GameObject> connectedPlayerDictionary = new Dictionary<string, GameObject>();
    protected Vector3 lastVufMark;

    public class clientMessages : MessageBase
    {
        public string deviceType, purpose, ipAddress;
        public Vector3 devicePosition;
        public Quaternion deviceRotation;
    }

    public class ClientLocations : MessageBase
    {
        //TODO: Not able to find an alternative for this. Message base does not support Gameobject sending.
        public Vector3 devicePosition1;
        public Quaternion deviceRotation1;
        public Vector3 devicePosition2;
        public Quaternion deviceRotation2;
        public Vector3 devicePosition3;
        public Quaternion deviceRotation3;
        public Vector3 devicePosition4;
        public Quaternion deviceRotation4;
        public Vector3 devicePosition5;
        public Quaternion deviceRotation5;
        public Vector3 devicePosition6;
        public Quaternion deviceRotation6;
        public Vector3 devicePosition7;
        public Quaternion deviceRotation7;
        public Vector3 devicePosition8;
        public Quaternion deviceRotation8;
    }

    void Start()
    {

    }

    private int nextUpdate = 1;
    void Update()
    {

        // If the next update is reached
        if (Time.time >= nextUpdate)
        {
            // Change the next update (current second+1)
            nextUpdate = Mathf.FloorToInt(Time.time) + 1;
            // Call your fonction
            UpdateEverySecond();
        }

    }
    void UpdateEverySecond()
    {
        if (clients > 0)
        {
            var sendMsg = new ClientLocations();
            int count = 0;
            foreach (GameObject player in connectedPlayerDictionary.Values)
            {
                switch (count)
                {
                    case 0:
                        sendMsg.devicePosition1 = player.transform.position;
                        sendMsg.deviceRotation1 = player.transform.rotation;
                        break;
                    case 1:
                        sendMsg.devicePosition2 = player.transform.position;
                        sendMsg.deviceRotation2 = player.transform.rotation;
                        break;
                    case 2:
                        sendMsg.devicePosition3 = player.transform.position;
                        sendMsg.deviceRotation3 = player.transform.rotation;
                        break;
                    case 3:
                        sendMsg.devicePosition4 = player.transform.position;
                        sendMsg.deviceRotation4 = player.transform.rotation;
                        break;
                    case 4:
                        sendMsg.devicePosition5 = player.transform.position;
                        sendMsg.deviceRotation5 = player.transform.rotation;
                        break;
                    case 5:
                        sendMsg.devicePosition6 = player.transform.position;
                        sendMsg.deviceRotation6 = player.transform.rotation;
                        break;
                    case 6:
                        sendMsg.devicePosition7 = player.transform.position;
                        sendMsg.deviceRotation7 = player.transform.rotation;
                        break;
                    case 7:
                        sendMsg.devicePosition8 = player.transform.position;
                        sendMsg.deviceRotation8 = player.transform.rotation;
                        break;
                    default:
                        print("Trying to add 9th player at server side. Please check");
                        break;
                }

                count++;
            }
            NetworkServer.SendToAll(778, sendMsg);
        }
    }

    //Required GUI function that currently produces two buttons
    private void OnGUI()
    {
        if (GUI.Button(new Rect(10, 50, 125, 50), "Start Server"))
            StartServer();
        if (GUI.Button(new Rect(10, 110, 125, 50), "Stop Server"))
            StopServer();
    }

    //Called when the server has started
    //All functions with "override" in the definition are NetworkManager predefined functions
    public override void OnStartServer()
    {
        Debug.Log("Server has Started!");
        connectionText.text = "Online";
        connectionText.color = Color.green;

        NetworkServer.RegisterHandler(messageID, OnReceivedMessage);
        base.OnStartServer();

    }

    //Called when the server has stopped
    public override void OnStopServer()
    {
        base.OnStopServer();
        connectedPlayerDictionary.Clear();
        Debug.Log("Server has Stopped :(");

        connectionText.text = "Offline";
        connectionText.color = Color.red;

        clients = 0;
        clientCount.text = " " + clients;
        clientCount.color = Color.red;
    }

    //Called when a client has connected to the server
    public override void OnServerConnect(NetworkConnection conn) //conn contains numerous paramters on the connected client
    {
        base.OnServerConnect(conn);
        clientID = conn.connectionId;
        Debug.Log("Client with ID " + conn.connectionId + "and with ip" + conn.address.ToString() + " has connected");
        // TODO: add player to dictionary
        clientAddressDictionary.Add(conn.connectionId.ToString(), conn.address.ToString());
        GameObject connectedPlayer = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ChangeMaterial(connectedPlayer.GetComponent<MeshRenderer>());

        connectedPlayer.transform.position = new Vector3(0, 0, 0);
        //connectedPlayers.Add(connectedPlayer);
        connectedPlayerDictionary.Add(conn.address.ToString(), connectedPlayer);
        //connectedPlayer.transform.position = Vector3.zero;
        clients++;
        clientCount.text = " " + clients;

        if (clients > 0)
        {
            clientCount.color = Color.green;
            //TODO: Trying to send message to client

        }


    }

    //Called when a client has disconnected from the server
    public override void OnServerDisconnect(NetworkConnection conn)
    {

        base.OnServerDisconnect(conn);
        Debug.Log("Client with ID " + conn.connectionId + "and with ip" + conn.address.ToString() + " has disconnected :(");

        string clientIP = clientAddressDictionary[conn.connectionId.ToString()];

        print("Removing prefab associated with ip " + conn.address.ToString());
        GameObject disconnectingPlayer = connectedPlayerDictionary[clientIP];
        Destroy(disconnectingPlayer);
        //disconnectingPlayer = null;
        connectedPlayerDictionary.Remove(conn.address.ToString());
        clientAddressDictionary.Remove(conn.connectionId.ToString());
        clients--;
        clientCount.text = " " + clients;

        /*
        GameObject exitPlayer;
        playerDict.TryGetValue(conn.connectionId, out exitPlayer);
        Destroy(exitPlayer);
        */

        if (clients == 0)
            clientCount.color = Color.red;
    }

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {

        Debug.Log("adding a player");
        //var player = (GameObject)GameObject.Instantiate(playerPrefab, startPosition, Quaternion.identity);
        //player.GetComponent<MeshRenderer>.material("_Color", Color.green);

    }

    public override void OnServerRemovePlayer(NetworkConnection conn, PlayerController player)
    {
        base.OnServerRemovePlayer(conn, player);

        if (player.gameObject != null)
            NetworkServer.Destroy(player.gameObject);
    }

    protected void OnReceivedMessage(NetworkMessage netMsg)
    {
        var msg = netMsg.ReadMessage<clientMessages>();

        if (msg.purpose == "Simulation")
        {

            print("Simulation message received");
            print("player " + msg.deviceType + " location fetched as " + msg.devicePosition);
            print("keys in dictionary : " + connectedPlayerDictionary.Keys);
            //if (msg.ipAddress == "")
            //{
            msg.ipAddress = clientAddressDictionary[netMsg.conn.connectionId.ToString()];
            //}
            GameObject syncingPlayer = connectedPlayerDictionary[msg.ipAddress];
            syncingPlayer.transform.position = msg.devicePosition;
            syncingPlayer.transform.rotation = msg.deviceRotation;
        }
    }

    void ChangeMaterial(MeshRenderer platformRenderer)
    {
        Material material = new Material(Shader.Find("Standard"));
        Color color = new Color(Random.Range(0, 1), Random.Range(0, 1), Random.Range(0, 1), 1);
        material.color = color;
        platformRenderer.material = material;
    }

}
