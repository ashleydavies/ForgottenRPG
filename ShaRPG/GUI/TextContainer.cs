using System;
using System.Collections.Generic;
using System.Linq;
using SFML.Graphics;
using ShaRPG.Util.Coordinate;

namespace ShaRPG.GUI {
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

            Text previous = null;
            string currentString = string.Empty.PadLeft(Indent);
            
            foreach (string word in Contents.Split(' ')) {
                string testString = $"{currentString} {word}";
                if (currentString == string.Empty) testString = word;
                Text testText = new Text(testString, Config.GuiFont, TextSize) {
                    Color = Color
                };

                if (testText.GetLocalBounds().Width > Width) {
                    if (currentString == string.Empty) return;
                    _textList.Add(previous);
                    currentString = word;
                    continue;
                }

                currentString = testString;
                previous = testText;
            }

            if (currentString.Trim() != string.Empty) {
                Text testText = new Text(currentString, Config.GuiFont, TextSize) {
                    Color = Color
                };
                if (testText.GetLocalBounds().Width > Width) return;
                _textList.Add(testText);
            }
        }
    }
}
