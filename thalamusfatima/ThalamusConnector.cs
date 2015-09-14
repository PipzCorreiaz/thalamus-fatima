using System;
using Thalamus;
using Thalamus.BML;
using SuecaMessages;
using ThalamusFAtiMA.Actions;
using ThalamusFAtiMA.Utils;
using EmoteCommonMessages;


namespace ThalamusFAtiMA
{
    public class ThalamusConnector : ThalamusClient, IIAActions, ISuecaPerceptions, IFMLSpeechEvents //, ISpeakEvents
    {
        private int myIdOnUnity;
        private Random random;
        public IThalamusFAtiMAPublisher TypifiedPublisher {  get;  private set; }
        public FAtiMAConnector FAtiMAConnector { private get; set; }
        public int NumGamesPerSession;
        public int PlayedGames;
        
        
        public ThalamusConnector(string clientName, string character = "") : base(clientName)
        {
            NumGamesPerSession = 1;
            PlayedGames = 0;
            myIdOnUnity = 3; // default
            random = new Random(Guid.NewGuid().GetHashCode());
            SetPublisher<IThalamusFAtiMAPublisher>();
            TypifiedPublisher = new ThalamusFAtiMAPublisher(Publisher);
        } 

        public override void Dispose()
        {
            base.Dispose();
        }

        void IIAActions.Decision(string card, string rank, string suit, string followingInfo)
        {
            FAtiMAConnector.Send("SUECA PLAY " + followingInfo + " " + rank + " " + suit + " " + card);
        }

        void IIAActions.MoveExpectations(int playerId, string desirability, string desirabilityForOther, string successProbability, string failureProbability)
        {
            ActionParameters param = new ActionParameters();
            param.Subject = "User" + playerId;
            param.ActionType = "MoveExpectations";
            param.Target = playerId.ToString();
            param.Parameters.Add(desirability);
            param.Parameters.Add(desirabilityForOther);
            param.Parameters.Add(successProbability);
            param.Parameters.Add(failureProbability);
            FAtiMAConnector.ActionSucceeded(param);
        }

        public void SessionStart(int numGames)
        {
            NumGamesPerSession = numGames;
            PlayedGames = 0;
            ActionParameters param = new ActionParameters();
            param.Subject = "GUI";
            param.ActionType = "SessionStart";
            param.Parameters.Add("1");
            param.Parameters.Add("0");
            FAtiMAConnector.ActionSucceeded(param);

            TypifiedPublisher.PerformUtteranceFromLibrary("", "SessionStart", "GREETING", new string[] {}, new string[] {});
        }

        public void GameStart(int gameId, int playerId, int teamId, string trump, string[] cards)
        {
            myIdOnUnity = playerId;

            ActionParameters param = new ActionParameters();
            param.Subject = "GUI";
            param.ActionType = "GameStart";
            param.Parameters.Add(teamId.ToString());
            if (teamId == 0)
            {
                param.Parameters.Add("1");
            }
            else
            {
                param.Parameters.Add("0");
            }
            FAtiMAConnector.ActionSucceeded(param);
        }

        public void GameEnd(int team0Score, int team1Score)
        {
            if (team0Score != 60)
            {
                PlayedGames++;
            }
            ActionParameters param = new ActionParameters();
            param.Subject = "GUI";
            param.ActionType = "GameEnd";
            if (team1Score >= team0Score)
            {

                param.Target = "1";
            }
            else
            {
                param.Target = "0";
            }

            FAtiMAConnector.ActionSucceeded(param);
            if (PlayedGames < NumGamesPerSession)
            {
                if (team0Score == -1)
                {
                    TypifiedPublisher.PerformUtteranceFromLibrary("", "GameEnd", "OTHER_CHEAT", new string[] { }, new string[] { });
                }
                if (team1Score == -1)
                {
                    TypifiedPublisher.PerformUtteranceFromLibrary("", "GameEnd", "TEAM_CHEAT", new string[] { }, new string[] { });
                }
                if (team0Score == 120)
                {
                    TypifiedPublisher.PerformUtteranceFromLibrary("", "GameEnd", "QUAD_LOST", new string[] { }, new string[] { });
                }
                if (team1Score == 120)
                {
                    TypifiedPublisher.PerformUtteranceFromLibrary("", "GameEnd", "QUAD_WIN", new string[] { }, new string[] { });
                }
                if (team0Score > 90)
                {
                    TypifiedPublisher.PerformUtteranceFromLibrary("", "GameEnd", "DOUBLE_LOST", new string[] { }, new string[] { });
                }
                if (team1Score > 90)
                {
                    TypifiedPublisher.PerformUtteranceFromLibrary("", "GameEnd", "DOUBLE_WIN", new string[] { }, new string[] { });
                }
                if (team0Score > 60)
                {
                    TypifiedPublisher.PerformUtteranceFromLibrary("", "GameEnd", "SINGLE_LOST", new string[] { }, new string[] { });
                }
                if (team1Score > 60)
                {
                    TypifiedPublisher.PerformUtteranceFromLibrary("", "GameEnd", "SINGLE_WIN", new string[] { }, new string[] { });
                }
                else
                {
                    TypifiedPublisher.PerformUtteranceFromLibrary("", "GameEnd", "DRAW", new string[] { }, new string[] { });
                }
            }
        }

        public void SessionEnd(int team0Score, int team1Score)
        {
            ActionParameters param = new ActionParameters();
            param.Subject = "GUI";
            param.ActionType = "SessionEnd";
            if (team0Score > team1Score)
            {
                param.Target = "0";
            }
            else
            {
                param.Target = "1";
            }
            FAtiMAConnector.ActionSucceeded(param);
            
            if (team0Score > team1Score)
            {
                TypifiedPublisher.PerformUtteranceFromLibrary("", "SessionEnd", "LOST", new string[] { }, new string[] { });
            }
            else if (team1Score > team0Score)
            {
                TypifiedPublisher.PerformUtteranceFromLibrary("", "SessionEnd", "WIN", new string[] { }, new string[] { });
            }
            else
            {
                TypifiedPublisher.PerformUtteranceFromLibrary("", "SessionEnd", "DRAW", new string[] { }, new string[] { });
            }
        }

        public void Shuffle(int playerId)
        {
            if (playerId == myIdOnUnity)
            {
                int playerId1 = (myIdOnUnity + 2) % 4; //team player
                TypifiedPublisher.GazeAtTarget("player" + playerId1);
                if (random.Next(100) <= 30)
                {
                    int playerId2 = random.Next(0, 4);
                    while (playerId2 == myIdOnUnity && playerId2 == playerId1) //choose someone besides me and my partner
                    {
                        playerId1 = random.Next(0, 4);
                    }
                    TypifiedPublisher.PerformUtteranceFromLibrary("", "Shuffle", "SELF", new string[] { "|playerId1|", "|playerId2|" }, new string[] { playerId1.ToString(), playerId2.ToString() });
                }
            }
            else
            {
                TypifiedPublisher.GazeAtTarget("player" + playerId);
                if (random.Next(100) <= 30)
                {
                    int playerId1 = playerId;
                    TypifiedPublisher.PerformUtteranceFromLibrary("", "Shuffle", "OTHER", new string[] { "|playerId1|" }, new string[] { playerId1.ToString() });
                }
            }
        }

        public void Cut(int playerId)
        {
            if (playerId == myIdOnUnity)
            {
                int playerId1 = (myIdOnUnity + 2) % 4; //team player
                TypifiedPublisher.GazeAtTarget("player" + playerId1);
                if (random.Next(100) <= 30)
                {
                    
                    int playerId2 = random.Next(0, 4);
                    while (playerId2 == myIdOnUnity && playerId2 == playerId1) //choose someone besides me and my partner
                    {
                        playerId1 = random.Next(0, 4);
                    }
                    TypifiedPublisher.PerformUtteranceFromLibrary("", "Cut", "SELF", new string[] { "|playerId1|", "|playerId2|" }, new string[] { playerId1.ToString(), playerId2.ToString() });
                }
            }
            else
            {
                TypifiedPublisher.GazeAtTarget("player" + playerId);
                if (random.Next(100) <= 30)
                {
                    int playerId1 = playerId;
                    TypifiedPublisher.PerformUtteranceFromLibrary("", "Cut", "OTHER", new string[] { "|playerId1|" }, new string[] { playerId1.ToString() });
                }
            }
        }

        public void Deal(int playerId)
        {
            if (playerId == myIdOnUnity)
            {
                int playerId1 = (myIdOnUnity + 2) % 4; //team player
                TypifiedPublisher.GazeAtTarget("player" + playerId1);
                if (random.Next(100) <= 30)
                {
                    TypifiedPublisher.PerformUtteranceFromLibrary("", "Deal", "SELF", new string[] { "|playerId1|" }, new string[] { playerId1.ToString() });
                }
            }
            else
            {
                TypifiedPublisher.GazeAtTarget("player" + playerId);
                if (random.Next(100) <= 20)
                {
                    int playerId2 = (myIdOnUnity + 2) % 4; //team player
                    TypifiedPublisher.PerformUtteranceFromLibrary("", "Deal", "OTHER", new string[] { "|playerId1|", "|playerId2|" }, new string[] { playerId.ToString(), playerId2.ToString() });
                }

            }
        }

        public void ReceiveRobotCards()
        {
            TypifiedPublisher.GazeAtTarget("cardPosition");

            int playerId1 = random.Next(0, 4);
            int playerId2 = random.Next(0, 4);
            while (playerId1 == myIdOnUnity)
            {
                playerId1 = random.Next(0, 4);
            }
            while (playerId2 == myIdOnUnity)
            {
                playerId2 = random.Next(0, 4);
            }

            TypifiedPublisher.PerformUtteranceFromLibrary("", "ReceiveCards", "SELF", new string[] { "|playerId1|", "|playerId2|" }, new string[] { playerId1.ToString(), playerId2.ToString() });
        }

        public void NextPlayer(int id)
        {
            if (id == myIdOnUnity)
            {
                TypifiedPublisher.GazeAtTarget("cards3");
                TypifiedPublisher.PlayAnimation("", "ownCardsAnalysis");
                if (random.Next(100) <= 40)
                {
                    TypifiedPublisher.PerformUtteranceFromLibrary("", "NextPlayer", "SELF", new string[] { }, new string[] { });
                    TypifiedPublisher.GazeAtTarget("cardPosition");
                }
            }
            else if (id == (myIdOnUnity + 2) % 4)
            {
                TypifiedPublisher.GazeAtTarget("player" + id);
                if (random.Next(100) <= 40)
                {
                    TypifiedPublisher.PerformUtteranceFromLibrary("", "NextPlayer", "TEAM_PLAYER", new string[] { }, new string[] { });
                    TypifiedPublisher.GazeAtTarget("cardPosition");
                }
            }
            else
            {
                TypifiedPublisher.GazeAtTarget("player" + id);
                if (random.Next(100) <= 40)
                {
                    TypifiedPublisher.PerformUtteranceFromLibrary("", "NextPlayer", "OPPONENT", new string[] { }, new string[] { });
                    TypifiedPublisher.GazeAtTarget("cardPosition");
                }
            }
        }

        public void Play(int id, string card)
        {
            TypifiedPublisher.GazeAtTarget("cardPosition");
        }

        void IFMLSpeechEvents.UtteranceFinished(string id)
        {
            //Console.WriteLine("FAtiMa received from Skene: " + id);

            if(FAtiMAConnector.CurrentSpeechAct != null)
            {
                if (id.Equals("Greeting"))
                {
                    FAtiMAConnector.ActionSucceeded(FAtiMAConnector.CurrentSpeechAct);
                }
                else if (id.Equals("Afterword"))
                {
                    FAtiMAConnector.ActionSucceeded(FAtiMAConnector.CurrentSpeechAct);
                }
                else
                {
                    FAtiMAConnector.ActionSucceeded(FAtiMAConnector.CurrentSpeechAct);
                }
            }
        }

        void IFMLSpeechEvents.UtteranceStarted(string id)
        {
        }

    }
}
