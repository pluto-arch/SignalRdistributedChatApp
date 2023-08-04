﻿using Dncy.SnowFlake;

namespace ChatService.Infra.Utils;

public static class SnowFlakeId
{
    private static readonly Lazy<SnowFlake> snowFlake = new Lazy<SnowFlake>(() => new SnowFlake(1));

    public static SnowFlake Generator => snowFlake.Value;
}