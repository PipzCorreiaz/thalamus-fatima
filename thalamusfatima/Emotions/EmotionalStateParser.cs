using System;
using System.Globalization;
using System.Xml;
using System.Xml.Schema;
using ThalamusFAtiMA.Utils;

namespace ThalamusFAtiMA.Emotions
{
    public class EmotionalStateParser : XmlParser
    {
        class Singleton
        {
            internal static readonly EmotionalStateParser Instance = new EmotionalStateParser();

            // explicit static constructor to assure a single execution
            static Singleton()
            {
            }
        }

        private EmotionalStateParser()
        {
        }

        public static EmotionalStateParser Instance
        {
            get
            {
                return Singleton.Instance;
            }
        }

        protected override void XmlErrorsHandler(object sender, ValidationEventArgs args)
        {
            // TO DO: deal with xml errors
            Console.WriteLine("Validation error: " + args.Message);
        }

        protected override object ParseElements(XmlDocument xml)
        {
            EmotionalState es = new EmotionalState();
            Emotion e;

            foreach (XmlNode node in xml.DocumentElement.ChildNodes)
            {
                if (node.Name.Equals("Mood"))
                {
                    es.Mood = float.Parse(node.InnerText, CultureInfo.InvariantCulture);
                    //es.Mood = float.Parse(node.InnerText);
                }
                else if (node.Name.Equals("Emotion"))
                {
                    e = Emotion.ParseEmotion(node);
                    es.AddEmotion(e);
                }
            }

            return es;
        }

        protected override void ParseElements(XmlDocument xml, object result)
        {
			result = ParseElements(xml);
        }
    }
}