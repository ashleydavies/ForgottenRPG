namespace ForgottenRPG.Map {
    public enum CompositeMapTileType {
        SPLIT_DOWN = 0,
        SPLIT_UP = 1,
        CORNER_N = 2,
        CORNER_E = 3,
        CORNER_S = 4,
        CORNER_W = 5
    }
    
    public struct CompositeMapTile {
        private readonly CompositeMapTileType compositeMapTileType;
        public CompositeMapTile(CompositeMapTileType compositeMapTileType) {
            this.compositeMapTileType = compositeMapTileType;
        }
    }
}
