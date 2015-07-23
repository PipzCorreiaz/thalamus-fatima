namespace ThalamusFAtiMA.Speech.AMSummary
{
    public class EventPair
    {
        public EventDescription TemplateEvent { get; set; }
        public EventDescription SourceEvent { get; set; }
        public float Distance { get; set; }
        public int TemplateChronologicalOrder { get; private set; }

        public EventPair(int chronologicalOrder)
        {
            this.TemplateChronologicalOrder = chronologicalOrder;
        }
    }
}
