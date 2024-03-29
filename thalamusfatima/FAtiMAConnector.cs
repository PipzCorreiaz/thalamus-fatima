// FAtiMAConnector.cs - 
//
// Copyright (C) 2014 GAIPS/INESC-ID
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
// 
// Company: GAIPS/INESC-ID
// Project: 
// Created: 2014
// Created by: Jo�o Dias
// Email to: joao.dias@gaips.inesc-id.pt
// 
//

using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Diagnostics;


//using Unity.Realizer;

using System.IO;
using System;
using ThalamusFAtiMA.Actions;
using ThalamusFAtiMA.Emotions;
using ThalamusFAtiMA.Speech;
using ThalamusFAtiMA.Speech.AMSummary;
using ThalamusFAtiMA.Utils;
using System.Xml;
using System.Collections.Generic;

namespace ThalamusFAtiMA
{
    public class FAtiMAConnector
    {
        private const string EOM_TAG = "\n";
        private const string EOF_TAG = "<EOF>";
        private const int bufferSize = 1024;
        private const int PENDING_CONNECTION_QUEUE_LENGTH = 10;
        private const int INITIAL_SERVER_PORT = 46874;
        private const int SERVER_PORT_RANGE = 100;


        public const string REMOTE_ACTION_PARAMETERS = "REMOTE_ACTION_PARAMETERS";

        public const string START_MESSAGE = "CMD Start";
        public const string STOP_MESSAGE = "CMD Stop";
        public const string RESET_MESSAGE = "CMD Reset";
        public const string SAVE_MESSAGE = "CMD Save";
        public const string REMOVE_ALL_GOALS_MESSAGE = "REMOVEALLGOALS";
        public const string ADD_GOALS_MESSAGE_START = "ADDGOALS ";
        public const string ACTION_FINISHED = "ACTION-FINISHED";
        public const string ACTION_STARTED = "ACTION-STARTED";
        public const string ACTION_FAILED = "ACTION-FAILED";
        public const string ENTITY_ADDED = "ENTITY-ADDED";
        public const string ENTITY_REMOVED = "ENTITY-REMOVED";
        public const string PROPERTY_CHANGED = "PROPERTY-CHANGED";
        public const string PROPERTY_REMOVED = "PROPERTY-REMOVED";
        public const string USER_SPEECH = "USER-SPEECH";
        public const string AGENTS = "AGENTS";
        public const string LOOK_AT = "LOOK-AT";
        public const string ADVANCE_TIME = "ADVANCE-TIME";
        public const string STOP_TIME = "STOP-TIME";
        public const string RESUME_TIME = "RESUME-TIME";
        public const string SHUTDOWN_MESSAGE = "SHUTDOWN";



        //communication/socket fields
        private Socket socket;
        private Socket serverSocket;
        private NetworkStream socketStream;
        private bool receiverAlive;
        private Thread receiverThread;
        protected byte[] buffer = new byte[bufferSize];
        protected static int currentServerPort = INITIAL_SERVER_PORT;
//        private byte[] buffer = new byte[bufferSize];

        public string Sex { get; private set; }
        public string Role { get; private set; }
        private string robotId;
        public string Name { get; private set; }

        public ThalamusConnector ThalamusConnector { private get; set; }

        private PredefinedUtteranceSelector PredefinedUtteranceSelector { get; set; }
        private AMSummaryTemplateMatcher AMSummaryTemplateMatcher { get; set; }

        private string _version;
		
		// Henrique Campos - auxiliary variable for emotional state messages in Parse(..)
		private string _previousEmotionalMsg = String.Empty;

        private EmotionalState _emotionalState;
        private string _previousEmotion = "";
        private Random random;

        public SpeechActParameters CurrentSpeechAct { get; set; }

        public Socket Socket
        {
            get
            {
                return this.socket;
            }
            set
            {
                this.socket = value;
                this.socketStream = new NetworkStream(this.socket);
            }
        }

        public bool ReceiverAlive
        {
            get
            {
                return this.receiverAlive;
            }
            set
            {
                this.receiverAlive = value;
            }
        }


		public bool ConnectionReady {get; set;}
        //actions  
       
        public FAtiMAConnector(ThalamusConnector thalamus, string robotId, string name, string sex, string role, string version) 
		{
            this.ThalamusConnector = thalamus;
            this.robotId = robotId;
            this.receiverAlive = false;
			this.ConnectionReady = false;
            //this.emotionalState = new Property<EmotionalState>(new EmotionalState());
            //this.relations = new Property<RelationSet>(new RelationSet());
            this.Sex = sex;
            this.Role = role;
            this.Name = name;
            this._emotionalState = new EmotionalState();
            this.random = new Random(Guid.NewGuid().GetHashCode());
			// Henrique Campos - emotional state property

            this.PredefinedUtteranceSelector = new PredefinedUtteranceSelector(version);
            this.AMSummaryTemplateMatcher = new AMSummaryTemplateMatcher();

            this._version = version;

            StartServer(currentServerPort);
            LaunchFAtiMA();
		}


        ~FAtiMAConnector()
        {
			this.OnDestroy();
        }


        // Henrique Campos - changed this in order to work on multiple OS and to receive ports from elsewhere
        public void LaunchFAtiMA()
        {

            Process proc = new Process();

            OperatingSystem osInfo = Environment.OSVersion;

            if (osInfo.Platform == PlatformID.Unix)
            {
                //LaunchUnix(args.Port, proc, args);
            }
            else if (osInfo.Platform == PlatformID.MacOSX)
            {
                //LaunchUnix(args.Port, proc, args);
            }
            else
            {
                LaunchWindows(proc);
            }



            // Redirect the standard output of the compile command.
            // Synchronously read the standard output of the spawned process.
            //string output = proc.StandardOutput.ReadToEnd();
            //if (output != null)
            //{
            //	Console.WriteLine(output);
            //}

            //output = proc.StandardError.ReadLine();
            //if (output != null)
            //{
            //	Console.WriteLine(output);
            //}




            //proc.StandardOutput.ReadLine();
            //Process.EnterDebugMode();
            //Process p2 = new Process();
            //p2.StartInfo.CreateNoWindow = true;
            //p2.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            //p2.StartInfo.UseShellExecute = true;
            //p2.StartInfo.FileName = "read";
            //p2.Start();

            ApplicationLogger.Instance().WriteLine("FAtiMA: Started Agent: ");
        }

        // Henrique Campos - made this Agent launcher for Windows 
        private void LaunchWindows(Process proc)
        {
            // For Windows:
            ApplicationLogger.Instance().WriteLine("FAtiMA: Launching Agent for Windows..");
            proc.StartInfo.FileName = "cmd";
            //proc.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
            proc.StartInfo.CreateNoWindow = false;
            //proc.StartInfo.UseShellExecute = true;


            proc.StartInfo.Arguments = "/K java -cp \"FAtiMA-Modular.jar;xmlenc-0.52.jar;gson-2.2.4.jar;sqlite-jdbc-3.7.2.jar" +
                    "\"" +
                    " FAtiMA.AgentLauncher Data/Sueca/ Scenarios-" + robotId + ".xml Sueca EMYS-" + robotId + " FAtiMA.db";

           

            proc.Start();
        }

       

        public void Start()
        {
            string agents = "User EMYS";

            this.Send("OK");
            this.receiverAlive = true;

            this.receiverThread = new Thread(new ThreadStart(ReceiveThread));
            this.receiverThread.Start();
			
			ApplicationLogger.Instance().WriteLine("sending AGENTS MESSAGE to " + this.Name + ": AGENTS " + agents);

            this.Send("AGENTS " + agents);
        }

        // MARCO: ADD BASE OnDestroy
        public void OnDestroy()
        {
			ApplicationLogger.Instance().WriteLine("AGENT ENTITY: " + this.Name + " was destroyed...");
			
            if (this.socketStream != null)
            {
                if (receiverAlive)
                {
					this.receiverAlive = false;
                    this.Send(SHUTDOWN_MESSAGE); //Makes the java mind close the socket
                }
                socketStream.Close();
            }

            if (this.socket != null)
            {
                try
                {
                    socket.Close();
                }
                catch(Exception)
                {
                }
            }
        }
		
		

        private void Parse(String msg)
        {
            ActionParameters parameters;

            if (msg.StartsWith(PROPERTY_CHANGED))
            {
                /*string[] aux = msg.Split(' ');
                string propertyName = aux[2];
                string value = aux[3];
                if(this.HasProperty<Property<String>>(propertyName))
                {
                    Property<String> p = this.GetProperty<Property<String>>(propertyName);
                    if (!p.Value.Equals(value))
                    {
                        p.Value = value;
                    }
                }*/
            }
            else if (msg.StartsWith("<EmotionalState"))
            {
                Emotion em;
               
                if (_previousEmotionalMsg.CompareTo(msg) != 0)
                {
                    _previousEmotionalMsg = msg;

                    _emotionalState = (EmotionalState)EmotionalStateParser.Instance.Parse(msg);
                    
                    em = _emotionalState.GetStrongestEmotion();
                    if (em != null && !em.Type.Equals(_previousEmotion))
                    {
                        _previousEmotion = em.Type;
                        if (ThalamusConnector.TrickActive && ThalamusConnector.GameActive && em.Intensity >= 7 && (em.Type.Equals("GLOATING") || em.Type.Equals("RESENTMENT")|| em.Type.Equals("PITTY") || em.Type.Equals("HAPPY_FOR")))
                        {
                            Console.WriteLine("PERFOMING A " + em.Type);
                            PlayExpressionWithUtterance(em);
                        }

                        Console.WriteLine("Setting posture (again) to " + em.Type.ToLower() + " with intensity of " + em.Intensity + " caused by " + em.Cause);
                        ThalamusConnector.TypifiedPublisher.SetPosture("", em.Type.ToLower());
                        Thread thread = new Thread(BackToNeutralPosture);
                        thread.Start();
                    }
                    
                   
                    

                }
            }
            else if (msg.StartsWith("look-at"))
            {
                string[] aux = msg.Split(' ');
                //LookAt(aux[1]);
                parameters = new ActionParameters();
                parameters.Subject = this.Name;
                parameters.ActionType = "look-at";
                parameters.Target = aux[1];

                LookAt(parameters.Target);
                ActionSucceeded(parameters);
				
				//Thread.Sleep(1000);
				
                //this.lookATAction.Start(parameters);
            }else if (msg.StartsWith("<Action"))
            {
                //Console.WriteLine("FAtiMA Connector - Received an action msg: " + msg);
                
                parameters = (ActionParameters) ActionParametersParser.Instance.Parse(msg);

                if(parameters.ActionType.Equals("Play"))
                {
                    string correctJson = parameters.Target.Replace(';', ',');
                    string followingInfo = parameters.Parameters[0];
                    string rank = parameters.Parameters[1];
                    string suit = parameters.Parameters[2];

                    //ThalamusConnector.TypifiedPublisher.GazeAtTarget("cardsZone");
                    List<string> possibleSubCats = new List<string>(new string[] { "FOLLOWING", "NEW_TRICK", "OURS_OURS_HIGH", "THEIRS_OURS_HIGH", "THEIRS_OURS_LOW", "THEIRS_THEIRS_HIGH" });

                    if (random.Next(100) <= 60 && possibleSubCats.Contains(followingInfo))
                    {
                        string cat = "Playing";
                        string subCat = followingInfo;
                        ThalamusConnector.RequestUtterance(cat, subCat);
                        ThalamusConnector.WaitForResponse();
                        if (ThalamusConnector.Talking)
                        {
                            ThalamusConnector.TypifiedPublisher.StartedUtterance(ThalamusConnector.ID, cat, subCat);
                            ThalamusConnector.TypifiedPublisher.PerformUtteranceFromLibrary("", "Playing", followingInfo, new string[] { "|rank|", "|suit|", "|partnerId|", "|opponentId1|", "|opponentId2|" }, new string[] { convertRankToPortuguese(rank), convertSuitToPortuguese(suit), ThalamusConnector.PartnerID.ToString(), ThalamusConnector.Opponent1ID.ToString(), ThalamusConnector.Opponent2ID.ToString() });

                        }
                    }
                    ThalamusConnector.TypifiedPublisher.Play(ThalamusConnector.ID, correctJson);
                    
                    this.ActionSucceeded(parameters);
                }
                else if (parameters.ActionType.Equals("NextPlayerAct"))
                {
                    int nextPlayerId = Int16.Parse(parameters.Parameters[0]);
                    if (ThalamusConnector.GameActive)
                    {
                        
                        string cat = "NextPlayer";
                        string subCat= "";
                        if (nextPlayerId == ThalamusConnector.ID)
                        {
                            subCat = "SELF";
                            ThalamusConnector.RequestUtterance(cat, subCat);
                            ThalamusConnector.WaitForResponse();
                            if (ThalamusConnector.Talking)
                            {
                                ThalamusConnector.TypifiedPublisher.GlanceAtTarget("ownCards");
                                ThalamusConnector.TypifiedPublisher.StartedUtterance(ThalamusConnector.ID, cat, subCat);
                                ThalamusConnector.TypifiedPublisher.PerformUtteranceFromLibrary("", cat, subCat, new string[] { "|nextPlayerId|", "|partnerId|", "|opponentId1|", "|opponentId2|" }, new string[] { nextPlayerId.ToString(), ThalamusConnector.PartnerID.ToString(), ThalamusConnector.Opponent1ID.ToString(), ThalamusConnector.Opponent2ID.ToString() });
                            }
                        }
                        else
                        {
                            if (random.Next(100) <= 60)
                            {
                                if (nextPlayerId == ThalamusConnector.PartnerID)
                                {
                                    subCat = "TEAM_PLAYER";

                                }
                                else
                                {
                                    subCat = "OPPONENT";
                                }
                                ThalamusConnector.RequestUtterance(cat, subCat);
                                ThalamusConnector.WaitForResponse();
                                if (ThalamusConnector.Talking)
                                {
                                    ThalamusConnector.TypifiedPublisher.GlanceAtTarget("ownCards");
                                    ThalamusConnector.TypifiedPublisher.StartedUtterance(ThalamusConnector.ID, cat, subCat);
                                    ThalamusConnector.TypifiedPublisher.PerformUtteranceFromLibrary("", cat, subCat, new string[] { "|nextPlayerId|", "|partnerId|", "|opponentId1|", "|opponentId2|" }, new string[] { nextPlayerId.ToString(), ThalamusConnector.PartnerID.ToString(), ThalamusConnector.Opponent1ID.ToString(), ThalamusConnector.Opponent2ID.ToString() });
                                }
                            }
                        }
                        
                        this.ActionSucceeded(parameters);
                    }
                }
            }
        }

        private string convertRankToPortuguese(string englishRank)
        {
            string portugueseRank = "";
            switch (englishRank)
            {
                case "Two":
                    portugueseRank = "um dois";
                    break;
                case "Three":
                    portugueseRank = "um tr�s";
                    break;
                case "Four":
                    portugueseRank = "um quatro";
                    break;
                case "Five":
                    portugueseRank = "um cinco";
                    break;
                case "Six":
                    portugueseRank = "um seis";
                    break;
                case "Queen":
                    portugueseRank = "uma dama";
                    break;
                case "Jack":
                    portugueseRank = "um v�l�te";
                    break;
                case "King":
                    portugueseRank = "um rei";
                    break;
                case "Seven":
                    portugueseRank = "uma manilha";
                    break;
                case "Ace":
                    portugueseRank = "um �s";
                    break;
                default:
                    break;
            }
            return portugueseRank;
        }

        private string convertSuitToPortuguese(string englishSuit)
        {
            string portugueseSuit = "";
            switch (englishSuit)
            {
                case "Clubs":
                    portugueseSuit = "paus";
                    break;
                case "Diamonds":
                    portugueseSuit = "ouros";
                    break;
                case "Hearts":
                    portugueseSuit = "copas";
                    break;
                case "Spades":
                    portugueseSuit = "espadas";
                    break;
                default:
                    break;
            }
            return portugueseSuit;
        }

        private void LookAt(string entityName)
        {
            ApplicationLogger.Instance().WriteLine(this.Name + " looks at " + entityName);
            string msg = LOOK_AT + " " + entityName + " location:DemoRoom isPerson:True displayName:" + entityName;

            if (this._version.Equals("B"))
            {
                msg += " emotionSharing:True";
            }
               
            Send(msg);
        }


        private void PlayOnlyPosture(Emotion em)
        {
            Console.WriteLine("Setting posture (only) to " + em.Type.ToLower() + " with intensity of " + em.Intensity + " caused by " + em.Cause);
            ThalamusConnector.TypifiedPublisher.SetPosture("", em.Type.ToLower());
            Thread thread = new Thread(BackToNeutralPosture);
            thread.Start();
        }


        private void PlayExpressionWithUtterance(Emotion em)
        {
            Console.WriteLine("Setting posture to " + em.Type.ToLower() + " with intensity of " + em.Intensity + " caused by " + em.Cause);
            ThalamusConnector.TypifiedPublisher.SetPosture("", em.Type.ToLower());
            Thread thread = new Thread(BackToNeutralPosture);
            thread.Start();

            int intensity = (int)Math.Ceiling(em.Intensity / 2.0f);
            
            string subcategory = em.Type;
            string eventName = em.Cause.Split(' ')[1];
            string playerId = em.Cause.Split(' ')[2];

            //if (playerId == "3")
            //{
            //    subcategory += "_SELF";
            //}
            //if (playerId != ThalamusConnector.ID.ToString())
            //{
            //    ThalamusConnector.TypifiedPublisher.PerformUtteranceFromLibrary("", "Play", subcategory, new string[] { "|intensity|, |playerId|", "|partnerId|", "|opponentId1|", "|opponentId2|" }, new string[] { intensity.ToString(), playerId, ThalamusConnector.PartnerID.ToString(), ThalamusConnector.Opponent1ID.ToString(), ThalamusConnector.Opponent2ID.ToString() });
            //}
            

            //quando recebe as suas cartas
            //ThalamusConnector.TypifiedPublisher.PlayAnimation("", "ownCardsAnalysis");
            //quando joga ou outro joga
            //ThalamusConnector.TypifiedPublisher.GazeAtTarget("cardPosition");
        }

        private void BackToNeutralPosture()
        {
            Thread.Sleep(random.Next(6000));
            ThalamusConnector.TypifiedPublisher.SetPosture("", "neutral");
        }

        private string GetExpressionText(Emotion em)
        {
            if (this._version.Equals("C")) return String.Empty;

            return this.PredefinedUtteranceSelector.GetUtteranceForEmotion(em);
        }

        private string GetExpressionAnimation(Emotion em)
        {
            switch(em.Type)
            {
                case Emotion.ANGER_EMOTION:
                    return "Anger";
                case Emotion.FEAR_EMOTION:
                    return "Fear";
                case Emotion.HOPE_EMOTION:
                    return "Joy";
                case Emotion.RESENTMENT_EMOTION:
                    return "Disgust";
                case Emotion.JOY_EMOTION:
                    return "Joy";
                case Emotion.DISTRESS_EMOTION:
                    return "Sadness";
                case Emotion.DISAPPOINTMENT_EMOTION:
                    return "Sadness";
                case Emotion.GLOATING_EMOTION:
                    return "Joy";
                case Emotion.RELIEF_EMOTION:
                    return "Joy";
                case Emotion.SATISFACTION_EMOTION:
                    return "Joy";
                case Emotion.FEARS_CONFIRMED_EMOTION:
                    return "Sadness";
                default:
                    return String.Empty;
            }
        }

        private void editScenarioFile(int port)
        {
            string newValue = string.Empty;
            XmlDocument xmlDoc = new XmlDocument();

            xmlDoc.Load("Data/Sueca/Scenarios-" + robotId + ".xml");

            XmlNode node = xmlDoc.SelectSingleNode("Scenarios/Scenario/WorldSimulator");
            node.Attributes[0].Value = port.ToString();
            node = xmlDoc.SelectSingleNode("Scenarios/Scenario/Agent");
            node.Attributes[0].Value = this.Name;
            node.Attributes[1].Value = this.Name;
            node.Attributes[5].Value = port.ToString();

            xmlDoc.Save("Data/Sueca/Scenarios-" + robotId + ".xml");
        }


        #region EventListeners

        public void ActionSucceeded(ActionParameters parameters)
        {

            ApplicationLogger.Instance().WriteLine("-- ActionEnded " + parameters.ActionType);

            if (this.receiverAlive)
            {

                Send(ACTION_FINISHED + " " + parameters.ToXML());
            }
        }

        #endregion

    #region RemoteCommunication

        private void ReceiveThread()
        {
            StreamReader socketReader = null;
            string msg = string.Empty;
            try
            {
                while (receiverAlive)
                {
                    if (socketReader == null)
                    {
                        socketReader = new StreamReader(this.socketStream, Encoding.UTF8);
                    }

                    msg = socketReader.ReadLine();

                    Parse(msg);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            } 
        }

        public void Send(String msg)
        {
            byte[] aux = Encoding.UTF8.GetBytes(msg + "\n");
            this.socketStream.Write(aux, 0, aux.Length);
            this.socketStream.Flush();
        }

        #endregion


    #region ServerConnection

        protected void StartServer(int port)
        {
            // Establish local endpoint...
            ApplicationLogger.Instance().WriteLine("FAtiMA: Creating the socket server..");
            IPAddress ipAddress = IPAddress.Any;
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);
            // Create a TCP/IP socket... (Henrique Campos - changes the AddressFamily.Unspecified to ..
            this.serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            
            // Bind the socket...
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    this.serverSocket.Bind(localEndPoint);
                    ApplicationLogger.Instance().WriteLine("FAtiMA: Succeded to bind socket on EndPoint - " + ipAddress.ToString() + ":" + port.ToString());
                    editScenarioFile(port);
                    
                    this.serverSocket.Listen(PENDING_CONNECTION_QUEUE_LENGTH);
                    // accept new connection...
                    this.serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), this.serverSocket);
                    // wait until a connection is made before continuing...
                    ApplicationLogger.Instance().WriteLine("ThalamusFAtiMA: FAtiMA-side Server ready!");
                    break;
                }
                catch (SocketException e)
                {
                    ApplicationLogger.Instance().WriteLine("FAtiMA: Failed to bind socket on EndPoint - " + ipAddress.ToString() + ":" + port.ToString() + " with Exception ErrorCode: " + e.ErrorCode);
                    port++;
                    localEndPoint = new IPEndPoint(ipAddress, port);
                }
            }
        }

        protected void AcceptCallback(IAsyncResult ar)
        {
            // get the socket handler...
            try
            {
                Socket s = (Socket) ar.AsyncState;
                this.Socket = s.EndAccept(ar);
                ApplicationLogger.Instance().WriteLine("ThalamusFAtiMA: Incoming connection from FAtiMA ...");

                // create the state object...

                StringBuilder data = new StringBuilder();
                // begin receiving the connection request
                this.Socket.BeginReceive(this.buffer, 0, bufferSize, 0,
                    new AsyncCallback(ReceiveCallback), data);

                //now that we received the connection, we can stop the serversocket
                ApplicationLogger.Instance().WriteLine("ThalamusFAtiMA: Shuting Down FAtiMA-side Server...");
                this.serverSocket.Close();

            }
            catch (Exception e)
            {
                System.Console.WriteLine(e);
            }
        }

        protected void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                String receivedMsg = String.Empty;
                // read data from remote device...
                int bytesRead = +this.Socket.EndReceive(ar);
                if (bytesRead > 0)
                {
                    // there may be more...
                    StringBuilder data = (StringBuilder)ar.AsyncState;
                    data.Append(Encoding.UTF8.GetString(this.buffer, 0, bytesRead));
                    // check for EOM.
                    receivedMsg = data.ToString();
                    int EomIndex = receivedMsg.IndexOf(EOM_TAG);
                    if (EomIndex > -1)
                    {
                        // finished receiving...
                        receivedMsg = receivedMsg.Substring(0, EomIndex);
                        // create the corresponding character
                        ApplicationLogger.Instance().WriteLine("FILIPA index: " + EomIndex + " msg: " + receivedMsg + " name: " + this.Name + " startsWith: " + receivedMsg.StartsWith(this.Name));
                        if (receivedMsg.StartsWith(this.Name))
                        {
                            //everything is ok, the agent that connected is the right agent
                            this.Start();
                            this.ConnectionReady = true;
							//Aqui tenho de lan�ar o evento de que a mente acabou de se ligar com sucesso
                        }
                        else
                        {
                            //Serious error - someone else tried to connect to this RemoteCharacter
                            this.Socket.Close();
                            this.Socket = null;
                            //lan�ar um evento a dizer que houve merda
                        }
                    }
                    else
                    {
                        // not all data read. Read more...
                        this.Socket.BeginReceive(this.buffer, 0, bufferSize, 0,
                            new AsyncCallback(ReceiveCallback), data);
                    }
                }
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e);
            }
        }
        #endregion ServerConnection
    }
}
