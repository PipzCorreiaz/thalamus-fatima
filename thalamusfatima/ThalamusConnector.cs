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
        private Random random;
        public IThalamusFAtiMAPublisher TypifiedPublisher {  get;  private set; }
        public FAtiMAConnector FAtiMAConnector { private get; set; }
        
        
        public ThalamusConnector(string clientName, string character = "") : base(clientName)
        {
            random = new Random(Guid.NewGuid().GetHashCode());
            SetPublisher<IThalamusFAtiMAPublisher>();
            TypifiedPublisher = new ThalamusFAtiMAPublisher(Publisher);
        } 

        public override void Dispose()
        {
            base.Dispose();
        }

        void IIAActions.Decision(string card, string followingInfo)
        {
            FAtiMAConnector.Send("SUECA PLAY " + followingInfo + " " + card);
        }

        void IIAActions.Expectation(string successProbability, string failureProbability)
        {
            FAtiMAConnector.Send("SUECA EXPECTATION " + successProbability + " " + failureProbability);
        }

        public void MoveDesirabilities(string desirability, string desirabilityForOther)
        {
            ActionParameters param = new ActionParameters();
            param.Subject = "User";
            param.ActionType = "MoveDesirabilities";
            // param.Target = numGames.ToString();
            param.Parameters.Add(desirability);
            param.Parameters.Add(desirabilityForOther);
            FAtiMAConnector.ActionSucceeded(param);
        }

        public void SessionStart(int numGames)
        {
            ActionParameters param = new ActionParameters();
            param.Subject = "GUI";
            param.ActionType = "SessionStart";
           // param.Target = numGames.ToString();
            param.Parameters.Add("1");
            param.Parameters.Add("0");
            FAtiMAConnector.ActionSucceeded(param);

            TypifiedPublisher.PerformUtteranceFromLibrary("", "SessionStart", "GREETING", new string[] {}, new string[] {});
        }

        public void GameStart(int gameId, int playerId, int teamId, string trump, string[] cards)
        {
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

            TypifiedPublisher.PerformUtteranceFromLibrary("", "GameStart", "RECEIVE_CARDS", new string[] { }, new string[] { });
        }

        public void GameEnd(int team0Score, int team1Score)
        {
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
        }

        public void SessionEnd(int team0Score, int team1Score)
        {
            ActionParameters param = new ActionParameters();
            param.Subject = "GUI";
            param.ActionType = "SessionEnd";
            param.Target = team0Score.ToString();
            FAtiMAConnector.ActionSucceeded(param);
        }

        public void Shuffle(int playerId)
        {
            ActionParameters param = new ActionParameters();
            param.Subject = "GUI";
            param.ActionType = "Shuffle";
            param.Target = playerId.ToString();
            FAtiMAConnector.ActionSucceeded(param);

            if (playerId == 1)
            {
                //if (random.Next(100) <= 66)
                {
                    TypifiedPublisher.PerformUtteranceFromLibrary("", "Shuffle", "SELF", new string[] { }, new string[] { });
                }
            }
            else
            {
                //if (random.Next(100) <= 66)
                {
                    TypifiedPublisher.PerformUtteranceFromLibrary("", "Shuffle", "OTHER", new string[] { }, new string[] { });
                }
            }
        }

        public void Cut(int playerId)
        {
            ActionParameters param = new ActionParameters();
            param.Subject = "GUI";
            param.ActionType = "Cut";
            param.Target = playerId.ToString();
            FAtiMAConnector.ActionSucceeded(param);

            if (playerId == 1)
            {
                //if (random.Next(100) <= 40)
                {
                    TypifiedPublisher.PerformUtteranceFromLibrary("", "Cut", "SELF", new string[] { }, new string[] { });
                }
            }
            else
            {
                //if (random.Next(100) <= 40)
                {
                    TypifiedPublisher.PerformUtteranceFromLibrary("", "Cut", "OTHER", new string[] { }, new string[] { });
                }
            }
        }

        public void Deal(int playerId)
        {
            ActionParameters param = new ActionParameters();
            param.Subject = "GUI";
            param.ActionType = "Deal";
            param.Target = playerId.ToString();
            FAtiMAConnector.ActionSucceeded(param);

            if (playerId == 1)
            {
                if (random.Next(100) <= 40)
                {
                    TypifiedPublisher.PerformUtteranceFromLibrary("", "Deal", "SELF", new string[] { }, new string[] { });
                }
            }
            else
            {
                if (random.Next(100) <= 40)
                {
                    TypifiedPublisher.PerformUtteranceFromLibrary("", "Deal", "OTHER", new string[] { }, new string[] { });
                }

            }
        }

        public void NextPlayer(int id)
        {
            ActionParameters param = new ActionParameters();
            param.Subject = "GUI";
            param.ActionType = "NextPlayer";
            param.Target = id.ToString();
            FAtiMAConnector.ActionSucceeded(param);

            if (id == 1)
            {
                //if (random.Next(100) <= 40)
                {
                    TypifiedPublisher.PerformUtteranceFromLibrary("", "NextPlayer", "SELF", new string[] { }, new string[] { });
                }
            }
            else if (id == 3)
            {
                //if (random.Next(100) <= 40)
                {
                    TypifiedPublisher.PerformUtteranceFromLibrary("", "NextPlayer", "TEAM_PLAYER", new string[] { }, new string[] { });
                }
            }
            else
            {
                //if (random.Next(100) <= 40)
                {
                    TypifiedPublisher.PerformUtteranceFromLibrary("", "NextPlayer", "OPPONENT", new string[] { }, new string[] { });
                }
            }
        }

        public void Play(int id, string card)
        {
            ActionParameters param = new ActionParameters();
            if (id == 1)
            {
                param.Subject = "SELF";
            }
            else
            {
                param.Subject = "User" + id;
            }
            param.ActionType = "Play";
            SuecaTypes.Card desirializedCard = SuecaTypes.JsonSerializable.DeserializeFromJson<SuecaTypes.Card>(card);
            param.AddParameter(desirializedCard.Rank.ToString());
            param.AddParameter(desirializedCard.Suit.ToString());
            FAtiMAConnector.ActionSucceeded(param);
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
