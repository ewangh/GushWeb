using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GushWeb.Utility
{
    public partial class RedisServer
    {
        public static ServiceStack.Redis.RedisClient Client = new ServiceStack.Redis.RedisClient(ConfigEntity.redisService, ConfigEntity.redisPort);
    }
}