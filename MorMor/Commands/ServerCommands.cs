﻿using System.Drawing;
using System.Text;
using MorMor.Attributes;
using MorMor.Permission;

namespace MorMor.Commands;

[CommandSeries]
public class ServerCommands
{
    [CommandMatch("泰拉商店", OneBotPermissions.TerrariaShop)]
    public static async ValueTask ShopList(ServerCommandArgs args)
    {
        if (args.Server == null) return;
        var sb = new StringBuilder();
        var shop = MorMorAPI.TerrariaShop.TrShop;
        var index = 1;
        foreach (var item in shop)
        {
            sb.AppendLine($"{index}.{item.Name} x {item.num}     {item.Price}");
            index++;
        }
        await args.Server.PrivateMsg(args.UserName, $"泰拉商店列表:\n{sb}", Color.GreenYellow);
    }

    [CommandMatch("抽", OneBotPermissions.TerrariaPrize)]
    public static async ValueTask Prize(ServerCommandArgs args)
    {
        if (args.Server == null) return;
        if (args.User == null)
        {
            await args.Server.PrivateMsg(args.UserName, "没有你的注册信息！", Color.DarkRed);
            return;
        }
        if (!args.Server.EnabledPrize)
        {
            await args.Server.PrivateMsg(args.UserName, "服务器未开启抽奖系统！", Color.DarkRed);
            return;
        }
        var count = 1;
        if (args.Parameters.Count > 0)
            _ = int.TryParse(args.Parameters[0], out count);
        if (count > 50)
            count = 50;
        var prizes = MorMorAPI.TerrariaPrize.Nexts(count);
        var curr = MorMorAPI.CurrencyManager.Query(args.User.GroupID, args.User.Id);
        if (curr == null || curr.num < count * MorMorAPI.TerrariaPrize.Fess)
        {
            await args.Server.PrivateMsg(args.UserName, $"你的星币不足抽取{count}次", Color.Red);
            return;
        }
        MorMorAPI.CurrencyManager.Del(args.User.GroupID, args.User.Id, count * MorMorAPI.TerrariaPrize.Fess);
        Random random = new();
        //var tasks = new List<ValueTask>();
        foreach (var prize in prizes)
        {
            await args.Server.Command($"/g {prize.ID} {args.UserName} {random.Next(prize.Min, prize.Max)}");
        }
        //await ValueTask.WhenAll(tasks);
    }


    [CommandMatch("购买", OneBotPermissions.TerrariaShop)]
    public static async ValueTask ShopBuy(ServerCommandArgs args)
    {
        if (args.Server == null) return;
        if (args.Parameters.Count != 1)
        {
            await args.Server.PrivateMsg(args.UserName, $"语法错误:\n正确语法:/购买 [名称|ID]", Color.GreenYellow);
            return;
        }
        if (!args.Server.EnabledShop)
        {
            await args.Server.PrivateMsg(args.UserName, "服务器未开启商店系统！", Color.DarkRed);
            return;
        }
        if (args.User != null)
        {
            if (int.TryParse(args.Parameters[0], out var id))
            {
                if (MorMorAPI.TerrariaShop.TryGetShop(id, out var shop) && shop != null)
                {
                    var curr = MorMorAPI.CurrencyManager.Query(args.User.GroupID, args.User.Id);
                    if (curr != null && curr.num >= shop.Price)
                    {
                        var res = await args.Server.Command($"/g {shop.ID} {args.UserName} {shop.num}");
                        if (res.Status)
                        {
                            MorMorAPI.CurrencyManager.Del(args.User.GroupID, args.User.Id, shop.Price);
                            await args.Server.PrivateMsg(args.UserName, "购买成功!", Color.GreenYellow);
                        }
                        else
                        {
                            await args.Server.PrivateMsg(args.UserName, "失败! 错误信息:\n" + res.Message, Color.GreenYellow);
                        }
                    }
                    else
                    {
                        await args.Server.PrivateMsg(args.UserName, "星币不足!", Color.GreenYellow);
                    }
                }
                else
                {
                    await args.Server.PrivateMsg(args.UserName, "该商品不存在!", Color.GreenYellow);
                }
            }
            else
            {
                if (MorMorAPI.TerrariaShop.TryGetShop(args.Parameters[0], out var shop) && shop != null)
                {
                    var curr = MorMorAPI.CurrencyManager.Query(args.User.GroupID, args.User.Id);
                    if (curr != null && curr.num >= shop.Price)
                    {
                        var res = await args.Server.Command($"/g {shop.ID} {args.UserName} {shop.num}");
                        if (res.Status)
                        {
                            MorMorAPI.CurrencyManager.Del(args.User.GroupID, args.User.Id, shop.Price);
                            await args.Server.PrivateMsg(args.UserName, "购买成功!", Color.GreenYellow);
                        }
                        else
                        {
                            await args.Server.PrivateMsg(args.UserName, "失败! 错误信息:\n" + res.Message, Color.GreenYellow);
                        }
                    }
                    else
                    {
                        await args.Server.PrivateMsg(args.UserName, "星币不足!", Color.GreenYellow);
                    }
                }
                else
                {
                    await args.Server.PrivateMsg(args.UserName, "该商品不存在!", Color.GreenYellow);
                }
            }
        }
        else
        {
            await args.Server.PrivateMsg(args.UserName, "未找到你的注册信息!", Color.GreenYellow);
        }
    }
}
