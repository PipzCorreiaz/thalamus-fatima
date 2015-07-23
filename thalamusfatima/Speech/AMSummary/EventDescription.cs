using System;
using System.Collections.Generic;

namespace ThalamusFAtiMA.Speech.AMSummary
{
    public class EventDescription
    {
        public const string STATUS_ACTIVATE = "activate";
        public const string STATUS_SUCCEED = "succeed";
        public const string STATUS_FAILED = "fail";

        private const float LOCATION_WEIGHT = 1.0f;
        private const float TIME_WEIGHT = 1.0f;
        private const float SUBJECT_WEIGHT = 1.0f;
        private const float ACTION_WEIGHT = 1.0f;
        private const float INTENTION_WEIGHT = 1.0f;
        private const float STATUS_WEIGHT = 2.0f;
        private const float TARGET_WEIGHT = 0.5f;
        private const float PARAMETER_WEIGHT = 0.2f;
        private const float EMOTION_WEIGHT = 1.0f;

        public int ChronologicalOrder { get; private set; }
        public string Location { get; set; }
        public string TimeCategory { get; set; }
        public string TimeCount { get; set; }
        public string Subject { get; set; }
        public string Status { get; set; }
        public string Action { get; set; }
        public string Intention { get; set; }
        public string Target { get; set; }
        public EmotionDescription Emotion { get; set; }
        public ICollection<string> Parameters { get; set; }

        public EventDescription(int chronologicalOrder)
        {
            this.ChronologicalOrder = chronologicalOrder;
            this.Parameters = new List<string>();
        }

        private float CalculateDistanceForTemplateProperty(string sourceProperty, string templateProperty, float propertyWeight)
        {
            float distance = 0;

            if (!String.IsNullOrEmpty(templateProperty))
            {
                if (String.IsNullOrEmpty(sourceProperty))
                {
                    distance += propertyWeight / 2;
                }
                else if (!templateProperty.Equals(sourceProperty))
                {
                    distance += propertyWeight;
                }
            }

            return distance;
        }

        private float CalculateDistanceForTemplateEmotion(EmotionDescription sourceEmotion, EmotionDescription templateEmotion)
        {
            float distance = 0;
            if (templateEmotion == null)
            {
                if (sourceEmotion != null)
                {
                    distance += EMOTION_WEIGHT/2.0f;
                }
            }
            else
            {
                if (sourceEmotion == null)
                {
                    distance += EMOTION_WEIGHT/2.0f;
                }
                else 
                {
                    if (!templateEmotion.Type.Equals(sourceEmotion.Type))
                    {
                        distance += EMOTION_WEIGHT;
                    }
                    else
                    {
                        distance += this.CalculateDistanceForTemplateProperty(sourceEmotion.Direction, templateEmotion.Direction,EMOTION_WEIGHT/2.0f);
                        distance += this.CalculateDistanceForTemplateProperty(sourceEmotion.Intensity, templateEmotion.Intensity,EMOTION_WEIGHT/4.0f);
                    }
                }
            }

            return distance;
        }

        public float CalculateDistanceFromTemplateEvent(EventDescription templateEvent)
        {
            float distance = 0;

            distance += this.CalculateDistanceForTemplateProperty(this.Location, templateEvent.Location, LOCATION_WEIGHT);
            distance += this.CalculateDistanceForTemplateProperty(this.TimeCategory, templateEvent.TimeCategory, TIME_WEIGHT);
            distance += this.CalculateDistanceForTemplateProperty(this.Subject, templateEvent.Subject, SUBJECT_WEIGHT);
            distance += this.CalculateDistanceForTemplateProperty(this.Action, templateEvent.Action, ACTION_WEIGHT);
            distance += this.CalculateDistanceForTemplateProperty(this.Intention, templateEvent.Intention, INTENTION_WEIGHT);
            distance += this.CalculateDistanceForTemplateProperty(this.Status, templateEvent.Status, STATUS_WEIGHT);
            distance += this.CalculateDistanceForTemplateProperty(this.Target, templateEvent.Target, TARGET_WEIGHT);
            distance += this.CalculateDistanceForTemplateEmotion(this.Emotion, templateEvent.Emotion);

            return distance;
        }
    }
}
