using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ThalamusFAtiMA.Emotions;

namespace ThalamusFAtiMA.Speech
{
    public class PredefinedUtterance
    {
        public string Text { get; set; }
        public int MaxUsages { get; set; }

        public PredefinedUtterance()
        {
            this.MaxUsages = 1;
        }
    }

    public class PredefinedUtteranceSelector
    {
        private IDictionary<string, ICollection<PredefinedUtterance>>  EmotionalUtterances { get; set; }
        private Random Random { get; set; } 

        public PredefinedUtteranceSelector(string version)
        {
            this.Random = new Random();

            this.EmotionalUtterances = new Dictionary<string, ICollection<PredefinedUtterance>>();

            var angerUtterances = new Collection<PredefinedUtterance>();
            angerUtterances.Add(new PredefinedUtterance {Text = " I will get you for this"});
            if (version.Equals("B"))
            {
                angerUtterances.Add(new PredefinedUtterance { Text = " I'm really angry"});
            }
            
            this.EmotionalUtterances.Add(Emotion.ANGER_EMOTION, angerUtterances);

            var fearUtterances = new Collection<PredefinedUtterance>();
            fearUtterances.Add(new PredefinedUtterance{Text = "I feel this is going to be a hard game"});
            fearUtterances.Add(new PredefinedUtterance{Text = "This will be difficult"});
            fearUtterances.Add(new PredefinedUtterance { Text = "This is another game which I will likely loose" });
            if (version.Equals("B"))
            {
                fearUtterances.Add(new PredefinedUtterance { Text = "I fear this will be hard for me"});
                fearUtterances.Add(new PredefinedUtterance { Text = "I fear I will struggle in this game"});
            }
            fearUtterances.Add(new PredefinedUtterance{Text = "Some of these boards are difficult for me. This is one of them."});
            this.EmotionalUtterances.Add(Emotion.FEAR_EMOTION, fearUtterances);

            var hopeUtterances = new Collection<PredefinedUtterance>();
            hopeUtterances.Add(new PredefinedUtterance{Text = "I think I can win this", MaxUsages = 2});
            if (version.Equals("B"))
            {
                hopeUtterances.Add(new PredefinedUtterance { Text = "I'm hopefull I'll win this one"});
            }
            hopeUtterances.Add(new PredefinedUtterance{Text = "I know I can win this.", MaxUsages = 2});
            this.EmotionalUtterances.Add(Emotion.HOPE_EMOTION, hopeUtterances);

            var selfResentmentUtterances = new Collection<PredefinedUtterance>();
            selfResentmentUtterances.Add(new PredefinedUtterance{Text = "I've just made a terrible mistake", MaxUsages = 2});
            this.EmotionalUtterances.Add(Emotion.RESENTMENT_EMOTION+"SELF", selfResentmentUtterances);

            var otherResentmentUtterances = new Collection<PredefinedUtterance>();
            otherResentmentUtterances.Add(new PredefinedUtterance{Text = "This is not fair"});
            otherResentmentUtterances.Add(new PredefinedUtterance{Text = "You are too good"});
            otherResentmentUtterances.Add(new PredefinedUtterance{Text = "Give me a break"});
            otherResentmentUtterances.Add(new PredefinedUtterance{Text = "This is unfair", MaxUsages = 2});
            this.EmotionalUtterances.Add(Emotion.RESENTMENT_EMOTION+"OTHER",otherResentmentUtterances);

            var joyUtterances = new Collection<PredefinedUtterance>();
            joyUtterances.Add(new PredefinedUtterance{Text = "Oh yes nice move", MaxUsages = 2});
            joyUtterances.Add(new PredefinedUtterance{Text = "Great", MaxUsages = 2});
            if (version.Equals("B"))
            {
                joyUtterances.Add(new PredefinedUtterance { Text = "I'm happy with that", MaxUsages = 1 });
            }
            
            this.EmotionalUtterances.Add(Emotion.JOY_EMOTION, joyUtterances);

            var distressUtterances = new Collection<PredefinedUtterance>();
            distressUtterances.Add(new PredefinedUtterance{Text = "That's bad for me"});
            distressUtterances.Add(new PredefinedUtterance{Text = "This is bad. I'm not sure if I should continue."});
            if (version.Equals("B"))
            {
                distressUtterances.Add(new PredefinedUtterance { Text = "I'm not happy with this move." });
            }
            this.EmotionalUtterances.Add(Emotion.DISTRESS_EMOTION, distressUtterances);

            var disappointmentUtterances = new Collection<PredefinedUtterance>();
            disappointmentUtterances.Add(new PredefinedUtterance{Text = "Damn it. I lost again."});
            disappointmentUtterances.Add(new PredefinedUtterance{Text = "I lost again.", MaxUsages = 3});
            if (version.Equals("B"))
            {
                disappointmentUtterances.Add(new PredefinedUtterance { Text = "This is sad. I keep loosing." });
            }
            this.EmotionalUtterances.Add(Emotion.DISAPPOINTMENT_EMOTION, disappointmentUtterances);

            var gloatingSelfActionUtterances = new Collection<PredefinedUtterance>();
            gloatingSelfActionUtterances.Add(new PredefinedUtterance {Text = "Ah Ah, nice play."});
            gloatingSelfActionUtterances.Add(new PredefinedUtterance {Text = "This was a good move."});
            gloatingSelfActionUtterances.Add(new PredefinedUtterance {Text = "I was good, right?"});
            gloatingSelfActionUtterances.Add(new PredefinedUtterance { Text = "Eh eh, you weren't expecting this move" });
            this.EmotionalUtterances.Add(Emotion.GLOATING_EMOTION+"SELF",gloatingSelfActionUtterances);

            var gloatingOtherActionUtterances = new Collection<PredefinedUtterance>();
            gloatingOtherActionUtterances.Add(new PredefinedUtterance{Text = "Keep playing like that, and I'll win for sure"});
            gloatingOtherActionUtterances.Add(new PredefinedUtterance{Text = "I'm not sure that was a good move"});
            gloatingOtherActionUtterances.Add(new PredefinedUtterance{Text = "ah ah", MaxUsages = 3});
            this.EmotionalUtterances.Add(Emotion.GLOATING_EMOTION+"OTHER", gloatingOtherActionUtterances);

            var reliefUtterances = new Collection<PredefinedUtterance>();
            reliefUtterances.Add(new PredefinedUtterance{Text = " Phew. That was close."});
            reliefUtterances.Add(new PredefinedUtterance{Text = " I almost lost this one."});
            this.EmotionalUtterances.Add(Emotion.RELIEF_EMOTION, reliefUtterances);

            var sharingUtterances = new Collection<PredefinedUtterance>();
            sharingUtterances.Add(new PredefinedUtterance {Text = "I'm glad to share this with you"});
            sharingUtterances.Add(new PredefinedUtterance {Text = "It's nice to have someone to talk to"});
            this.EmotionalUtterances.Add(Emotion.SATISFACTION_EMOTION+"SHARING",sharingUtterances);

            var satisfactionUtterances = new Collection<PredefinedUtterance>();
            satisfactionUtterances.Add(new PredefinedUtterance {Text = "Yes, I won"});
            satisfactionUtterances.Add(new PredefinedUtterance { Text = "Great. I won." });
            if (version.Equals("B"))
            {
                satisfactionUtterances.Add(new PredefinedUtterance { Text = "I won. I'm so good." });
                satisfactionUtterances.Add(new PredefinedUtterance { Text = "I  won this time. Great!" });
            }
            
            this.EmotionalUtterances.Add(Emotion.SATISFACTION_EMOTION,satisfactionUtterances);

            var fearsConfrmedUtterances = new Collection<PredefinedUtterance>();
            if (version.Equals("B"))
            {
                fearsConfrmedUtterances.Add(new PredefinedUtterance { Text = "This was sad." });
            }
            fearsConfrmedUtterances.Add(new PredefinedUtterance {Text = "I had no chance this game"});
            fearsConfrmedUtterances.Add(new PredefinedUtterance {Text = "Well, I think I did my best, but you are too good."});
            this.EmotionalUtterances.Add(Emotion.FEARS_CONFIRMED_EMOTION, fearsConfrmedUtterances);            
        }

        public string SelectUtteranceFromSet(ICollection<PredefinedUtterance> utteranceCollection)
        {
            //first step, determine the overall usages of all available utterances
            var totalMaxUsages = utteranceCollection.Sum(u => u.MaxUsages);

            //if all available utterances were selected the maximum ammount of times, return an empty string.
            if (totalMaxUsages == 0) return String.Empty;

            var selectedIndex = this.Random.Next(1, totalMaxUsages);

            foreach (var utterance in utteranceCollection)
            {
                if (selectedIndex <= utterance.MaxUsages)
                {
                    utterance.MaxUsages--;
                    return utterance.Text;
                }
                else
                {
                    selectedIndex -= utterance.MaxUsages;
                }
            }

            //We shouldn't reach this point, but if we do, return an empty string
            return String.Empty;
        }

        public string GetUtteranceForEmotion(Emotion emotion)
        {
            //there is a chance that nothing will be said depending on the emotion intensity (i.e. the lower the intensity, the higher the
            // probability of not saying anything
            var probabilityOfSpeaking = this.Random.Next(2, 6);
            if (emotion.Intensity < probabilityOfSpeaking) return String.Empty;
            
            var emotionKey = emotion.Type;

            if (emotion.Type.Equals(Emotion.RESENTMENT_EMOTION) || emotion.Type.Equals(Emotion.GLOATING_EMOTION))
            {
                if (emotion.Cause.Contains("SELF"))
                {
                    emotionKey += "SELF";
                }
                else
                {
                    emotionKey += "OTHER";
                }
            }
            else if (emotion.Type.Equals(Emotion.SATISFACTION_EMOTION))
            {
                if (emotion.Cause.Contains("Sharing"))
                {
                    emotionKey += "SHARING";
                }
            }

            var utteranceSetForEmotion = this.EmotionalUtterances[emotionKey];

            //if there is no defined utterance for the emotion, just return an empty string
            if (utteranceSetForEmotion == null) return String.Empty;

            return this.SelectUtteranceFromSet(utteranceSetForEmotion);
        }
    }
}
