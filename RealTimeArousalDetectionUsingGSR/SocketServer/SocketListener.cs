﻿using Assets.Rage.GSRAsset;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SocketServer.Socket
{
    public class SocketListener
    {
        TcpListener server = null;

        public void StartListening()
        {
            try
            {
                // Set the TcpListener on port 13000.
                Int32 port = 10116;
                IPAddress localAddr = IPAddress.Parse("127.0.0.1");

                // TcpListener server = new TcpListener(port);
                server = new TcpListener(localAddr, port);

                // Start listening for client requests.
                server.Start();

                GSRSignalProcessor gsrHandler = new GSRSignalProcessor();

                // Buffer for reading data
                Byte[] bytes = new Byte[256];
                String data = null;

                // Enter the listening loop.
                while (true)
                {
                    Logger.Log("Waiting for a connection... ");

                    // Perform a blocking call to accept requests.
                    // You could also user server.AcceptSocket() here.
                    TcpClient client = server.AcceptTcpClient();
                    Logger.Log("Connected!");

                    data = null;

                    // Get a stream object for reading and writing
                    NetworkStream stream = client.GetStream();

                    int i;

                    // Loop to receive all the data sent by the client.
                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        // Translate data bytes to a ASCII string.
                        data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                        Logger.Log("Text received from client: " + data);

                        StringBuilder response = new StringBuilder();
                        response.Append("Received at ");
                        response.Append(DateTime.Now.ToString());
                        response.Append("\r\n");
                        response.Append(data);

                        //End Of Calibration Period
                        if (data.Equals("EOCP"))
                        {
                            response.Append("\r\n");
                            response.Append(gsrHandler.EndOfCalibrationPeriod());
                        }


                        if (data.Equals("GET_EDA"))
                        {
                            response.Append("\r\n");
                            string result = gsrHandler.GetJSONArousalStatistics(gsrHandler.GetArousalStatistics());
                            response.Append(result);
                        }

                        //End Of Measurement
                        if (data.Equals("EOM"))
                        {
                            response.Append("\r\n");
                            response.Append(gsrHandler.EndOfMeasurement());
                        }

                        byte[] msg = System.Text.Encoding.ASCII.GetBytes(response.ToString());

                        // Send back a response.
                        stream.Write(msg, 0, msg.Length);
                        Logger.Log("Sent: " + data);
                    }

                    // Shutdown and end connection
                    client.Close();
                }
            }
            catch (SocketException e)
            {
                Logger.Log("SocketException: " + e);
            }
            finally
            {
                // Stop listening for new clients.
                server.Stop();
            }
        }

        public void CloseSocket()
        {
            try
            {
                server.Stop();
            }
            catch(Exception e)
            {
                Logger.Log(e.ToString());
            }
        }

        public void Start()
        {
            Thread t = new Thread(StartListening);
            t.Start();
        }
    }
}