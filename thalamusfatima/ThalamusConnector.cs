using System;
using Thalamus;
using Thalamus.BML;
using SuecaMessages;
using ThalamusFAtiMA.Actions;
using ThalamusFAtiMA.Utils;
using EmoteCommonMessages;
using System.Threading;


namespace ThalamusFAtiMA
{
    public class ThalamusConnector : ThalamusClient, IIAActions, IFMLSpeechEvents //, ISpeakEvents
    {
        private int myIdOnUnity;
        public Random random;
        public IThalamusFAtiMAPublisher TypifiedPublisher {  get;  private set; }
        public FAtiMAConnector FAtiMAConnector { private get; set; }
        public int NumGamesPerSession;
        public int PlayedGames;
        public bool GameActive;
        public bool TrickActive;
        public bool Renounce;
        
        
        public ThalamusConnector(string clientName, string character = "") : base(clientName)
        {
            NumGamesPerSession = 1;
            PlayedGames = 0;
            myIdOnUnity = 3; // default
            random = new Random(Guid.NewGuid().GetHashCode());
            SetPublisher<IThalamusFAtiMAPublisher>();
            TypifiedPublisher = new ThalamusFAtiMAPublisher(Publisher);
            GameActive = false;
            TrickActive = false;
            Renounce = false;
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
            //sTypifiedPublisher.GazeAtTarget("cardsZone");

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

        public void ForwardSessionStart(int numGames)
        {
            Renounce = false;
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

        public void ForwardGameStart(int gameId, int playerId, int teamId, string trump, string[] cards)
        {
            myIdOnUnity = playerId;
            Renounce = false;

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
            GameActive = true;
        }

        public void ForwardGameEnd(int team0Score, int team1Score)
        {
            GameActive = false;
            PlayedGames++;
            
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
            if (!Renounce && PlayedGames < NumGamesPerSession)
            {
                if (team0Score == 120)
                {
                    TypifiedPublisher.PerformUtteranceFromLibrary("", "GameEnd", "QUAD_LOST", new string[] { }, new string[] { });
                }
                else if (team1Score == 120)
                {
                    TypifiedPublisher.PerformUtteranceFromLibrary("", "GameEnd", "QUAD_WIN", new string[] { }, new string[] { });
                }
                else if (team0Score > 90)
                {
                    TypifiedPublisher.PerformUtteranceFromLibrary("", "GameEnd", "DOUBLE_LOST", new string[] { }, new string[] { });
                }
                else if (team1Score > 90)
                {
                    TypifiedPublisher.PerformUtteranceFromLibrary("", "GameEnd", "DOUBLE_WIN", new string[] { }, new string[] { });
                }
                else if (team0Score > 60)
                {
                    TypifiedPublisher.PerformUtteranceFromLibrary("", "GameEnd", "SINGLE_LOST", new string[] { }, new string[] { });
                }
                else if (team1Score > 60)
                {
                    TypifiedPublisher.PerformUtteranceFromLibrary("", "GameEnd", "SINGLE_WIN", new string[] { }, new string[] { });
                }
                else
                {
                    TypifiedPublisher.PerformUtteranceFromLibrary("", "GameEnd", "DRAW", new string[] { }, new string[] { });
                }
            }
        }

        public void ForwardSessionEnd(int team0Score, int team1Score)
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

        public void ForwardShuffle(int playerId)
        {
            if (playerId == myIdOnUnity)
            {
                int playerId1 = (myIdOnUnity + 2) % 4; //team player
                TypifiedPublisher.GazeAtTarget("player" + playerId1);
                if (random.Next(100) <= 66)
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
                if (random.Next(100) <= 66)
                {
                    int playerId1 = playerId;
                    TypifiedPublisher.PerformUtteranceFromLibrary("", "Shuffle", "OTHER", new string[] { "|playerId1|" }, new string[] { playerId1.ToString() });
                }
            }
        }

        public void ForwardCut(int playerId)
        {
            if (playerId == myIdOnUnity)
            {
                int playerId1 = (myIdOnUnity + 2) % 4; //team player
                TypifiedPublisher.GazeAtTarget("player" + playerId1);
                if (random.Next(100) <= 66)
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
                if (random.Next(100) <= 66)
                {
                    int playerId1 = playerId;
                    TypifiedPublisher.PerformUtteranceFromLibrary("", "Cut", "OTHER", new string[] { "|playerId1|" }, new string[] { playerId1.ToString() });
                }
            }
        }

        public void ForwardDeal(int playerId)
        {
            if (playerId == myIdOnUnity)
            {
                int playerId1 = (myIdOnUnity + 2) % 4; //team player
                TypifiedPublisher.GazeAtTarget("player" + playerId1);
                if (random.Next(100) <= 66)
                {
                    TypifiedPublisher.PerformUtteranceFromLibrary("", "Deal", "SELF", new string[] { "|playerId1|" }, new string[] { playerId1.ToString() });
                }
            }
            else
            {
                TypifiedPublisher.GazeAtTarget("player" + playerId);
                if (random.Next(100) <= 66)
                {
                    int playerId2 = (myIdOnUnity + 2) % 4; //team player
                    TypifiedPublisher.PerformUtteranceFromLibrary("", "Deal", "OTHER", new string[] { "|playerId1|", "|playerId2|" }, new string[] { playerId.ToString(), playerId2.ToString() });
                }

            }
        }

        public void ForwardReceiveRobotCards()
        {
            //TypifiedPublisher.GazeAtTarget("cardPosition");
            TypifiedPublisher.GazeAtTarget("cardsZone");

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

        public void ForwardNextPlayer(int id)
        {
            TrickActive = true;
            ActionParameters param = new ActionParameters();
            param.Subject = "User" + id;
            param.ActionType = "NextPlayer";
            param.Parameters.Add(id.ToString());
            if (id == myIdOnUnity)
            {
                TypifiedPublisher.GazeAtTarget("cards3");
                //TypifiedPublisher.PlayAnimation("", "ownCardsAnalysis");
                param.Target = "SELF";
            }
            else if (id == (myIdOnUnity + 2) % 4)
            {
                //TypifiedPublisher.GazeAtTarget("player" + id);
                //TypifiedPublisher.GlanceAtTarget("player" + id);
                //TypifiedPublisher.GlanceAtTarget("cards3");
                param.Target = "TEAM_PLAYER";
            }
            else
            {
                //TypifiedPublisher.GazeAtTarget("player" + id);
                //TypifiedPublisher.GlanceAtTarget("player" + id);
                param.Target = "OPPONENT";
            }
            FAtiMAConnector.ActionSucceeded(param);
            Thread.Sleep(1000);
            TypifiedPublisher.GlanceAtTarget("cards3");
            
            
        }

        public void ForwardTrickEnd(int winnerId, int trickPoints)
        {
            string points;
            TrickActive = false;

            if (trickPoints == 0)
            {
                points = "palha";
            }
            else
            {
                points = trickPoints.ToString() + " pontos";
            }

            if (winnerId == myIdOnUnity)
            {
                if (random.Next(100) <= 70)
                {
                    TypifiedPublisher.PerformUtteranceFromLibrary("", "TrickEnd", "SELF", new string[] { "|playerId|", "|trickPoints|" }, new string[] { winnerId.ToString(), points });
                }
            }
            else if (winnerId == (myIdOnUnity + 2) % 4)
            {

                if (random.Next(100) <= 70)
                {
                    TypifiedPublisher.PerformUtteranceFromLibrary("", "TrickEnd", "TEAM_PLAYER", new string[] { "|playerId|", "|trickPoints|" }, new string[] { winnerId.ToString(), points });
                }
            }
            else
            {
                if (trickPoints == 0)
                {

                    if (random.Next(100) <= 70)
                    {
                        TypifiedPublisher.PerformUtteranceFromLibrary("", "TrickEnd", "OPPONENT_ZERO", new string[] { "|playerId|", "|trickPoints|" }, new string[] { winnerId.ToString(), points });
                    }
                }
                else
                {

                    if (random.Next(100) <= 70)
                    {
                        TypifiedPublisher.PerformUtteranceFromLibrary("", "TrickEnd", "OPPONENT", new string[] { "|playerId|", "|trickPoints|" }, new string[] { winnerId.ToString(), points });
                    }
                }
            }
        }

        void IFMLSpeechEvents.UtteranceFinished(string id)
        {
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


        public void ForwardRenounce(int playerId)
        {
            Renounce = true;
            if (playerId == myIdOnUnity)
            {
                Console.WriteLine("Bot has just renounced!!!  WHAT?!");
                TypifiedPublisher.PlayAnimation("", "surprise3");
            }
            else if (playerId == (myIdOnUnity + 2) % 4)
            {
                TypifiedPublisher.PerformUtteranceFromLibrary("", "GameEnd", "TEAM_CHEAT", new string[] { "|playerId|" }, new string[] { playerId.ToString() });
            }
            else
            {
                TypifiedPublisher.PerformUtteranceFromLibrary("", "GameEnd", "OTHER_CHEAT", new string[] { "|playerId|" }, new string[] { playerId.ToString() });
            }
        }


        public void ForwardResetTrick()
        {
            TypifiedPublisher.PerformUtteranceFromLibrary("", "ResetTrick", "AGREE", new string[] { }, new string[] { });
        }
    }
}
