using MomoAPI.Converter;
using MomoAPI.Entities.Segment.DataModel;
using MomoAPI.Enumeration;
using MomoAPI.Extensions;
using MomoAPI.Model.API;
using System.Text.Json.Serialization;

namespace MomoAPI.Entities.Segment;

public class MomoSegment
{
    [JsonPropertyName("type")]
    [JsonConverter(typeof(EnumConverter<SegmentType>))]
    public SegmentType Type { get; set; }

    [JsonPropertyName("data")]
    public BaseMessage MessageData { get; set; }

    public MomoSegment(SegmentType type, BaseMessage messageData)
    {
        Type = type;
        MessageData = messageData;
    }

    internal OnebotSegment ToOnebotSegment()
    {
        return new OnebotSegment
        {
            MsgType = Type,
            RawData = MessageData.ToJsonObject()!
        };
    }

    public static MomoSegment Text(string text)
    {
        return new MomoSegment(SegmentType.Text, new Text()
        {
            Content = text
        });
    }

    public static MomoSegment Image(string file)
    {
        return new MomoSegment(SegmentType.Image, new Image()
        {
            File = file
        });
    }

    public static MomoSegment Image(byte[] buffer)
    {
        return Image("base64://" + Convert.ToBase64String(buffer));
    }

    public static MomoSegment Image(Stream stream)
    {
        byte[] buffer = new byte[stream.Length];
        stream.Read(buffer, 0, buffer.Length);
        return Image(buffer);
    }

    public static MomoSegment At(long qq)
    {
        return new MomoSegment(SegmentType.At, new At()
        {
            Target = qq.ToString()
        });
    }

    public static MomoSegment Face(int id)
    {
        return new MomoSegment(SegmentType.Face, new Face()
        {
            Id = id
        });
    }

    public static MomoSegment Reply(long messageid)
    {
        return new MomoSegment(SegmentType.Reply, new Reply()
        {
            Id = messageid,
        });
    }

    public static MomoSegment AtAll()
    {
        return new MomoSegment(SegmentType.At, new At()
        {
            Target = "all",
        });
    }

    public static MomoSegment File(string data, string name = "")
    {
        return new MomoSegment(SegmentType.File, new DataModel.File()
        {
            Data = data,
            Name = name
        });
    }

    public static MomoSegment Video(string data)
    {
        return new MomoSegment(SegmentType.Video, new DataModel.Video()
        {
            Data = data
        });
    }

    public static MomoSegment Record(string data)
    {
        return new MomoSegment(SegmentType.Record, new Record()
        {
            File = data
        });
    }

    public static MomoSegment Json(string data)
    {
        return new MomoSegment(SegmentType.Json, new Json()
        {
            Connect = data
        });
    }


    public static MomoSegment Music(string jumpUrl, string AudioUrl, string imageUrl, string song, string singer)
    {
        return new MomoSegment(SegmentType.Music, new Music()
        {
            Audio = AudioUrl,
            Url = jumpUrl,
            Image = imageUrl,
            Title = song,
            Singer = singer
        });
    }

    public static MomoSegment CustomMusic(MusicType type, string jumpUrl, string AudioUrl, string imageUrl, string song, string singer)
    {
        var data = Utils.Utils.SignMusic(type, jumpUrl, AudioUrl, imageUrl, song, singer);
        return Json(data);
    }

    public static MomoSegment Music_QQ(string jumpUrl, string AudioUrl, string imageUrl, string song, string singer)
    {
        return CustomMusic(MusicType.QQ, jumpUrl, AudioUrl, imageUrl, song, singer);
    }

    public static MomoSegment Music_163(string jumpUrl, string AudioUrl, string imageUrl, string song, string singer)
    {
        return CustomMusic(MusicType._163, jumpUrl, AudioUrl, imageUrl, song, singer);
    }

    public static implicit operator MomoSegment(string text)
    {
        return Text(text);
    }
}
