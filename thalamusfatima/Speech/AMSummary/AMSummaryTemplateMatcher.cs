using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ThalamusFAtiMA.Emotions;

namespace ThalamusFAtiMA.Speech.AMSummary
{
    public class AMSummaryTemplateMatcher
    {
        private ICollection<AMSummaryTemplate> Summaries { get; set; }
        private Random Random { get; set; }

        public AMSummaryTemplateMatcher()
        {
            this.Random = new Random();
            this.Summaries = new Collection<AMSummaryTemplate>();

            this.Summaries.Add(new AMSummaryTemplate
            {
                Summary = new AMSummary
                {
                    Events = new[]
                    {
                        new EventDescription(1)
                        {
                            Intention = "WinGame",
                            Status = EventDescription.STATUS_ACTIVATE,
                            Emotion = new EmotionDescription
                            {
                                Type = Emotion.FEAR_EMOTION
                            }
                        },
                        new EventDescription(2)
                        {
                            Intention = "WinGame",
                            Status = EventDescription.STATUS_FAILED,
                        }
                    }
                },
                ValidTexts = new[]
                {
                    "A couple of minutes ago, I wanted to win but I was feeling anxious. Unfortunately, I could not defeat you.",
                    "I was planning to defeat you earlier but I gess I failed.",
                    "I actually wanted to win you in this board, but because I was so anxious I totally failed."
                }
            });

            this.Summaries.Add(new AMSummaryTemplate
            {
                Summary = new AMSummary
                {
                    Events = new[]
                    {
                        new EventDescription(1)
                        {
                            Intention = "WinGame",
                            Status = EventDescription.STATUS_ACTIVATE,
                            Emotion = new EmotionDescription
                            {
                                Type = Emotion.FEAR_EMOTION
                            }
                        },
                        new EventDescription(2)
                        {
                            Intention = "WinGame",
                            Status = EventDescription.STATUS_FAILED,
                            Emotion = new EmotionDescription
                            {
                                Type = Emotion.DISAPPOINTMENT_EMOTION
                            }
                        }
                    }
                },
                ValidTexts = new[]
                {
                    "I was hoping to win you but in the end you won. I'm really disappointed.",
                    "Ok. I'm dispointed because I planned to win."
                }
            });

            this.Summaries.Add(new AMSummaryTemplate
            {
                Summary = new AMSummary
                {
                    Events = new[]
                    {
                        new EventDescription(1)
                        {
                            Intention = "WinGame",
                            Status = EventDescription.STATUS_ACTIVATE,
                            Emotion = new EmotionDescription
                            {
                                Type = Emotion.FEAR_EMOTION
                            }
                        },
                        new EventDescription(2)
                        {
                            Intention = "WinGame",
                            Status = EventDescription.STATUS_SUCCEED,
                            Emotion = new EmotionDescription
                            {
                                Type = Emotion.SATISFACTION_EMOTION,
                            }
                        }
                    }
                },
                ValidTexts = new[]
                {
                    "I was so anxious because I wanted to win the game. Now I'm better.",
                    "I wanted to win you in the last game. Now that I managed to do it, I feel a bit relieved."
                }
            });

            this.Summaries.Add(new AMSummaryTemplate
            {
                Summary = new AMSummary
                {
                    Events = new []
                    {
                        new EventDescription(1)
                        {
                            Intention = "WinGame",
                            Status = EventDescription.STATUS_ACTIVATE,
                            Emotion = new EmotionDescription
                            {
                                Type = Emotion.FEAR_EMOTION
                            }
                        },
                        new EventDescription(2)
                        {
                            Action = "Play",
                            Status = EventDescription.STATUS_SUCCEED,
                            Emotion = new EmotionDescription
                            {
                                Type = Emotion.JOY_EMOTION
                            }
                        }
                    }
                },
                ValidTexts = new []{"I wanted to win the last game, but I wasn't very hopefull. Fortunately, I was able to make a nice move, which made me feel really happy."}
            });

            this.Summaries.Add(new AMSummaryTemplate
            {
                Summary = new AMSummary
                {
                    Events = new[]
                    {
                        new EventDescription(1)
                        {
                            Action = "Play",
                            Status = EventDescription.STATUS_SUCCEED,
                            Emotion = new EmotionDescription
                            {
                                Type = Emotion.JOY_EMOTION
                            }
                        },
                        new EventDescription(2)
                        {
                            Intention = "WinGame",
                            Status = EventDescription.STATUS_SUCCEED,
                            Emotion = new EmotionDescription
                            {
                                Type = Emotion.SATISFACTION_EMOTION
                            }
                        }
                    }
                },
                ValidTexts = new []{"I made a good move and I was able to win he game. That was great. I'm still smiling."}
            });

            this.Summaries.Add(new AMSummaryTemplate
            {
                Summary = new AMSummary
                {
                    Events = new[]
                    {
                        new EventDescription(1)
                        {
                            Action = "Play",
                            Status = EventDescription.STATUS_SUCCEED,
                            Emotion = new EmotionDescription
                            {
                                Type = Emotion.DISTRESS_EMOTION
                            }
                        },
                        new EventDescription(2)
                        {
                            Intention = "WinGame",
                            Status = EventDescription.STATUS_SUCCEED,
                            Emotion = new EmotionDescription
                            {
                                Type = Emotion.SATISFACTION_EMOTION
                            }
                        }
                    }
                },
                ValidTexts = new[] { "I was distressed because I initially did a bad move. Fortunately, I managed to win despite that. It was nice." }
            });

            this.Summaries.Add(new AMSummaryTemplate
            {
                Summary = new AMSummary
                {
                    Events = new[]
                    {
                        new EventDescription(1)
                        {
                            Action = "Play",
                            Status = EventDescription.STATUS_SUCCEED,
                            Emotion = new EmotionDescription
                            {
                                Type = Emotion.DISTRESS_EMOTION
                            }
                        },
                        new EventDescription(2)
                        {
                            Intention = "WinGame",
                            Status = EventDescription.STATUS_FAILED,
                            Emotion = new EmotionDescription
                            {
                                Type = Emotion.FEARS_CONFIRMED_EMOTION
                            }
                        }
                    }
                },
                ValidTexts = new[] { "In the last game I was upset because I did a bad move. I lost because of that." }
            });


            this.Summaries.Add(new AMSummaryTemplate
            {
                Summary = new AMSummary
                {
                    Events = new[]
                    {
                        new EventDescription(1)
                        {
                            Action = "Play",
                            Status = EventDescription.STATUS_SUCCEED,
                        },
                        new EventDescription(2)
                        {
                            Intention = "Play",
                            Status = EventDescription.STATUS_SUCCEED,
                            Emotion = new EmotionDescription
                            {
                                Type = Emotion.JOY_EMOTION
                            }
                        }
                    }
                },
                ValidTexts = new []
                {
                    "Earlier I did this unexpected move that made me feel great. I'm not sure if I can repeat it.",
                }
            });

            this.Summaries.Add(new AMSummaryTemplate
            {
                Summary = new AMSummary
                {
                    Events = new[]
                    {
                        new EventDescription(1)
                        {
                            Action = "Play",
                            Status = EventDescription.STATUS_SUCCEED,
                            Emotion = new EmotionDescription
                            {
                                Type = Emotion.DISTRESS_EMOTION
                            }
                        },
                        new EventDescription(2)
                        {
                            Intention = "Play",
                            Status = EventDescription.STATUS_SUCCEED,
                            Emotion = new EmotionDescription
                            {
                                Type = Emotion.JOY_EMOTION
                            }
                        }
                    }
                },
                ValidTexts = new[]
                {
                    "Earlier you surprised me with a clever move, but I managed to do one even better. It made me really happy."
                }
            });
        }

        public string GenerateTextForSummary(AMSummary summary)
        {
            if (summary == null || this.Summaries.Count == 0) return String.Empty;

            var bestTemplate = this.Summaries.First();
            var bestDistance = summary.CalculateDistanceFromSummaryTemplate(bestTemplate.Summary);

            foreach (var templateSummary in this.Summaries)
            {
                var distance = summary.CalculateDistanceFromSummaryTemplate(templateSummary.Summary);
                if (distance < bestDistance)
                {
                    bestTemplate = templateSummary;
                    bestDistance = distance;
                }
            }

            return bestTemplate.ValidTexts.ElementAt(this.Random.Next(bestTemplate.ValidTexts.Count()));
        }
    }
}
