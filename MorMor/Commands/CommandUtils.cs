﻿using System.Reflection;
using MomoAPI.Entities;
using MomoAPI.Entities.Segment;

namespace MorMor.Commands;

internal static class CommandUtils
{
    public static async ValueTask SendImagsEmoji(string url, CommandArgs args)
    {
        var at = args.EventArgs.MessageContext.GetAts();
        long target = -1;
        if (at.Count > 0)
        {
            target = at.First().UserId;
        }
        else
        {
            if (args.Parameters.Count > 0)
            {
                _ = long.TryParse(args.Parameters[0], out target);
            }
        }
        if (target != -1)
            await args.EventArgs.Reply(new MessageBody().Image(url + "?QQ=" + target));
    }
    public static bool ParseBool(string str)
    {
        return str switch
        {
            "true" or "开启" or "开" => true,
            "false" or "关闭" or "关" => false,
            _ => false,
        };
    }
    public static bool ClassConstructParamIsZerp(this ConstructorInfo[] constructors)
    {
        return constructors.Any(ctor => ctor.GetParameters().Length == 0);
    }

    public static bool CommandParamPares(this MethodInfo method, Type type)
    {
        if (method != null)
        {
            var param = method.GetParameters();
            if (param.Length == 1)
            {
                return param[0].ParameterType == type;
            }
        }
        return false;
    }

    public static MessageBody GetAccountInfo(long groupid, long uin, string groupName)
    {
        var userid = uin;
        var serverName = MorMorAPI.UserLocation.TryGetServer(userid, groupid, out var server) ? server?.Name ?? "NULL" : "NULL";
        var bindUser = MorMorAPI.TerrariaUserManager.GetUserById(userid, serverName);
        var bindName = bindUser.Count == 0 ? "NULL" : string.Join(",", bindUser.Select(x => x.Name));
        //var api = server != null ? (await server.QueryEconomicBank(bindName)): null;
        var signInfo = MorMorAPI.SignManager.Query(groupid, userid);
        var sign = signInfo != null ? signInfo.Date : 0;
        var currencyInfo = MorMorAPI.CurrencyManager.Query(groupid, userid);
        var currency = currencyInfo != null ? currencyInfo.num : 0;
        MessageBody body = new()
        {
            MomoSegment.Image($"http://q.qlogo.cn/headimg_dl?dst_uin={uin}&spec=640&img_type=png"),
            MomoSegment.Text($"[QQ账号]:{userid}\n"),
            MomoSegment.Text($"[签到时长]:{sign}\n"),
            MomoSegment.Text($"[星币数量]:{currency}\n"),
            MomoSegment.Text($"[拥有权限]:{groupName}\n"),
            MomoSegment.Text($"[绑定角色]:{bindName}\n"),
            MomoSegment.Text($"[所在服务器]:{serverName}")
        };
        return body;
    }
}
