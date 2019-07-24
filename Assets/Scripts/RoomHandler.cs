using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using Colyseus;
using Colyseus.Schema;

using GameDevWare.Serialization;

public class RoomHandler : MonoBehaviour
{
    public Button m_ConnectButton, m_JoinButton, m_StartButton, m_DCButton, m_ShowRoomButton;

    public Text m_IdText;	
    private string endpoint = "ws://34.73.252.192:2567";

    //private string endpoint = "ws://localhost:2567";
    protected Client client;
	protected Room<State> room;
    public string roomName = "demo";

	IEnumerator Start () {
		/* Demo UI */
		m_ConnectButton.onClick.AddListener(ConnectToServer);
        m_JoinButton.onClick.AddListener(JoinRoom);
        m_StartButton.onClick.AddListener(StartGame);
        m_DCButton.onClick.AddListener(DisconnectFromServer);
        m_ShowRoomButton.onClick.AddListener(GetAvailableRooms);

		/* Always call Recv if Colyseus connection is open */
		while (true)
		{
			if (client != null)
			{
				client.Recv();
			}
			yield return 0;
		}
	}

	async void ConnectToServer ()
	{
		Debug.Log("Connecting to " + endpoint);
		client = new Client(endpoint);

		client.OnOpen += (object sender, EventArgs e) => {
			 Debug.Log("id: " + client.Id);
		};
		client.OnError += (sender, e) => {
            Debug.LogError("error on connect: " + e.Message);
            Debug.Log("error on connect 2: " + e.Message);
        };
		client.OnClose += (sender, e) => Debug.Log("CONNECTION CLOSED");
		StartCoroutine(client.Connect());
	}

    void JoinRoom ()
	{
		Debug.Log("JOINING ROOM ---");
		room = client.Join<State>(roomName, new Dictionary<string, object>()
		{
			{ "create", true },
			{ "xyz", "abc" }

		});

		room.OnReadyToConnect += (sender, e) => {
			Debug.Log("Ready to connect to room!");
			StartCoroutine(room.Connect());
		};
		room.OnError += (sender, e) =>
		{
			Debug.Log("ERROR ON ROOM ---");
			Debug.LogError(e.Message);
		};
		room.OnJoin += (sender, e) => {
			Debug.Log("Joined room successfully. " +room.SessionId);

            RoomManager.room = room;
            RoomManager.client = client;
            //PlayerPrefs.SetString("sessionId", room.SessionId);
			//PlayerPrefs.Save();
		};

		//room.OnStateChange += OnStateChangeHandler;
		//room.OnMessage += OnMessage;
	}

    void StartGame() {
        Debug.Log("starting game");
        SceneManager.LoadScene("MainScene");
    }
    void Update()
    {
        
    }

    void OnApplicationQuit()
	{
		DisconnectFromServer();
	}

    void DisconnectFromServer() {
        m_IdText.text = "";
		if (room != null)
		{
			room.Leave();
		}

		if (client != null)
		{
			client.Close();
		}
    }

	void GetAvailableRooms()
	{
		client.GetAvailableRooms(roomName, (RoomAvailable[] roomsAvailable) =>
		{
			Debug.Log("Available rooms (" + roomsAvailable.Length + ")");
			for (var i=0; i< roomsAvailable.Length;i++)
			{
                m_IdText.text += roomsAvailable[i].roomId + " ,";
				Debug.Log("roomId: " + roomsAvailable[i].roomId);
				Debug.Log("maxClients: " + roomsAvailable[i].maxClients);
				Debug.Log("clients: " + roomsAvailable[i].clients);
				Debug.Log("metadata: " + roomsAvailable[i].metadata);
			}
		});
	}
}
