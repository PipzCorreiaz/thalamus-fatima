using System.Collections.Generic;

namespace ThalamusFAtiMA.Speech
{
    public class SmallTalkFactory
    {

        private static string EventDescription(string subject, string action, string target = "", ICollection<string> parameters = null, string location = "", int timeCount = 0, string timeCategory = "day")
        {
            string eventDescription ="<Event>";
            
            if(!string.IsNullOrWhiteSpace(location))
            {
                eventDescription += "<Location>"+location+"</Location>";
            }
            
            if(timeCount > 0)
            {
                eventDescription += "<Time count=\"" + timeCount + "\">" + timeCategory + "</Time>";
            }
            eventDescription += "<Subject>" + subject + "</Subject>";

            eventDescription += "<Action>" + action + "</Action><Status>Succeeded</Status>";
            
            if(!string.IsNullOrWhiteSpace(target))
            {
                eventDescription += "<Target>" + target + "</Target>";
            }

            if(parameters != null)
            {
                foreach(var s in parameters)
                {
                    eventDescription += "<Param>" + s + "</Param>";
                }
            }

            eventDescription += "</Description>";

            return eventDescription;
        }

        private static string StartAMSummary(string receiver)
        {
            return "<ABMemory><Receiver>" + receiver + "</Receiver>";
        }

        private static string CloseAmSummary()
        {
            return "</ABMemory>";
        }

        public static string GetCrashedSummary(string receiver)
        {
            string summary = StartAMSummary(receiver);
            summary += EventDescription("I", "StartGame", "Board1", new List<string>() { "Luke" }, "DemoRoom");
            summary += EventDescription("Touch Screen", "Crash");
            summary += EventDescription("I", "StopedGame");
            summary += CloseAmSummary();

            return summary;
        }

    }
}
