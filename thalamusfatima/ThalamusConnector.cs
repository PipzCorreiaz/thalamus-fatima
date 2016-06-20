﻿using System;
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


        public ThalamusConnector(string clientName, int robotId, string character = "")
            : base(clientName, character)
        {
            NumGamesPerSession = 1;
            PlayedGames = 0;
            ID = 3; // default
            PartnerID = 1; // default
            Opponent1ID = 0; //default
            Opponent2ID = 2; // default
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
        }

        public void ForwardSessionStart(int numGames, int playerId)
        {
            SessionActive = false;
            Renounce = false;
            ID = playerId;
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
            string subCat = "GREETING";
            RequestUtterance(cat, subCat);
            WaitForResponse();
            if (Talking)
            {
                TypifiedPublisher.StartedUtterance(-1, "SessionStart", "GREETING");
                TypifiedPublisher.PerformUtteranceFromLibrary("", "SessionStart", "GREETING", new string[] {}, new string[] {});
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
                if (team0Score == 120)
                {
                    string cat = "GameEnd";
                    string subCat = "QUAD_LOST";
                    RequestUtterance(cat, subCat);
                    WaitForResponse();
                    if (Talking)
                    {
                        TypifiedPublisher.StartedUtterance(ID, "GameEnd", "QUAD_LOSS");
                        TypifiedPublisher.PerformUtteranceFromLibrary("", "GameEnd", "QUAD_LOSS", new string[] { }, new string[] { });
                    }   
                }
                else if (team1Score == 120)
                {
                    string cat = "GameEnd";
                    string subCat = "QUAD_WIN";
                    RequestUtterance(cat, subCat);
                    WaitForResponse();
                    if (Talking)
                    {
                        TypifiedPublisher.StartedUtterance(ID, "GameEnd", "QUAD_WIN");
                        TypifiedPublisher.PerformUtteranceFromLibrary("", "GameEnd", "QUAD_WIN", new string[] { }, new string[] { });
                    }
                }
                else if (team0Score > 90)
                {
                    string cat = "GameEnd";
                    string subCat = "DOUBLE_LOST";
                    RequestUtterance(cat, subCat);
                    WaitForResponse();
                    if (Talking)
                    {
                        TypifiedPublisher.StartedUtterance(ID, "GameEnd", "DOUBLE_LOSS");
                        TypifiedPublisher.PerformUtteranceFromLibrary("", "GameEnd", "DOUBLE_LOSS", new string[] { }, new string[] { });
                    }
                }
                else if (team1Score > 90)
                {
                    string cat = "GameEnd";
                    string subCat = "DOUBLE_WIN";
                    RequestUtterance(cat, subCat);
                    WaitForResponse();
                    if (Talking)
                    {
                        TypifiedPublisher.StartedUtterance(ID, "GameEnd", "DOUBLE_WIN");
                        TypifiedPublisher.PerformUtteranceFromLibrary("", "GameEnd", "DOUBLE_WIN", new string[] { }, new string[] { });
                    }
                }
                else if (team0Score > 60)
                {
                    string cat = "GameEnd";
                    string subCat = "SINGLE_LOST";
                    RequestUtterance(cat, subCat);
                    WaitForResponse();
                    if (Talking)
                    {
                        TypifiedPublisher.StartedUtterance(ID, "GameEnd", "SINGLE_LOSS");
                        TypifiedPublisher.PerformUtteranceFromLibrary("", "GameEnd", "SINGLE_LOSS", new string[] { }, new string[] { });
                    }
                }
                else if (team1Score > 60)
                {
                    string cat = "GameEnd";
                    string subCat = "SINGLE_WIN";
                    RequestUtterance(cat, subCat);
                    WaitForResponse();
                    if (Talking)
                    {
                        TypifiedPublisher.StartedUtterance(ID, "GameEnd", "SINGLE_WIN");
                        TypifiedPublisher.PerformUtteranceFromLibrary("", "GameEnd", "SINGLE_WIN", new string[] { }, new string[] { });
                    }
                }
                else
                {
                    string cat = "GameEnd";
                    string subCat = "DRAW";
                    RequestUtterance(cat, subCat);
                    WaitForResponse();
                    if (Talking)
                    {
                        TypifiedPublisher.StartedUtterance(ID, "GameEnd", "DRAW");
                        TypifiedPublisher.PerformUtteranceFromLibrary("", "GameEnd", "DRAW", new string[] { }, new string[] { });
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
                string cat = "SessionEnd";
                string subCat = "LOST";
                RequestUtterance(cat, subCat);
                WaitForResponse();
                if (Talking)
                {
                    TypifiedPublisher.StartedUtterance(ID, "SessionEnd", "LOSS");
                    TypifiedPublisher.PerformUtteranceFromLibrary("", "SessionEnd", "LOSS", new string[] { }, new string[] { });
                }
            }
            else if (team1Score > team0Score)
            {
                string cat = "SessionEnd";
                string subCat = "WIN";
                RequestUtterance(cat, subCat);
                WaitForResponse();
                if (Talking)
                {
                    TypifiedPublisher.StartedUtterance(ID, "SessionEnd", "WIN");
                    TypifiedPublisher.PerformUtteranceFromLibrary("", "SessionEnd", "WIN", new string[] { }, new string[] { });
                }
            }
            else
            {
                string cat = "SessionEnd";
                string subCat = "DRAW";
                RequestUtterance(cat, subCat);
                WaitForResponse();
                if (Talking)
                {
                    TypifiedPublisher.StartedUtterance(ID, "SessionEnd", "DRAW");
                    TypifiedPublisher.PerformUtteranceFromLibrary("", "SessionEnd", "DRAW", new string[] { }, new string[] { });
                }
            }
        }

        public void ForwardShuffle(int playerId)
        {
            while (!SessionActive)
            { }

            if (PlayedGames != 0)
            {
                if (playerId == ID)
                {
                    int playerId1 = (ID + 2) % 4; //team player
                    TypifiedPublisher.GazeAtTarget("player" + playerId1);
                    if (random.Next(100) <= 100)
                    {
                        int playerId2 = random.Next(0, 4);
                        while (playerId2 == ID && playerId2 == playerId1) //choose someone besides me and my partner
                        {
                            playerId1 = random.Next(0, 4);
                        }
                        string cat = "Shuffle";
                        string subCat = "SELF";
                        RequestUtterance(cat, subCat);
                        WaitForResponse();
                        if (Talking)
                        {
                            TypifiedPublisher.StartedUtterance(ID, "Shuffle", "SELF");
                            TypifiedPublisher.PerformUtteranceFromLibrary("", "Shuffle", "SELF", new string[] { "|playerId1|", "|playerId2|" }, new string[] { playerId1.ToString(), playerId2.ToString() });
                        }
                    }
                }
                else
                {
                    TypifiedPublisher.GazeAtTarget("player" + playerId);
                    if (random.Next(100) <= 100)
                    {
                        int playerId1 = playerId;
                        string cat = "Shuffle";
                        string subCat = "OTHER";
                        RequestUtterance(cat, subCat);
                        WaitForResponse();
                        if (Talking)
                        {
                            TypifiedPublisher.StartedUtterance(ID, "Shuffle", "OTHER");
                            TypifiedPublisher.PerformUtteranceFromLibrary("", "Shuffle", "OTHER", new string[] { "|playerId1|" }, new string[] { playerId1.ToString() });
                        }
                    }
                }    
            }
        }

        public void ForwardCut(int playerId)
        {
            if (playerId == ID)
            {
                int playerId1 = (ID + 2) % 4; //team player
                TypifiedPublisher.GazeAtTarget("player" + playerId1);
                if (random.Next(100) <= 100)
                {
                    
                    int playerId2 = random.Next(0, 4);
                    while (playerId2 == ID && playerId2 == playerId1) //choose someone besides me and my partner
                    {
                        playerId1 = random.Next(0, 4);
                    }
                    string cat = "Cut";
                    string subCat = "SELF";
                    RequestUtterance(cat, subCat);
                    WaitForResponse();
                    if (Talking)
                    {
                        TypifiedPublisher.StartedUtterance(ID, "Cut", "SELF");
                        TypifiedPublisher.PerformUtteranceFromLibrary("", "Cut", "SELF", new string[] { "|playerId1|", "|playerId2|" }, new string[] { playerId1.ToString(), playerId2.ToString() });
                    }
                }
            }
            else
            {
                TypifiedPublisher.GazeAtTarget("player" + playerId);
                if (random.Next(100) <= 100)
                {
                    int playerId1 = playerId;
                    string cat = "Cut";
                    string subCat = "OTHER";
                    RequestUtterance(cat, subCat);
                    WaitForResponse();
                    if (Talking)
                    {
                        TypifiedPublisher.StartedUtterance(ID, "Cut", "OTHER");
                        TypifiedPublisher.PerformUtteranceFromLibrary("", "Cut", "OTHER", new string[] { "|playerId1|" }, new string[] { playerId1.ToString() });
                    }
                }
            }
        }

        public void ForwardDeal(int playerId)
        {
            if (playerId == ID)
            {
                int playerId1 = (ID + 2) % 4; //team player
                TypifiedPublisher.GazeAtTarget("player" + playerId1);
                if (random.Next(100) <= 100)
                {
                    string cat = "Deal";
                    string subCat = "SELF";
                    RequestUtterance(cat, subCat);
                    WaitForResponse();
                    if (Talking)
                    {
                        TypifiedPublisher.StartedUtterance(ID, "Deal", "SELF");
                        TypifiedPublisher.PerformUtteranceFromLibrary("", "Deal", "SELF", new string[] { "|playerId1|" }, new string[] { playerId1.ToString() });
                    }
                }
            }
            else
            {
                TypifiedPublisher.GazeAtTarget("player" + playerId);
                if (random.Next(100) <= 100)
                {
                    int playerId2 = (ID + 2) % 4; //team player
                    string cat = "Deal";
                    string subCat = "OTHER";
                    RequestUtterance(cat, subCat);
                    WaitForResponse();
                    if (Talking)
                    {
                        TypifiedPublisher.StartedUtterance(ID, "Deal", "OTHER");
                        TypifiedPublisher.PerformUtteranceFromLibrary("", "Deal", "OTHER", new string[] { "|playerId1|", "|playerId2|" }, new string[] { playerId.ToString(), playerId2.ToString() });
                    }
                }
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

            if (playerId == ID)
            {
                if (ace)
                {
                    string cat = "TrumpCard";
                    string subCat = "SELF_ACE";
                    RequestUtterance(cat, subCat);
                    WaitForResponse();
                    if (Talking)
                    {
                        TypifiedPublisher.StartedUtterance(ID, "TrumpCard", "SELF_ACE");
                        TypifiedPublisher.PerformUtteranceFromLibrary("", "TrumpCard", "SELF_ACE", new string[] { "|partnerID|", "|opponent1Id|", "|opponent2Id|" }, new string[] { partnerId.ToString(), opponent1Id.ToString(), opponent2Id.ToString() });
                    }
                }
                else if (seven)
                {
                    string cat = "TrumpCard";
                    string subCat = "SELF_SEVEN";
                    RequestUtterance(cat, subCat);
                    WaitForResponse();
                    if (Talking)
                    {
                        TypifiedPublisher.StartedUtterance(ID, "TrumpCard", "SELF_SEVEN");
                        TypifiedPublisher.PerformUtteranceFromLibrary("", "TrumpCard", "SELF_SEVEN", new string[] { "|partnerID|", "|opponent1Id|", "|opponent2Id|" }, new string[] { partnerId.ToString(), opponent1Id.ToString(), opponent2Id.ToString() });
                    }
                }
                else if (two)
                {
                    string cat = "TrumpCard";
                    string subCat = "SELF_TWO";
                    RequestUtterance(cat, subCat);
                    WaitForResponse();
                    if (Talking)
                    {
                        TypifiedPublisher.StartedUtterance(ID, "TrumpCard", "SELF_TWO");
                        TypifiedPublisher.PerformUtteranceFromLibrary("", "TrumpCard", "SELF_TWO", new string[] { "|partnerID|", "|opponent1Id|", "|opponent2Id|" }, new string[] { partnerId.ToString(), opponent1Id.ToString(), opponent2Id.ToString() });
                    }
                }
            }
            else if (playerId == partnerId)
            {
                if (ace)
                {
                    string cat = "TrumpCard";
                    string subCat = "PARTNER_ACE";
                    RequestUtterance(cat, subCat); WaitForResponse();

                    if (Talking)
                    {
                        TypifiedPublisher.StartedUtterance(ID, "TrumpCard", "PARTNER_ACE");
                        TypifiedPublisher.PerformUtteranceFromLibrary("", "TrumpCard", "PARTNER_ACE", new string[] { "|partnerID|", "|opponent1Id|", "|opponent2Id|" }, new string[] { partnerId.ToString(), opponent1Id.ToString(), opponent2Id.ToString() });
                    }
                }
                else if (seven)
                {
                    string cat = "TrumpCard";
                    string subCat = "PARTNER_SEVEN";
                    RequestUtterance(cat, subCat);
                    WaitForResponse();
                    if (Talking)
                    {
                        TypifiedPublisher.StartedUtterance(ID, "TrumpCard", "PARTNER_SEVEN");
                        TypifiedPublisher.PerformUtteranceFromLibrary("", "TrumpCard", "PARTNER_SEVEN", new string[] { "|partnerID|", "|opponent1Id|", "|opponent2Id|" }, new string[] { partnerId.ToString(), opponent1Id.ToString(), opponent2Id.ToString() });
                    }
                }
                else if (two)
                {
                    string cat = "TrumpCard";
                    string subCat = "PARTNER_TWO";
                    RequestUtterance(cat, subCat);
                    WaitForResponse();
                    if (Talking)
                    {
                        TypifiedPublisher.StartedUtterance(ID, "TrumpCard", "PARTNER_TWO");
                        TypifiedPublisher.PerformUtteranceFromLibrary("", "TrumpCard", "PARTNER_TWO", new string[] { "|partnerID|", "|opponent1Id|", "|opponent2Id|" }, new string[] { partnerId.ToString(), opponent1Id.ToString(), opponent2Id.ToString() });
                    }
                }

            }
            else
            {
                if (ace)
                {
                    string cat = "TrumpCard";
                    string subCat = "OPPONENT_ACE";
                    RequestUtterance(cat, subCat);
                    WaitForResponse();
                    if (Talking)
                    {
                        TypifiedPublisher.StartedUtterance(ID, "TrumpCard", "OPPONENT_ACE");
                        TypifiedPublisher.PerformUtteranceFromLibrary("", "TrumpCard", "OPPONENT_ACE", new string[] { "|partnerID|", "|opponent1Id|", "|opponent2Id|" }, new string[] { partnerId.ToString(), opponent1Id.ToString(), opponent2Id.ToString() });
                    }
                }
                else if (seven)
                {
                    string cat = "TrumpCard";
                    string subCat = "OPPONENT_SEVEN";
                    RequestUtterance(cat, subCat);
                    WaitForResponse();
                    if (Talking)
                    {
                        TypifiedPublisher.StartedUtterance(ID, "TrumpCard", "OPPONENT_SEVEN");
                        TypifiedPublisher.PerformUtteranceFromLibrary("", "TrumpCard", "OPPONENT_SEVEN", new string[] { "|partnerID|", "|opponent1Id|", "|opponent2Id|" }, new string[] { partnerId.ToString(), opponent1Id.ToString(), opponent2Id.ToString() });
                    }
                }
                else if (two)
                {
                    string cat = "TrumpCard";
                    string subCat = "OPPONENT_TWO";
                    RequestUtterance(cat, subCat);
                    WaitForResponse();
                    if (Talking)
                    {
                        TypifiedPublisher.StartedUtterance(ID, "TrumpCard", "OPPONENT_TWO");
                        TypifiedPublisher.PerformUtteranceFromLibrary("", "TrumpCard", "OPPONENT_TWO", new string[] { "|partnerID|", "|opponent1Id|", "|opponent2Id|" }, new string[] { partnerId.ToString(), opponent1Id.ToString(), opponent2Id.ToString() });
                    }
                }
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
                    TypifiedPublisher.StartedUtterance(ID, "ReceiveCards", "SELF");
                    TypifiedPublisher.PerformUtteranceFromLibrary("", "ReceiveCards", "SELF", new string[] { "|playerId1|", "|playerId2|" }, new string[] { playerId1.ToString(), playerId2.ToString() });
                }
            }
        }

        public void ForwardNextPlayer(int id)
        {
            TrickActive = true;

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
            //TypifiedPublisher.GlanceAtTarget("cards3");
            
        }

        public void ForwardTrickEnd(int winnerId, int trickPoints)
        {
            string points = "", category = "TrickEnd", subcategory = "";
            TrickActive = false;

            Thread.Sleep(1500);
            if ((winnerId == ID || winnerId == (ID + 2) % 4) && currentPoints <= 60 && currentPoints + trickPoints > 60)
            {
                subcategory = "WIN";
                currentPoints += trickPoints;
            }
            else
            {
                if (winnerId == ID || winnerId == (ID + 2) % 4)
                {
                    subcategory = "OURS_";
                    currentPoints += trickPoints;
                    currentPoints += trickPoints;
                }
                else
                {
                    subcategory = "THEIRS_";
                }

                if (trickPoints == 0)
                {
                    subcategory += "ZERO";
                    points = "palha";
                }
                else if (trickPoints < 10)
                {
                    subcategory += "LOW";
                    points = trickPoints.ToString() + " pontos";
                }
                else
                {
                    subcategory += "HIGH";
                    points = trickPoints.ToString() + " pontos";
                }
            }

            TypifiedPublisher.StartedUtterance(ID, category, subcategory);
            TypifiedPublisher.PerformUtteranceFromLibrary("", category, subcategory, new string[] { "|playerId|", "|trickPoints|", "|partnerId|", "|opponentId1|", "|opponentId2|" }, new string[] { winnerId.ToString(), points, PartnerID.ToString(), Opponent1ID.ToString(), Opponent2ID.ToString() });
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
            if (playerId == ID)
            {
                Console.WriteLine("Bot has just renounced!!!  WHAT?!");
                TypifiedPublisher.PlayAnimation("", "surprise3");
            }
            else if (playerId == (ID + 2) % 4)
            {
                string cat = "GameEnd";
                string subCat = "TEAM_CHEAT";
                RequestUtterance(cat, subCat);
                WaitForResponse();
                if (Talking)
                {
                    TypifiedPublisher.StartedUtterance(ID, "GameEnd", "TEAM_CHEAT");
                    TypifiedPublisher.PerformUtteranceFromLibrary("", "GameEnd", "TEAM_CHEAT", new string[] { "|playerId|" }, new string[] { playerId.ToString() });
                }
            }
            else
            {
                string cat = "GameEnd";
                string subCat = "OTHER_CHEAT";
                RequestUtterance(cat, subCat);
                WaitForResponse();
                if (Talking)
                {
                    TypifiedPublisher.StartedUtterance(ID, "GameEnd", "OTHER_CHEAT");
                    TypifiedPublisher.PerformUtteranceFromLibrary("", "GameEnd", "OTHER_CHEAT", new string[] { "|playerId|" }, new string[] { playerId.ToString() });
                }
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
                TypifiedPublisher.StartedUtterance(ID, "ResetTrick", "AGREE");
                TypifiedPublisher.PerformUtteranceFromLibrary("", "ResetTrick", "AGREE", new string[] { }, new string[] { });
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
            PendingRequest = true;
            pendingCategory = category;
            pendingSubcategory = subcategory;
            requestCounter++;
            TypifiedPublisher.RequestUtterance(ID, category, subcategory);

        }

        private void retryRequest()
        {
            requestCounter++;
            RequestUtterance(pendingCategory, pendingSubcategory);
        }
    }
}
