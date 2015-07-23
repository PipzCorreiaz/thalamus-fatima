using System;
using Thalamus;
using SuecaMessages;

namespace ThalamusFAtiMA
{
    public interface IFMLSpeech : IAction
    {
        void PerformUtterance(string utterance, string category);
    }

    public interface IAnimationActions : IAction
    {
        void PlayAnimation(string id, string animation);
        void PlayAnimationQueued(string id, string animation);
    }



    public interface IThalamusFAtiMAPublisher : IThalamusPublisher, ISuecaActions, IFMLSpeech, IAnimationActions { }

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

        public void PerformUtterance(string utterance, string category)
        {
            this._publisher.PerformUtterance(utterance, category);
        }


        public void PlayAnimation(string id, string animation)
        {
            this._publisher.PlayAnimation(id, animation);
        }

        public void PlayAnimationQueued(string id, string animation)
        {
            this._publisher.PlayAnimationQueued(id, animation);
        }
    }
}
