using Thalamus;
using Thalamus.BML;
using SuecaMessages;
using ThalamusFAtiMA.Actions;


namespace ThalamusFAtiMA
{
    public class ThalamusConnector : ThalamusClient, IIAActions, ISpeakEvents
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
            ActionParameters param = new ActionParameters();
            param.Subject = "IA";
            param.ActionType = "IADecision";
            param.Target = card;
            FAtiMAConnector.ActionSucceeded(param);
        }

        public void SpeakStarted(string id) { }

        public void SpeakFinished(string id)
        {
            if(FAtiMAConnector.CurrentSpeechAct != null)
            {
                FAtiMAConnector.ActionSucceeded(FAtiMAConnector.CurrentSpeechAct);
                FAtiMAConnector.CurrentSpeechAct = null;
            }
        }
    }
}
