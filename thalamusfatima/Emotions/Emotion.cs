using System;
using System.Globalization;
using System.Xml;

namespace ThalamusFAtiMA.Emotions
{
    public class Emotion
    {
        public enum Valence { negative = 0, positive = 1 };

        public const string LOVE_EMOTION = "LOVE";
        public const string HATE_EMOTION = "HATE";
        public const string HOPE_EMOTION = "HOPE";
        public const string FEAR_EMOTION = "FEAR";
        public const string SATISFACTION_EMOTION = "SATISFACTION";
        public const string RELIEF_EMOTION = "RELIEF";
        public const string FEARS_CONFIRMED_EMOTION = "FEARS_CONFIRMED";
        public const string DISAPPOINTMENT_EMOTION = "DISAPPOINTMENT";
        public const string JOY_EMOTION = "JOY";
        public const string DISTRESS_EMOTION = "DISTRESS";
        public const string HAPPY_FOR_EMOTION = "HAPPY_FOR";
        public const string PITTY_EMOTION = "PITTY";
        public const string RESENTMENT_EMOTION = "RESENTMENT";
        public const string GLOATING_EMOTION = "GLOATING";
        public const string PRIDE_EMOTION = "PRIDE";
        public const string SHAME_EMOTION = "SHAME";
        public const string GRATIFICATION_EMOTION = "GRATIFICATION";
        public const string REMORSE_EMOTION = "REMORSE";
        public const string ADMIRATION_EMOTION = "ADMIRATION";
        public const string REPROACH_EMOTION = "REPROACH";
        public const string GRATITUDE_EMOTION = "GRATITUDE";
        public const string ANGER_EMOTION = "ANGER";
		public const string CONTEMPT = "CONTEMPT";
		

        public static Emotion ParseEmotion(XmlNode emotionXml)
        {
            XmlAttribute aux;
            Emotion em = new Emotion();

            aux = emotionXml.Attributes["type"];
            if (aux != null)
            {
                em.type = aux.InnerXml;
            }

            aux = emotionXml.Attributes["valence"];
            if (aux != null)
            {
				if (aux.InnerXml.ToUpper().Equals("POSITIVE"))
				{
					em.valence = Valence.positive;
				}
				else
				{
					em.valence = Valence.negative;
				}
            }

            aux = emotionXml.Attributes["cause"];
            if (aux != null)
            {
                em.cause = aux.InnerXml;
            }

            aux = emotionXml.Attributes["direction"];
            if (aux != null)
            {
                em.direction = aux.InnerXml;
            }

            aux = emotionXml.Attributes["intensity"];
            if (aux != null)
            {
                em.intensity = Convert.ToSingle(aux.InnerXml, CultureInfo.InvariantCulture);
            }

            return em;

        }

        private string type;
        private Valence valence;
        private string cause;
        private string direction;
        private float intensity;

        public Emotion()
        {
        }

        public string Type
        {
            get { return this.type; }
            set { this.type = value; }
        }

        public string Cause
        {
            get { return this.cause; }
            set { this.cause = value; }
        }

        public Valence EmotionValence
        {
            get { return this.valence; }
            set { this.valence = value; }
        }

        public string Direction
        {
            get { return this.direction; }
            set { this.direction = value; }
        }

        public float Intensity
        {
            get { return this.intensity; }
            set { this.intensity = value; }
        }

        public string ToXml()
        {
            return "<Emotion type=\"" + this.type +
                   "\" valence=\"" + this.valence +
                   "\" cause=\"" + this.cause +
                   "\" direction=\"" + this.direction +
                   "\" intensity=\"" + this.intensity + "\" />";
        }
    }
}
