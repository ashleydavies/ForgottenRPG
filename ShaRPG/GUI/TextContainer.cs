using System;
using System.Collections.Generic;
using System.Linq;
using ShaRPG.Util;
using ShaRPG.Util.Coordinate;

namespace ShaRPG.GUI {
    public class TextContainer : AbstractGuiComponent, IGuiComponent {
        public int LineSpacing { get; set; } = 2;
        public override int Height => _textList.Aggregate(0, (h, text) => h + text.Height) + TotalSpacing;
        public override int Width => Parent.Width;
        private readonly string _contents;
        private readonly List<Text> _textList = new List<Text>();
        private int TotalSpacing => Math.Max((_textList.Count - 1) * LineSpacing, 0);

        public TextContainer(string contents) {
            _contents = contents;
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
            string currentString = string.Empty;
            
            foreach (string word in _contents.Split(' ')) {
                string testString = $"{currentString} {word}";
                if (currentString == string.Empty) testString = word;
                Text testText = new Text(Config.GuiFont, testString);
                
                if (testText.Width > Width) {
                    if (currentString == string.Empty) return;
                    _textList.Add(previous);
                    currentString = string.Empty;
                    continue;
                }

                currentString = testString;
                previous = testText;
            }

            if (currentString != string.Empty) {
                Text testText = new Text(Config.GuiFont, currentString);
                if (testText.Width > Width) return;
                _textList.Add(testText);
            }
        }
    }
}
