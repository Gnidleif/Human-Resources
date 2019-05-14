namespace HumanResources
{
  public interface IStaticResource
  {
    void Initialize();
    bool Save();
    bool Close();
    bool Push(ulong gid, ulong uid);
    bool Pop(ulong gid, ulong uid);
    bool Contains(ulong gid, ulong uid);
    bool RemoveGuild(ulong gid);
  }
}
