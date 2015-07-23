using System;
using System.Xml;
using System.Xml.Schema;
using ThalamusFAtiMA.Utils;

namespace ThalamusFAtiMA.Actions
{
    public class ActionParametersParser : XmlParser
    {
        class Singleton
        {
            internal static readonly ActionParametersParser Instance = new ActionParametersParser();

            // explicit static constructor to assure a single execution
            static Singleton()
            {
            }
        }

        private ActionParametersParser()
        {
        }

        public static ActionParametersParser Instance
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


        /*protected override void ValidationErrorHandler(object sender, ValidationEventArgs args)
        {
            // TO DO: deal with xml errors
            Console.WriteLine("Validation error: " + args.Message);
        }*/

        protected override object ParseElements(XmlDocument xml)
        {

            ActionParameters a = new ActionParameters();

            foreach (XmlNode node in xml.DocumentElement.ChildNodes)
            {
                if (node.Name.Equals("Subject"))
                {
                    a.Subject = node.InnerText;
                }
                else if (node.Name.Equals("Target"))
                {
                    a.Target = node.InnerText;
                }
                else if (node.Name.Equals("Type"))
                {
                    a.ActionType = node.InnerText;
                }
                else if (node.Name.Equals("Parameters"))
                {
                    foreach (XmlNode auxNode in node.ChildNodes)
                    {
                        a.AddParameter(auxNode.InnerText);
                    }
                }
                else if (node.Name.Equals("Camera"))
                {
                    ParseCamera(a, node);
                }
                /*else if (node.Name.Equals("Emotion"))
                {
                    a.Emotion = Emotion.ParseEmotion(node);
                }*/
            }

            return a;
        }

        private void ParseCamera(ActionParameters a, XmlNode cameraNode)
        {
            foreach (XmlNode node in cameraNode.ChildNodes)
            {
                if (node.Name.Equals("Intensity"))
                {
                    a.Intensity = node.InnerText;
                }
                else if (node.Name.Equals("CameraTarget"))
                {
                    a.CameraTarget = Int32.Parse(node.InnerText);
                }
                else if (node.Name.Equals("Camerashot"))
                {
                    a.CameraShot = node.InnerText;
                }
                else if (node.Name.Equals("CameraAngle"))
                {
                    a.CameraAngle = node.InnerText;
                }
            }
        }

        protected override void ParseElements(XmlDocument xml, object result)
        {
        }
    }
}
