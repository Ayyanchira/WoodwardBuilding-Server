using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CustomNetworkManager : NetworkManager {

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

    void Start()
    {
       
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
        Debug.Log("Client with ID " + conn.connectionId + "and with ip"+ conn.address.ToString()+" has connected");
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
            clientCount.color = Color.green;

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

        if(player.gameObject != null)
            NetworkServer.Destroy(player.gameObject);
    }

    protected void OnReceivedMessage(NetworkMessage netMsg)
    {
        var msg = netMsg.ReadMessage<clientMessages>();

        if (msg.purpose == "Initialization")
        {
            if (msg.deviceType == "Hololens")
            {
                Debug.Log("Client of type " + msg.deviceType + " has connected at " + msg.devicePosition);
                lens_player = (GameObject)GameObject.Instantiate(lensPlayer, msg.devicePosition, Quaternion.identity);
               
                //Assign a random colour
                ChangeMaterial(lens_player.GetComponent<MeshRenderer>());

                lastVufMark = msg.devicePosition;
                //connectedPlayers.Add(lens_player);
                NetworkServer.Spawn(lens_player);
            }

            else if (msg.deviceType == "iPad")
            {
                Debug.Log("Client of type " + msg.deviceType + " has connected. " + " Position of " + msg.devicePosition + " was given");
                ipad_player = (GameObject)GameObject.Instantiate(iPadPlayer, msg.devicePosition, Quaternion.identity);

                //Assign a random colour
                ChangeMaterial(ipad_player.GetComponent<MeshRenderer>());

                //connectedPlayers.Add(ipad_player);
                NetworkServer.Spawn(ipad_player);
            }
        }

        else if(msg.purpose == "Synchronization")
        {
            Vector3 location = msg.devicePosition + lastVufMark;
            float temp = location.z;
            location.z = location.x; // delete this coordinate switching code and flip the map by 180 degrees on the Y-axis
            location.x = -temp;
            lens_player.transform.position = location;
            lens_player.transform.rotation = msg.deviceRotation;
            
        }
        else if(msg.purpose == "Simulation")
        {
           
            print("Simulation message received");
            print("player " + msg.deviceType +" location fetched as " + msg.devicePosition);
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

    /*
    protected void receiveClientMessage(NetworkMessage netMsg)
    {
        //netMsg.reader.SeekZero(); //Index out of range error without this
        var msg = netMsg.ReadMessage<clientMessages>();

        if (msg.firstConnect)
        {
            firstConnectSetup(msg.devicePosition, msg.deviceType);

            var forClient = new toClientMessages();
            forClient.clientID = clientID;

          //  server.Send(messageID, forClient);
        }

        else
            trackingSetup(msg.devicePosition, msg.deviceType, msg.clientID);

        
    }

    protected void firstConnectSetup(Vector3 devicePosition, string deviceType)
    {
        Debug.Log("Device with the type " + deviceType + " at position " + devicePosition + " has connected!");

        if (deviceType == "iPad")
        {
            GameObject padPlayer = Instantiate(iPad, devicePosition, Quaternion.identity).gameObject;
            iPad.transform.position = devicePosition;
            playerDict.Add(clientID, padPlayer);
            Debug.Log("Client ID " + clientID + " saved to dictionary.");
        }

        else if (deviceType == "HoloLens")
        {
            GameObject lensPlayer = Instantiate(lens, devicePosition, Quaternion.identity).gameObject;
            lens.transform.position = devicePosition;
            playerDict.Add(clientID, lensPlayer);
            Debug.Log("Client ID " + clientID + " saved to dictionary.");
        }
    }

    protected void trackingSetup(Vector3 devicePosition, string deviceType, int deviceID)
    {
        Debug.Log("Device with the type " + deviceType + " has tracked an Image target at position " + devicePosition);

        GameObject player;
        playerDict.TryGetValue(deviceID, out player);
        player.transform.position = devicePosition;
    }
    */
}
