using System.Collections.Generic;

namespace ThalamusFAtiMA.Speech.AMSummary
{
    public class AMSummaryTemplate
    {
        public AMSummary Summary { get; set; }
        public ICollection<string> ValidTexts { get; set; }
    }
}
