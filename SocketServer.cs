using System;
using System.Collections.Generic;
using Fleck;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Tobii.Interaction;
using Tobii.Interaction.Framework;
using GazeHelpServer;

namespace Server
{
    // Object to send to clients
    struct State
    {
        public GazeTracking gazeTracking { get; set; }
        public UserPresence userPresence { get; set; }
        public EyeTrackingDeviceStatus eyeTrackingDeviceStatus { get; set; }
        public Rectangle screenBounds { get; set; }
        public Size displaySize { get; set; }
    }

    /**
     * WebSocket Server sending gazepoint and user presence data to the GazeHelp connection
     */
    public class SocketServer
    {
        private WebSocketServer server;
        private int port;
        private string address;
        private Host host;
        private HashSet<IWebSocketConnection> clients = new HashSet<IWebSocketConnection>();
        private GazePointDataStream gazePointDataStream;
        private State state = new State();
        private MainWindow window;

        public SocketServer(int port, string address, MainWindow window)
        {
            this.port = port;
            this.address = address;
            this.window = window;
            this.server = new WebSocketServer($"ws://{this.address}:{this.port}");
            this.server.RestartAfterListenError = true;
            this.server.ListenerSocket.NoDelay = true;

            // Publish updates when host changes
            this.host = new Host();
            this.host.States.CreateUserPresenceObserver().Changed += (e, userPresence) =>
            {
                this.state.userPresence = userPresence.Value;
                publishStateToAll();
            };

            this.host.States.CreateEyeTrackingDeviceStatusObserver().Changed += (e, eyeTrackingDeviceStatus) =>
            {
                this.state.eyeTrackingDeviceStatus = eyeTrackingDeviceStatus.Value;
                publishStateToAll();
            };

            this.host.States.CreateGazeTrackingObserver().Changed += (e, gazeTracking) =>
            {
                this.state.gazeTracking = gazeTracking.Value;
                publishStateToAll();
            };

            this.host.States.CreateScreenBoundsObserver().Changed += (e, screenBounds) =>
            {
                this.state.screenBounds = screenBounds.Value;
                publishStateToAll();
            };

            this.host.States.CreateDisplaySizeObserver().Changed += (e, displaySize) =>
            {
                this.state.displaySize = displaySize.Value;
                publishStateToAll();
            };
        }

        /**
         * Publishes the current state to all clients
         */
        private void publishStateToAll()
        {
            foreach (var client in clients)
            {
                publishState(client);
            }
        }

        /**
         * Publishes the state to one client
         */
        private void publishState(IWebSocketConnection client)
        {
            client.Send(JsonConvert.SerializeObject(new { type = "state", data = this.state }, new StringEnumConverter()));
        }

        /**
         * Starts the server
         */
        public void start()
        {

            this.server.Start(socket =>
            {

                socket.OnOpen = () =>
                {
                    window.showSuccess("Server is connected.");
                    
                    // subscribe the new socket to the gaze point stream
                    clients.Add(socket);
                    if (this.gazePointDataStream == null)
                    {
                        this.gazePointDataStream = this.host.Streams.CreateGazePointDataStream();
                        this.gazePointDataStream.Next += (sender, e) =>
                        {
                            foreach (IWebSocketConnection client in this.clients)
                            {
                                client.Send(JsonConvert.SerializeObject(new { type = "gazePoint", data = e.Data }, new StringEnumConverter()));
                            }
                        };
                    }
                    else
                    {
                        this.gazePointDataStream.IsEnabled = true;
                    }
                    publishState(socket);
                };

                socket.OnClose = () =>
                {
                    clients.Remove(socket);
                };

                socket.OnError = (e) =>
                {
                    window.showError("There was an error, please restart!");
                };

                socket.OnMessage = msg =>
                {
                    window.showError(msg);
                };
            });
        }

        /**
         * Stops the server
         */
        public void stop()
        {

            foreach (var client in clients)
            {
                client.Close();
            }
            this.server.Dispose();
        }
    }
}
