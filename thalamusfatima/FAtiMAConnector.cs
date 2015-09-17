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
// Created by: João Dias
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

        public string Name { get; private set; }

        public ThalamusConnector ThalamusConnector { private get; set; }

        private PredefinedUtteranceSelector PredefinedUtteranceSelector { get; set; }
        private AMSummaryTemplateMatcher AMSummaryTemplateMatcher { get; set; }

        private string _version;
		
		// Henrique Campos - auxiliary variable for emotional state messages in Parse(..)
		private string _previousEmotionalMsg = String.Empty;

        private EmotionalState _emotionalState;
        private string _previousEmotion = "";

        private LanguageEngineMaster _languageEngine;
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
       
        public FAtiMAConnector(ThalamusConnector thalamus, string name, string sex, string role, string version) 
		{
            this.ThalamusConnector = thalamus;
            this.receiverAlive = false;
			this.ConnectionReady = false;
            //this.emotionalState = new Property<EmotionalState>(new EmotionalState());
            //this.relations = new Property<RelationSet>(new RelationSet());
            this.Sex = sex;
            this.Role = role;
            this.Name = name;
            this._emotionalState = new EmotionalState();
			// Henrique Campos - emotional state property

            this.PredefinedUtteranceSelector = new PredefinedUtteranceSelector(version);
            this.AMSummaryTemplateMatcher = new AMSummaryTemplateMatcher();

            this._languageEngine = new LanguageEngineMaster("M", "M", "data/sueca/language/agent/en/language-set-1", "");
            this._languageEngine.Load();

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
                    " FAtiMA.AgentLauncher Data/Sueca/ Scenarios.xml Sueca EMYS FAtiMA.db";

           

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
                        if (ThalamusConnector.GameActive && em.Intensity >= 5 && (em.Type.Equals("GLOATING") || em.Type.Equals("RESENTMENT")|| em.Type.Equals("PITTY") || em.Type.Equals("HAPPY_FOR")))
                        {
                            Console.WriteLine("PERFOMING A " + em.Type);
                            PlayExpressionWithUtterance(em);
                        }
                        ThalamusConnector.TypifiedPublisher.SetPosture("", em.Type.ToLower());
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
            }
            else if (msg.StartsWith("<SpeechAct"))
            {
                Console.WriteLine("FAtiMA Connector - Received a speech act!");

                SpeechActParameters speechParams = (SpeechActParameters)SpeechActParser.Instance.Parse(msg);
                if (speechParams.Meaning.Equals("episodesummary"))
                {
                    var amSummary = AMSummaryParser.Instance.Parse(speechParams.AMSummary) as AMSummary;
                    if (amSummary != null)
                    {
                        var summaryText = this.AMSummaryTemplateMatcher.GenerateTextForSummary(amSummary);
                        ApplicationLogger.Instance().WriteLine("Generated Summary:" + summaryText);
                        System.Console.WriteLine(this.Name + ": " + summaryText);
                        ThalamusConnector.TypifiedPublisher.PerformUtterance("", summaryText,"");
                    }
                    CurrentSpeechAct = speechParams;
                    //hack used when FAtiMA is not connected to EMYS that is going to perform the speech act
                    this.ActionSucceeded(speechParams);
                    return;
                }
                else if (speechParams.Meaning.Equals("sharepreviousinteraction"))
                {
                    CurrentSpeechAct = speechParams;
                    var utterance =
                        "You know, the other day I was playing against another player. I wanted to win, but I wasn't very hopefull. Fortunately, he made a mistake and I was able to make a nice move, which made me feel really happy.";
                    System.Console.WriteLine(this.Name + ": " + utterance);
                    ThalamusConnector.TypifiedPublisher.PerformUtterance("", utterance, "");
                    //hack used when FAtiMA is not connected to EMYS that is going to perform the speech act
                    this.ActionSucceeded(speechParams);
                    return;
                }
                else if (speechParams.Meaning.Equals("greeting"))
                {
                    CurrentSpeechAct = speechParams;
                    var utterance =
                        "Hi, I'm emys and I'm here to play with you. You are player 1, so press the button to start whenever you're ready.";
                    System.Console.WriteLine(this.Name + ": " + utterance);
                    ThalamusConnector.TypifiedPublisher.PerformUtterance("", utterance, "");
                    //hack used when FAtiMA is not connected to EMYS that is going to perform the speech act
                    this.ActionSucceeded(speechParams);
                    return;
                }
                else if (speechParams.Meaning.Equals("asktowin"))
                {
                    CurrentSpeechAct = speechParams;
                    var utterance =
                        "Could you let me win the next game, please?";
                    System.Console.WriteLine(this.Name + ": " + utterance);
                    ThalamusConnector.TypifiedPublisher.PerformUtterance("", utterance, "");
                    //hack used when FAtiMA is not connected to EMYS that is going to perform the speech act
                    this.ActionSucceeded(speechParams);
                    return;
                }
                else
                {
                    speechParams.Utterance = this._languageEngine.Say(speechParams);
                }
                CurrentSpeechAct = speechParams;
                string finalUtterance = speechParams.Utterance.Replace("Board", "Board ");
                finalUtterance = finalUtterance.Replace("&lt","<");
                finalUtterance = finalUtterance.Replace("&gt",">");
                ThalamusConnector.TypifiedPublisher.PerformUtterance("", finalUtterance, "");
            }
            else if (msg.StartsWith("<Action"))
            {
                //Console.WriteLine("FAtiMA Connector - Received an action msg: " + msg);
                
                parameters = (ActionParameters) ActionParametersParser.Instance.Parse(msg);

                if(parameters.ActionType.Equals("Play"))
                {
                    string correctJson = parameters.Target.Replace(';', ',');
                    string followingInfo = parameters.Parameters[0];
                    string rank = parameters.Parameters[1];
                    string suit = parameters.Parameters[2];


                    ThalamusConnector.TypifiedPublisher.Play(3, correctJson);
                    ThalamusConnector.TypifiedPublisher.GazeAtTarget("cardsZone");
                    ThalamusConnector.TypifiedPublisher.PerformUtteranceFromLibrary("", "Playing", followingInfo, new string[] { "|rank|", "|suit|" }, new string[] { convertRankToPortuguese(rank), convertSuitToPortuguese(suit) });
                    this.ActionSucceeded(parameters);
                }
                else if (parameters.ActionType.Equals("NextPlayerAct"))
                {
                    string utteranceSubcategory = parameters.Target;
                    string nextPlayerId = parameters.Parameters[0];
                    if (utteranceSubcategory != "EMYS")
                    {
                        ThalamusConnector.TypifiedPublisher.PerformUtteranceFromLibrary("", "NextPlayer", utteranceSubcategory, new string[] { "|nextPlayerId|" }, new string[] { nextPlayerId });
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
                    portugueseRank = "um três";
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
                    portugueseRank = "um váléte";
                    break;
                case "King":
                    portugueseRank = "um rei";
                    break;
                case "Seven":
                    portugueseRank = "uma manilha";
                    break;
                case "Ace":
                    portugueseRank = "um ás";
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
            ThalamusConnector.TypifiedPublisher.SetPosture("", em.Type.ToLower());
        }


        private void PlayExpressionWithUtterance(Emotion em)
        {
            ThalamusConnector.TypifiedPublisher.SetPosture("", em.Type.ToLower());

            int intensity = (int)Math.Ceiling(em.Intensity / 2.0f);
            
            string subcategory = em.Type;
            string eventName = em.Cause.Split(' ')[1];
            string playerId = em.Cause.Split(' ')[2];

            if (playerId == "3")
            {
                subcategory += "_SELF";
            }
            ThalamusConnector.TypifiedPublisher.PerformUtteranceFromLibrary("", "Play", subcategory, new string[] { "|intensity|, |playerId|" }, new string[] { intensity.ToString(), playerId });
            

            //quando recebe as suas cartas
            //ThalamusConnector.TypifiedPublisher.PlayAnimation("", "ownCardsAnalysis");
            //quando joga ou outro joga
            //ThalamusConnector.TypifiedPublisher.GazeAtTarget("cardPosition");
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
            this.serverSocket.Bind(localEndPoint);
            this.serverSocket.Listen(PENDING_CONNECTION_QUEUE_LENGTH);

            // accept new connection...
            this.serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), this.serverSocket);
            // wait until a connection is made before continuing...
            ApplicationLogger.Instance().WriteLine("ThalamusFAtiMA: FAtiMA-side Server ready!");
        }

        protected void AcceptCallback(IAsyncResult ar)
        {
            // get the socket handler...
            try
            {
                this.Socket = ((Socket)ar.AsyncState).EndAccept(ar);
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
                        if (receivedMsg.StartsWith(this.Name))
                        {
                            //everything is ok, the agent that connected is the right agent
                            this.Start();
                            this.ConnectionReady = true;
							//Aqui tenho de lançar o evento de que a mente acabou de se ligar com sucesso
                        }
                        else
                        {
                            //Serious error - someone else tried to connect to this RemoteCharacter
                            this.Socket.Close();
                            this.Socket = null;
                            //lançar um evento a dizer que houve merda
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
