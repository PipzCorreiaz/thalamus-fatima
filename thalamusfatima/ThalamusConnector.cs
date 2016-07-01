using System;
using Thalamus;
using Thalamus.BML;
using SuecaMessages;
using SuecaTypes;
using ThalamusFAtiMA.Actions;
using ThalamusFAtiMA.Utils;
using EmoteCommonMessages;
using System.Threading;
using System.Diagnostics;


namespace ThalamusFAtiMA
{
    public class ThalamusConnector : ThalamusClient, IIAActions, IFMLSpeechEvents, IRobotPerceptions //, ISpeakEvents
    {
        public int ID;
        public int PartnerID;
        public int Opponent1ID;
        public int Opponent2ID;
        public Random random;
        public IThalamusFAtiMAPublisher TypifiedPublisher {  get;  private set; }
        public FAtiMAConnector FAtiMAConnector { private get; set; }
        public int NumGamesPerSession;
        public int PlayedGames;
        public bool GameActive;
        public bool SessionActive;
        public bool TrickActive;
        public bool Renounce;

        private int robotId;
        public bool PendingRequest;
        public bool Talking;
        public bool SomeoneIsTalking;
        private string pendingCategory;
        private string pendingSubcategory;
        private int requestCounter;
        public bool Retrying;
        private int currentPoints;
        private int numRobots;
        private int nextPlayerId;


        public ThalamusConnector(string clientName, int robotId, string character = "")
            : base(clientName, character)
        {
            NumGamesPerSession = 1;
            PlayedGames = 0;
            ID = 3; // default
            PartnerID = 1; // default
            Opponent1ID = 0; //default
            Opponent2ID = 2; // default
            numRobots = 1; // default
            random = new Random(Guid.NewGuid().GetHashCode());
            SetPublisher<IThalamusFAtiMAPublisher>();
            TypifiedPublisher = new ThalamusFAtiMAPublisher(Publisher);
            GameActive = false;
            SessionActive = false;
            TrickActive = false;
            Renounce = false;

            this.robotId = robotId;
            PendingRequest = false;
            Retrying = false;
            Talking = false;
            SomeoneIsTalking = false;
            requestCounter = 0;
            pendingCategory = "";
            pendingSubcategory = "";
            currentPoints = 0;
        } 

        public override void Dispose()
        {
            base.Dispose();
        }

        void IIAActions.Decision(string card, string rank, string suit, string followingInfo)
        {
            FAtiMAConnector.Send("SUECA PLAY " + followingInfo + " " + rank + " " + suit + " " + card);
        }

        void IIAActions.MoveExpectations(int playerId, string desirability, string desirabilityForOther, string successProbability, string failureProbability, string additionalInfo)
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
            param.Parameters.Add(additionalInfo);
            FAtiMAConnector.ActionSucceeded(param);

            if (playerId != ID && random.Next(100) <= 60)
            {
                string cat = "Play";
                string subCat = additionalInfo;
                int intensity = 3;
                RequestUtterance(cat, subCat);
                WaitForResponse();
                if (Talking)
                {
                    TypifiedPublisher.StartedUtterance(-1, cat, subCat);
                    TypifiedPublisher.PerformUtteranceFromLibrary("", cat, subCat, new string[] { "|intensity|", "|playerId|", "|partnerId|", "|opponentId1|", "|opponentId2|", "|nextPlayerId|" }, new string[] { intensity.ToString(), playerId.ToString(), PartnerID.ToString(), Opponent1ID.ToString(), Opponent2ID.ToString(), nextPlayerId.ToString() });
                }
            }
        }

        public void ForwardSessionStart(int sessionId, int numGames, int numRobots, int playerId)
        {
            SessionActive = false;
            Renounce = false;
            this.numRobots = numRobots;
            ID = playerId;
            Console.WriteLine("Setting ID to " + playerId + " at sessionStart");
            PartnerID = (ID + 2) % 4;
            Opponent1ID = (ID + 1) % 4;
            Opponent2ID = (ID + 3) % 4;
            NumGamesPerSession = numGames;
            PlayedGames = 0;
            ActionParameters param = new ActionParameters();
            param.Subject = "GUI";
            param.ActionType = "SessionStart";
            param.Parameters.Add("1");
            param.Parameters.Add("0");
            FAtiMAConnector.ActionSucceeded(param);

            string cat = "SessionStart";
            string subCat = "SESSION_" + sessionId;
            RequestUtterance(cat, subCat);
            WaitForResponse();
            if (Talking)
            {
                TypifiedPublisher.StartedUtterance(-1, cat, subCat);
                TypifiedPublisher.PerformUtteranceFromLibrary("", cat, subCat, new string[] { }, new string[] { });
            }
            SessionActive = true;
        }

        public void ForwardGameStart(int gameId, int playerId, int teamId, string trumpCard, int trumpCardPlayer, string[] cards)
        {
            if (playerId != ID)
            {
                Console.WriteLine("LOOOOOOOOOOL");
            }
            ID = playerId;
            Console.WriteLine("Setting ID to " + playerId + " at gameStart");
            PartnerID = (ID + 2) % 4;
            Opponent1ID = (ID + 1) % 4;
            Opponent2ID = (ID + 3) % 4;
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
            currentPoints = 0;
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
                string cat = "", subCat = "";
                if (team0Score == 120)
                {
                    cat = "GameEnd";
                    subCat = "QUAD_LOSS";
                    RequestUtterance(cat, subCat);
                    WaitForResponse();
                    if (Talking)
                    {
                        TypifiedPublisher.StartedUtterance(ID, cat, subCat);
                        TypifiedPublisher.PerformUtteranceFromLibrary("", cat, subCat, new string[] { }, new string[] { });
                    }   
                }
                else if (team1Score == 120)
                {
                    cat = "GameEnd";
                    subCat = "QUAD_WIN";
                    RequestUtterance(cat, subCat);
                    WaitForResponse();
                    if (Talking)
                    {
                        TypifiedPublisher.StartedUtterance(ID, cat, subCat);
                        TypifiedPublisher.PerformUtteranceFromLibrary("", cat, subCat, new string[] { }, new string[] { });
                    }
                }
                else if (team0Score > 90)
                {
                    cat = "GameEnd";
                    subCat = "DOUBLE_LOSS";
                    RequestUtterance(cat, subCat);
                    WaitForResponse();
                    if (Talking)
                    {
                        TypifiedPublisher.StartedUtterance(ID, cat, subCat);
                        TypifiedPublisher.PerformUtteranceFromLibrary("", cat, subCat, new string[] { }, new string[] { });
                    }
                }
                else if (team1Score > 90)
                {
                    cat = "GameEnd";
                    subCat = "DOUBLE_WIN";
                    RequestUtterance(cat, subCat);
                    WaitForResponse();
                    if (Talking)
                    {
                        TypifiedPublisher.StartedUtterance(ID, cat, subCat);
                        TypifiedPublisher.PerformUtteranceFromLibrary("", cat, subCat, new string[] { }, new string[] { });
                    }
                }
                else if (team0Score > 60)
                {
                    cat = "GameEnd";
                    subCat = "SINGLE_LOSS";
                    RequestUtterance(cat, subCat);
                    WaitForResponse();
                    if (Talking)
                    {
                        TypifiedPublisher.StartedUtterance(ID, cat, subCat);
                        TypifiedPublisher.PerformUtteranceFromLibrary("", cat, subCat, new string[] { }, new string[] { });
                    }
                }
                else if (team1Score > 60)
                {
                    cat = "GameEnd";
                    subCat = "SINGLE_WIN";
                    RequestUtterance(cat, subCat);
                    WaitForResponse();
                    if (Talking)
                    {
                        TypifiedPublisher.StartedUtterance(ID, cat, subCat);
                        TypifiedPublisher.PerformUtteranceFromLibrary("", cat, subCat, new string[] { }, new string[] { });
                    }
                }
                else
                {
                    cat = "GameEnd";
                    subCat = "DRAW";
                    RequestUtterance(cat, subCat);
                    WaitForResponse();
                    if (Talking)
                    {
                        TypifiedPublisher.StartedUtterance(ID, cat, subCat);
                        TypifiedPublisher.PerformUtteranceFromLibrary("", cat, subCat, new string[] { }, new string[] { });
                    }
                }
            }
            currentPoints = 0;
        }

        public void WaitForResponse()
        {
            Stopwatch s = new Stopwatch();
            s.Start();
            while (PendingRequest || Retrying || SomeoneIsTalking)
            {
                if (s.ElapsedMilliseconds > 3000)
                {
                    PendingRequest = false;
                    Retrying = false;
                    SomeoneIsTalking = false;
                    Talking = false;
                    pendingCategory = "";
                    pendingSubcategory = "";
                    requestCounter = 0;
                    s.Stop();
                }
            }
        }

        public void ForwardSessionEnd(int sessionId, int team0Score, int team1Score)
        {
            ActionParameters param = new ActionParameters();
            param.Subject = "GUI";
            param.ActionType = "SessionEnd";
            string cat = "", subCat = "SESSION_" + sessionId;

            if (team0Score > team1Score)
            {
                param.Target = "0";
                cat = "SessionEnd";
                subCat += "_LOSS";
            }
            else if (team1Score > team0Score)
            {
                param.Target = "1";
                cat = "SessionEnd";
                subCat += "_WIN";
            }
            else
            {
                param.Target = "1";
                cat = "SessionEnd";
                subCat += "_DRAW";
            }

            FAtiMAConnector.ActionSucceeded(param);

            RequestUtterance(cat, subCat);
            WaitForResponse();
            if (Talking)
            {
                TypifiedPublisher.StartedUtterance(ID, cat, subCat);
                TypifiedPublisher.PerformUtteranceFromLibrary("", cat, subCat, new string[] { }, new string[] { });
            }
        }

        public void ForwardShuffle(int playerId)
        {
            while (!SessionActive)
            { }

            if (PlayedGames != 0)
            {
                string cat = "", subCat = "";
                int playerId1 = (ID + 2) % 4; //team player
                int playerId2 = random.Next(0, 4);
                while (playerId2 == ID && playerId2 == playerId1) //choose someone besides me and my partner
                {
                    playerId1 = random.Next(0, 4);
                }

                if (playerId == ID)
                {
                    TypifiedPublisher.GazeAtTarget("player" + playerId1);
                    cat = "Shuffle";
                    subCat = "SELF";
                }
                else
                {
                    TypifiedPublisher.GazeAtTarget("player" + playerId);
                    cat = "Shuffle";
                    subCat = "OTHER";
                }

                RequestUtterance(cat, subCat);
                WaitForResponse();
                if (Talking)
                {
                    TypifiedPublisher.StartedUtterance(ID, cat, subCat);
                    TypifiedPublisher.PerformUtteranceFromLibrary("", cat, subCat, new string[] { "|playerId|", "|partnerId|", "|opponentId1|", "|opponentId2|", "|nextPlayerId|", "|playerId1|", "|playerId2|" }, new string[] { playerId.ToString(), PartnerID.ToString(), Opponent1ID.ToString(), Opponent2ID.ToString(), nextPlayerId.ToString(), playerId1.ToString(), playerId2.ToString() });
                }   
            }
        }

        public void ForwardCut(int playerId)
        {
            string cat = "", subCat = "";
            int playerId1 = (ID + 2) % 4; //team player
            int playerId2 = random.Next(0, 4);
            while (playerId2 == ID && playerId2 == playerId1) //choose someone besides me and my partner
            {
                playerId1 = random.Next(0, 4);
            }

            if (playerId == ID)
            {
                TypifiedPublisher.GazeAtTarget("player" + playerId1);
    
                cat = "Cut";
                subCat = "SELF";
            }
            else
            {
                TypifiedPublisher.GazeAtTarget("player" + playerId);
                cat = "Cut";
                subCat = "OTHER";
            }

            RequestUtterance(cat, subCat);
            WaitForResponse();
            if (Talking)
            {
                TypifiedPublisher.StartedUtterance(ID, cat, subCat);
                TypifiedPublisher.PerformUtteranceFromLibrary("", cat, subCat, new string[] { "|playerId|", "|partnerId|", "|opponentId1|", "|opponentId2|", "|nextPlayerId|", "|playerId1|", "|playerId2|" }, new string[] { playerId.ToString(), PartnerID.ToString(), Opponent1ID.ToString(), Opponent2ID.ToString(), nextPlayerId.ToString(), playerId1.ToString(), playerId2.ToString() });
            }
        }

        public void ForwardDeal(int playerId)
        {
            string cat = "", subCat = "";
            int playerId1 = (ID + 2) % 4; //team player
            int playerId2 = (ID + 2) % 4; //team player

            if (playerId == ID)
            {
                TypifiedPublisher.GazeAtTarget("player" + playerId1);
                cat = "Deal";
                subCat = "SELF";
            }
            else
            {
                TypifiedPublisher.GazeAtTarget("player" + playerId);
                cat = "Deal";
                subCat = "OTHER";
            }

            RequestUtterance(cat, subCat);
            WaitForResponse();
            if (Talking)
            {
                TypifiedPublisher.StartedUtterance(ID, cat, subCat);
                TypifiedPublisher.PerformUtteranceFromLibrary("", cat, subCat, new string[] { "|playerId|", "|partnerId|", "|opponentId1|", "|opponentId2|", "|nextPlayerId|" }, new string[] { playerId.ToString(), PartnerID.ToString(), Opponent1ID.ToString(), Opponent2ID.ToString(), nextPlayerId.ToString() });
            }
        }

        public void ForwardTrumpCard(string trumpCard, int playerId)
        {
            bool ace = false, seven = false, two = false;
            SuecaTypes.Card card = JsonSerializable.DeserializeFromJson<SuecaTypes.Card>(trumpCard);
            if (card.Rank == SuecaTypes.Rank.Ace)
            {
                ace = true;
            }
            else if (card.Rank == SuecaTypes.Rank.Seven)
            {
                seven = true;
            }
            else if (card.Rank == SuecaTypes.Rank.Two || card.Rank == SuecaTypes.Rank.Three)
            {
                two = true;
            }

            int partnerId = (ID + 2) % 4;
            int opponent1Id = (ID + 1) % 4;
            int opponent2Id = (ID + 3) % 4;

            TypifiedPublisher.GazeAtTarget("player" + playerId);
            string cat = "", subCat = "";

            if (playerId == ID)
            {
                if (ace)
                {
                    cat = "TrumpCard";
                    subCat = "SELF_ACE";
                }
                else if (seven)
                {
                    cat = "TrumpCard";
                    subCat = "SELF_SEVEN";
                }
                else if (two)
                {
                    cat = "TrumpCard";
                    subCat = "SELF_TWO";
                }
            }
            else if (playerId == partnerId)
            {
                if (ace)
                {
                    cat = "TrumpCard";
                    subCat = "PARTNER_ACE";
                }
                else if (seven)
                {
                    cat = "TrumpCard";
                    subCat = "PARTNER_SEVEN";
                }
                else if (two)
                {
                    cat = "TrumpCard";
                    subCat = "PARTNER_TWO";
                }

            }
            else
            {
                if (ace)
                {
                    cat = "TrumpCard";
                    subCat = "OPPONENT_ACE";
                }
                else if (seven)
                {
                    cat = "TrumpCard";
                    subCat = "OPPONENT_SEVEN";
                }
                else if (two)
                {
                    cat = "TrumpCard";
                    subCat = "OPPONENT_TWO";
                }
            }

            RequestUtterance(cat, subCat);
            WaitForResponse();
            if (Talking)
            {
                TypifiedPublisher.StartedUtterance(ID, cat, subCat);
                TypifiedPublisher.PerformUtteranceFromLibrary("", cat, subCat, new string[] { "|partnerID|", "|opponent1Id|", "|opponent2Id|" }, new string[] { partnerId.ToString(), opponent1Id.ToString(), opponent2Id.ToString() });
            }
        }

        public void ForwardReceiveRobotCards(int playerId)
        {
            //TypifiedPublisher.GazeAtTarget("cardPosition");
            TypifiedPublisher.GazeAtTarget("cardsZone");
            if (playerId == ID)
            {
                int playerId1 = random.Next(0, 4);
                int playerId2 = random.Next(0, 4);
                while (playerId1 == ID)
                {
                    playerId1 = random.Next(0, 4);
                }
                while (playerId2 == ID)
                {
                    playerId2 = random.Next(0, 4);
                }

                string cat = "ReceiveCards";
                string subCat = "SELF";
                RequestUtterance(cat, subCat);
                WaitForResponse();
                if (Talking)
                {
                    TypifiedPublisher.StartedUtterance(ID, cat, subCat);
                    TypifiedPublisher.PerformUtteranceFromLibrary("", cat, subCat, new string[] { "|playerId|", "|partnerId|", "|opponentId1|", "|opponentId2|", "|nextPlayerId|", "|playerId1|", "|playerId2|" }, new string[] { playerId.ToString(), PartnerID.ToString(), Opponent1ID.ToString(), Opponent2ID.ToString(), nextPlayerId.ToString(), playerId1.ToString(), playerId2.ToString() });
                }
            }
        }

        public void ForwardNextPlayer(int id)
        {
            TrickActive = true;
            nextPlayerId = id;
            ActionParameters param = new ActionParameters();
            param.Subject = "User" + id;
            param.ActionType = "NextPlayer";
            param.Parameters.Add(id.ToString());
            if (id == ID)
            {
                TypifiedPublisher.GazeAtTarget("cards3");
                param.Target = "SELF";
            }
            else if (id == (ID + 2) % 4)
            {
                TypifiedPublisher.GazeAtTarget("player" + id);
                param.Target = "TEAM_PLAYER";
            }
            else
            {
                TypifiedPublisher.GazeAtTarget("player" + id);
                param.Target = "OPPONENT";
            }
            FAtiMAConnector.ActionSucceeded(param);
        }

        public void ForwardTrickEnd(int winnerId, int trickPoints)
        {
            if (random.Next(100) <= 60)
            {
                string points = "", cat = "TrickEnd", subCat = "";
                TrickActive = false;


                Thread.Sleep(1500);
                if ((winnerId == ID || winnerId == (ID + 2) % 4) && currentPoints <= 60 && currentPoints + trickPoints > 60)
                {
                    subCat = "WIN";
                    currentPoints += trickPoints;
                }
                else
                {
                    if (winnerId == ID || winnerId == (ID + 2) % 4)
                    {
                        subCat = "OURS_";
                        currentPoints += trickPoints;
                        currentPoints += trickPoints;
                    }
                    else
                    {
                        subCat = "THEIRS_";
                    }

                    if (trickPoints == 0)
                    {
                        subCat += "ZERO";
                        points = "palha";
                    }
                    else if (trickPoints < 10)
                    {
                        subCat += "LOW";
                        points = trickPoints.ToString() + " pontos";
                    }
                    else
                    {
                        subCat += "HIGH";
                        points = trickPoints.ToString() + " pontos";
                    }
                }


                RequestUtterance(cat, subCat);
                WaitForResponse();
                if (Talking)
                {
                    TypifiedPublisher.StartedUtterance(ID, cat, subCat);
                    TypifiedPublisher.PerformUtteranceFromLibrary("", cat, subCat, new string[] { "|playerId|", "|trickPoints|", "|partnerId|", "|opponentId1|", "|opponentId2|" }, new string[] { winnerId.ToString(), points, PartnerID.ToString(), Opponent1ID.ToString(), Opponent2ID.ToString() });
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

            TypifiedPublisher.FinishedUtterance(ID);
            Talking = false;
        }

        void IFMLSpeechEvents.UtteranceStarted(string id)
        {
        }


        public void ForwardRenounce(int playerId)
        {
            Renounce = true;
            string cat = "", subCat = "";

            if (playerId == ID)
            {
                Console.WriteLine("Bot has just renounced!!!  WHAT?!");
                TypifiedPublisher.PlayAnimation("", "surprise3");
            }
            else if (playerId == (ID + 2) % 4)
            {
                cat = "GameEnd";
                subCat = "TEAM_CHEAT";
            }
            else
            {
                cat = "GameEnd";
                subCat = "OTHER_CHEAT";
            }

            RequestUtterance(cat, subCat);
            WaitForResponse();
            if (Talking)
            {
                TypifiedPublisher.StartedUtterance(ID, cat, subCat);
                TypifiedPublisher.PerformUtteranceFromLibrary("", cat, subCat, new string[] { "|playerId|" }, new string[] { playerId.ToString() });
            }
        }


        public void ForwardResetTrick()
        {
            string cat = "ResetTrick";
            string subCat = "AGREE";
            RequestUtterance(cat, subCat);
            WaitForResponse();
            if (Talking)
            {
                TypifiedPublisher.StartedUtterance(ID, cat, subCat);
                TypifiedPublisher.PerformUtteranceFromLibrary("", cat, subCat, new string[] { }, new string[] { });
            }
        }


        public void RequestUtterance(int playerId, string category, string subcategory)
        {
            if (playerId != ID)
            {
                if (PendingRequest || Talking)
                {
                    TypifiedPublisher.NOUtterance(ID);
                }
                else
                {
                    TypifiedPublisher.OKUtterance(ID);
                    SomeoneIsTalking = true;
                }
            }
        }


        public void OKUtterance(int playerId)
        {
            if (playerId != ID)
            {
                Talking = true;
                PendingRequest = false;
                Retrying = false;
                requestCounter = 0;
            }
        }


        public void NOUtterance(int playerId)
        {
            if (playerId != ID)
            {
                if (PendingRequest && requestCounter < 3)
                {
                    Retrying = true;
                    PendingRequest = false;
                    Thread.Sleep(random.Next(2000));
                    retryRequest();
                }
                else
                {
                    requestCounter = 0;
                    Retrying = false;
                    PendingRequest = false;
                }
            }
        }

        
        public void StartedUtterance(int playerId, string category, string subcategory)
        {
            if (playerId != ID)
            {
                //otherRobotIsTalking = true;
            }
        }


        public void FinishedUtterance(int playerId)
        {
            if (playerId != ID)
            {
                SomeoneIsTalking = false;
            }
        }

        public void RequestUtterance(string category, string subcategory)
        {
            if (numRobots > 1)
            {
                PendingRequest = true;
                pendingCategory = category;
                pendingSubcategory = subcategory;
                requestCounter++;
                TypifiedPublisher.RequestUtterance(ID, category, subcategory);
            }
            else
            {
                Talking = true;
            }

        }

        private void retryRequest()
        {
            requestCounter++;
            RequestUtterance(pendingCategory, pendingSubcategory);
        }
    }
}
