// SpeechAct.cs - 
//
// Copyright (C) 2006 GAIPS/INESC-ID
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
// 
// Company: GAIPS/INESC-ID
// Project: FearNot!
// Created: 24/09/2006
// Created by: João Dias
// Email to: joao.dias@inesc-id.pt
// 
// History:
// João Dias: 24/09/2006 - File created
//

using System.Collections.Generic;
using ThalamusFAtiMA.Actions;

namespace ThalamusFAtiMA.Speech
{
	/// <summary>
	/// Summary description for SpeechAct.
	/// </summary>
	public class SpeechActParameters : ActionParameters
	{
        private string meaning;
        private string utterance;
        private string amSummary;

        private Dictionary<string, string> contextVariables;

        public const string QUESTION = "Question";
	    public const string REPLY = "Reply";
	    public const string REINFORCE = "Reinforce";
	    public const string SPEECH = "SpeechAct";
	    public const string CONFRONTATION = "Confrontation";
	    public const string USERSPEECH = "UserSpeech";
	    //public const String ConfirmationRequest = "Confirmation";
	    //public const String CopingSuggestion = "Suggestion";
	    public const string COPINGSPEECH = "CopingSpeech";

		public SpeechActParameters() 
		{
            contextVariables = new Dictionary<string, string>();
		}

		public string Meaning
        {
            get { return this.meaning; }
            set { this.meaning = value; }
        }

        public string Utterance
        {
            get { return this.utterance; }
            set { this.utterance = value; }
        }

        public string AMSummary
        {
            get
            {
                return this.amSummary;
            }
            set
            {
                this.amSummary = value;
            }
        }

        public Dictionary<string, string> ContextVariables
        {
            get { return this.contextVariables; } 
        }

        public void AddContextVariable(string name, string value)
        {
            if(!this.contextVariables.ContainsKey(name))
            {
                this.contextVariables.Add(name, value);
            }
        }

		public override string ToXML() 
		{
            string aux;
            aux = "<SpeechAct type=\"" + ActionType + "\"><Sender>" + Subject
                + "</Sender><Receiver>" + Target
                + "</Receiver><Type>" + this.meaning + "</Type>";

            foreach(KeyValuePair<string,string> entry in this.contextVariables)
            {
                aux = aux + "<Context id=\"" + entry.Key + "\">" + entry.Value + "</Context>";
            }

            aux = aux + "<Parameters>";

            foreach (string param in this.Parameters)
            {
                aux = aux + "<Param>" + param + "</Param>";
            }

            aux = aux + "</Parameters>";

            if (this.utterance != null)
            {
                aux = aux + "<Utterance>" + this.utterance + "</Utterance>";
            }

            //if (this.Emotion != null)
            //{
            //    aux = aux + this.Emotion.ToXml();
            //}

            aux = aux + CameraToXMl();

            aux = aux + "</SpeechAct>";

            return aux;
		}

        /**
	 * Converts the SpeechAct to XML that is ready to be sent to the 
	 * LanguageEngine for the generation of a specific utterance
	 * @return a XML string that contains the SpeechAct information
	 */
        public string toLanguageEngine()
        {
            string aux;
            string speechType = this.meaning;

            if (this.ActionType.Equals(SpeechActParameters.REPLY))
            {
                speechType = speechType + this.Parameters[0];
            }
            else if (this.ActionType.Equals(SpeechActParameters.REINFORCE))
            {
                speechType = speechType + "reinforce";
            }
            else if (this.ActionType.Equals(SpeechActParameters.COPINGSPEECH))
            {
                this.contextVariables.Add("copingstrategy", this.Parameters[0]);
            }

            aux = "<SpeechAct><Sender>" + this.Subject
                + "</Sender><Receiver>" + this.Target
                + "</Receiver><Type>" + speechType + "</Type>";

            foreach (KeyValuePair<string,string> p in this.contextVariables)
            {
                aux = aux + "<Context id=\"" + p.Key + "\">" + p.Value + "</Context>";
            }

            if (this.utterance != null)
            {
                aux = aux + "<Utterance>" + this.utterance + "</Utterance>";
            }

            aux = aux + "</SpeechAct>";

            return aux;
        }
	}
}
