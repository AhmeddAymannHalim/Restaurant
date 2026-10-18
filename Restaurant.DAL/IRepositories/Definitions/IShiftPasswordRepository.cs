namespace Restaurant.DAL.IRepositories.Definitions;

public interface IShiftPasswordRepository
{
    string Hash(string password);
    bool Verify(string stored, string password, out bool needsRehash);
}
