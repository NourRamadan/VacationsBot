using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace VacationsBot.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceived);

            return Task.CompletedTask;
        }

        private async Task MessageReceived(IDialogContext context, IAwaitable<object> result)
        {
            if (context.UserData.ContainsKey("name"))
            {
                await context.PostAsync($"Hello, {context.UserData.Get<string>("name")}. Would you like to request a vacation?");
                context.Wait(VacationRequestIntended);
            }
            else
            {
                await context.PostAsync($"Hello. What's your name?");
                context.Wait(NameEntered);
            }                      
        }

        private async Task NameEntered(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;            
            context.UserData.SetValue("name",activity.Text);
            await context.PostAsync($"Hello, {context.UserData.Get<string>("name")}. Would you like to request a vacation?");
            context.Wait(VacationRequestIntended);
        }

        private async Task VacationRequestIntended(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;
            if (activity.Text.ToLower() == "yes" || activity.Text.ToLower() == "y")
            {
                await context.PostAsync($"Great! When would you like your vacation to start?");
                context.Wait(VacationStartSelected);
            }
            else
            {                
                await context.PostAsync($"Ok {context.UserData.Get<string>("name")}. Let me know if you change your mind.");
                context.Wait(MessageReceived);                
            }
        }

        private async Task VacationStartSelected(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;
            context.UserData.RemoveValue("name");
            context.Wait(MessageReceived);
        }
    }
}