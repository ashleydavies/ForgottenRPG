using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using ForgottenRPG.Util.Coordinate;
using SFML.Graphics;

namespace ForgottenRPG.GUI {
    public class TextContainer : AbstractGuiComponent {
        public int LineSpacing { get; set; } = 2;
        public int Indent { get; set; } = 0;
        public Color Color { get; set; } = Color.Black;
        public uint TextSize { get; }
        public override int Height => _textList.Aggregate(0, (h, text) => h + (int) text.CharacterSize) + TotalSpacing;
        public override int Width => Parent?.Width ?? 0;

        public string Contents {
            get => _contents;
            set {
                _contents = value;
                ReflowAll();
            }
        }

        private string _contents;
        private readonly List<Text> _textList = new List<Text>();
        private int TotalSpacing => Math.Max((_textList.Count - 1) * LineSpacing, 0);

        public TextContainer(string contents, uint size = 16) {
            Contents = contents;
            TextSize = size;
        }

        public override void Render(RenderTarget renderSurface) {
            int h = 0;
            _textList.ForEach(text => {
                text.Position = new ScreenCoordinate(0, h);
                renderSurface.Draw(text);
                h += (int) text.CharacterSize + LineSpacing;
            });
        }

        public override void Reflow() {
            _textList.Clear();

            Text previous = new Text("", Config.GuiFont, TextSize);
            string currentString = string.Empty.PadLeft(Indent);

            // This code used to use Split, which is nicer, but we want to maintain spacing e.g. double spaces
            string remainingContent = Contents;
            while (remainingContent.Length > 0) {
                int nextIndex = remainingContent.IndexOfAny(new[] { '\n', ' ' });
                if (nextIndex == -1) nextIndex = remainingContent.Length - 1;

                if (nextIndex == 0) {
                    if (remainingContent[0] == '\n') {
                        _textList.Add(previous);
                        currentString = "";
                        remainingContent = remainingContent.Substring(1);
                        previous = new Text("", Config.GuiFont, TextSize);
                        continue;
                    }

                    // Just add the single space as a word
                    nextIndex++;
                }

                string word = remainingContent.Substring(0, nextIndex);
                remainingContent = remainingContent.Substring(nextIndex);

                string candidateString = $"{currentString}{word}";
                Text candidateText = new Text(candidateString, Config.GuiFont, TextSize) {
                    FillColor = Color
                };

                if (candidateText.GetLocalBounds().Width > Width) {
                    // TODO: Hyphenate
                    if (currentString.Trim() == string.Empty) {
                        if (Width < 12) return;
                        throw new GuiException(
                            $"Tried to display text which could not be flowed. Problematic word: '{candidateString}'");
                    }

                    _textList.Add(previous);
                    currentString = word;
                    continue;
                }

                currentString = candidateString;
                previous = candidateText;
            }

            if (currentString.Trim() != string.Empty) {
                _textList.Add(previous);
            }
        }
    }
}
