﻿using MomoAPI.Converter;
using MomoAPI.Entities;
using MomoAPI.Entities.Info;
using MomoAPI.Entities.Segment.DataModel;
using MomoAPI.Enumeration.ApiType;
using MomoAPI.Enumeration.EventParamType;
using MomoAPI.Model.API;
using MomoAPI.Extensions;
using MomoAPI.Net;
using System.Text.Json.Nodes;
using System.Text.Json;

namespace MomoAPI.Adapter;

internal class ApiAdapter
{
    /// <summary>
    /// 发送群消息
    /// </summary>
    /// <param name="target"></param>
    /// <param name="body"></param>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public static async ValueTask<(ApiStatus, long)> SendGroupMessage(long target, MessageBody body, TimeSpan? timeout = null)
    {
        /*(ApiStatus status, JObject obj) = await*/ await ReactiveApiManager.SendApiRequest(new ApiRequest()
        {
            ApiParams = new SendMessageParams
            {
                Message = body,
                GroupId = target,
                MessageType = MessageType.Group
            },
            ApiRequestType = ActionType.SendGroupMsg
        }, timeout);
        //if (status.RetCode != ApiStatusType.Ok)
        //    return (status, -1);
        //Log.ConsoleInfo($"Bot 发送群消息: group-> {target} result ->{obj?["status"]}");
        //long messageid = long.TryParse(obj?["data"]?["message_id"]?.ToString(), out var id) ? id : -1;
        return (new ApiStatus(), -1);
    }

    public static async ValueTask<(ApiStatus, long)> SendPrivateMessage(long target, MessageBody body, long? group_id, TimeSpan? timeout = null)
    {
        (ApiStatus status, JsonObject obj) = await ReactiveApiManager.SendApiRequest(new ApiRequest()
        {
            ApiParams = new SendMessageParams
            {
                Message = body,
                GroupId = group_id,
                UserId = target,
                MessageType = MessageType.Private
            },
            ApiRequestType = ActionType.SendMsg
        }, timeout);
        if (status.RetCode != ApiStatusType.Ok)
            return (status, -1);
        long messageid = long.TryParse(obj?["data"]?["message_id"]?.ToString(), out var id) ? id : -1;
        return (status, messageid);
    }

    public static async ValueTask<ApiStatus> Recall(long messageId)
    {
        (ApiStatus status, _) = await ReactiveApiManager.SendApiRequest(new ApiRequest()
        {
            ApiRequestType = ActionType.DeleteMsg,
            ApiParams = new
            {
                message_id = messageId
            }
        });
        return status;
    }

    public static async ValueTask<(ApiStatus, List<GroupInfo>)> GetGroupList()
    {
        (ApiStatus status, JsonObject obj) = await ReactiveApiManager.SendApiRequest(new ApiRequest()
        {
            ApiRequestType = ActionType.GetGroupList
        });
        if (status.RetCode != ApiStatusType.Ok || obj?["data"] == null)
            return (status, new List<GroupInfo>());
        List<GroupInfo> groupList = JsonSerializer.Deserialize<List<GroupInfo>>( obj["data"]) ?? new();
        return (status, groupList);
    }

    public static async ValueTask<(ApiStatus, GroupInfo)> GetGroupInfo(long groupid, bool cache)
    {
        (ApiStatus status, JsonObject obj) = await ReactiveApiManager.SendApiRequest(new ApiRequest()
        {
            ApiRequestType = ActionType.GetGroupInfo,
            ApiParams = new
            {
                group_id = groupid,
                no_cache = !cache
            }
        });
        if (status.RetCode != ApiStatusType.Ok || obj?["data"] == null)
            return (status, new GroupInfo());
        var groups = obj?["data"]?.ToObject<GroupInfo>() ?? new GroupInfo();
        return (status, groups);
    }

    public static async ValueTask<(ApiStatus, GroupMemberInfo)> GetGroupMemberInfo(long groupid, long target, bool cache = false)
    {
        (ApiStatus status, JsonObject obj) = await ReactiveApiManager.SendApiRequest(new ApiRequest()
        {
            ApiRequestType = ActionType.GetGroupMemberInfo,
            ApiParams = new
            {
                group_id = groupid,
                user_id = target,
                no_cache = !cache
            }
        });
        if (status.RetCode != ApiStatusType.Ok)
            return (status, new GroupMemberInfo());
        var groups = obj?["data"]?.ToObject<GroupMemberInfo>() ?? new GroupMemberInfo();
        return (status, groups);
    }

    public static async ValueTask<(ApiStatus, List<GroupMemberInfo>)> GetGroupMemberList(long groupid)
    {
        (ApiStatus status, JsonObject obj) = await ReactiveApiManager.SendApiRequest(new ApiRequest()
        {
            ApiRequestType = ActionType.GetGroupMemberList,
            ApiParams = new
            {
                group_id = groupid
            }
        });
        if (status.RetCode != ApiStatusType.Ok)
            return (status, new List<GroupMemberInfo>());
        var groups = obj?["data"]?.ToObject<List<GroupMemberInfo>>() ?? new();
        return (status, groups);
    }

    public static async ValueTask<(ApiStatus, List<FriendInfo>)> GetFriendList()
    {
        (ApiStatus status, JsonObject obj) = await ReactiveApiManager.SendApiRequest(new ApiRequest()
        {
            ApiRequestType = ActionType.GetGroupMemberList,
        });
        if (status.RetCode != ApiStatusType.Ok)
            return (status, new List<FriendInfo>());
        var friends = obj?["data"]?.ToObject<List<FriendInfo>>() ?? new List<FriendInfo>();
        return (status, friends);
    }

    public static async ValueTask<ApiStatus> SetFriendAddRequest(string flag, bool approve, string remark)
    {
        (ApiStatus status, _) = await ReactiveApiManager.SendApiRequest(new ApiRequest()
        {
            ApiRequestType = ActionType.FriendAddRequest,
            ApiParams = new { flag, approve, remark }
        });
        return status;
    }

    public static async ValueTask<(ApiStatus, MessageContext, Sender, long)> GetMessage(long messageid)
    {
        (ApiStatus status, JsonObject obj) = await ReactiveApiManager.SendApiRequest(new ApiRequest()
        {
            ApiRequestType = ActionType.GetMsg,
            ApiParams = new
            {
                message_id = messageid
            }
        });
        if (status.RetCode != ApiStatusType.Ok)
            return (status, new MessageContext(0, TimeSpan.Zero, [], string.Empty, -1), new Sender(-1), -1);
        List<OnebotSegment> rawMessage = obj?["data"]?["message"]?.ToObject<List<OnebotSegment>>() ?? new();
        var Sender = new Sender(Convert.ToInt64(obj?["data"]?["sender"]?["user_id"] ?? -1));
        var realId = Convert.ToInt64(obj?["data"]?["real_id"] ?? 0);
        var body = new MessageContext(0,
            TimeSpan.FromMilliseconds(Convert.ToInt64(obj?["data"]?["time"] ?? -1)),
            rawMessage?.ToMessageBody() ?? new MessageBody(),
            "",
            messageid
            );
        return (status, body, Sender, realId);
    }

    public static async ValueTask<ApiStatus> SendLike(long userid, int count)
    {
        (ApiStatus status, _) = await ReactiveApiManager.SendApiRequest(new ApiRequest()
        {
            ApiRequestType = ActionType.Like,
            ApiParams = new
            {
                user_id = userid,
                times = count
            }
        });
        return status;
    }

    public static async ValueTask<ApiStatus> SetGroupAddRequest(string flag, bool approve, string remark)
    {
        (ApiStatus status, _) = await ReactiveApiManager.SendApiRequest(new ApiRequest()
        {
            ApiRequestType = ActionType.GroupAddRequest,
            ApiParams = new { flag, approve, remark }
        });
        return status;
    }

    public static async ValueTask<ApiStatus> SetGroupLeave(long groupid, bool dismiss = false)
    {
        (ApiStatus status, _) = await ReactiveApiManager.SendApiRequest(new ApiRequest()
        {
            ApiRequestType = ActionType.GroupLeave,
            ApiParams = new
            {
                group_id = groupid,
                is_dismiss = dismiss
            }
        });
        return status;
    }

    public static async ValueTask<ApiStatus> Kick(long groupid, long userid, bool reject = false)
    {
        (ApiStatus status, _) = await ReactiveApiManager.SendApiRequest(new ApiRequest()
        {
            ApiRequestType = ActionType.Kick,
            ApiParams = new
            {
                user_id = userid,
                group_id = groupid,
                reject_add_request = reject
            }
        });
        return status;
    }

    public static async ValueTask<ApiStatus> Mute(long groupid, long userid, int duration)
    {
        (ApiStatus status, _) = await ReactiveApiManager.SendApiRequest(new ApiRequest()
        {
            ApiRequestType = ActionType.Mute,
            ApiParams = new
            {
                user_id = userid,
                group_id = groupid,
                duration
            }
        });
        return status;
    }

    public static async ValueTask<ApiStatus> MuteAll(long groupid, bool enable = false)
    {
        (ApiStatus status, _) = await ReactiveApiManager.SendApiRequest(new ApiRequest()
        {
            ApiRequestType = ActionType.MuteAll,
            ApiParams = new
            {
                group_id = groupid,
                enable
            }
        });
        return status;
    }

    public static async ValueTask<ApiStatus> SetAdmin(long groupid, long userid, bool enable)
    {
        (ApiStatus status, _) = await ReactiveApiManager.SendApiRequest(new ApiRequest()
        {
            ApiRequestType = ActionType.SetAdmin,
            ApiParams = new
            {
                user_id = userid,
                group_id = groupid,
                enable
            }
        });
        return status;
    }

    public static async ValueTask<ApiStatus> SetGroupCard(long groupid, long userid, string card)
    {
        (ApiStatus status, _) = await ReactiveApiManager.SendApiRequest(new ApiRequest()
        {
            ApiRequestType = ActionType.SetCard,
            ApiParams = new
            {
                user_id = userid,
                group_id = groupid,
                card
            }
        });
        return status;
    }

    public static async ValueTask<ApiStatus> SetGroupName(long groupid, string name)
    {
        (ApiStatus status, _) = await ReactiveApiManager.SendApiRequest(new ApiRequest()
        {
            ApiRequestType = ActionType.SetName,
            ApiParams = new
            {
                group_name = name,
                group_id = groupid,
            }
        });
        return status;
    }

    public static async ValueTask<(ApiStatus, JsonObject)> GetVersion()
    {
        (ApiStatus status, JsonObject obj) = await ReactiveApiManager.SendApiRequest(new ApiRequest()
        {
            ApiRequestType = ActionType.GetVersion,
        });
        return (status, obj);
    }

    public static async ValueTask<(ApiStatus, JsonObject)> GetStatus()
    {
        (ApiStatus status, JsonObject obj) = await ReactiveApiManager.SendApiRequest(new ApiRequest()
        {
            ApiRequestType = ActionType.GetStatus,
        });
        return (status, obj);
    }

    public static async ValueTask<(ApiStatus, bool)> CanSendImage()
    {
        (ApiStatus status, JsonObject obj) = await ReactiveApiManager.SendApiRequest(new ApiRequest()
        {
            ApiRequestType = ActionType.CanSendImage,
        });
        return (status, Convert.ToBoolean(obj?["data"]?["yes"] ?? false));
    }

    public static async ValueTask<(ApiStatus, bool)> CanSendRecord()
    {
        (ApiStatus status, JsonObject obj) = await ReactiveApiManager.SendApiRequest(new ApiRequest()
        {
            ApiRequestType = ActionType.CanSendRecord,
        });
        return (status, Convert.ToBoolean(obj?["data"]?["yes"] ?? false));
    }

    internal static async ValueTask<(ApiStatus apiStatus, int messageId, string forwardId)> SendGroupForwardMsg(
        long groupId,
        IEnumerable<CustomNode> msgList,
        TimeSpan? timeout = null)
    {
        if (msgList == null)
            throw new NullReferenceException("msgList is null or empty");
        //将消息段转换为数组
        CustomNode[] customNodes = msgList as CustomNode[] ?? msgList.ToArray();
        //发送消息
        (ApiStatus apiStatus, JsonObject ret) =
            await ReactiveApiManager.SendApiRequest(new ApiRequest
            {
                ApiRequestType = ActionType.SendGroupForwardMsg,
                ApiParams = new
                {
                    group_id = groupId,
                    messages = customNodes.Select(node => new
                    {
                        type = "node",
                        data = node
                    }).ToList()
                }
            }, timeout);
        if (apiStatus.RetCode != ApiStatusType.Ok || ret?["data"] == null)
            return (apiStatus, -1, string.Empty);
        int msgCode = int.TryParse(ret["data"]?["message_id"]?.ToString(), out int messageCode) ? messageCode : -1;
        string fwId = ret["data"]?["forward_id"]?.ToString() ?? string.Empty;
        return (apiStatus, msgCode, fwId);
    }

    internal static async ValueTask<(ApiStatus apiStatus, int messageId)> SendPrivateForwardMsg(
        long userId,
        IEnumerable<CustomNode> msgList,
        TimeSpan? timeout = null)
    {
        if (msgList == null)
            throw new NullReferenceException("msgList is null or empty");
        //将消息段转换为数组
        CustomNode[] customNodes = msgList as CustomNode[] ?? msgList.ToArray();

        //发送消息
        (ApiStatus apiStatus, JsonObject ret) =
            await ReactiveApiManager.SendApiRequest(new ApiRequest
            {
                ApiRequestType = ActionType.SendPrivateForwardMsg,
                ApiParams = new
                {
                    user_id = userId,
                    messages = customNodes.Select(node => new
                    {
                        type = "node",
                        data = node
                    }).ToList()
                }
            }, timeout);
        if (apiStatus.RetCode != ApiStatusType.Ok || ret?["data"] == null)
            return (apiStatus, -1);
        int msgCode = int.TryParse(ret["data"]?["message_id"]?.ToString(), out int messageCode) ? messageCode : -1;
        return (apiStatus, msgCode);
    }

    public static async ValueTask<(ApiStatus, UserInfo, string)> GetStrangerInfo(long target, bool cache = false)
    {
        (ApiStatus status, JsonObject obj) = await ReactiveApiManager.SendApiRequest(new ApiRequest()
        {
            ApiRequestType = ActionType.GetStrangerInfo,
            ApiParams = new
            {
                user_id = target,
                no_cache = !cache
            }
        });
        if (status.RetCode != ApiStatusType.Ok || obj?["data"] == null)
            return (status, new UserInfo(), string.Empty);
        UserInfo info = obj["data"]?.ToObject<UserInfo>() ?? new UserInfo();
        //检查服务管理员权限

        return (status, info, obj["data"]?["qid"]?.ToString() ?? "");
    }

    public static async ValueTask<(ApiStatus, string)> GetImage(string file)
    {
        (ApiStatus status, JsonObject obj) = await ReactiveApiManager.SendApiRequest(new ApiRequest()
        {
            ApiRequestType = ActionType.GetImage,
            ApiParams = new
            {
                file
            }
        });
        return (status, obj["data"]?["file"]?.ToString() ?? "");
    }

    public static async ValueTask<(ApiStatus, string)> GetRecord(string file, RecordType type = RecordType.Mp3)
    {
        (ApiStatus status, JsonObject obj) = await ReactiveApiManager.SendApiRequest(new ApiRequest()
        {
            ApiRequestType = ActionType.GetImage,
            ApiParams = new
            {
                file,
                out_format = type
            }
        });
        return (status, obj["data"]?["file"]?.ToString() ?? "");
    }

    public static async ValueTask<(ApiStatus, CookieInfo)> GetCookie(string domain = "qun.qq.com")
    {
        (ApiStatus status, JsonObject res) = await ReactiveApiManager.SendApiRequest(new ApiRequest()
        {
            ApiRequestType = ActionType.GetCookie,
            ApiParams = new
            {
                domain,
            }
        });
        CookieInfo Info;
        var cookie = res["data"]?["cookies"]?.ToString()!;
        if (!string.IsNullOrEmpty(cookie))
        {
            var val = cookie.Split(";");
            if (val.Length >= 2)
            {
                Info = new()
                {
                    Cookie = cookie,
                    Pskey = val[0][7..],
                    Skey = val[1][6..]
                };
            }
            else
            {
                Info = new();
            }
        }
        else
        {
            Info = new();
        }
        return (status, Info);
    }

    public static async ValueTask<(ApiStatus, Entities.Info.FileInfo)> GetFile(string fileid)
    {
        var (status, data) = await ReactiveApiManager.SendApiRequest(new ApiRequest()
        {
            ApiRequestType = ActionType.GetFile,
            ApiParams = new
            {
                file_id = fileid
            }
        });
        return (status, data?["data"]?.ToObject<Entities.Info.FileInfo>() ?? new());
    }

    public static async ValueTask<ApiStatus> EmojiLike(long msgId, string emojiid)
    {
        var (status, data) = await ReactiveApiManager.SendApiRequest(new ApiRequest()
        {
            ApiRequestType = ActionType.EmojiLike,
            ApiParams = new
            {
                message_id = msgId,
                emoji_id = emojiid
            }
        });
        return status;
    }

    public static async ValueTask<ApiStatus> ForwardMsgSignleGroup(long groupid, long msgId)
    {
        var (status, _) = await ReactiveApiManager.SendApiRequest(new ApiRequest()
        {
            ApiRequestType = ActionType.ForwardSingleMsgToGroup,
            ApiParams = new
            {
                message_id = msgId,
                group_id = groupid
            }
        });
        return status;
    }

    public static async ValueTask<ApiStatus> ForwardMsgSignlePrivate(long userid, long msgId)
    {
        var (status, _) = await ReactiveApiManager.SendApiRequest(new ApiRequest()
        {
            ApiRequestType = ActionType.ForwardSingleMsgToPrivate,
            ApiParams = new
            {
                message_id = msgId,
                user_id = userid
            }
        });
        return status;
    }

    public static async ValueTask<ApiStatus> MarkPrivateMsgAsRead(long userid)
    {
        var (status, _) = await ReactiveApiManager.SendApiRequest(new ApiRequest()
        {
            ApiRequestType = ActionType.MarkPrivateMsgRead,
            ApiParams = new
            {
                user_id = userid
            }
        });
        return status;
    }

    public static async ValueTask<ApiStatus> MarkGroupMsgAsRead(long groupid)
    {
        var (status, _) = await ReactiveApiManager.SendApiRequest(new ApiRequest()
        {
            ApiRequestType = ActionType.MarkPrivateMsgRead,
            ApiParams = new
            {
                group_id = groupid
            }
        });
        return status;
    }

    public static async ValueTask<(ApiStatus, List<PeerInfo>)> GetGroupFileList(long groupid)
    {
        var (status, obj) = await ReactiveApiManager.SendApiRequest(new ApiRequest()
        {
            ApiRequestType = ActionType.GetGroupFileList,
            ApiParams = new
            {
                group_id = groupid,
                start_index = 0,
                file_count = 999
            }
        });
        return (status, obj?["data"]?["FileList"]?.ToObject<List<PeerInfo>>() ?? new List<PeerInfo>());
    }

    public static async ValueTask<ApiStatus> DelGroupFile(long groupid, string fileid)
    {
        var (status, obj) = await ReactiveApiManager.SendApiRequest(new ApiRequest()
        {
            ApiRequestType = ActionType.DelGrpupFile,
            ApiParams = new
            {
                group_id = groupid,
                file_id = fileid
            }
        });
        return status;
    }


    public static async ValueTask<ApiStatus> CleanCache()
    {
        var (status, _) = await ReactiveApiManager.SendApiRequest(new ApiRequest()
        {
            ApiRequestType = ActionType.CleanChahe,
        });
        return status;
    }
}
