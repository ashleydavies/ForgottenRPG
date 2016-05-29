namespace ShaRPG.Entity {
    public interface IEntityIdAssigner {
        int GetNextId(Entity e);
        Entity GetEntity(int id);
    }
}
