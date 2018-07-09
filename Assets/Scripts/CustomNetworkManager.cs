using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CustomNetworkManager : NetworkManager {

    public Text connectionText, clientCount;
    private int clients;
    public short messageID = 777;
    public Transform iPad, lens;

    public class clientMessages : MessageBase
    {
        public string deviceType;
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

        NetworkServer.RegisterHandler(messageID, receiveClientMessage);

        base.OnStartServer();
    }

    //Called when the server has stopped
    public override void OnStopServer()
    {
        base.OnStopServer();
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

        clients++;
        clientCount.text = " " + clients;

        if (clients > 0)
            clientCount.color = Color.green;

    }

    //Called when a client has disconnected from the server
    public override void OnServerDisconnect(NetworkConnection conn)
    {
        base.OnServerDisconnect(conn);
        Debug.Log("Client " + conn.address + " has disconnected :(");
        clients--;
        clientCount.text = " " + clients;

        if (clients == 0)
            clientCount.color = Color.red;
    }

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
       
    }

    void receiveClientMessage(NetworkMessage netMsg)
    {
        var msg = netMsg.ReadMessage<clientMessages>();
        Debug.Log("Device with the type " + msg.deviceType + " has connected!");

        if(msg.deviceType == "iPad")
        {
            Instantiate(iPad, new Vector3(18, .5f, 26), Quaternion.identity);
        }
        
        else if (msg.deviceType == "HoloLens")
        {
            Instantiate(lens, new Vector3(22, .75f, 30), Quaternion.identity);
        }


    }
}
