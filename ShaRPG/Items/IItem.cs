﻿using SFML.Graphics;

namespace ShaRPG.Items {
    public interface IItem {
        int Id { get; }
        string Name { get; }
        string Codename { get; }
        string Description { get; }
        Sprite Texture { get; }
    }
}