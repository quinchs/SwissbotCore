using System.Threading.Tasks;

namespace SwissbotCore.Modules
{
   [DiscordCommandClass()]
   public class CovidCommandClass : CommandModuleBase
  {
    [DiscordCommand("covid", commandHelp = "(PREFIX)covid19", description = "Covid-19 symptom tracker")]
    public async Task Covid()
    {
        var typingChannel = Context.Channel;
        //await typingChannel.TriggerTypingAsync();
        await Context.Message.Channel.SendMessageAsync("You have requested for the COVID-19 **symptom** tracker! **IF YOU ARE IN MAINLAND UK," +
             " YOU MUST WEAR FACE COVERING BEFORE TRAVELLING ON THE TUBE, NATIONAL RAIL TRAINS, BUSES, OVERGROUND, TRAMS,AS WELL AS DLR FROM 15/6/2020** " +
             "**STAY AT HOME IF YOU HAVE SYMPTOMS IN MOST COUNTRIES!** The information you provide in this tracker will not be kept nor shared/rented to anyone." +
             " As the creator of this module is in the UK some information may not apply to you. Here at Swiss001, we encourage you to stay at home as much as you can," +
             " this virus has proven to be deadly and indiscriminant to **all** ages.To proceed with the tracker use the command `*proceed`!");
    }

      [DiscordCommand("proceed", commandHelp = "(PREFIX)proceed", description = "Procced to step1/INQ1")]
      public async Task Proceed()
      {
            var typingChannel = Context.Channel;
            //await typingChannel.TriggerTypingAsync();
            await Context.Message.Channel.SendMessageAsync("Do you have any of the following symptoms? A fever or chills or a new persistent cough." +
                " If you have these symptoms, based on your local medical authority you may need to self-isolate. Do this to protect your loved ones, to" +
                " protect your community, to save lives. do `*nosymp1`!");
    }

     [DiscordCommand("nosymp1", commandHelp = "(PREFIX)nosymp1", description = "Procced to step2/INQ2 if no symptoms")]
     public async Task Nosymp1()
     {
            var typingChannel = Context.Channel;
            //await typingChannel.TriggerTypingAsync();
            await Context.Message.Channel.SendMessageAsync("Do you have any of the following symptoms? Have you developed a new loss of smell and taste or an " +
                "ache in your body parts and have. If you have these symptoms, based on your local medical authority you may need to be **TESTED**. Do this to protect your" +
                " loved ones, to protect your community, to save lives.If you currently have these symptoms, you may leave this session, if you don't have any symptoms yet, " +
                "you may continue! To continue use command: nosymp2.");
     }

     [DiscordCommand("nosymp2", commandHelp = "(PREFIX)nosymp2", description = "Procced to step3/INQ3 if no symptoms")]
     public async Task Nosymp2()
     {
            var typingChannel = Context.Channel;
            //await typingChannel.TriggerTypingAsync();
            await Context.Message.Channel.SendMessageAsync("Do you **BOTH** of the following symptoms? Diarrhea or an unusual headache. If you have " +
                "**BOTH** of these symptoms, based on your local medical authority you may need to be **TESTED**. Do this to protect your loved ones, to " +
                "protect your community, to save lives.If you currently have these symptoms, you may leave this session, if you don't have any symptoms " +
                "yet, you may continue! To continue use command: nosymp3.");
     }

     [DiscordCommand("nosymp3", commandHelp = "(PREFIX)nosymp3", description = "Procced to step4/INQ4 if no symptoms")]
     public async Task Nosymp3()
     {
            var typingChannel = Context.Channel;
            //await typingChannel.TriggerTypingAsync();
            await Context.Message.Channel.SendMessageAsync("You do not currently have any **major** symptoms, if you have any other symptoms,wait for a a week," +
      " see if they go, if not then contact your doctor or medical authority for help. On behalf of the Swiss001 team we hope you get well soon! Made by Azuma#9673! " +
      "**Remember any serious medical issue need to be treated immediatly, your medical authority is still open for business. Stay safe and ensure you keep 2metres away!");
          }
     }
 }
