using Microsoft.Bot.Connector;

public class Message
{
    public ConversationReference RelatesTo { get; set; }
    public String Text { get; set; }
}
