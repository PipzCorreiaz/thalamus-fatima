using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ThalamusFAtiMA.Speech.AMSummary
{
    public class AMSummary
    {
        public ICollection<EventDescription> Events { get; set; }

        public AMSummary()
        {
            this.Events = new List<EventDescription>();
        }

        public float CalculateDistanceFromSummaryTemplate(AMSummary summaryTemplate)
        {
            float distance = 0;

            var listSummaryTemplates = summaryTemplate.Events.ToList();
            EventPair eventPair;

            Collection<EventPair> eventPairs = new Collection<EventPair>(); 
            //first step, determine which event from the summary template is most similar to each event
            foreach (var eventDescription in this.Events)
            {
                eventPair = this.DetermineMostSimilarEventTemplate(eventDescription, listSummaryTemplates);
                if (eventPair != null)
                {
                    eventPairs.Add(eventPair);

                    //remove the event template that is most similar to the current event, because we don't want repeated event templates
                    listSummaryTemplates.Remove(eventPair.TemplateEvent);
                }
            }

            if (eventPairs.Count > 1)
            {
                for (int i = 1; i < eventPairs.Count(); i++)
                {
                    if (eventPairs.ElementAt(i).TemplateChronologicalOrder <
                        eventPairs.ElementAt(i - 1).TemplateChronologicalOrder)
                    {
                        //strong penalization for each event with an incompatible order
                        distance += 5.0f;
                    }
                }
            }

            distance += eventPairs.Sum(pair => pair.Distance);

            //also take into consideration the difference between the number of events specified in the template and the number of events in the specified summary
            if (this.Events.Count() < summaryTemplate.Events.Count())
            {
                distance += (summaryTemplate.Events.Count() - this.Events.Count())*2.5f;
            }
            else if(this.Events.Count() > summaryTemplate.Events.Count())
            {
                distance += (this.Events.Count() - summaryTemplate.Events.Count())*5.0f;
            }

            return distance;
        }

        private EventPair DetermineMostSimilarEventTemplate(EventDescription sourceEvent, ICollection<EventDescription> eventTemplates)
        {
            if (!eventTemplates.Any())
            {
                return null;
            }

            EventPair eventPair;
            int chronologicalOrder = 1;
            var firstTemplate = eventTemplates.First();
            
            var bestEventPair = new EventPair(chronologicalOrder)
            {
                SourceEvent = sourceEvent,
                TemplateEvent = firstTemplate,
                Distance = sourceEvent.CalculateDistanceFromTemplateEvent(firstTemplate)
            };

            foreach (var eventTemplate in eventTemplates)
            {
                eventPair = new EventPair(chronologicalOrder)
                {
                    SourceEvent = sourceEvent,
                    TemplateEvent = eventTemplate,
                    Distance = sourceEvent.CalculateDistanceFromTemplateEvent(eventTemplate)
                };

                if (eventPair.Distance < bestEventPair.Distance)
                {
                    bestEventPair = eventPair;
                }

                chronologicalOrder++;
            }

            return bestEventPair;
        }
    }
}
