using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public sealed class NetworkManager : MonoBehaviour
{
    [SerializeField]
    UIManager uiManager;

    private static readonly NetworkManager instance = new NetworkManager();
    private static Socket _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

    [SerializeField]
    private byte[] _buffer;

    public int attempts = 0;

    private void Awake()
    {
        uiManager = GetComponent<UIManager>();
    }

    public void LoopConnect()
    {
            var endPoint = new IPEndPoint(IPAddress.Loopback, 100);
            _clientSocket.BeginConnect(endPoint, ConnectCallback, null);
    }


    private void ConnectCallback(IAsyncResult AR)
    {
        try
        {
            _clientSocket.EndConnect(AR);
            _buffer = new byte[1024];
            _clientSocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, ReceiveCallback, null);
        }
        catch (SocketException ex)
        {
            Debug.Log(ex.Message);
        }
        catch (ObjectDisposedException ex)
        {
            Debug.Log(ex.Message);
        }
    }

    private void ReceiveCallback(IAsyncResult AR)
    {
        try
        {
            int received = _clientSocket.EndReceive(AR);

            if (received == 0)
            {
                return;
            }

            byte[] dataBuff = new byte[received];
            Array.Copy(_buffer, dataBuff, received);

            _clientSocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, ReceiveCallback, null);
        }
        catch (SocketException ex)
        {
            ConnectionLost();
            Debug.Log(ex.Message);
        }
        catch (ObjectDisposedException ex)
        {
            Debug.Log(ex.Message);
        }
    }

    public bool GetStatusConnection()
    {
        return _clientSocket.Connected;
    }

    private void ConnectionLost()
    {
        Array.Clear(_buffer, 0, _buffer.Length);
        _clientSocket.Shutdown(SocketShutdown.Both);
        _clientSocket.Disconnect(true);
    }

    public static NetworkManager Instance
    {
        get
        {
            return instance;
        }
    }
}
