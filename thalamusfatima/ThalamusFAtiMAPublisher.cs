using System;
using Thalamus;
using SuecaMessages;
using EmoteCommonMessages;

namespace ThalamusFAtiMA
{

    public interface IThalamusFAtiMAPublisher : IThalamusPublisher, ISuecaActions, IRobotPerceptions, IFMLSpeech, Thalamus.BML.IAnimationActions, Thalamus.BML.IPostureActions, EmoteCommonMessages.IGazeStateActions { }

    public class ThalamusFAtiMAPublisher : IThalamusFAtiMAPublisher
    {
        private dynamic _publisher;

        public ThalamusFAtiMAPublisher(dynamic publisher)
        {
            this._publisher = publisher;
        }

        public void Play(int id, string card)
        {
            this._publisher.Play(id, card);
        }

        public void PerformUtterance(string id, string utterance, string category)
        {
            this._publisher.PerformUtterance(id, utterance, category);
        }

        public void CancelUtterance(string id)
        {
            this._publisher.CancelUtterance(id);
        }

        public void PerformUtteranceFromLibrary(string id, string category, string subcategory, string[] tagNames, string[] tagValues)
        {
            this._publisher.PerformUtteranceFromLibrary(id, category, subcategory, tagNames, tagValues);
        }

        public void PlayAnimation(string id, string animation)
        {
            this._publisher.PlayAnimation(id, animation);
        }

        public void PlayAnimationQueued(string id, string animation)
        {
            this._publisher.PlayAnimationQueued(id, animation);
        }

        public void StopAnimation(string id)
        {
            this._publisher.StopAnimation(id);
        }

        public void ResetPose()
        {
            this._publisher.ResetPose();
        }

        public void SetPosture(string id, string posture, double percent = 1, double decay = 1)
        {
            this._publisher.SetPosture(id, posture, percent, decay);
        }

        public void GazeAtScreen(double x, double y)
        {
            this._publisher.GazeAtScreen(x, y);
        }

        public void GazeAtTarget(string targetName)
        {
            this._publisher.GazeAtTarget(targetName);
        }

        public void GlanceAtScreen(double x, double y)
        {
            this._publisher.GlanceArScreen(x, y);
        }

        public void GlanceAtTarget(string targetName)
        {
            this._publisher.GlanceAtTarget(targetName);
        }

        public void RequestUtterance(int playerId, string category, string subcategory)
        {
            this._publisher.RequestUtterance(playerId, category, subcategory);
        }

        public void OKUtterance(int playerId)
        {
            this._publisher.OKUtterance(playerId);
        }

        public void NOUtterance(int playerId)
        {
            this._publisher.NOUtterance(playerId);
        }

        public void StartedUtterance(int playerId, string category, string subcategory)
        {
            this._publisher.StartedUtterance(playerId, category, subcategory);
        }

        public void FinishedUtterance(int playerId)
        {
            this._publisher.FinishedUtterance(playerId);
        }
    }
}
