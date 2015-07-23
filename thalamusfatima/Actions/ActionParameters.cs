using System;
using System.Collections.Generic;

namespace ThalamusFAtiMA.Actions
{
    public class ActionParameters
    {
        private string subject;
        private string actionType;
        private string target;
        private List<string> parameters;

        //camera and perspective fields
        private string intensity = null;
        private string cameraShot = null;
        private string cameraAngle = null;
        private int cameraTarget = -1;

        public ActionParameters()
        {
            parameters = new List<String>();
        }

        public string Subject
        {
            get
            {
                return this.subject;
            }
            set
            {
                this.subject = value;
            }
        }

        public string ActionType
        {
            get
            {
                return this.actionType;
            }
            set
            {
                this.actionType = value;
            }
        }

        public string Target
        {
            get
            {
                return this.target;
            }
            set
            {
                this.target = value;
            }
        }

        public string Intensity
        {
            get
            {
                return this.intensity;
            }
            set
            {
                this.intensity = value;
            }
        }

        public int CameraTarget
        {
            get 
            { 
                return this.cameraTarget; 
            }
            set 
            { 
                this.cameraTarget = value; 
            }
        }

        public String CameraShot
        {
            get { return this.cameraShot; }
            set { this.cameraShot = value; }
        }

        public String CameraAngle
        {
            get { return this.cameraAngle; }
            set { this.cameraAngle = value; }
        }


        public List<String> Parameters
        {
            get
            {
                return this.parameters;
            }
        }

        public void AddParameter(String parameter)
        {
            this.parameters.Add(parameter);
        }

        protected String CameraToXMl()
        {
            String aux;
            //camera arguments
            aux = "<Camera>";
            if (this.intensity != null)
            {
                aux = aux + "<Intensity>" + this.intensity + "</Intensity>";
            }
            if (this.cameraTarget >= 0)
            {
                aux = aux + "<CameraTarget>" + this.cameraTarget + "</CameraTarget>";
            }
            if (this.cameraShot != null)
            {
                aux = aux + "<CameraShot>" + this.cameraShot + "</CameraShot>";
            }
            if (this.cameraAngle != null)
            {
                aux = aux + "<CameraAngle>" + this.cameraAngle + "</CameraAngle>";
            }
            aux = aux + "</Camera>";

            return aux;
        }

        public virtual string ToXML()
        {
            string xmlAction;

            xmlAction = "<Action><Subject>" + this.subject + "</Subject><Type>" +
                this.actionType + "</Type>";

            if (this.target != null)
            {
                xmlAction = xmlAction + "<Target>" + this.target + "</Target>";
            }

            xmlAction = xmlAction + "<Parameters>";

            foreach (string s in this.parameters)
            {
                xmlAction = xmlAction + "<Param>" + s + "</Param>";
            }

            xmlAction = xmlAction + "</Parameters>";

            //if (this.emotion != null)
            //{
            //    xmlAction = xmlAction + this.emotion.ToXml();
            //}

            xmlAction = xmlAction + CameraToXMl();

            xmlAction += "</Action>";

            return xmlAction;
        }
    }
}
