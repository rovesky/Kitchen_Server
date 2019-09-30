using FootStone.Kcp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using UnityEngine;

namespace Assets.Scripts.ECS
{

    [DisableAutoCreation]
    public class NetworkServerSystem : ComponentSystem, INetworkCallbacks
    {
        private KcpServer kcpServer;
       // private int conId = -1;
        private List<int> connections = new List<int>();

        public void OnConnect(KcpConnection connection)
        {
            FSLog.Info($"server connection created:{connection.Id}");
            connections.Add(connection.Id);

            connection.Recv += (inSequence, buffer) =>
            {    
                FSLog.Debug($"[{inSequence}] server recv data");
                kcpServer.SendData(connection.Id, buffer, buffer.Length);
            };
        }

        public void OnDisconnect(int connectionId)
        {
            FSLog.Info($"server connection destroyed:{connectionId}");
            connections.Remove(connectionId);
        }

        protected override void OnCreate()
        {            
            base.OnCreate();
            kcpServer = new KcpServer(this, 1001);

        }

        protected override void OnDestroy()
        {
            connections.Clear();
            if (kcpServer != null)
                kcpServer.Shutdown();
        }
        protected override void OnUpdate()
        {
            if (kcpServer != null)
            {
                kcpServer.Update();

            }

            //Entities.ForEach((ref PlayerCommand command) =>
            //     {
            //         if (connections.Count == 0)
            //         {
            //             conId = kcpClient.Connect("192.168.0.128", 1001);
            //         }
            //         else
            //         {
            //             var connection = kcpClient.GetConnection(conId);
            //             if (connection != null)
            //             {
            //                 kcpClient.Update();

            //                 if (connection.IsConnected)
            //                 {
            //                     byte[] data = command.ToData();
            //                     kcpClient.SendData(conId, data, data.Length);
            //                 }
            //             }

            //         }
            //     });
        }
    }
}
