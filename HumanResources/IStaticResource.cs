using System;
using System.Collections.Generic;
using System.Text;

namespace HumanResources
{
    public interface IStaticResource
    {
        bool Save();
        bool Close();
        bool PushUser(ulong gid, ulong uid);
        bool PopUser(ulong gid, ulong uid);
        bool Contains(ulong gid, ulong uid);
        bool PopGuild(ulong gid);
    }
}
