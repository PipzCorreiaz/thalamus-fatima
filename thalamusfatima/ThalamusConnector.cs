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
        public IThalamusFAtiMAPublisher TypifiedPublisher {  get;  private set; }
        public FAtiMAConnector FAtiMAConnector { private get; set; }

        public ThalamusConnector(string clientName, string character = "") : base(clientName)
        {
            //Thalamus.Environment.Instance.setDebug("messages", false);
            SetPublisher<IThalamusFAtiMAPublisher>();
            TypifiedPublisher = new ThalamusFAtiMAPublisher(Publisher);
        } 

        public override void Dispose()
        {
            base.Dispose();
        }

        void IIAActions.Decision(string card)
        {
            FAtiMAConnector.Send("SUECA PLAY " + card);
        }

        void IIAActions.ExpectedScores(int team0Score, int team1Score)
        {
            FAtiMAConnector.Send("SUECA EXPECTED_SCORES " + team0Score + " " + team1Score);
        }

        //public void SpeakStarted(string id) { }

        //public void SpeakFinished(string id)
        //{
        //    if(FAtiMAConnector.CurrentSpeechAct != null)
        //    {
        //        FAtiMAConnector.ActionSucceeded(FAtiMAConnector.CurrentSpeechAct);
        //        FAtiMAConnector.CurrentSpeechAct = null;
        //    }
        //}

        public void GameStart(int id, int teamId, string trump, string[] cards)
        {
            ActionParameters param = new ActionParameters();
            param.Subject = "EMYS";
            param.ActionType = "GameStart";
            param.Parameters.Add(teamId.ToString());
            if(teamId == 0)
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
            ActionParameters param = new ActionParameters();
            param.Subject = "EMYS";
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

        public void NextPlayer(int id)
        {
        }

        public void Play(int id, string card)
        {
            ActionParameters param = new ActionParameters();
            if (id == 1)
            {
                param.Subject = "EMYS";
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
            Console.WriteLine("FAtiMa received from Skene: " + id);

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
