using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Arkasa
{
    // Lets the operator control the session from a PC on the same Wi-Fi (tools/operator.ps1).
    // Listens for UDP text messages: PAUSE, RESUME, ESTOP, RESET, STATUS.
    // Replies with the current state, e.g. "STATE Running STEP toolkit TIME 95".
    public class OperatorLink : MonoBehaviour
    {
        public int port = 47770;

        [Header("Live values (read only)")]
        public bool listening;
        public string lastCommand;

        UdpClient udp;
        Thread listenThread;
        // Messages arrive on a background thread; they are handled on the main thread in Update.
        Queue<string> commands = new Queue<string>();
        Queue<IPEndPoint> senders = new Queue<IPEndPoint>();
        object queueLock = new object();

        void OnEnable()
        {
            try
            {
                udp = new UdpClient(port);
                listening = true;
                listenThread = new Thread(Listen);
                listenThread.IsBackground = true;
                listenThread.Start();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("OperatorLink could not open UDP port " + port + ": " + e.Message);
            }
        }

        void OnDisable()
        {
            listening = false;
            if (udp != null) udp.Close();
            udp = null;
        }

        void Listen()
        {
            while (listening)
            {
                try
                {
                    IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                    byte[] data = udp.Receive(ref sender);
                    string command = Encoding.UTF8.GetString(data).Trim().ToUpper();
                    lock (queueLock)
                    {
                        commands.Enqueue(command);
                        senders.Enqueue(sender);
                    }
                }
                catch
                {
                    // Socket closed (OnDisable) or a bad packet; keep going until listening is false.
                }
            }
        }

        void Update()
        {
            while (true)
            {
                string command;
                IPEndPoint sender;
                lock (queueLock)
                {
                    if (commands.Count == 0) return;
                    command = commands.Dequeue();
                    sender = senders.Dequeue();
                }
                Handle(command);
                Reply(sender);
            }
        }

        void Handle(string command)
        {
            lastCommand = command;
            SafetyManager safety = SafetyManager.Instance;
            if (safety == null) return;

            if (command == "PAUSE") safety.Pause();
            else if (command == "RESUME") safety.Resume();
            else if (command == "ESTOP" || command == "STOP") safety.EmergencyStop();
            else if (command == "RESET") safety.ResetSession();
        }

        void Reply(IPEndPoint to)
        {
            if (udp == null) return;
            string state = SafetyManager.Instance != null ? SafetyManager.Instance.state.ToString() : "-";
            string step = TaskSequencer.Instance != null ? TaskSequencer.Instance.CurrentStepId() : "-";
            float time = TaskSequencer.Instance != null ? TaskSequencer.Instance.elapsedTime : Time.time;
            byte[] message = Encoding.UTF8.GetBytes("STATE " + state + " STEP " + step + " TIME " + Mathf.RoundToInt(time));
            try { udp.Send(message, message.Length, to); } catch { }
        }
    }
}
