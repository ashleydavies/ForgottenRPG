namespace ShaRPG.Entity {
    public interface IEntityIdAssigner {
        int GetNextId(GameEntity e);
        GameEntity GetEntity(int id);
    }
}
