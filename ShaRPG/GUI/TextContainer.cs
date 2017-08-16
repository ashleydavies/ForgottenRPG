using System;
using System.Collections.Generic;
using System.Linq;
using ShaRPG.Util;
using ShaRPG.Util.Coordinate;

namespace ShaRPG.GUI {
    public class TextContainer : AbstractGuiComponent {
        public int LineSpacing { get; set; } = 2;
        public int Indent { get; set; } = 0;
        public Color Color { get; set; } = Color.Black;
        public uint TextSize { get; }
        public override int Height => _textList.Aggregate(0, (h, text) => h + text.Height) + TotalSpacing;
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

        public TextContainer(string contents, uint size = 12) {
            Contents = contents;
            TextSize = size;
        }

        public override void Render(IRenderSurface renderSurface) {
            int h = 0;
            _textList.ForEach(text => {
                renderSurface.Render(text, new ScreenCoordinate(0, h));
                h += text.Height + LineSpacing;
            });
        }

        public override void Reflow() {
            _textList.Clear();

            Text previous = null;
            string currentString = string.Empty.PadLeft(Indent);
            
            foreach (string word in Contents.Split(' ')) {
                string testString = $"{currentString} {word}";
                if (currentString == string.Empty) testString = word;
                Text testText = new Text(Config.GuiFont, testString, TextSize, Color);
                
                if (testText.Width > Width) {
                    if (currentString == string.Empty) return;
                    _textList.Add(previous);
                    currentString = word;
                    continue;
                }

                currentString = testString;
                previous = testText;
            }

            if (currentString.Trim() != string.Empty) {
                Text testText = new Text(Config.GuiFont, currentString, TextSize, Color);
                if (testText.Width > Width) return;
                _textList.Add(testText);
            }
        }
    }
}
