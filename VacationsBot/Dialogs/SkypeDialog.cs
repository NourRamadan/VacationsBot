using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Autofac;

namespace VacationsBot.Dialogs
{

    [Serializable]
    public class SkypeDialog : IDialog<object>
    {
        public static ResumptionCookie resumptionCookie;

        public SkypeDialog()
        {

        }
        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;
            if (message.Text == null) //Invoked when resuming the conversation, so it starts the knock knock joke
            {
                bool waitingForSkype = false;
                context.UserData.TryGetValue<bool>("waitingForSkype", out waitingForSkype);
                if (!waitingForSkype)
                {
                    context.UserData.SetValue<bool>("waitingForSkype", true);
                    await context.PostAsync("Knock knock...");
                }
            }
            else if (message.Text == "reset") //resets the flow
            {
                await context.PostAsync("reset");
                context.UserData.SetValue<bool>("waitingForSkype", false);
            }
            else if (message.Text.ToLower().Contains("there")) //Who's there?
            {
                await context.PostAsync("bot");
            }
            else if (message.Text.ToLower().Contains("register")) //Makes the bot remember who you are on Skype
            {
                SkypeDialog.resumptionCookie = new ResumptionCookie(message);
                await context.PostAsync("Registered");
            }
            else //Bot who?
            {
                context.UserData.SetValue<bool>("waitingForSkype", false);
            }

            context.Wait(MessageReceivedAsync);
        }
    }
    public class ResumeSkype
    {
        public static async Task Resume(ResumptionCookie resumptionCookie)
        {
            var message = resumptionCookie.GetMessage();
            using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, message))
            {
                IStateClient sc = scope.Resolve<IStateClient>();
                BotData userData = sc.BotState.GetUserData(message.ChannelId, message.From.Id);

                //Tell Skype to continue the conversation we registered before
                await Conversation.ResumeAsync(resumptionCookie, message);

                bool waitingForSkype = true;

                while (waitingForSkype)
                {
                    //Keep checking if Skype is done with the questions on that channel
                    userData = sc.BotState.GetUserData(message.ChannelId, message.From.Id);
                    waitingForSkype = userData.GetProperty<bool>("waitingForSkype");
                }
            }
        }
    }
}