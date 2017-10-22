using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    private static Socket _clientSocket;

    private PacketHelper packetHelper;
    private NetworkHelper networkHelper;

    private void Awake()
    {
        packetHelper = new PacketHelper();
        networkHelper = GetComponent<NetworkHelper>();

        _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    }

    private void Start()
    {
        //singleton initialization
        var ugly = UnityThreadHelper.Dispatcher;
    }

    [SerializeField]
    private byte[] _buffer;

    public int attempts = 0;

    public void LoopConnect()
    {
        _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPEndPoint endPoint = new IPEndPoint(IPAddress.Loopback, 100);
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

    public void SendPacket(ref byte[] data)
    {
        _clientSocket.Send(data);

        byte[] debugBytes = data;

        UnityThreadHelper.Dispatcher.Dispatch(() =>
            {
                networkHelper.SetPacketDisplay(networkHelper.PrintBytes(ref debugBytes), true);
            });

        _clientSocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), this);
    }

    private void ReceiveCallback(IAsyncResult AR)
    {
        
        try
        {
            int recieved = _clientSocket.EndReceive(AR);

            if (recieved == 0)
            {
                return;
            }

            byte[] dataBuff = new byte[recieved];
            Array.Copy(_buffer, dataBuff, recieved);
            
            //Breaking down buffer into segments
            byte[] message_number_bytes = new byte[2];
            byte[] packet_length_bytes = new byte[2];
            byte[] data = new byte[recieved - 4];

            //case of bad message
            UInt16 message_number = UInt16.MaxValue, packet_length = UInt16.MaxValue;


            

            UnityThreadHelper.Dispatcher.Dispatch(() =>
            {
                networkHelper.SetPacketDisplay(networkHelper.PrintBytes(ref dataBuff), false);
            });

            if (recieved >= 4)
            {
                Array.Copy(dataBuff, message_number_bytes, 2);
                Array.Copy(dataBuff, 2, packet_length_bytes, 0, 2);
                Array.Copy(dataBuff, 4, data, 0, dataBuff.Length - 4);
                
                //Turn bytes to usuable data
                networkHelper.BytesToMessageLength(ref message_number_bytes, ref packet_length_bytes, ref message_number, ref packet_length);
                Debug.Log(message_number + "  " + packet_length);
            }
            
            //Packet handling recieving
            byte[] dataToSendTemp = new byte[512];

            #region MESSAGE HANDLING
            switch (message_number)
            {
                case Messages.SET_NAME_RESPONSE:
                    {
                        UInt16 server_response = packetHelper.BytesToUInt16(ref data);

                        //Decoding
                        {
                            if(server_response == Constants.USERNAME_OK)
                            {
                                UnityThreadHelper.Dispatcher.Dispatch(() =>
                                {
                                    networkHelper.userNameOK();
                                });
                            }
                            else
                            {
                                UnityThreadHelper.Dispatcher.Dispatch(() =>
                                {
                                    networkHelper.userNameBAD(server_response);
                                });
                            }
                        }
                        break;
                    }
                case Messages.USER_ID_RESPONSE:
                    {
                        int server_response = packetHelper.BytesToInt(ref data);

                        UnityThreadHelper.Dispatcher.Dispatch(() =>
                        {
                            networkHelper.idRecieved(server_response);
                        });

                        break;
                    }
                case Messages.USER_DISCONNECT:
                    {
                        UInt16 server_response = packetHelper.BytesToUInt16(ref data);
 
                        //Decoding
                        {
                            if(server_response == Constants.LOGOUT_NORMAL)
                            {
                                UnityThreadHelper.Dispatcher.Dispatch(() =>
                                {
                                    networkHelper.loggedOut(Constants.LOGOUT_NORMAL);
                                });
                            }
                            else if(server_response == Constants.LOGOUT_WARNING)
                            {
                                
                                UnityThreadHelper.Dispatcher.Dispatch(() =>
                                {
                                    networkHelper.loggedOut(Constants.LOGOUT_WARNING);
                                });
                            }
                            else
                            {
                                UnityThreadHelper.Dispatcher.Dispatch(() =>
                                {
                                    networkHelper.loggedOut(Constants.LOGOUT_FORCED);
                                });
                            }
                        }
                        break;
                    }
                case Messages.ROOM_CREATE_RESPONSE:
                    {
                        UInt16 server_response = packetHelper.BytesToUInt16(ref data);

                        //Decoding
                        {
                            if(server_response == Constants.ROOM_CREATE_SUCCESS)
                            {
                                UnityThreadHelper.Dispatcher.Dispatch(() =>
                                {
                                    networkHelper.roomCreatedSuccessfull();
                                });
                            }
                            else
                            {
                                UnityThreadHelper.Dispatcher.Dispatch(() =>
                                {
                                    networkHelper.roomCreationFailed(ref server_response);
                                });
                            }
                        }
                        break;
                    }
                case Messages.ROOM_ABANDON_RESPONSE:
                    {
                        UInt16 server_response = packetHelper.BytesToUInt16(ref data);

                        //Decoding
                        {
                                UnityThreadHelper.Dispatcher.Dispatch(() =>
                                {
                                    networkHelper.roomAbandonedCheck(ref server_response);
                                });
                        }
                        break;
                    }

                case Messages.ROOM_JOIN_PUBLIC_ROOM_RESPONSE:
                {
                        UInt16 server_response = packetHelper.BytesToUInt16(ref data);

                        //Decoding
                        {
                            if(server_response == Constants.ROOM_JOIN_SUCCESS)
                            {
                                UnityThreadHelper.Dispatcher.Dispatch(() =>
                                {
                                    networkHelper.roomJoinSuccess();
                                });
                            }
                            else
                            {
                                UnityThreadHelper.Dispatcher.Dispatch(() =>
                                {
                                    networkHelper.roomJoinFailed();
                                });
                            }
                        }

                        break;
                }

                case Messages.ROOM_CANCEL_FINDING_RESPONSE:
                    {
                        UInt16 server_response = packetHelper.BytesToUInt16(ref data);

                        //Decoding
                        {
                            if (server_response == Constants.ROOM_CANCEL_FINDING_SUCCESS)
                            {
                                UnityThreadHelper.Dispatcher.Dispatch(() =>
                                {
                                    networkHelper.roomCancelFindingSuccess();
                                });
                            }
                            else
                            {
                                UnityThreadHelper.Dispatcher.Dispatch(() =>
                                {
                                    networkHelper.roomCancelFindingFailed();
                                });
                            }
                        }

                        break;
                    }

                case Messages.ROOM_JOINED_MY_ROOM:
                    {
                        string opponent = Encoding.ASCII.GetString(data);

                        if(!string.IsNullOrEmpty(opponent))
                        {
                            UnityThreadHelper.Dispatcher.Dispatch(() =>
                            {
                                networkHelper.playerJoinedMyRoom(opponent);
                            });
                        }
                        break;
                    }

                case Messages.ROOM_JOINED_PUBLIC_ROOM:
                    {
                        int room_id;
                        string opponentUserName = "";
                        room_id = packetHelper.BytesToJoinedRoomData(ref data, ref opponentUserName);

                        UnityThreadHelper.Dispatcher.Dispatch(() =>
                        {
                            networkHelper.joinedRoom(room_id, opponentUserName);
                        });

                        break;
                    }

                case Messages.ROOM_LEAVE_RESPONSE:
                    {
                        UInt16 server_response = packetHelper.BytesToUInt16(ref data);

                        if(server_response == Constants.ROOM_LEAVE_SUCCESS)
                        {
                            UnityThreadHelper.Dispatcher.Dispatch(() =>
                            {
                                networkHelper.leaveRoomSuccessful();
                            });
                        }
                        else
                        {
                            UnityThreadHelper.Dispatcher.Dispatch(() =>
                            {
                                networkHelper.leaveRoomFailed();
                            });
                        }

                        break;
                    }

                case Messages.ROOM_PLAYER_LEFT_MY_ROOM:
                    {
                        UnityThreadHelper.Dispatcher.Dispatch(() =>
                        {
                            networkHelper.playerLeftMyRoom();
                        });
                        break;
                    }

                case Messages.ROOM_CLOSED:
                    {
                        UnityThreadHelper.Dispatcher.Dispatch(() =>
                        {
                            networkHelper.roomClosed();
                        });

                        break;
                    }
                default:

                    break;
            }
            #endregion

            _clientSocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, ReceiveCallback, null);
        }
        catch (SocketException ex)
        {
            ConnectionLost();
            Debug.Log(ex.Message);
        }
        catch (ObjectDisposedException ex)
        {
            ConnectionLost();
            Debug.Log(ex.Message);
        }
    }

    public bool GetStatusConnection()
    {
        if(_clientSocket != null)
            return _clientSocket.Connected;
        return false;
    }

    private void ConnectionLost()
    {
        Array.Clear(_buffer, 0, _buffer.Length);
        _clientSocket.Shutdown(SocketShutdown.Both);
        _clientSocket.Disconnect(true);
    }
}
